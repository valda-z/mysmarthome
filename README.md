# README #

Smart home solution

## To build
- MySmartHomeCore - WEB UI + WEB API backend for units
- SmartHomeCore - raspberry Pi net core based application for communication with devices (HTTP/REST for ESP8266 modules, RS485 for JA82)
- IoT/ESP-WiFi/HeatingRelay - wireless switch for heating and heating motor based on ESP8266
- IoT/ESP-WiFi/OutdoorUnit - Outdoor unit for Dog House, Irrigation and temperature sensors based on ESP8266 (don't forget to insert WIFI Password and unit GUID to variables in INO file)
  - also you have to define ID of dog home temperature sensor, you can run sketch and check logs for 18DS20 MAC adress and than you can change logic in function getTemperatures()

## To install

### MySmartHomeCore
- build Docker image
- configure appconfig.json with desired configuration data

### install SmartHomeCore
- copy app from bin/Debug to raspberry pi `/opt/smarthome`
- register startup script `/opt/smarthome/smarthome.sh` via `update-rc.d`
- configure file appsetting.json (define unit GUID and your Azure web app URL - https preferred)


