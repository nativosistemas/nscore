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

# Crear un cursor para ejecutar comandos SQL
cursor = conexion.cursor()

planets = load('de421.bsp')
earth = planets['earth']

ts = load.timescale()

while True:
    cursor.execute('SELECT * FROM AntTrackings WHERE status = 1 OR tracking = 1')
    registros = cursor.fetchall()
    oConfig = getConfig()
    city = earth + wgs84.latlon(oConfig.latitude , oConfig.longitude )
    for registro in registros:
        print(registro)
        type = registro[2]
        parametroH = 0
        parametroV = 0
        publicID = registro[1]
        if type == "star":
            ra = registro[4]
            dec = registro[5]        
            t = ts.now()
            barnard2 = Star(ra_hours=decimal_a_tiempo(ra), dec_degrees=decimal_a_grado(dec))
            astrometric_star = city.at(t).observe(barnard2)  
            local = astrometric_star.apparent()
            altitud, azimut, d = local.altaz()
            
            float_altitud = float(altitud.degrees)
            float_azimut = float(azimut.degrees)
            cursor.execute('UPDATE AntTrackings SET altitude=?,azimuth=?,status=?,dateProcess=? WHERE publicID=?', (float_altitud,float_azimut,2,datetime.now(), publicID))
            #        
            horizontal =float_azimut
            vertical = float_altitud

            if float_azimut < 180.0:
                horizontal = 180.0 - float_azimut
                vertical = 180.0 - float_altitud
            else:
                horizontal = 360.0 - float_azimut                    
            parametroH = horizontal#float(sys.argv[1])
            parametroV = vertical#float(sys.argv[2]) 




            print("parametroH: " + str(parametroH))
            print("parametroV: " + str(parametroV))  
            print("star")           
        elif type == "servoAngle":
            parametroH = registro[8]
            parametroV = registro[9]
            # Actualizar 
            cursor.execute('UPDATE AntTrackings SET status=?,dateProcess=? WHERE publicID=?', (2,datetime.now(), publicID))
            #                 
            print("servoAngle")
        else:
            print("No es contemplado.")



        parametroH_rango_min = oConfig.horizontal_grados_min
        parametroH_rango_max =  oConfig.horizontal_grados_max
        parametroV_rango_min = oConfig.vertical_grados_min
        parametroV_rango_max = oConfig.vertical_grados_max


        angle_servo_origen = getServoAngle()
        servoH_angle = angle_servo_origen[0]
        servoV_angle = angle_servo_origen[1]

        cambioRangoH = abs(parametroH - servoH_angle)
        cambioRangoV = abs(parametroV - servoV_angle)

        valorH = calcular_ciclo_de_trabajo_rango(parametroH,parametroH_rango_min,parametroH_rango_max)
        #pH.start(valorH)#pH.ChangeDutyCycle(valorH)

        valorV = calcular_ciclo_de_trabajo_rango(parametroV,parametroV_rango_min,parametroV_rango_max)
        #pV.start(valorV)#pV.ChangeDutyCycle(valorV)
        sleep_secs = 3
        if cambioRangoH > cambioRangoV:
            sleep_secs = round((sleep_secs * cambioRangoH) / 180.0, 1)
        else:
            sleep_secs = round((sleep_secs * cambioRangoV) / 180.0, 1)

        if sleep_secs < 0.5:
            sleep_secs = 0.5

        cursor.execute('UPDATE Configs SET valueDouble=? WHERE name=?', (parametroH,'servoH'))
        cursor.execute('UPDATE Configs SET valueDouble=? WHERE name=?', (parametroV, 'servoV'))

        # Llamar program mover Servo
        ruta_ejecutable = 'py_astroTracking_servo'
        argumentos = [str(valorH), str(valorV),str(sleep_secs),str(0)] # 0 => laser apagado
        subprocess.run([ruta_ejecutable] + argumentos)
        #

        # Cambiar estado de que se llamo a mover servo estado 3?    
        cursor.execute('UPDATE AntTrackings SET status=?,dateProcess=? WHERE publicID=?', (3,datetime.now(), publicID))
        conexion.commit()
        time.sleep(0.5) # hacer espera?
        #

    #conexion.commit()
    time.sleep(0.5)
    
# Cerrar el cursor y la conexion
cursor.close()
conexion.close()