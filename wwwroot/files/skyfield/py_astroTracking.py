import sqlite3
import os
from skyfield.api import Star, load,  wgs84
import sys
import time
import json
import platform
from py_util import getConfig, calcular_ciclo_de_trabajo_rango
import subprocess
import RPi.GPIO as GPIO
from datetime import datetime

GPIO.setmode(GPIO.BOARD)
GPIO.setup(7, GPIO.OUT)
GPIO.setup(11, GPIO.OUT)
GPIO.setup(21, GPIO.OUT)  # laser

pV = GPIO.PWM(7, 50)
pH = GPIO.PWM(11, 50)
GPIO.output(21, GPIO.LOW)

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

oConfig = getConfig()
parametroH_rango_min = oConfig.horizontal_grados_min
parametroH_rango_max = oConfig.horizontal_grados_max
parametroV_rango_min = oConfig.vertical_grados_min
parametroV_rango_max = oConfig.vertical_grados_max
valorH = calcular_ciclo_de_trabajo_rango(0,parametroH_rango_min,parametroH_rango_max)
valorV = calcular_ciclo_de_trabajo_rango(0,parametroV_rango_min,parametroV_rango_max)
pH.start(valorH)
pV.start(valorV)

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
    cursor.execute('SELECT * FROM AntTrackings WHERE status = 1 OR tracking = 1')
    registros = cursor.fetchall()
    oConfig = getConfig()
    city = earth + wgs84.latlon(oConfig.latitude , oConfig.longitude )
    #print("Registros actuales en la tabla:")
    for registro in registros:
        type = registro[2]
        parametroH = 0
        parametroV = 0
        if type == "star":
            ra = registro[4]
            dec = registro[5]        
            t = ts.now()
            barnard2 = Star(ra_hours=decimal_a_tiempo(ra), dec_degrees=decimal_a_grado(dec))
            astrometric_star = city.at(t).observe(barnard2)  
            local = astrometric_star.apparent()
            altitud, azimut, d = local.altaz()
            # Actualizar 
            publicID = registro[1]
            cursor.execute('UPDATE AntTrackings SET altitude=?,azimuth=?,status=?,dateProcess=? WHERE publicID=?', (altitud.degrees,azimut.degrees,2,datetime.now(), publicID))
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
            print("star")           
        elif type == "servoAngle":
            parametroH = registro[8]
            parametroV = registro[9]
            # Actualizar 
            publicID = registro[1]
            cursor.execute('UPDATE AntTrackings SET status=?,dateProcess=? WHERE publicID=?', (2,datetime.now(), publicID))
            #                 
            print("servoAngle")
        else:
            print("No es contemplado.")



        parametroLaser = 0
        parametroH_rango_min = oConfig.horizontal_grados_min
        parametroH_rango_max = oConfig.horizontal_grados_max
        parametroV_rango_min = oConfig.vertical_grados_min
        parametroV_rango_max = oConfig.vertical_grados_max
        sleep_secs = 3#float(sys.argv[8])       
        #subprocess.call(['python', 'py_astro_servos.py', parametroH, parametroV,parametroLaser,parametroH_rango_min,parametroH_rango_max,parametroV_rango_min,parametroV_rango_max,sleep_secs])


        # Convertir el valor a grados y mover los servos
        try:
            valorH = calcular_ciclo_de_trabajo_rango(parametroH,parametroH_rango_min,parametroH_rango_max)
            valorV = calcular_ciclo_de_trabajo_rango(parametroV,parametroV_rango_min,parametroV_rango_max)
            pH.ChangeDutyCycle(valorH)
            pV.ChangeDutyCycle(valorV)
            # timer
            time.sleep(sleep_secs)
            pV.stop()
            pH.stop()
        except ValueError:
            print("except ValueError")

        #
        cursor.execute('UPDATE Configs SET valueDouble=? WHERE name=?', (parametroH,'servoH'))
        cursor.execute('UPDATE Configs SET valueDouble=? WHERE name=?', (parametroV, 'servoV'))
        #
        print(registro)

    conexion.commit()
    time.sleep(0.5)
    
# Cerrar el cursor y la conexion
cursor.close()
conexion.close()
GPIO.cleanup()