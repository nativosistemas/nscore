import sqlite3
import os
from skyfield.api import Star, load,  wgs84
import sys
import time
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

# Especifica la ruta completa de la base de datos
ruta_base_datos = r'C:\dockerns\astro.db'

# Conectar a la base de datos (creará la base de datos si no existe)
conexion = sqlite3.connect(ruta_base_datos)

# Crear un cursor para ejecutar comandos SQL
cursor = conexion.cursor()

planets = load('de421.bsp')
earth = planets['earth']
rosario = earth + wgs84.latlon(-32.94681944444444 ,  -60.6393194444444 )
ts = load.timescale()

while True:
    # Consultar todos los registros de la tabla usuarios
    cursor.execute('SELECT * FROM AstroTrackings WHERE estado = 1')
    registros = cursor.fetchall()

    print("Registros actuales en la tabla:")
    for registro in registros:
        #print( registro[3]) # ra
        #print( registro[4]) # dec
        ra = registro[3]
        dec = registro[4]
        

        t = ts.now()

        barnard2 = Star(ra_hours=decimal_a_tiempo(ra), dec_degrees=decimal_a_grado(dec))

        astrometric_star = rosario.at(t).observe(barnard2)

        local = astrometric_star.apparent()

        altitud, azimut, d = local.altaz()
        # Actualizar 
        publicID = registro[1]
        #print(publicID)
        #Altitude = 0
        #Azimuth = 1
        cursor.execute('UPDATE AstroTrackings SET Altitude=?,Azimuth=?,estado=? WHERE publicID=?', (altitud.degrees,azimut.degrees,2, publicID))
        # fin Actualizar
        print(registro)


    # Guardar (commit) los cambios
    conexion.commit()
    # Pausa de 1 segundo entre las consultas
    time.sleep(0.5)
    
# Cerrar el cursor y la conexión
cursor.close()
conexion.close()
