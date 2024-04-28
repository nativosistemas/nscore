#!/bin/bash

sudo systemctl stop nscore.service

# Detener y eliminar el contenedor existente (si existe)
sudo docker stop nscore && sudo docker rm nscore

# Eliminar la imagen existente (si existe)
sudo docker rmi nscore

# Construir la nueva imagen de Docker
#sudo docker-compose build

# Desplegar la nueva imagen de Docker
#sudo docker-compose up -d

# INICIO TEMPORAL (FALTA docker-compose)
sudo docker build -t nscore .
sudo docker run -p 80:8080 -p 443:443  -v /usr/src/dockerns:/usr/src -v /usr/src/appsettings.json:/App/appsettings.json  --name nscore --hostname nscore  -d nscore
# FIN TEMPORAL (FALTA docker-compose)

sudo systemctl start nscore.service