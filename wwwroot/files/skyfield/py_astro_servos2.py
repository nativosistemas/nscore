import sys
import RPi.GPIO as GPIO
import time
from py_util import getConfig,setServoAngle,getServoAngle

GPIO.setmode(GPIO.BOARD)
GPIO.setup(7, GPIO.OUT)
GPIO.setup(11, GPIO.OUT)
GPIO.setup(21, GPIO.OUT)  # laser

pV = GPIO.PWM(7, 50)
pH = GPIO.PWM(11, 50)
GPIO.output(21, GPIO.LOW)


# Función para calcular el ciclo de trabajo correspondiente a un ángulo dado
def calcular_ciclo_de_trabajo_rango(angulo,ciclo_minimo,ciclo_maximo):
    #ciclo_minimo = 2.5
    #ciclo_maximo = 12.0
    rango = ciclo_maximo - ciclo_minimo
    ciclo = ciclo_minimo + ((rango / 180.0) * angulo)
    return ciclo

# Verificar si se proporcionaron suficientes argumentos
if len(sys.argv) < 2:
    print("Se esperaba al menos un argumento.")
    sys.exit(1)

# Obtener el primer argumento (índice 0 es el nombre del script)
parametroH = float(sys.argv[1])
parametroV = float(sys.argv[2])
parametroLaser = int(sys.argv[3])

oConfig = getConfig()
parametroH_rango_min = oConfig.horizontal_grados_min
parametroH_rango_max =  oConfig.horizontal_grados_max
parametroV_rango_min = oConfig.vertical_grados_min
parametroV_rango_max = oConfig.vertical_grados_max
#sleep_secs = float(sys.argv[8])


angle_servo_origen = getServoAngle()
servoH_angle = angle_servo_origen[0]
servoV_angle = angle_servo_origen[1]

cambioRangoH = abs(parametroH - servoH_angle)
cambioRangoV = abs(parametroV - servoV_angle)

#time.sleep(1)


valorH = calcular_ciclo_de_trabajo_rango(parametroH,parametroH_rango_min,parametroH_rango_max)
pH.start(valorH)#pH.ChangeDutyCycle(valorH)

valorV = calcular_ciclo_de_trabajo_rango(parametroV,parametroV_rango_min,parametroV_rango_max)
pV.start(valorV)#pV.ChangeDutyCycle(valorV)

# timer
# ¿aca va un timer?
time.sleep(sleep_secs)


pV.stop()
pH.stop()


if bool(parametroLaser):
    GPIO.output(21, GPIO.HIGH)  # led on
else:
    GPIO.output(21, GPIO.LOW)  # led off

#time.sleep(10)
#GPIO.output(21, GPIO.HIGH)
#GPIO.cleanup()


# Utilizar el parámetro recibido
print("H: " + str(parametroH) + " ("+str(valorH)+")"+ " - V: " +
      str(parametroV) + " ("+str(valorV)+")"+ " - laser: " + str(parametroLaser) + " - sleep_secs: " + str(sleep_secs))
