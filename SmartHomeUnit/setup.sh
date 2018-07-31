#!/bin/bash

echo 18 > /sys/class/gpio/export
echo 17 > /sys/class/gpio/export
echo in > /sys/class/gpio/gpio18/direction
echo out > /sys/class/gpio/gpio17/direction

rm -rf ./devices
mkdir ./devices

ln -rs /sys/class/gpio/gpio18 ./devices/watersensor
ln -rs /sys/class/gpio/gpio17 ./devices/wateron

ln -rs /sys/bus/w1/devices/28-* ./devices/ds18b20

