from py_util import getConfig,setServoAngle,getServoAngle
import numpy as np
o = getConfig()
print(f"latitude: {o.latitude}")
print(f"longitude: {o.longitude}")

inicio = 5.1
fin = 9.3
paso = 0.1

#rango_numerico = range(inicio, fin)
rango_numerico = np.arange(inicio, fin, paso)
#lista_numeros = list(rango_numerico)

print(f"Rango de números entre {inicio} y {fin}: {rango_numerico}")

print(f"Los angulos son servoH y servoV: {getServoAngle()}")

