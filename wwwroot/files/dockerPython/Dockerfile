# Usa la imagen oficial de Python como base
FROM python:latest

# Establece el directorio de trabajo en /app
WORKDIR /app

# Copia el archivo Python al directorio de trabajo en el contenedor
COPY py_astroTracking.py /app/
COPY py_util.py /app/

# Instala la biblioteca requests
RUN pip install skyfield

# Ejecuta el script Python cuando se inicie el contenedor
CMD ["python", "py_astroTracking.py"]