/* *
MySmartHome Outdoor Unit
v 1.0.0
*/

#include <Arduino.h>
#include <ArduinoJson.h>

#include <ezTime.h>

#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>

#include <ESP8266HTTPClient.h>

#include <WiFiClientSecureBearSSL.h>

#include <OneWire.h>
#include <DallasTemperature.h>

// ############################
// #define UNITID "XXXX"
// #define WIFIPWD "XXXX"
// use include instead
#include "C:\Src\MySmartHome\IoT\ESP-WiFi\OutdoorUnit\.mysecrets.h"

// Fingerprint for demo URL, expires on June 2, 2019, needs to be updated well before this date
// const uint8_t fingerprint[20] = {0x5A, 0xCF, 0xFE, 0xF0, 0xF1, 0xA6, 0xF4, 0x5F, 0xD2, 0x11, 0x11, 0xC6, 0x1D, 0x2F, 0x0E, 0xBC, 0x39, 0x8D, 0x50, 0xE0};

ESP8266WiFiMulti WiFiMulti;
int wifiConnAttempt = 0;

#define IO_OUT1 5
#define IO_OUT2 4
#define IO_IN 0
#define IO_TEMP 2

// Setup a oneWire instance to communicate with any OneWire devices
OneWire oneWire(IO_TEMP);

// Pass our oneWire reference to Dallas Temperature sensor 
DallasTemperature sensors(&oneWire);

// Number of temperature devices found
int numberOfDevices;
float tempDogHouse = 0;
float tempOut = 0;
bool dataIsWet = false;
bool dataWaterSwitchOn = false;
bool dataDoghouseHeating = false;

float srvDoghouseTemp = 0;
bool srvIrrigationOn = false;
bool srvChristmasOn = false;
String srvWaterOnStart = "";
String srvWaterOnEnd = "";
String srvWater1Start = "";
int srvWater1Int = 0;
String srvWater2Start = "";
int srvWater2Int = 0;

// We'll use this variable to store a found device address
DeviceAddress tempDeviceAddress; 

Timezone MYTIME;


void setup() {

  Serial.begin(115200);
  // Serial.setDebugOutput(true);

  Serial.println();
  Serial.println();
  Serial.println();

  for (uint8_t t = 4; t > 0; t--) {
    Serial.printf("[SETUP] (v1.0.0) WAIT %d...\n", t);
    Serial.flush();
    delay(1000);
  }

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP("grizzly", WIFIPWD);
  WiFiMulti.addAP("grizzly5G_EXT", WIFIPWD);

  wifiConnAttempt = 0;

  // setup GPIOS
  pinMode(IO_OUT1, OUTPUT); 
  pinMode(IO_OUT2, OUTPUT); 
  digitalWrite(IO_OUT1, HIGH);
  digitalWrite(IO_OUT2, HIGH);
  pinMode(IO_IN, INPUT); 

  // Start up the library
  sensors.begin();
  
  // Grab a count of devices on the wire
  numberOfDevices = sensors.getDeviceCount();
  
  // 
  // BOUDA Found device 0 with address: 289A4DB72719019C
  // OUTTEMP Found device 1 with address: 28631956B5013C8A

  // locate devices on the bus
  Serial.print("Locating devices...");
  Serial.print("Found ");
  Serial.print(numberOfDevices, DEC);
  Serial.println(" devices.");

  // Loop through each device, print out address
  for(int i=0;i<numberOfDevices; i++){
    // Search the wire for address
    if(sensors.getAddress(tempDeviceAddress, i)){
      Serial.print("Found device ");
      Serial.print(i, DEC);
      Serial.print(" with address: ");
      printAddress(tempDeviceAddress);
      Serial.println();
    } else {
      Serial.print("Found ghost device at ");
      Serial.print(i, DEC);
      Serial.print(" but could not detect address. Check power and cabling");
    }
  }

  setInterval(60);
  MYTIME.setLocation("Europe/Prague");
  // wait for sync time
  waitForSync(60);

  setNTP();
  digitalClockDisplay();

}

void setNTP(){
  events();
  //configTime("CET-1CEST,M3.5.0,M10.5.0/3", "pool.ntp.org", "time.nist.gov");
}

// function to print a device address
void printAddress(DeviceAddress deviceAddress) {
  for (uint8_t i = 0; i < 8; i++){
    if (deviceAddress[i] < 16) Serial.print("0");
      Serial.print(deviceAddress[i], HEX);
  }
}

void digitalClockDisplay() {
  //time_t tnow = time(nullptr);
  //Serial.println(ctime(&tnow));

  Serial.println("UTC RFC3339_EXT: " + UTC.dateTime(RFC3339_EXT));
  // PRG time
  Serial.println("Prague RFC3339_EXT: " + MYTIME.dateTime(RFC3339_EXT));
}

void getWet(){
  dataIsWet = digitalRead(IO_IN)==0;
}

void getTemperatures(){
    sensors.requestTemperatures(); // Send the command to get temperatures
    
    // Loop through each device, print out temperature data
    for(int i=0;i<numberOfDevices; i++){
      // Search the wire for address
      if(sensors.getAddress(tempDeviceAddress, i)){
        // Output the device ID
        Serial.print("Temperature for device (");
        Serial.print(i,DEC);
        Serial.print(":");
        printAddress(tempDeviceAddress);
        Serial.print(") : ");
        // Print the data
        float tempC = sensors.getTempC(tempDeviceAddress);
        Serial.println(tempC);
        // check doghouse device 
        if( tempDeviceAddress[0] == 0x28 &&
            tempDeviceAddress[1] == 0x9A &&
            tempDeviceAddress[2] == 0x4D &&
            tempDeviceAddress[3] == 0xB7 &&
            tempDeviceAddress[4] == 0x27 &&
            tempDeviceAddress[5] == 0x19 &&
            tempDeviceAddress[6] == 0x01 &&
            tempDeviceAddress[7] == 0x9C  ){
          Serial.print("DOGHOUSE device ...");
          Serial.println();
          tempDogHouse = tempC;
        }else{
          tempOut = tempC;
        }
      }
    }
}

// {"data":[{"outsidetemp":99.9,"doghousetemp":0.0,"doghouseheating":false,
//  "waterswitchon":false,"iswet":false,"timestamp":"2019-08-27T11:25:34.6779085+02:00"}
String createJSON(){
  DynamicJsonDocument doc(2048);
  String ret;

  doc["data"][0]["outsidetemp"] = tempOut;
  doc["data"][0]["doghousetemp"] = tempDogHouse;
  doc["data"][0]["doghouseheating"] = dataDoghouseHeating;
  doc["data"][0]["waterswitchon"] = dataWaterSwitchOn;
  doc["data"][0]["iswet"] = dataIsWet;
  doc["data"][0]["timestamp"] = MYTIME.dateTime(RFC3339_EXT);

  serializeJson(doc, ret);

  Serial.print("JSON for send:\n");
  Serial.print(ret);
  Serial.print("\n");
  return ret;
}

String convertWStart(String hr){
  String ret = MYTIME.dateTime(RFC3339_EXT).substring(0,11) + hr + ":00";
  return ret;
}

void sendSSL(){
    std::unique_ptr<BearSSL::WiFiClientSecure>client(new BearSSL::WiFiClientSecure);

    //client->setFingerprint(fingerprint);
    client->setInsecure();

    // https://arduinojson.org/v6/how-to/use-arduinojson-with-esp8266httpclient/

    HTTPClient https;

    Serial.print("[HTTPS] begin...\n");
    if (https.begin(*client, "https://myhome.valda.cloud/api/Device/Log?id=" UNITID)) {  // HTTPS
      https.addHeader("Content-Type", "application/json");

      Serial.print("[HTTPS] POST...\n");
      // start connection and send HTTP header
      int httpCode = https.POST(createJSON());

      // httpCode will be negative on error
      if (httpCode > 0) {
        // HTTP header has been send and Server response header has been handled
        Serial.printf("[HTTPS] POST... code: %d\n", httpCode);

        // file found at server
        if (httpCode == HTTP_CODE_OK || httpCode == HTTP_CODE_MOVED_PERMANENTLY) {
          DynamicJsonDocument doc(2048);
          String pload = https.getString();
          Serial.println(pload);
          deserializeJson(doc, pload);
          
          srvDoghouseTemp = doc["dogontemp"].as<float>();
          srvChristmasOn = doc["christmasOn"].as<bool>();
          srvIrrigationOn = doc["irrigationOn"].as<bool>();
          srvWaterOnStart = doc["water"]["from"].as<String>();
          srvWaterOnStart = srvWaterOnStart.substring(0,19);
          srvWaterOnEnd = doc["water"]["to"].as<String>();
          srvWaterOnEnd = srvWaterOnEnd.substring(0,19);
          if(srvIrrigationOn){
            srvWater1Start = convertWStart(doc["wateritems"][0]["starthour"].as<String>());
            srvWater1Int = doc["wateritems"][0]["intervalsec"].as<int>();
            srvWater2Start = convertWStart(doc["wateritems"][1]["starthour"].as<String>());
            srvWater2Int = doc["wateritems"][1]["intervalsec"].as<int>();
          }else{
            srvWater1Start = "0000";
            srvWater1Int = 0;
            srvWater2Start = "0000";
            srvWater2Int = 0;
          }
        }
      } else {
        Serial.printf("[HTTPS] POST... failed, error: %s\n", https.errorToString(httpCode).c_str());
      }

      https.end();
    } else {
      Serial.printf("[HTTPS] Unable to connect\n");
    }  
}

void setSSR(){
  digitalWrite(IO_OUT1, dataDoghouseHeating ? LOW : HIGH);
  digitalWrite(IO_OUT2, dataWaterSwitchOn ? LOW : HIGH);
}

void processLogic(){
  Serial.println("------ PROCESS LOGIC ------");
  Serial.print("srvChristmasOn: "); Serial.println(srvChristmasOn);
  Serial.print("srvIrrigationOn: "); Serial.println(srvIrrigationOn);
  Serial.print("srvDoghouseTemp: "); Serial.println(srvDoghouseTemp);
  Serial.print("srvWaterOnStart: "); Serial.println(srvWaterOnStart);
  Serial.print("srvWaterOnEnd: "); Serial.println(srvWaterOnEnd);
  Serial.print("srvWater1Start: "); Serial.println(srvWater1Start);
  Serial.print("srvWater1Int: "); Serial.println(srvWater1Int);
  Serial.print("srvWater2Start: "); Serial.println(srvWater2Start);
  Serial.print("srvWater2Int: "); Serial.println(srvWater2Int);

  //water LOGIC
  dataWaterSwitchOn = false;
  String wnow = UTC.dateTime(RFC3339_EXT).substring(0,19);
  if(srvWaterOnStart<=wnow && srvWaterOnEnd>=wnow){
    dataWaterSwitchOn = true;
    Serial.println(" -> switch on water = forced switch on");
  }else{
    if(srvIrrigationOn && (!dataIsWet || srvChristmasOn)){
      if(srvWater1Start<=MYTIME.dateTime(RFC3339_EXT).substring(0,19) 
          && srvWater1Start>=(MYTIME.dateTime(MYTIME.now() - srvWater1Int, RFC3339_EXT).substring(0,19))){
        dataWaterSwitchOn = true;
        Serial.println(" -> switch on water = timeinterval for 1");
      }
      if(srvWater2Start<=MYTIME.dateTime(RFC3339_EXT).substring(0,19) 
          && srvWater2Start>=(MYTIME.dateTime(MYTIME.now() - srvWater2Int, RFC3339_EXT).substring(0,19))){
        dataWaterSwitchOn = true;
        Serial.println(" -> switch on water = timeinterval for 2");
      }
    }
  }

  // heating LOGIC
  if(dataDoghouseHeating){
    if((srvDoghouseTemp+0.5)<tempDogHouse){
      dataDoghouseHeating = false;
      Serial.println(" -> switch off heating");
    }
  }else{
    if(srvDoghouseTemp>tempDogHouse){
      dataDoghouseHeating = true;
      Serial.println(" -> switch on heating");
    }
  }
}

void loop() {

  setNTP();

  digitalClockDisplay();

  getTemperatures();

  getWet();

  // wait for WiFi connection
  if ((WiFiMulti.run() == WL_CONNECTED)) {
    wifiConnAttempt = 0;
    sendSSL();
  }else{
    wifiConnAttempt++;
    Serial.printf("Wifi not connected, next attempt: %d\n", wifiConnAttempt);
    if(wifiConnAttempt>5){
      Serial.println("WiFi error - trying to reset ..."); 
      ESP.restart();   
    }
  }

  processLogic();

  setSSR();

  Serial.println("Wait 10s before next round...");
  delay(10000);
}
