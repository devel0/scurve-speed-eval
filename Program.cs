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
        public Duration Duration { get; set; } = Duration.FromSeconds(0.1);
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
            
            var accelFn = "1-cos(x)";
            sb.AppendLine($"accelFn ---> {accelFn}");

            var accelOverDuration = accelFn.Substitute("x", $"x/duration*2*pi").Simplify();            
            sb.AppendLine($"accelOverDuration ---> {accelOverDuration}");
            
            var realAccel = $"targetspeed / ({accelOverDuration.Integrate("x").Substitute("x", "duration").Simplify()}) * ({accelOverDuration})".Simplify();
            sb.AppendLine($"realAccel ---> {realAccel}");

            var realSpeed = realAccel.Integrate("x").Simplify();
            sb.AppendLine($"realSpeed ---> {realSpeed}");            

            var t = Duration.FromSeconds(0);
            var t_step = m.Duration / m.RenderPts;

            var accelDataSet = new List<PlotData>();
            var speedDataSet = new List<PlotData>();

            var accelExpandFnCompiled = realAccel
                .Substitute("duration", m.Duration.Seconds)
                .Substitute("targetspeed", m.TargetSpeed.RevolutionsPerSecond)
                .Compile("x");

            var speedExpandFnCompiled = realSpeed
                .Substitute("targetspeed", m.TargetSpeed.RevolutionsPerSecond)
                .Substitute("duration", m.Duration.Seconds)
                .Compile("x");

            while (t < m.Duration)
            {
                accelDataSet.Add(new PlotData(
                    t.Seconds,
                    double.Parse(accelExpandFnCompiled.Substitute(t.Seconds).Real.ToString())));

                speedDataSet.Add(new PlotData(
                    t.Seconds,
                    double.Parse(speedExpandFnCompiled.Substitute(t.Seconds).Real.ToString())));

                t += t_step;
            }

            var accelSerie = new OxyPlot.Series.LineSeries()
            {
                Title = "Accel (rps2)",
                DataFieldX = "x",
                DataFieldY = "y",
                ItemsSource = accelDataSet
            };
            pv.Model.Series.Add(accelSerie);

            var speedSerie = new OxyPlot.Series.LineSeries()
            {
                Title = "Speed (rps)",
                DataFieldX = "x",
                DataFieldY = "y",
                ItemsSource = speedDataSet
            };
            pv.Model.Series.Add(speedSerie);

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
                        SmallChange = 0.5
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
                //p.Height = 150;                

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

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            //p.Render(context);
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("t-sin(t)".Differentiate("t").Latexise());

            GuiToolkit.CreateGui<MainWindow>();
        }
    }
}
