from sympy import *
from sympy.plotting import plot
import numpy as np
import math

t = symbols('t')

print("acceleration")
e1 = 1-cos(t)
pprint(e1)

print("\nspeed")
e2 = integrate(e1, t)
pprint(e2)

print("\nposition")
e3 = integrate(e2, t)
pprint(e3)

p1 = plot(e1, show=False, line_color='#ff0000',
          legend=True,
          xlim=[0, 2*math.pi],
          ylim=[0, 2*math.pi*1.3])
p2 = plot(e2, show=False, line_color='#0000ff')
p3 = plot(e3, show=False, line_color='#00ff00')
p1.append(p2[0])
p1.append(p3[0])

p1[0].label = 'accel [' + p1[0].label + ']'
p2[0].label = 'speed [' + p2[0].label + ']'
p3[0].label = 'pos [' + p3[0].label + ']'

p1.show()
