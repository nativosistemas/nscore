import sys
import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BOARD)
GPIO.setup(7, GPIO.OUT)
GPIO.setup(11, GPIO.OUT)
#GPIO.setup(21, GPIO.OUT)  # laser

#pV = GPIO.PWM(7, 50)
pH = GPIO.PWM(11, 50)
#GPIO.output(21, GPIO.LOW)


# Función para calcular el ciclo de trabajo correspondiente a un ángulo dado
def calcular_ciclo_de_trabajo(pMin, pMax, angulo):
    ciclo_minimo = pMin
    ciclo_maximo = pMax
    rango = ciclo_maximo - ciclo_minimo
    ciclo = ciclo_minimo + (rango / 180.0) * angulo
    return ciclo

# Verificar si se proporcionaron suficientes argumentos
if len(sys.argv) < 2:
    print("Se esperaba al menos un argumento.")
    sys.exit(1)

# Obtener el primer argumento (índice 0 es el nombre del script)
min = float(sys.argv[1])
max = float(sys.argv[2])
parametroH = float(sys.argv[3])

valorH = calcular_ciclo_de_trabajo(min, max, parametroH)
pH.start(valorH)

time.sleep(1)

pH.stop()
GPIO.cleanup()


# Utilizar el parámetro recibido
print("H: " + str(parametroH) + " ("+str(valorH)+")")
