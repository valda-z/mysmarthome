#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>

#define TYPUNKNOWN  0
#define TYPDATAFROMSENSOR  1
#define TYPDATATOSENSOR  2
#define TYPDATAQUERY  3

RF24 radio(8, 7);
const byte rxAddr[6] = "00001";

// QUERY|
// SET;5;5.5;|

struct boudaData{
  int type;
  float temp;
  bool heat;
  float onTemp;
  float offTemp;
  int sensorCount;
};

struct boudaData data;
int x = 0;
String inData;
char received;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for native USB
  }  
  radio.begin();
  radio.setRetries(15, 15);
  radio.openWritingPipe(rxAddr);
  radio.openReadingPipe(0, rxAddr);
  radio.stopListening();

  Serial.println("STARTING ...");  
}

void loop() {
  if(Serial.available() > 0){
    inData = "";
    for(int i=0; i<250; i++){
      if(Serial.available() > 0){
        received = Serial.read();
        if (received == '|'){
          String cmd = getValue(inData, ';', 0);
          if(cmd == "QUERY"){
            sendReceive(TYPDATAQUERY, 0, 0);
          }else if(cmd == "SET"){
            String son = getValue(inData, ';', 1);
            String soff = getValue(inData, ';', 2);
            if(son == "" || soff == ""){
              Serial.print("ERR;Syntax error|");
            }else{
              float fon = son.toFloat();
              float foff = soff.toFloat();
              if( (fon<0.0 && fon > 25.0) ||
                  (foff<0.0 && foff > 25.0) ||
                  (fon>foff) ){
                Serial.print("ERR;Syntax error|");
              }else{
                sendReceive(TYPDATATOSENSOR, fon, foff);
              }
            }
          }else{
            Serial.print("ERR;Syntax error|");
          }
          break;
        }
        inData += received;
        continue;
      }
      delay(1);
    }
  }
}

String getValue(String data, char separator, int index)
{

    int maxIndex = data.length()-1;
    int j=0;
    String chunkVal = "";

    for(int i=0; i<=maxIndex && j<=index; i++)
    {
      if(data[i]==separator)
      {
        j++;

        if(j>index)
        {
          chunkVal.trim();
          return chunkVal;
        }    

        chunkVal = "";    
      }else{
        chunkVal.concat(data[i]);  
      }
    }  
}

void sendReceive(int type, float onTemp, float offTemp){
  bool nfrerr = true;
  radio.stopListening();
  if(type == TYPDATATOSENSOR){
    data.type = TYPDATATOSENSOR;
    data.onTemp = onTemp;
    data.offTemp = offTemp;
    radio.write(&data, sizeof(data));
  }else{
    data.type = TYPDATAQUERY;
    radio.write(&data, sizeof(data));
  }
  
  radio.startListening();
  for(int i=0;i<20000;i++){
    if (radio.available())
    {
      data.sensorCount = 0;
      radio.read(&data, sizeof(data));

      nfrerr = false;
  
      if(data.type != TYPDATAFROMSENSOR){
        Serial.print("ERR;Error NFR!|");
        delay(1000);
      }else{
        Serial.print("OK;");
        Serial.print(data.sensorCount);
        Serial.print(";");
        Serial.print(data.onTemp);
        Serial.print(";");
        Serial.print(data.offTemp);
        Serial.print(";");
        Serial.print(data.temp);
        Serial.print(";");
        Serial.print(data.heat);
        Serial.print("|");
      }    
    }
  }
  radio.stopListening();
  if(nfrerr){
    Serial.print("ERR;Error NFR Timeout!|");
  }
}

