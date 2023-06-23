import sys
import RPi.GPIO as GPIO

GPIO.setmode(GPIO.BOARD)

GPIO.setup(21, GPIO.OUT)  # laser

# Verificar si se proporcionaron suficientes argumentos
if len(sys.argv) < 2:
    print("Se esperaba al menos un argumento.")
    sys.exit(1)

# Obtener el primer argumento (índice 0 es el nombre del script)
leer = int(sys.argv[1])
parametroLaser = int(sys.argv[2])

if bool(leer):
     parametroLaser = GPIO.input(21)
else:
    if bool(parametroLaser):
        GPIO.output(21, GPIO.HIGH)  # led on
    else:
        GPIO.output(21, GPIO.LOW)  # led off

#GPIO.cleanup()

# Utilizar el parámetro recibido
print("laser: " + str(parametroLaser))
