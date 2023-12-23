import sqlite3
import os
from skyfield.api import Star, load,  wgs84
import sys
import time
import json
import platform
from py_util import getConfig
import subprocess

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

# Obtener el nombre del sistema operativo
sistema_operativo = platform.system()

# Verificar si es Windows
if sistema_operativo == 'Windows':
    ruta_base_datos = r'C:\dockerns\astro.db'
# Verificar si es Linux
elif sistema_operativo == 'Linux':
    ruta_base_datos = r'/usr/src/nscore/astro.db'
else:
    ruta_base_datos = r'/usr/src/nscore/astro.db'

# Conectar a la base de datos (creara la base de datos si no existe)
conexion = sqlite3.connect(ruta_base_datos)

# Crear un cursor para ejecutar comandos SQL
cursor = conexion.cursor()

planets = load('de421.bsp')
earth = planets['earth']

ts = load.timescale()

while True:
    # Consultar todos los registros de la tabla usuarios
    cursor.execute('SELECT * FROM AstroTrackings WHERE estado = 1')
    registros = cursor.fetchall()
    oConfig = getConfig()
    city = earth + wgs84.latlon(oConfig.latitude , oConfig.longitude )
    #print("Registros actuales en la tabla:")
    for registro in registros:
        ra = registro[3]
        dec = registro[4]
        
        t = ts.now()

        barnard2 = Star(ra_hours=decimal_a_tiempo(ra), dec_degrees=decimal_a_grado(dec))

        astrometric_star = city.at(t).observe(barnard2)  

        local = astrometric_star.apparent()

        altitud, azimut, d = local.altaz()
        # Actualizar 
        publicID = registro[1]

        cursor.execute('UPDATE AstroTrackings SET Altitude=?,Azimuth=?,estado=? WHERE publicID=?', (altitud.degrees,azimut.degrees,2, publicID))
        #
        float_azimut = float(azimut.degrees)
        float_altitud = float(altitud.degrees)
        horizontal =float_azimut
        vertical = float_altitud

        if float_azimut < 180.0:
            horizontal = 180.0 - float_azimut
            vertical = 180.0 - float_altitud
        else:
            horizontal = 360.0 - float_azimut    

        parametroH = horizontal#float(sys.argv[1])
        parametroV = vertical#float(sys.argv[2])
        parametroLaser = 0
        parametroH_rango_min = oConfig.horizontal_grados_min
        parametroH_rango_max = oConfig.horizontal_grados_max
        parametroV_rango_min = oConfig.vertical_grados_min
        parametroV_rango_max = oConfig.vertical_grados_max
        sleep_secs = 3#float(sys.argv[8])       
        subprocess.call(['python', 'py_astro_servos.py', parametroH, parametroV,parametroLaser,parametroH_rango_min,parametroH_rango_max,parametroV_rango_min,parametroV_rango_max,sleep_secs])
        #
        print(registro)

    conexion.commit()
    time.sleep(0.5)
    
# Cerrar el cursor y la conexion
cursor.close()
conexion.close()
