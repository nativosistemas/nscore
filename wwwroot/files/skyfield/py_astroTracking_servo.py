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


# Verificar si se proporcionaron suficientes argumentos
if len(sys.argv) < 2:
    print("Se esperaba al menos un argumento.")
    sys.exit(1)

# Obtener el primer argumento (índice 0 es el nombre del script)
valorH = float(sys.argv[1])
valorV = float(sys.argv[2])
sleep_secs = float(sys.argv[3])
parametroLaser = int(sys.argv[4])

pH.start(valorH)#pH.ChangeDutyCycle(valorH)
time.sleep(sleep_secs)
pH.stop()

pV.start(valorV)#pV.ChangeDutyCycle(valorV)
time.sleep(sleep_secs)
pV.stop()

#pH.stop()
#pV.stop()

if bool(parametroLaser):
    GPIO.output(21, GPIO.HIGH)  # led on
else:
    GPIO.output(21, GPIO.LOW)  # led off

GPIO.cleanup()

# Utilizar el parámetro recibido
print("H: "+str(valorH) + " - V: " + str(valorV)+ " - laser: " + str(parametroLaser) + " - sleep_secs: " + str(sleep_secs))
