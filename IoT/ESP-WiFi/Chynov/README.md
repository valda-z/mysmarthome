# ESP32 / Arduino Mega 2650 based smart home system

Main function:
* alarm system with 8 independent inputs (alarm zones) - simple IO inputs with optical isolation
* temperature sensors for indoor / outdoor
* display unit for temperature / system status
* heating switch on/off based on indoor / outdoor temperature
* remote command module via WiFi for Drazice LX35 photovoltaic clima-unit

## Modules

* MASTER  = Master unit - ESP32
    * Connection to cloud unit
    * 8 IO inputs for alarm
    * 1 IO output for alarm (acoustic siren) - relay
    * 4 IO output for heating - relay
    * alarm logic
    * heating logic
    * LED indicators for maintenance
* SLAVE DISPLAY (A) = Slave unit - ESP32 - display / temperature shield
    * docs:
        * https://github.com/Xinyuan-LilyGO/T5-Ink-Screen-Series
        * https://github.com/goldfish1974/T5-Ink-Screen-Series
        * https://navody.arduino-shop.cz/navody-k-produktum/e-ink-2-13-displej-il0373f.html
    * indoor temperature sensor
    * outdoor temperature sensor
    * eInk display - status info
        * date / time
        * indoor temperature
        * outdoor temperature
        * alarm status
        * heating status
        * LX35 status
* SLAVE RFID (B) = Slave unit - Mega 2560 - 125kHz RFID reader, alarm status indicator
    * RFID reader
    * LED 2 color output (alarm status)
    * beeper

## Communication protocol

Based on RS485 on 9600 baud rate (9600, SERIAL_8N1) with one master unit and slave units.

Master unit is sending query packet each 200 ms to slave units and waits for max 200ms for answer.

Each packets from master starts with character `>` and ends with linux new line `\n`. Each packet from slave unit starts with `<` and ends with linux new line `\n`.

⏎ is used for `\n` symbol in protocol definition.

```
# MASTER - query SLAVE DISPLAY
#-----------------------------------------------------
>AxxxxxxXYZZZ##⏎
│││     │││  │ │
│││     │││  │ └─── NewLine
│││     │││  └───── Hex encoded CRC-8
│││     ││└──────── 3 bytes Lx 35 status
│││     │└───────── Heating status:
│││     │               0 = Off
│││     │               1 = On
│││     └────────── Alarm status:
│││                     0 = Idle
│││                     O = Outgoing delay 
│││                     I = Incoming delay
│││                     1 = Alarm in zone 1
│││                     2 = Alarm in zone 2
│││                     3 = Alarm in zone 3
│││                     4 = Alarm in zone 4
│││                     5 = Alarm in zone 5
│││                     6 = Alarm in zone 6
│││                     7 = Alarm in zone 7
│││                     8 = Alarm in zone 8
││└──────────────── 6 chars - base64 encoded unix time (4 bytes)
│└───────────────── A = request SLAVE DISPLAY
└────────────────── Start character


# SLAVE DISPLAY - answer to MASTER
#-----------------------------------------------------
................
<A+000+000...##⏎
│││   │   │  │ │
│││   │   │  │ └─── NewLine
│││   │   │  └───── Hex encoded CRC-8
│││   │   └──────── 3 chars empty = dots
│││   └──────────── 4 chars - outdoor temperature in Celsius in 1/10 units. First char is sign + or -
││└──────────────── 4 chars - indoor temperature in Celsius in 1/10 units. First char is sign + or -
│└───────────────── A = answer from SLAVE DISPLAY
└────────────────── Start character

# MASTER - query SLAVE RFID
#-----------------------------------------------------
>BX..........##⏎
│││          │ │
│││          │ └─── NewLine
│││          └───── Hex encoded CRC-8
││└──────────────── Alarm status:
││                      0 = Idle
││                      O = Outgoing delay 
││                      I = Incoming delay
││                      1 = Alarm in zone 1
││                      2 = Alarm in zone 2
││                      3 = Alarm in zone 3
││                      4 = Alarm in zone 4
││                      5 = Alarm in zone 5
│└───────────────── B = request SLAVE RFID
└────────────────── Start character

# SLAVE RFID - answer to MASTER
#-----------------------------------------------------
<BX0000000000##⏎
││││         │ │
││││         │ └─── NewLine
││││         └───── Hex encoded CRC-8
│││└─────────────── 10 chars of RFID data if present
││└──────────────── RFID status:
││                      0 = No data
││                      1 = RFID data in packet
│└───────────────── B = answer from SLAVE RFID
└────────────────── Start character

```
