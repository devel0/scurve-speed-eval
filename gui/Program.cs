using System;
using static System.Math;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using AngouriMath;
using AngouriMath.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using OxyPlot;
using OxyPlot.Avalonia;
using SearchAThing;
using SearchAThing.Gui;
using UnitsNet;

namespace scurve_speed_eval
{

    public class PlotData
    {
        public PlotData(double x, double y) { this.x = x; this.y = y; }
        public double x { get; set; }
        public double y { get; set; }
    }

    public class Model
    {
        public Duration Duration { get; set; } = Duration.FromSeconds(5);
        public RotationalSpeed TargetSpeed { get; set; } = RotationalSpeed.FromRevolutionsPerSecond(0.8);
        public int RenderPts { get; set; } = 100;
    }

    public class MainWindow : Win
    {
        PlotView pv = null;
        TextBox tboxLog = null;
        Model m = new Model();

        void Recompute()
        {
            pv.Model.Series.Clear();

            var sb = new StringBuilder();

            var accelBase = "1-cos(t)";
            var accelOverDuration = accelBase.Substitute("t", $"(t/(d/2)*2*pi)");
            var accelCoeff = $"s / abs({accelOverDuration.Integrate("t").Substitute("t", "(d/2)")})";
            var accel = $"({accelCoeff}) * ({accelOverDuration})";
            var accelInt = accel.Integrate("t");
            var speed = accelInt;
            var speedAtHalf = accelInt.Substitute("t", "(d/2)");
            var speedInt = speed.Integrate("t");
            var posAtZero = speedInt.Substitute("t", 0);
            var pos = $"({speedInt})-{(posAtZero)}";
            var posAtHalf = pos.Substitute("t", "(d/2)");

            var targetspeed = $"{pos.ToString()}=x".Substitute("t", "d").Solve("s");

            var deAccelBase = "cos(t)-1";
            var deAccelOverDuration = deAccelBase.Substitute("t", $"(t/(d/2)*2*pi)");
            var deAccelCoeff = $"s / abs({deAccelOverDuration.Integrate("t").Substitute("t", "(d/2)")})";
            var deAccel = $"({deAccelCoeff})*({deAccelOverDuration})";
            var deAccelInt = deAccel.Integrate("t");
            var deSpeedAtZero = deAccelInt.Substitute("t", 0);
            var deSpeedAtHalf = deAccelInt.Substitute("t", "(d/2)");
            var deSpeed = $"({deAccelInt})-({deSpeedAtZero})+({speedAtHalf})";
            var deSpeedInt = deSpeed.Integrate("t");
            var dePosAtZero = deSpeedInt.Substitute("t", 0);
            var dePos = $"({deSpeedInt})-({dePosAtZero})+({posAtHalf})";

            var t = Duration.FromSeconds(0);
            var t_step = m.Duration / m.RenderPts;

            var accelDataSet = new List<PlotData>();
            var speedDataSet = new List<PlotData>();
            var posDataSet = new List<PlotData>();

            var deAccelDataSet = new List<PlotData>();
            var deSpeedDataSet = new List<PlotData>();
            var dePosDataSet = new List<PlotData>();

            var accelCompiled = accel
                .Substitute("s", m.TargetSpeed.RevolutionsPerSecond)
                .Substitute("d", m.Duration.Seconds)
                .Compile("t");

            var deAccelCompiled = deAccel
                .Substitute("s", m.TargetSpeed.RevolutionsPerSecond)
                .Substitute("d", m.Duration.Seconds)
                .Compile("t");

            var speedCompiled = speed
                .Substitute("s", m.TargetSpeed.RevolutionsPerSecond)
                .Substitute("d", m.Duration.Seconds)
                .Compile("t");

            var deSpeedCompiled = deSpeed
                .Substitute("s", m.TargetSpeed.RevolutionsPerSecond)
                .Substitute("d", m.Duration.Seconds)
                .Compile("t");

            var posCompiled = pos
                .Substitute("s", m.TargetSpeed.RevolutionsPerSecond)
                .Substitute("d", m.Duration.Seconds)
                .Compile("t");

            var dePosCompiled = dePos
                .Substitute("s", m.TargetSpeed.RevolutionsPerSecond)
                .Substitute("d", m.Duration.Seconds)
                .Compile("t");

            var halfDuration = m.Duration / 2;

            var timeTol = Duration.FromNanoseconds(1);

            while (t.LessThanOrEqualsTol(timeTol, m.Duration))
            {
                if (t.LessThanOrEqualsTol(timeTol, halfDuration))
                {
                    accelDataSet.Add(new PlotData(
                        t.Seconds,
                        double.Parse(accelCompiled.Substitute(t.Seconds).Real.ToString())));

                    speedDataSet.Add(new PlotData(
                        t.Seconds,
                        double.Parse(speedCompiled.Substitute(t.Seconds).Real.ToString())));

                    posDataSet.Add(new PlotData(
                        t.Seconds,
                        double.Parse(posCompiled.Substitute(t.Seconds).Real.ToString())));
                }

                if (t.GreatThanOrEqualsTol(timeTol, halfDuration))
                {
                    deAccelDataSet.Add(new PlotData(
                        t.Seconds,
                        double.Parse(deAccelCompiled.Substitute((t - halfDuration).Seconds).Real.ToString())));

                    deSpeedDataSet.Add(new PlotData(
                        t.Seconds,
                        double.Parse(deSpeedCompiled.Substitute((t - halfDuration).Seconds).Real.ToString())));

                    dePosDataSet.Add(new PlotData(
                        t.Seconds,
                        double.Parse(dePosCompiled.Substitute((t - halfDuration).Seconds).Real.ToString())));
                }

                t += t_step;
            }

            var accelSerie = new OxyPlot.Series.LineSeries()
            {
                Title = "Accel (rps2)",
                DataFieldX = "x",
                DataFieldY = "y",
                ItemsSource = accelDataSet,
                Color = OxyColor.Parse("#9ccc65")
            };
            pv.Model.Series.Add(accelSerie);

            var deAccelSerie = new OxyPlot.Series.LineSeries()
            {
                Title = "DeAccel (rps2)",
                DataFieldX = "x",
                DataFieldY = "y",
                ItemsSource = deAccelDataSet,
                Color = OxyColor.Parse("#6b9b37")
            };
            pv.Model.Series.Add(deAccelSerie);

            var speedSerie = new OxyPlot.Series.LineSeries()
            {
                Title = "Speed (rps)",
                DataFieldX = "x",
                DataFieldY = "y",
                ItemsSource = speedDataSet,
                Color = OxyColor.Parse("#42a5f5")
            };
            pv.Model.Series.Add(speedSerie);

            var deSpeedSerie = new OxyPlot.Series.LineSeries()
            {
                Title = "DeSpeed (rps)",
                DataFieldX = "x",
                DataFieldY = "y",
                ItemsSource = deSpeedDataSet,
                Color = OxyColor.Parse("#0077c2")
            };
            pv.Model.Series.Add(deSpeedSerie);

            var posSerie = new OxyPlot.Series.LineSeries()
            {
                Title = "Pos (rev)",
                DataFieldX = "x",
                DataFieldY = "y",
                ItemsSource = posDataSet,
                Color = OxyColor.Parse("#ef5350")
            };
            pv.Model.Series.Add(posSerie);

            var dePosSerie = new OxyPlot.Series.LineSeries()
            {
                Title = "DePos (rev)",
                DataFieldX = "x",
                DataFieldY = "y",
                ItemsSource = dePosDataSet,
                Color = OxyColor.Parse("#b61827")
            };
            pv.Model.Series.Add(dePosSerie);

            pv.Model.Annotations.Clear();
            var note = new OxyPlot.Annotations.ArrowAnnotation
            {
                StartPoint = new DataPoint(m.Duration.Seconds / 2, m.TargetSpeed.RevolutionsPerSecond * 1.3),
                EndPoint = new DataPoint(m.Duration.Seconds / 2, m.TargetSpeed.RevolutionsPerSecond),
                Text = "targetspeed"
            };
            pv.Model.Annotations.Add(note);

            pv.ResetAllAxes();
            foreach (var x in pv.Model.Axes)
            {
                x.MajorGridlineStyle = LineStyle.Dot;
            }
            pv.InvalidatePlot();

            tboxLog.Text = sb.ToString();
        }

        public MainWindow() : base(new[]
        {
            "resm:OxyPlot.Avalonia.Themes.Default.xaml?assembly=OxyPlot.Avalonia"
        })
        {
            var grRoot = new Grid() { DataContext = m, Margin = new Thickness(10) };

            grRoot.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
            grRoot.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            grRoot.RowDefinitions.Add(new RowDefinition(100, GridUnitType.Pixel));

            pv = new PlotView();
            pv.Model = new PlotModel();
            Grid.SetRow(pv, 1);
            grRoot.Children.Add(pv);

            var grSplit = new GridSplitter() { Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
            Grid.SetRow(grSplit, 2);
            grRoot.Children.Add(grSplit);

            tboxLog = new TextBox() { Margin = new Thickness(0, 20, 0, 0) };
            Grid.SetRow(tboxLog, 2);
            grRoot.Children.Add(tboxLog);

            var durationConverter = new QuantityConverter((s, c) => Duration.Parse(s, c));
            var rotationalSpeedConverter = new QuantityConverter((s, c) => RotationalSpeed.Parse(s, c));

            {
                var sp = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Vertical };

                Func<string, TextBlock> tblkField = (s) =>
                    new TextBlock
                    {
                        Text = s,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        FontWeight = FontWeight.Bold,
                        Margin = new Thickness(10, 0, 0, 0)
                    };

                Func<string, IValueConverter, TextBox> tboxField = (propname, cvt) =>
                {
                    var tbox = new TextBox { MinWidth = 100, Margin = new Thickness(10, 0, 0, 0) };
                    tbox[!TextBox.TextProperty] = new Binding(propname) { Mode = BindingMode.TwoWay, Converter = cvt };
                    return tbox;
                };


                {
                    var spH = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
                    sp.Children.Add(spH);

                    TextBox durationTbox = null;
                    spH.Children.Add(tblkField("Duration").Eval((tblk) => { tblk.Margin = new Thickness(); return tblk; }));
                    spH.Children.Add(durationTbox = tboxField("Duration", durationConverter));

                    var sld = new Slider()
                    {
                        Width = 200,
                        Minimum = 0.5,
                        Maximum = 10,
                        LargeChange = 0.5,
                        SmallChange = 0.5,
                        Value = m.Duration.Seconds
                    };
                    sld.PropertyChanged += (a, b) =>
                    {
                        if (b.Property.Name == "Value")
                        {
                            var prev = m.Duration.Seconds;
                            var next = sld.Value.MRound(0.5);
                            if (prev != next)
                            {
                                m.Duration = Duration.FromSeconds(next);
                                durationTbox.Text = m.Duration.ToString();
                                Recompute();
                            }
                        }
                    };
                    spH.Children.Add(sld);

                    spH.Children.Add(tblkField("TargetSpeed"));
                    spH.Children.Add(tboxField("TargetSpeed", rotationalSpeedConverter));

                    spH.Children.Add(tblkField("RenderPts"));
                    spH.Children.Add(tboxField("RenderPts", null));
                }

                {
                    sp.Children.Add(new TextBlock()
                    {
                        Text = "plot mouse LEFT:(show value) - WHEEL:(zoom 1clk=rect-zoom 2clk=fit) - RIGHT:pan",
                        Margin = new Thickness(0, 10, 0, 0)
                    });
                }

                var btnRefresh = new Button() { Content = "Refresh" };
                btnRefresh.Click += (a, b) =>
                {
                    Recompute();
                };
                sp.Children.Add(btnRefresh);

                Grid.SetRow(sp, 0);
                grRoot.Children.Add(sp);
            }

            {
                Recompute();
                pv.ResetAllAxes();
                pv.InvalidatePlot();
            }

            this.Content = grRoot;
        }

        protected override void OnMeasureInvalidated()
        {
            base.OnMeasureInvalidated();

            pv.InvalidatePlot();
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            GuiToolkit.CreateGui<MainWindow>();
        }
    }
}
