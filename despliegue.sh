#!/bin/bash

systemctl stop nscore.service

# Detener y eliminar el contenedor existente (si existe)
docker stop nscore && docker rm nscore

# Eliminar la imagen existente (si existe)
sudo docker rmi nscore

# Construir la nueva imagen de Docker
docker-compose build

# Desplegar la nueva imagen de Docker
docker-compose up -d

systemctl start nscore.service