import socket
import sys
import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BOARD)
GPIO.setup(7, GPIO.OUT)
GPIO.setup(11, GPIO.OUT)
GPIO.setup(21, GPIO.OUT)  # laser
laser = 0
pV = GPIO.PWM(7, 50)
pH = GPIO.PWM(11, 50)
valorH = 3.75
valorV = 3.75
gradoH = 0
gradoV = 0
pV.start(valorV)
pH.start(valorH)
GPIO.output(21, GPIO.LOW)  # led off


def getDC_grados(pGrados):
    return round((((float(pGrados) - float(180)) * float(-5)) / float(-180)) + float(10), 1)


def moveServo(pGradoH, pGradoV):
    valorH = getDC_grados(pGradoH)
    pH.ChangeDutyCycle(valorH)

    valorV = getDC_grados(pGradoV)
    pV.ChangeDutyCycle(valorV)

    #GPIO.output(21, GPIO.LOW) # led off
    pV.stop()
    pH.stop()
    GPIO.cleanup()        

    return "Python: " + f'{gradoH + gradoV:.3f}'
