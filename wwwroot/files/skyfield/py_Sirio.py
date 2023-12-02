from astropy import units as u
from astropy.coordinates import SkyCoord, AltAz, EarthLocation, get_sun
from astropy.time import Time
import datetime

# Coordenadas en equatoriales (RA, Declinación)
ra = 10.68458  # en grados
dec = 41.26917  # en grados

# Crear un objeto SkyCoord con las coordenadas equatoriales
equatorial_coord = SkyCoord(ra=ra*u.deg, dec=dec*u.deg, frame='icrs')

# Ubicación del observador (por ejemplo, para la ciudad de Nueva York)
location = EarthLocation(lat=40.7128*u.deg, lon=-74.0060*u.deg, height=0*u.m)

# Tiempo de observación (por ejemplo, ahora)
observing_time = Time(datetime.datetime.utcnow())

# Crear un objeto AltAz para representar coordenadas locales
altaz_coord = equatorial_coord.transform_to(AltAz(obstime=observing_time, location=location))

# Imprimir las coordenadas locales
print("Coordenadas Locales:")
print(altaz_coord)
