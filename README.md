# README #

Smart home solution

## To build
- MySmartHome - WEB UI + WEB API backend for units
- SmartHomeUnit - raspberry Pi mono based application for communication with devices
- SmartHomeJablotronUnit - raspberry Pi mono based application for communication with devices (HTTP/REST for ESP8266 modules, RS485 for JA82)

## To install

### MySmartHome
- build
- publish to Azure web app
- define in webb app these settings variables
 - `ConnectionString` - define connection string to your Azure SQL database in section connection strings
 - `REGISTER_TOKEN` - define unique string for registration of new user (new user can be registered there: https://nasdum.azurewebsites.net/Account/Register?token=[YOUR_TOKEN])
 - `UNIT1` - define GUID of unit type "1" - it means implementation "SmartHomeUnit"
 - `JABLOTRON` - define GUID of unit type "SmartHomeJablotron"
 - `JABLOTRONZONES` - change your JA82 alarm system zone names
- insert into table "Device" one record for unit type "1" - you have to generate GUID for unit (GUID is used like identification/secret of unit)

### install SmartHomeUnit
- install dependencies `apt install mono-complete`
- copy app from bin/Debug to raspberry pi `/opt/smarthome`
- register startup script `/opt/smarthome/smarthome.sh` via `update-rc.d`
- configure file appsetting.json (define unit GUID and your Azure web app URL - https preferred)

### install SmartHomeJablotronUnit
- install dependencies `apt install mono-complete`
- copy app from bin/Debug to raspberry pi `/opt/smarthome`
- register startup script `/opt/smarthome/smarthome.sh` via `update-rc.d`
- configure file appsetting.json (define unit GUID and your Azure web app URL - https preferred)


