from skyfield.api import N, W, load, wgs84
from skyfield.trigonometry import position_angle_of

ts = load.timescale()
t = ts.utc(2019, 9, 30, 23)

eph = load('de421.bsp')
sun, moon, earth = eph['sun'], eph['moon'], eph['earth']
boston = earth + wgs84.latlon(42.3583 * N, 71.0636 * W)

b = boston.at(t)
m = b.observe(moon).apparent()
s = b.observe(sun).apparent()
print(position_angle_of(m.altaz(), s.altaz()))