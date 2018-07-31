#include <OneWire.h>
#include <DallasTemperature.h>
#include <Wire.h>
#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>

// Data wire is plugged into pin 2 on the Arduino
#define ONE_WIRE_BUS 2

#define TYPUNKNOWN  0
#define TYPDATAFROMSENSOR  1
#define TYPDATATOSENSOR  2
#define TYPDATAQUERY  3

RF24 radio(8, 7);
const byte rxAddr[6] = "00001";

// Setup a oneWire instance to communicate with any OneWire devices 
// (not just Maxim/Dallas temperature ICs)
OneWire oneWire(ONE_WIRE_BUS);
// Pass our oneWire reference to Dallas Temperature.
DallasTemperature sensors(&oneWire);

struct boudaData{
  int type;
  float temp;
  bool heat;
  float onTemp;
  float offTemp;
  int sensorCount;
};

struct boudaData data;
struct boudaData dataIn;

void setup() {
  // put your setup code here, to run once:
  pinMode(13, OUTPUT);
  pinMode(5, OUTPUT);
  Serial.begin(9600);

  data.type = TYPUNKNOWN;
  data.temp = 0.0;
  data.heat = false;
  data.onTemp = 5.0;
  data.offTemp = 5.5; 
  data.sensorCount = 0;

  // Start up the library
  sensors.begin();

  radio.begin();
  radio.setRetries(15, 15);
  radio.openWritingPipe(rxAddr);
  radio.openReadingPipe(0, rxAddr);
  radio.stopListening();
}

void loop() {
  // put your main code here, to run repeatedly:

  sensors.requestTemperatures(); // Send the command to get temperatures
  data.sensorCount = sensors.getDeviceCount();
  data.temp = sensors.getTempCByIndex(0); // Why "byIndex"? 

  if(data.sensorCount>0){
    if(data.temp < data.onTemp){
      data.heat = true;
    }
    if(data.temp > data.offTemp){
      data.heat = false;
    }
  }else{
    data.heat = false;
  }
  
  digitalWrite(5, (data.heat ? HIGH : LOW));

  Serial.print("sensors: ");
  Serial.print(data.sensorCount);
  Serial.print(" | temp: ");
  Serial.print(data.temp);
  Serial.print(" | heat: ");
  Serial.print(data.heat);
  Serial.println("");

  // radio.write(&data, sizeof(data));

  radio.startListening();
  // delay for while
  for(int i=0; i<10000;i++){
    if (radio.available()){
        radio.stopListening();
        delay(20);
        radio.read(&dataIn, sizeof(dataIn));
        if(dataIn.type == TYPDATATOSENSOR){
          data.onTemp = dataIn.onTemp;
          data.offTemp = dataIn.offTemp;
          data.type = TYPDATAFROMSENSOR;
          radio.write(&data, sizeof(data));
        }else if(dataIn.type == TYPDATAQUERY){
          data.type = TYPDATAFROMSENSOR;
          radio.write(&data, sizeof(data));
        }
      break;
    }
    delay(1);
  }
  radio.stopListening();
  
}
