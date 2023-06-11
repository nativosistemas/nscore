import sys
import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BOARD)
GPIO.setup(7, GPIO.OUT)
GPIO.setup(11, GPIO.OUT)
GPIO.setup(21, GPIO.OUT)  # laser

pV = GPIO.PWM(7, 50)
pH = GPIO.PWM(11, 50)

pV.start(2.5)
pH.start(2.5)
time.sleep(1)
#pV.stop()
#pH.stop()

def getDC_grados(pGrados):
    return round((((float(pGrados) - float(180)) * float(-5)) / float(-180)) + float(10), 1)


def moveServo(pGradoH, pGradoV, pIsOnLaser):
    valorH = getDC_grados(pGradoH)
    pH.ChangeDutyCycle(valorH)

    valorV = getDC_grados(pGradoV)
    pV.ChangeDutyCycle(valorV)

    if pIsOnLaser:
        GPIO.output(21, GPIO.HIGH)  # led on
    else:
        GPIO.output(21, GPIO.LOW)  # led off

    # timer
    # ¿aca va un timer?
    time.sleep(3)

    pV.stop()
    pH.stop()
    GPIO.cleanup()

    return "Python (servo): " + f'{gradoH:.3f}' + "_" + f'{gradoV:.3f}' + "_" + pIsOnLaser
