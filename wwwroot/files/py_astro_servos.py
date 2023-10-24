import sys
import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BOARD)
GPIO.setup(7, GPIO.OUT)
GPIO.setup(11, GPIO.OUT)
GPIO.setup(21, GPIO.OUT)  # laser

pV = GPIO.PWM(7, 50)
pH = GPIO.PWM(11, 50)
GPIO.output(21, GPIO.LOW)


# Función para calcular el ciclo de trabajo correspondiente a un ángulo dado
def calcular_ciclo_de_trabajo(angulo):
    ciclo_minimo = 2.5
    ciclo_maximo = 12.0
    rango = ciclo_maximo - ciclo_minimo
    ciclo = ciclo_minimo + ((rango / 180.0) * angulo)
    return ciclo

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
parametroH_rango_min = float(sys.argv[4])
parametroH_rango_max = float(sys.argv[5])
parametroV_rango_min = float(sys.argv[6])
parametroV_rango_max = float(sys.argv[7])
#suma = parametroH + parametroV + parametroLaser


#pV.start(2.5)
#pH.start(2.5)
#time.sleep(1)
## pV.stop()
## pH.stop()
valorH = calcular_ciclo_de_trabajo_rango(parametroH,parametroH_rango_min,parametroH_rango_max)
pH.start(valorH)#pH.ChangeDutyCycle(valorH)

valorV = calcular_ciclo_de_trabajo_rango(parametroV,parametroV_rango_min,parametroV_rango_max)
pV.start(valorV)#pV.ChangeDutyCycle(valorV)

# timer
# ¿aca va un timer?
time.sleep(3)


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
      str(parametroV) + " ("+str(valorV)+")"+ " - laser: " + str(parametroLaser))
