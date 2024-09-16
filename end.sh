#!/bin/bash

sudo systemctl stop nscore-python.service
sudo docker stop nscore-python && sudo docker rm nscore-python
sudo docker rmi nscore-python

sudo systemctl stop nscore.service
sudo docker stop nscore && sudo docker rm nscore
sudo docker rmi nscore