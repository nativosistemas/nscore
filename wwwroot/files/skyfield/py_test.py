from skyfield.api import load, Topos

# Cargar los datos de los cuerpos celestes
ts = load.timescale()
planets = load('de421.bsp')  # Datos de efemérides planetarias
stars = load('hip_main.bsp')  # Datos de estrellas de Hipparcos

# Configurar la ubicación (puedes ajustar las coordenadas según tu ubicación)
ubicacion = Topos(latitude_degrees=0, longitude_degrees=0)

# Obtener la posición de una estrella específica (en este caso, Alfa Centauri)
estrella = stars['HIP 71683']
astrometric = (planets + stars).at(ts.now()).observe(estrella).apparent()
alt, az, d = astrometric.altaz()

# Obtener la ascensión recta y declinación
ra, dec, distancia = astrometric.radec()

print(f"Ascensión Recta: {ra.hms()}")
print(f"Declinación: {dec.dms()}")
