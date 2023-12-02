from skyfield.api import N, W, Star, load, Topos, wgs84
from skyfield.data import hipparcos
from skyfield.trigonometry import position_angle_of
import json

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
sun, moon = planets['sun'], planets['moon']

ts = load.timescale()
t = ts.now()

ra = 101.55350
dec = -16.7470

#barnard2 = Star(ra_hours=(6, 45, 09.11), dec_degrees=(-16, 43, 22.5))
barnard2 = Star(ra_hours=decimal_a_tiempo(ra), dec_degrees=decimal_a_grado(dec))

rosario = earth + wgs84.latlon(-32.94681944444444 ,  -60.6393194444444 )


astrometric_star = rosario.at(t).observe(barnard2)

local = astrometric_star.apparent()



altitud, azimut, d = local.altaz()

# Imprimir resultados
#print(f"Altitud: {altitud.radians()} ")
print('Altitud {:.8f} degrees'.format(altitud.degrees))
print('Azimut {:.8f} hours'.format(azimut.degrees))
#print(f"Azimut: {azimut} ")
#print(f"Altitud: {altitud.degrees:.2f} grados")
#print(f"Azimut: {azimut.degrees:.2f} grados")
print(f"distancia: {d} ")

#vaa = '{"Altitude": {:.8f},"Azimuth": {:.8f}}'.format(altitud.degrees,azimut.degrees) 
print(obtener_objeto_json(altitud.degrees,azimut.degrees) )

