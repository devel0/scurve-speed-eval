using System;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using UnitsNet;
using static System.Math;
using SearchAThing;
using UnitsNet.Units;

// in this example
// describe scurve shaped profile speed motion
// as function of EXECUTION_TIME and ROUND_CNT
// note: cruise speed will not maintained, only increase and decrease immediately

namespace scurve_xlsx
{
    class Program
    {
        static void Main(string[] args)
        {
            // nema17 - 24v datasheet
            var MOTOR_TYPE = "nema17";
            var MOTOR_VOLTAGE = ElectricPotentialDc.FromVoltsDc(24);
            var SPEED_MAX = RotationalSpeed.FromRevolutionsPerMinute(200);
            var TORQUE_MAX = Torque.FromNewtonMeters(0.4);
            // problem data
            var MASS = Mass.FromKilograms(2);
            var LEVER_ARM_LEN = Length.FromCentimeters(1);
            var ROUND_CNT = Angle.FromRevolutions(1d);
            var EXECUTION_TIME = Duration.FromSeconds(2);
            // config
            var TIME_STEP = Duration.FromMilliseconds(1);

            // speed required to achieve given ROUND_CNT in EXECUTION_TIME without cruise
            var minTargetSpeed = 2 * ROUND_CNT / EXECUTION_TIME;
            if (minTargetSpeed > SPEED_MAX)
            {
                System.Console.WriteLine($"W: given position {ROUND_CNT} round cannot established due to speed_max:{SPEED_MAX.RevolutionsPerSecond} rps vs actual required target speed:{minTargetSpeed.RevolutionsPerSecond} rps");
                return;
            }

            // s:target speed
            var s = minTargetSpeed;

            // d:duration
            var d = EXECUTION_TIME;

            var minHoldingTorque = Torque.FromKilogramForceCentimeters(MASS.Kilograms * LEVER_ARM_LEN.Centimeters);
            if (minHoldingTorque > TORQUE_MAX)
            {
                System.Console.WriteLine($"W: given mass {MASS} at lever arm distance {LEVER_ARM_LEN} generate {minHoldingTorque.KilogramForceCentimeters} kgfcm torque versus max {TORQUE_MAX.KilogramForceCentimeters} kgfcm");
                return;
            }

            var minDynAccel = RotationalAcceleration.FromRevolutionsPerSecondSquared(4 * s.RevolutionsPerSecond / d.Seconds);
            var I = MassMomentOfInertia.FromKilogramSquareCentimeters(MASS.Kilograms * Pow(LEVER_ARM_LEN.Centimeters, 2));
            // torque            = inertia * angularaccel
            // F:[M*L*T-2]*r:[L] = I:[M*L2]*a:[T-2]
            // [M*L2*T-2]        = [M*L2*T-2]
            var minDynTorque = Torque.FromKilogramForceMeters(I.KilogramSquareMeters * minDynAccel.RadiansPerSecondSquared);

            if (minDynTorque > TORQUE_MAX)
            {
                System.Console.WriteLine($"W: accelerating given mass {MASS} at angaccel {minDynAccel} generates torque {minDynTorque.NewtonCentimeters} Ncm great than max {TORQUE_MAX.NewtonCentimeters} Ncm");
                return;
            }

            var srcPathfilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "template.xlsx");
            var dstPathfilename = "output.xlsx";
            File.Copy(srcPathfilename, dstPathfilename, true);
            using (var wb = new ClosedXML.Excel.XLWorkbook(dstPathfilename))
            {
                var ws = wb.Worksheets.First();

                IXLCell cell = null;

                Action<int, int, object> setCell = (r, c, val) =>
                {
                    cell = ws.Cell(r, c);
                    cell.Value = val;
                };

                Action<int, int, object> setCellBold = (r, c, val) =>
                {
                    cell = ws.Cell(r, c);
                    cell.Value = val;
                    cell.Style.Font.SetBold();
                };

                var row = 1;
                int col = 1;
                var colTime = col++;
                var colAccel = col++;
                var colSpeed = col++;
                var colPosRot = col++;

                setCellBold(row, colTime, "TIME (s)");
                setCellBold(row, colAccel, "ACCEL (rps2)");
                setCellBold(row, colSpeed, "SPEED (rps)");
                setCellBold(row, colPosRot, "POS (rot)");
                ++row;

                var t = Duration.FromSeconds(0);
                var tMax = t + EXECUTION_TIME;

                var halfTMax = tMax / 2;                

                ws.Cell("MotorType").Value = MOTOR_TYPE;
                ws.Cell("MotorSpeedMax").Value = SPEED_MAX;
                ws.Cell("MotorTorqueMaxAtSpeedMax").Value = TORQUE_MAX;
                ws.Cell("MotorVoltage").Value = MOTOR_VOLTAGE;
                ws.Cell("ProblemDuration").Value = EXECUTION_TIME;
                ws.Cell("ProblemLoadLeverArmLength").Value = LEVER_ARM_LEN;
                ws.Cell("ProblemLoadMass").Value = MASS;
                ws.Cell("ProblemRevolutions").Value = ROUND_CNT;
                ws.Cell("ResultingTorque").Value = minDynTorque.ToUnit(TorqueUnit.NewtonMeter);
                ws.Cell("ResultingAccel").Value = minDynAccel;
                ws.Cell("ResultingSpeedMax").Value = minTargetSpeed.ToUnit(RotationalSpeedUnit.RevolutionPerMinute);

                var tEps = Duration.FromNanoseconds(1);
                while (t.LessThanOrEqualsTol(tEps, tMax))
                {
                    setCell(row, colTime, t.Seconds);

                    var accel = RotationalAcceleration.FromRevolutionsPerSecondSquared(0);
                    var speed = RotationalSpeed.FromRevolutionsPerSecond(0);
                    var pos = Angle.FromRevolutions(0);

                    if (t.LessThanOrEqualsTol(tEps, halfTMax))
                    {
                        accel = RotationalAcceleration.FromRevolutionsPerSecondSquared(
                            2 * s.RevolutionsPerSecond / d.Seconds * (1 - Cos(4 * PI * t / d)));

                        speed = RotationalSpeed.FromRevolutionsPerSecond(
                            2 * s.RevolutionsPerSecond / d.Seconds * (t - d * Sin(4 * PI * t / d) / (4 * PI)).Seconds);

                        pos = s * d * (Cos(4 * PI * t / d) - 1) / (8 * Pow(PI, 2)) + (s * t) * (t / d);
                    }

                    if (t.GreatThanOrEqualsTol(tEps, halfTMax))
                    {
                        var th = t - d / 2;

                        accel = RotationalAcceleration.FromRevolutionsPerSecondSquared(
                            2 * s.RevolutionsPerSecond / d.Seconds * (Cos(4 * PI * th / d) - 1));

                        speed = RotationalSpeed.FromRevolutionsPerSecond(
                            2 * s.RevolutionsPerSecond * Sin(4 * PI * th / d) / (4 * PI)
                            - (2 * s * th / d - s).RevolutionsPerSecond);

                        pos = s * d * (1 - Cos(4 * PI * th / d)) / (8 * Pow(PI, 2)) - (s * th) * (th / d) + s * th + s * d / 4;
                    }

                    setCell(row, colAccel, accel.RevolutionsPerSecondSquared);
                    setCell(row, colSpeed, speed.RevolutionsPerSecond);
                    setCell(row, colPosRot, pos.Revolutions);

                    ++row;
                    t += TIME_STEP;
                }

                wb.Save();
            }
        }
    }
}
