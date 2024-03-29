#!/bin/bash

sudo systemctl stop nscore-python.service

# Detener y eliminar el contenedor existente (si existe)
sudo docker stop nscore-python && sudo docker rm nscore-python

# Eliminar la imagen existente (si existe)
sudo docker rmi nscore-python


sudo docker build -t nscore-python .
sudo docker run  -v /usr/src/dockerns:/usr/src  --name nscore-python --hostname nscore-python  -d nscore-python


sudo systemctl start nscore-python.service