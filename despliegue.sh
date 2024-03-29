#!/bin/bash

sudo systemctl stop nscore.service

# Detener y eliminar el contenedor existente (si existe)
sudo docker stop nscore && sudo docker rm nscore

# Eliminar la imagen existente (si existe)
sudo docker rmi nscore

# Construir la nueva imagen de Docker
sudo docker-compose build

# Desplegar la nueva imagen de Docker
sudo docker-compose up -d

sudo systemctl start nscore.service