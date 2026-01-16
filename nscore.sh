#!/bin/bash

# Script para reiniciar servicios Docker Compose

echo "=== Deteniendo y eliminando contenedores ==="
docker-compose down

echo "=== Eliminando imágenes Docker ==="
docker rmi -f $(docker images -q)

echo -e "\n=== Levantando contenedores en segundo plano ==="
docker-compose up -d --pull always

echo -e "\n=== Estado de los contenedores ==="
docker-compose ps