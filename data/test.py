from sympy import *
from sympy.plotting import plot
import numpy as np
import math

D = 2*math.pi

t = symbols('t')

print("acceleration")
e1 = 1-cos(t)
pprint(e1)

print("\nspeed")
e2 = integrate(e1, t)
pprint(e2)

print("\nposition")
ie2 = integrate(e2, t)
e3 = integrate(e2, t) - ie2.subs(t, 0)
pprint(e3)

print("\ndecelAccel")
ed1 = cos(t)-1
pprint(ed1)

print("\ndecelSpeed")
ied2 = integrate(ed1, t)
ed2 = ied2 - ied2.subs(t, D) + e2.subs(t, D)
pprint(ed2)

print("\ndecelPos")
ied3 = integrate(ed2, t)
ed3 = ied3 - ied3.subs(t, D) + e3.subs(t, D)
pprint(ed3)

accelRange = (t, 0, D)
decelRange = (t, D, 2*D)

p1 = plot(e1, accelRange,
          show=False, line_color='#ff0000',
          legend=True,
          xlim=[0, 2*D],
          ylim=[-5, 65])

p2 = plot(e2, accelRange, show=False, line_color='#0000ff')
p3 = plot(e3, accelRange, show=False, line_color='#00ff00')
pd1 = plot(ed1, decelRange, show=False, line_color='#ffaa00')
pd2 = plot(ed2, decelRange, show=False, line_color='#00ffaa')
pd3 = plot(ed3, decelRange, show=False, line_color='#00aaff')

p1.append(p2[0])
p1.append(p3[0])
p1.append(pd1[0])
p1.append(pd2[0])
p1.append(pd3[0])

p1[0].label = 'accel [' + str(e1) + ']'
p2[0].label = 'speed [' + str(e2) + ']'
p3[0].label = 'pos [' + str(e3) + ']'
pd1[0].label = 'decelAccel [' + str(ed1) + ']'
pd2[0].label = 'decelSpeed [-t + sin(t) - decelSpeed(D) + speed(D)]'
pd3[0].label = 'decelPos [-t**2/2 - cos(t) - decelPos(D) + pos(D)]'

p1.show()
