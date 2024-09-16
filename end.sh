#!/bin/bash

sudo systemctl stop nscore-python.service
sudo docker stop nscore-python 

sudo systemctl stop nscore.service
sudo docker stop nscore 
