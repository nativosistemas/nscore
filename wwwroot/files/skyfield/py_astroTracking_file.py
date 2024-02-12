import sys

# Verificar si se proporcionaron suficientes argumentos
if len(sys.argv) < 2:
    print("Se esperaba al menos un argumento.")
    sys.exit(1)

# Obtener el primer argumento (índice 0 es el nombre del script)
valorH = float(sys.argv[1])
valorV = float(sys.argv[2])
sleep_secs = float(sys.argv[3])
parametroLaser = int(sys.argv[4])


with open('archivo.txt', 'w') as archivo:
    # Escribir en el archivo
    archivo.write('Hola, mundo!\n')
    archivo.write('valorH '+str(valorH)+'\n')
    archivo.write('valorV '+str(valorV)+'\n')
    archivo.write('valorV '+str(sleep_secs)+'\n')
    archivo.write('valorV '+str(parametroLaser)+'\n')