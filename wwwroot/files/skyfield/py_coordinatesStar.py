from skyfield.api import Star, load,  wgs84
import sys
import time
import json

# Verificar si se proporcionaron suficientes argumentos
if len(sys.argv) < 1:
    print("Se esperaba al menos un argumento.")
    sys.exit(1)

# Obtener el primer argumento (índice 0 es el nombre del script)
ra = float(sys.argv[1])
dec = float(sys.argv[2])

def decimal_a_tiempo(valor):
    decimal = (valor * 24.0) / 360.0
    horas = int(decimal)
    minutos = int((decimal - horas) * 60)
    segundos = (((decimal - horas) * 60 - minutos) * 60.0)

    return horas, minutos, segundos

def decimal_a_grado(decimal):
    grados = int(decimal)
    minutos_float = (decimal - grados) * 60
    minutos = int(minutos_float)
    segundos = (minutos_float - minutos) * 60.0
    return grados, minutos, segundos

def obtener_objeto_json(pAltitud, pAzimut):
    # Crear un diccionario como ejemplo
    datos = {
        'Altitud': pAltitud,
        'Azimut': pAzimut
    }

    # Devolver el diccionario convertido a una cadena JSON
    return json.dumps(datos)

planets = load('de421.bsp')
earth = planets['earth']

ts = load.timescale()
t = ts.now()

barnard2 = Star(ra_hours=decimal_a_tiempo(ra), dec_degrees=decimal_a_grado(dec))

rosario = earth + wgs84.latlon(-32.94681944444444 ,  -60.6393194444444 )


astrometric_star = rosario.at(t).observe(barnard2)

local = astrometric_star.apparent()

altitud, azimut, d = local.altaz()

print(obtener_objeto_json(altitud.degrees,azimut.degrees) )
