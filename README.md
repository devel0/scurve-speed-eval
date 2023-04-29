# scurve speed eval

Show how s-curve speed type can be obtained.

<img src="data/img/demo.gif" width="700"/>

---

<!-- TOC -->
* [Description](#description)
* [Performance considerations](#performance-considerations)
* [Example](#example)
* [Prerequisites](#prerequisites)
* [Quickstart](#quickstart)
* [How this project was built](#how-this-project-was-built)
* [References](#references)
<!-- TOCEND -->

---

## Description

For symbolic calculus [AngouriMath](https://github.com/asc-community/AngouriMath) library was used.

For preliminary analysis [sympy](https://github.com/sympy/sympy) with [this](data/test.py) test was used.

![](data/img/scurve-accel-decel.png)

- lets name
    - **t** (time)
    - **x** (distance) 
    - **d** (total duration accel/decel)
    - **s** (final speed)

- suppose `[0,d/2]` the time for acceleration while `[d/2,d]` the time for deceleration to produce a target final speed `s` starting from speed=0 and pos=0
- from base acceleration function [1-cos(t)][1]
- expanding the period domain from `[0,2pi]` to `[0,d/2]` through [this][2] subst
- [stretching][3] the accel function so that its integral, the speed, achieve targetspeed
- acceleration results as [this][4]

:point_right: hit F5 if math not rendered

$$
\large
accel(t)=\frac{2\cdot s}{d}\cdot\left(1-\cos\left(\frac{4\cdot\pi\cdot t}{d}\right)\right)
$$

- positive max accel value is at midpoint ( t=1/4d because accel positive in `[0,d/2]` ) thus above can be simplified into

$$
\large
maxAccel=\frac{4\cdot s}{d}
$$

- [integration of accel][5] gives speed

$$
\large
speed(t) = \frac{2\cdot s}{d} \cdot \left( t - \frac{d\cdot \sin\left(\frac{4\cdot\pi\cdot t}{d}\right)}{4\cdot\pi} \right)
$$

- [integration of speed][6] gives pos ( normalized removing integration constant computed for t=0 )

$$
\large
pos(t) = \frac{s\cdot d \cdot \left( \cos\left(\frac{4\cdot\pi\cdot t}{d}\right)-1 \right)}{8\cdot\pi^2} + \frac{s\cdot t^2}{d}
$$

- [targetspeed][7] required from known final position `p` to reach in required time duration `d`

$$
\large
s = \frac{2\cdot p}{d}
$$

- from base deceleration function [cos(t)-1][8]

- sets `th` as

$$
\large
th=t-\frac{d}{2}
$$

- deceleration results as [this][9]

$$
\large
deAccel\left(th\right) = \frac{2\cdot s}{d}\cdot\left(\cos\left(\frac{4\cdot\pi\cdot th}{d}\right)-1\right)
$$

- despeed results as [this][10] ( given from deAccel integral subtracting integration constant and adding speed achieved by accel at `d/2` )

$$
\large
deSpeed\left(th\right) = \frac{2\cdot s\cdot\sin\left(\frac{4\cdot\pi\cdot th}{d} \right)}{4\cdot\pi}-\frac{2\cdot s\cdot th}{d}+s
$$

- depos results as [this][11] ( given from deSpeed integral subtracting integration constant and adding pos achieved by accel at `d/2` )

$$
\large
dePos\left(th\right) = \frac{s\cdot d\cdot\left( 1-\cos\left(\frac{4\cdot\pi\cdot th}{d}\right) \right)}{8\cdot\pi^2}-\frac{s\cdot th^2}{d}+s\cdot th+\frac{s\cdot d}{4}
$$

[1]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L48
[2]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L49
[3]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L50
[4]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L51
[5]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L53
[6]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L57
[7]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L60
[8]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L62
[9]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L65
[10]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L69
[11]: https://github.com/devel0/scurve-speed-eval/blob/435327c86769302a22fa54fb6b349a60a1b640d1/gui%2FProgram.cs#L72

## Performance considerations

For a realtime purpose a bezier approach should be used like the one implemented in the [Marlin](https://github.com/MarlinFirmware/Marlin/blob/d364babbc0cdb73337d86e2c4fd9170b46b7361a/Marlin/src/module/stepper.cpp#L651).

## Example

![](data/img/example.png)

This [example](examples/scurve-xlsx/Program.cs) can be executed through

```
dotnet run --project examples/scurve-xlsx
```

and it will produce follow [output.xlsx](examples/scurve-xlsx/output.xlsx) by applying above formulas and doing some test calc about max allowable torque based on motion and supposed load.

## Prerequisites

- [vscode](https://code.visualstudio.com/)
- [dotnet 5.0](https://dotnet.microsoft.com/download)

## Quickstart

```sh
git clone https://github.com/devel0/netcore-sci
git clone https://github.com/devel0/netcore-util
git clone https://github.com/devel0/scurve-speed-eval
cd scurve-speed-eval
dotnet build
dotnet run --project gui
```

## How this project was built

```sh
dotnet new console -n scurve-speed-eval
cd scurve-speed-eval
dotnet sln add scurve-speed-eval.csproj
dotnet sln add .
dotnet add reference ../netcore-util/netcore-util
dotnet add reference ../netcore-sci/netcore-sci
dotnet add package AngouriMath --version 1.2.0-preview.5
dotnet add package Avalonia.Desktop --version 0.10.0-preview6
dotnet add package OxyPlot.Avalonia --version 2.1.0-20200725.1
dotnet add package UnitsNet --version 4.76.0
dotnet run
```

## References

- [How to get sinusoidal s-curve for a stepper motor](http://fightpc.blogspot.com/2018/04/how-to-get-sinusoidal-s-curve-for.html)
