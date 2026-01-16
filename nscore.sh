#!/bin/bash

# Script para reiniciar servicios Docker Compose

echo "=== Deteniendo y eliminando contenedores ==="
docker-compose down

echo -e "\n=== Levantando contenedores en segundo plano ==="
docker-compose up -d --pull always

echo -e "\n=== Estado de los contenedores ==="
docker-compose ps