import sqlite3
import platform

class Config:
    def __init__(self, latitude, longitude, altitude, horizontal_grados_min, horizontal_grados_max, vertical_grados_min, vertical_grados_max):
        self.latitude = latitude
        self.longitude = longitude
        self.altitude = altitude
        self.horizontal_grados_min = horizontal_grados_min
        self.horizontal_grados_max = horizontal_grados_max
        self.vertical_grados_min = vertical_grados_min
        self.vertical_grados_max = vertical_grados_max

sistema_operativo = platform.system()

if sistema_operativo == 'Windows':
    ruta_base_datos = r'C:\dockerns\astro.db'
elif sistema_operativo == 'Linux':
    ruta_base_datos = r'/usr/src/nscore/astro.db'
else:
    ruta_base_datos = r'/usr/src/nscore/astro.db'

conexion = sqlite3.connect(ruta_base_datos)

def getConfig():
    latitude = 0.0
    longitude =  0.0
    altitude =  0.0
    horizontal_grados_min =  0.0
    horizontal_grados_max =  0.0
    vertical_grados_min =  0.0
    vertical_grados_max =  0.0
    cursor = conexion.cursor()    
    cursor.execute('SELECT * FROM Configs')
    registros = cursor.fetchall()
    for registro in registros:
        if registro[0] == 'latitude':
            latitude = registro[3]
        if registro[0] == 'longitude':
            longitude = registro[3]
        if registro[0] == 'altitude':
            altitude = registro[3]
        if registro[0] == 'horizontal_grados_min':
            horizontal_grados_min = registro[3]       
        if registro[0] == 'horizontal_grados_max':
            horizontal_grados_max = registro[3]                     
        if registro[0] == 'vertical_grados_min':
            vertical_grados_min = registro[3]       
        if registro[0] == 'vertical_grados_max':
            vertical_grados_max = registro[3]       
            
    return Config(latitude,longitude,altitude,horizontal_grados_min,horizontal_grados_max,vertical_grados_min,vertical_grados_max)        

def calcular_ciclo_de_trabajo_rango(angulo,ciclo_minimo,ciclo_maximo):
    #ciclo_minimo = 2.5
    #ciclo_maximo = 12.0
    rango = ciclo_maximo - ciclo_minimo
    ciclo = ciclo_minimo + ((rango / 180.0) * angulo)
    return ciclo