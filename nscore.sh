#!/bin/bash

# Script para reiniciar servicios Docker Compose

echo "=== Deteniendo y eliminando contenedores ==="
docker-compose down

echo "=== Eliminando imágenes Docker ==="
#docker rmi -f $(docker images -q)
docker rmi ghcr.io/nativosistemas/seweb:latest
docker rmi ghcr.io/nativosistemas/nscore:latest
docker rmi ghcr.io/nativosistemas/nssky:latest

#echo "=== Eliminando volumen Docker ==="
#docker volume rm shared-data

echo -e "\n=== Levantando contenedores en segundo plano ==="
docker-compose up -d --pull always

echo -e "\n=== Estado de los contenedores ==="
docker-compose ps