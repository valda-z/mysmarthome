/**
 * BasicHTTPClient.ino
 *
 *  Created on: 24.05.2015
 *
 */

#include <Arduino.h>

#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>

#include <ESP8266HTTPClient.h>

#define USE_SERIAL Serial

#define IOPUMP 4 
#define IOHEATING 5 

#define MAXERRCOUNT 10
#define CHECKDELAY 10000

ESP8266WiFiMulti WiFiMulti;
int errCount = 0;
int resp =0;

void setup() {

    pinMode(IOPUMP, OUTPUT); 
    pinMode(IOHEATING, OUTPUT); 
    digitalWrite(IOPUMP, LOW);
    digitalWrite(IOHEATING, LOW);

    USE_SERIAL.begin(115200);
   // USE_SERIAL.setDebugOutput(true);

    USE_SERIAL.println();
    USE_SERIAL.println();
    USE_SERIAL.println();

    for(uint8_t t = 4; t > 0; t--) {
        USE_SERIAL.printf("[SETUP] WAIT %d...\n", t);
        USE_SERIAL.flush();
        delay(1000);
    }

    WiFiMulti.addAP("SSID", "PWD");

    errCount = 0;
    resp = 0;
}

void loop() {
    // wait for WiFi connection
    if((WiFiMulti.run() == WL_CONNECTED)) {

        HTTPClient http;

        USE_SERIAL.print("[HTTP] begin...\n");
        // configure traged server and url
        http.begin("http://192.168.0.10:11080/heating.api"); //HTTP

        USE_SERIAL.print("[HTTP] GET...\n");
        // start connection and send HTTP header
        int httpCode = http.GET();

        // httpCode will be negative on error
        if(httpCode > 0) {
            // HTTP header has been send and Server response header has been handled
            USE_SERIAL.printf("[HTTP] GET... code: %d\n", httpCode);

            // file found at server
            if(httpCode == HTTP_CODE_OK) {
                String payload = http.getString();
                USE_SERIAL.println(payload);
                resp = payload.toInt();

                digitalWrite(IOPUMP, ( ((resp&0x01) == 0x01) ? HIGH : LOW ));
                digitalWrite(IOHEATING, ( ((resp&0x02) == 0x02) ? HIGH : LOW ));
            }
        } else {
            USE_SERIAL.printf("[HTTP] GET... failed, error: %s\n", http.errorToString(httpCode).c_str());
            errCount++;
            if(errCount > MAXERRCOUNT) {
                ESP.restart();
            }
        }

        http.end();
    }else{
        USE_SERIAL.printf("[WiFi] cannot connect to network\n");
        errCount++;
        if(errCount > MAXERRCOUNT) {
            ESP.restart();
        }
    }

    delay(CHECKDELAY);

}

