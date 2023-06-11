import socket
import sys
import time
import importlib
#import py_servo

# Inicializamos la aplicacion
# Create a UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Bind the socket to the port
server_address = ('localhost', 10000)
print('starting up on {} port {}'.format(*server_address))
sock.bind(server_address)

# try:
while True:
    print('\nwaiting to receive message')
    data, address = sock.recvfrom(4096)

    print('received {} bytes from {}'.format(
        len(data), address))
    print(data)
    #
    x = str(data.decode('utf8')).split("_")
    gradoH = x[0]
    print('--> x[0] gradoH ' + x[0])  # gradoH
    gradoV = x[1]
    print('--> x[1] gradoV ' + x[1])  # gradoV
    Laser = bool(x[2])
    print('--> x[2] Laser ' + x[2])  # Laser
    #
    py_servo = importlib.import_module("py_servo")
    result = py_servo.moveServo(gradoH, gradoV, Laser)

    if data:
        sent = sock.sendto(data, address)
        print('sent {} bytes back to {}'.format(
            sent, address))

# except KeyboardInterrupt:
