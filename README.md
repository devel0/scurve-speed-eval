# scurve speed eval

Show how s-curve speed type can be obtained.

<img src="data/img/demo.gif" width="700"/>

references:
- [How to get sinusoidal s-curve for a stepper motor](http://fightpc.blogspot.com/2018/04/how-to-get-sinusoidal-s-curve-for.html)

## Description

This app is not intended to produce scurve speed in realtime for constrained devices for those an approach is already explained in the article cited in references above, but wants to explain how much acceleration is required to reach a target speed in a given time using a s-curve speed profile.

I used the wonderful [AngouriMath](https://github.com/asc-community/AngouriMath) library to execute calculus in the code to demostrate the process described following, but final formula can be used of course.

- from the fn [1-cos(x)][1] that will produce follow for x=0..2pi

![](data/img/scurve-base.png)

where its integral ( the speed ) reach 2pi for x=2pi

- [accelOverDuration][2] was computed to make the accel function applicable to the `[0..duration]` domain instead of the original `[0..2pi]`

<!-- $$
\Large
accelOverDuration=1-\cos\left(\frac{x}{duration}\cdot 2\cdot \pi\right)
$$ --> 

<br/>
<div align="center"><img src="https://render.githubusercontent.com/render/math?math=%5CLarge%0AaccelOverDuration%3D1-%5Ccos%5Cleft(%5Cfrac%7Bx%7D%7Bduration%7D%5Ccdot%202%5Ccdot%20%5Cpi%5Cright)"></div>
<br/>

- accelOverDuration integral produce a speed that we need to scale in order to fit with required targetspeed; [a factor][3] is computed to obtain *realAccel*:

<!-- $$
\Large
realAccel = \frac{targetspeed}{\int_0^{duration} accelOverDuration}\cdot accelOverDuration
$$ --> 

<div align="center"><img src="https://render.githubusercontent.com/render/math?math=%5CLarge%0ArealAccel%20%3D%20%5Cfrac%7Btargetspeed%7D%7B%5Cint_0%5E%7Bduration%7D%20accelOverDuration%7D%5Ccdot%20accelOverDuration"></div> 

<!-- $$
\Large
realAccel=\frac{targetspeed}{duration}\cdot \left(1-\cos\left(\frac{x}{duration}\cdot 2\cdot \pi\right)\right)
$$ --> 

<br/>
<div align="center"><img src="https://render.githubusercontent.com/render/math?math=%5CLarge%0ArealAccel%3D%5Cfrac%7Btargetspeed%7D%7Bduration%7D%5Ccdot%20%5Cleft(1-%5Ccos%5Cleft(%5Cfrac%7Bx%7D%7Bduration%7D%5Ccdot%202%5Ccdot%20%5Cpi%5Cright)%5Cright)"></div>
<br/>

having accel a shape where the max accel is at x=duration/2 the above can simplified as

<!-- $$
\Large
maxRealAccel=\frac{targetspeed}{duration}\times2
$$ --> 

<br/>
<div align="center"><img src="https://render.githubusercontent.com/render/math?math=%5CLarge%0AmaxRealAccel%3D%5Cfrac%7Btargetspeed%7D%7Bduration%7D%5Ctimes2"></div>
<br/>

*example*: targetspeed=0.8r/s ; duration=0.5s ; maxRealAccel=3.2r/s2

- The *realSpeed* results as the [integral of realAccel][4]:

<!-- $$
\Large
realSpeed=\frac{\frac{-1}{2}\cdot \sin\left(\frac{2\cdot \pi\cdot x}{duration}\right)\cdot targetspeed}{\pi}+\frac{targetspeed\cdot x}{duration}
$$ --> 

<br/>
<div align="center"><img src="https://render.githubusercontent.com/render/math?math=%5CLarge%0ArealSpeed%3D%5Cfrac%7B%5Cfrac%7B-1%7D%7B2%7D%5Ccdot%20%5Csin%5Cleft(%5Cfrac%7B2%5Ccdot%20%5Cpi%5Ccdot%20x%7D%7Bduration%7D%5Cright)%5Ccdot%20targetspeed%7D%7B%5Cpi%7D%2B%5Cfrac%7Btargetspeed%5Ccdot%20x%7D%7Bduration%7D"></div>
<br/>

- position from time `x`

<!-- $$
\Large
pos = \frac{\cos\left(\frac{2\cdot \pi\cdot x}{duration}\right)\cdot {duration}\cdot targetspeed}{4\cdot {\pi}^{2}}+\frac{targetspeed\cdot {x}^{2}}{2\cdot duration}
$$ --> 

<div align="center"><img src="https://render.githubusercontent.com/render/math?math=%5CLarge%0Apos%20%3D%20%5Cfrac%7B%5Ccos%5Cleft(%5Cfrac%7B2%5Ccdot%20%5Cpi%5Ccdot%20x%7D%7Bduration%7D%5Cright)%5Ccdot%20%7Bduration%7D%5Ccdot%20targetspeed%7D%7B4%5Ccdot%20%7B%5Cpi%7D%5E%7B2%7D%7D%2B%5Cfrac%7Btargetspeed%5Ccdot%20%7Bx%7D%5E%7B2%7D%7D%7B2%5Ccdot%20duration%7D"></div>

- targetSpeed required from known distance to cover `pos` in given `duration` time

<!-- $$
\Large
targetspeed=\frac{4 \cdot \pi^{2} \cdot pos}{duration \cdot (1+2 \pi)}
$$ --> 

<div align="center"><img src="https://render.githubusercontent.com/render/math?math=%5CLarge%0Atargetspeed%3D%5Cfrac%7B4%20%5Ccdot%20%5Cpi%5E%7B2%7D%20%5Ccdot%20pos%7D%7Bduration%20%5Ccdot%20(1%2B2%20%5Cpi)%7D"></div>


[1]: https://github.com/devel0/scurve-speed-eval/blob/0bff63605a3f7fae49d6f56aab4b813efa755242/Program.cs#L48

[2]: https://github.com/devel0/scurve-speed-eval/blob/0bff63605a3f7fae49d6f56aab4b813efa755242/Program.cs#L51

[3]: https://github.com/devel0/scurve-speed-eval/blob/0bff63605a3f7fae49d6f56aab4b813efa755242/Program.cs#L54

[4]: https://github.com/devel0/scurve-speed-eval/blob/0bff63605a3f7fae49d6f56aab4b813efa755242/Program.cs#L57

## Prerequisites

- [vscode](https://code.visualstudio.com/)
- [dotnet 5.0](https://dotnet.microsoft.com/download)

## Quickstart

```sh
git clone https://github.com/devel0/netcore-sci
git clone https://github.com/devel0/netcore-util
git clone https://github.com/devel0/scurve-speed-eval
cd scurve-speed-eval
dotnet run
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
