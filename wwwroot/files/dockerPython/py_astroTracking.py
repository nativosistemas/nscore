import sqlite3
import os
from skyfield.api import Star, load,  wgs84
import sys
import time
import json
import platform
from py_util import getConfig, calcular_ciclo_de_trabajo_rango,decimal_a_tiempo,decimal_a_grado,getConexion,getServoAngle
import subprocess
from datetime import datetime

conexion = getConexion()
cursor = conexion.cursor()
planets = load('de421.bsp')
earth = planets['earth']

ts = load.timescale()
status_create = 'create'
while True:
    print("hola py")
    cursor.execute('SELECT * FROM AntTrackings WHERE tracking = 1 OR status=\'create\'')
    registros = cursor.fetchall()
    oConfig = getConfig()
    city = earth + wgs84.latlon(oConfig.latitude , oConfig.longitude )
    for registro in registros:
        print(registro)
        type = registro[1]
        device_name = registro[18]
        #print("device_name:" + device_name)
        parametroH = 0
        parametroV = 0
        publicID = registro[0]
        if type == "star":
            ra = registro[3]
            dec = registro[4]        
            t = ts.now()
            barnard2 = Star(ra_hours=decimal_a_tiempo(ra), dec_degrees=decimal_a_grado(dec))
            astrometric_star = city.at(t).observe(barnard2)  
            local = astrometric_star.apparent()
            altitud, azimut, d = local.altaz()
            
            float_altitud = float(altitud.degrees)
            float_azimut = float(azimut.degrees)

            horizontal =float_azimut
            vertical = float_altitud
            #if device_name == "esp32_servos_laser":
            if float_azimut < 180.0:
                horizontal = 180.0 - float_azimut
                vertical = 180.0 - float_altitud
            else:
                horizontal = 360.0 - float_azimut                    
            parametroH = horizontal
            parametroV = vertical
            cursor.execute('UPDATE AntTrackings SET altitude=?,azimuth=?,h=?,v=?,status=? WHERE publicID=?', (float_altitud,float_azimut,parametroH,parametroV,'calculationResolution',publicID))

            print("parametroH: " + str(parametroH))
            print("parametroV: " + str(parametroV))  
            print("star")           
        elif type == "servoAngle":
            parametroH = registro[7]
            parametroV = registro[8]
            # Actualizar 
            cursor.execute('UPDATE AntTrackings SET status=? WHERE publicID=?', ('calculationResolution', publicID))
            #                 
            print("servoAngle")
        else:
            print("No es contemplado.")



        # parametroH_rango_min = oConfig.horizontal_grados_min
        # parametroH_rango_max =  oConfig.horizontal_grados_max
        # parametroV_rango_min = oConfig.vertical_grados_min
        # parametroV_rango_max = oConfig.vertical_grados_max
        # angle_servo_origen = getServoAngle()
        # servoH_angle = angle_servo_origen[0]
        # servoV_angle = angle_servo_origen[1]
        # cambioRangoH = abs(parametroH - servoH_angle)
        # cambioRangoV = abs(parametroV - servoV_angle)
        # valorH = calcular_ciclo_de_trabajo_rango(parametroH,parametroH_rango_min,parametroH_rango_max)
        # valorV = calcular_ciclo_de_trabajo_rango(parametroV,parametroV_rango_min,parametroV_rango_max)
        # sleep_secs = 3
        # if cambioRangoH > cambioRangoV:
        #     sleep_secs = round((sleep_secs * cambioRangoH) / 180.0, 1)
        # else:
        #     sleep_secs = round((sleep_secs * cambioRangoV) / 180.0, 1)
        # if sleep_secs < 0.5:
        #     sleep_secs = 0.5
        # cursor.execute('UPDATE Configs SET valueDouble=? WHERE name=?', (parametroH,'servoH'))
        # cursor.execute('UPDATE Configs SET valueDouble=? WHERE name=?', (parametroV, 'servoV'))

        conexion.commit()

    time.sleep(0.5)
    
# Cerrar el cursor y la conexion
cursor.close()
conexion.close()