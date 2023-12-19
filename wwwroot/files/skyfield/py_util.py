class Config:
    def __init__(self, latitude, longitude, altitude, horizontal_grados_min, horizontal_grados_max, vertical_grados_min, vertical_grados_max):
        self.latitude = latitude
        self.longitude = longitude
        self.altitude = altitude
        self.horizontal_grados_min = horizontal_grados_min
        self.horizontal_grados_max = horizontal_grados_max
        self.vertical_grados_min = vertical_grados_min
        self.vertical_grados_max = vertical_grados_max

#    def saludar(self):
#        print(f'Hola, soy {self.nombre} y tengo {self.edad} anios.')