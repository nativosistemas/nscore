import sys
import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BOARD)
GPIO.setup(7, GPIO.OUT)
GPIO.setup(11, GPIO.OUT)
GPIO.setup(21, GPIO.OUT)  # laser

pV = GPIO.PWM(7, 50)
pH = GPIO.PWM(11, 50)


def getDC_grados(pGrados):
    return round((((float(pGrados) - float(180)) * float(-5)) / float(-180)) + float(10), 1)


# Verificar si se proporcionaron suficientes argumentos
if len(sys.argv) < 2:
    print("Se esperaba al menos un argumento.")
    sys.exit(1)

# Obtener el primer argumento (índice 0 es el nombre del script)
parametroH = float(sys.argv[1])
parametroV = float(sys.argv[2])
parametroLaser = int(sys.argv[3])
suma = parametroH + parametroV + parametroLaser


#pV.start(2.5)
#pH.start(2.5)
#time.sleep(1)
## pV.stop()
## pH.stop()
valorH = getDC_grados(parametroH)
pH.start(parametroH)#pH.ChangeDutyCycle(valorH)

valorV = getDC_grados(parametroV)
pV.start(valorV)#pV.ChangeDutyCycle(valorV)

# timer
# ¿aca va un timer?
time.sleep(2)

if bool(parametroLaser):
    GPIO.output(21, GPIO.HIGH)  # led on
else:
    GPIO.output(21, GPIO.LOW)  # led off



pV.stop()
pH.stop()
#GPIO.cleanup()


# Utilizar el parámetro recibido
print("Los parámetros recibido es: parametroH: " + str(parametroH)+" - parametroV: " +
      str(parametroV) + " - parametroLaser: " + str(parametroLaser) + " - suma: " + str(suma))
