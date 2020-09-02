#include <ArduinoJson.h>

#include <WiFi.h>
#include <WiFiMulti.h>
#include <WiFiClient.h>
#include <WebServer.h>
#include <ESPmDNS.h>

#include <ezTime.h>

#include <HTTPClient.h>

QueueHandle_t queue;
int queueSize = 10;

typedef enum {
    ALARMCMD_ARM,
    ALARMCMD_DISARM,
    ALARMCMD_ARM_A,
    ALARMCMD_ARM_B,
    ALARMCMD_ARM_ABC
} AlarmCommands;

// Log serial device
#define LOG Serial

WebServer server(11080);

TaskHandle_t Task0;

Timezone MYTIME;

WiFiMulti WiFiMulti;

int sslSendDelay = 0;

// https://navody.arduino-shop.cz/navody-k-produktum/esp32-a-hardwarova-seriova-linka.html
#define RX1 16
#define TX1 17
HardwareSerial hwSerial(1);

// ############################
// #define UNITID "XXXX"
// #define WIFIPWD "XXXX"
// use include instead
#include "C:\Src\MySmartHome\IoT\ESP-WiFi\JablotronUnit\.mysecrets.h"


const int led = 13;

void handleRoot() {
  server.send(200, "text/plain", (jabIsMoving()?"1":"0"));
}

void handleNotFound() {
  digitalWrite(led, 1);
  String message = "File Not Found\n\n";
  message += "URI: ";
  message += server.uri();
  message += "\nMethod: ";
  message += (server.method() == HTTP_GET) ? "GET" : "POST";
  message += "\nArguments: ";
  message += server.args();
  message += "\n";
  for (uint8_t i = 0; i < server.args(); i++) {
    message += " " + server.argName(i) + ": " + server.arg(i) + "\n";
  }
  server.send(404, "text/plain", message);
  digitalWrite(led, 0);
}


void setNTP(){
  events();
  //configTime("CET-1CEST,M3.5.0,M10.5.0/3", "pool.ntp.org", "time.nist.gov");
}

void digitalClockDisplay() {
  //time_t tnow = time(nullptr);
  //LOG.println(ctime(&tnow));

  LOG.println("UTC RFC3339_EXT: " + UTC.dateTime(RFC3339_EXT));
  // PRG time
  LOG.println("Prague RFC3339_EXT: " + MYTIME.dateTime(RFC3339_EXT));
  LOG.print("UTC.now: "); LOG.println(UTC.now());
}

void setup(void) {
  pinMode(led, OUTPUT);
  digitalWrite(led, 0);
  LOG.begin(115200);

  queue = xQueueCreate( queueSize, sizeof( AlarmCommands ) );

  hwSerial.begin(9600, SERIAL_8N1, RX1, TX1);

  if(queue == NULL){
    Serial.println("Error creating the queue !!");
    delay(1000);
    ESP.restart();
  }

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP("grizzly", WIFIPWD);
  WiFiMulti.addAP("grizzly5G_EXT", WIFIPWD);

  int i =0;
  LOG.print("WiFi connecting ."); 
  while(WiFiMulti.run() == WL_CONNECTED){
    if ((WiFiMulti.run() == WL_CONNECTED)) {
      LOG.println(".");
      LOG.println("WiFi connected"); 
      break;
    }
    i++;
    if(i>120){
      LOG.println("WiFi error - trying to reset ..."); 
      ESP.restart();   
    }
    delay(1000);
    LOG.print(".");
  }

  server.on("/", handleRoot);
  server.on("/heating.api", handleRoot);
  server.onNotFound(handleNotFound);

  server.begin();
  LOG.println("HTTP server started");
  LOG.print("setup() running on core ");
  LOG.println(xPortGetCoreID());

   xTaskCreatePinnedToCore(
                    Task0code,   /* Task function. */
                    "Task0",     /* name of task. */
                    10000,       /* Stack size of task */
                    NULL,        /* parameter of the task */
                    1,           /* priority of the task */
                    &Task0,      /* Task handle to keep track of created task */
                    0);          /* pin task to core 0 */   

  setInterval(60);
  MYTIME.setLocation("Europe/Prague");
  // wait for sync time
  waitForSync(60);

  setNTP();
  digitalClockDisplay();
}

// MySmartHome communication
String createJSON(){
  DynamicJsonDocument doc(2048);
  String ret;

  doc["data"][0]["timestamp"] = MYTIME.dateTime(RFC3339_EXT);

  serializeJson(doc, ret);

  LOG.print("JSON for send:\n");
  LOG.print(ret);
  LOG.print("\n");
  return ret;
}

String sendSSL(){
    String ret = "";
    HTTPClient https;

    LOG.print("[HTTPS] begin...\n");
    if (https.begin("https://myhome.valda.cloud/api/Jablotron/Log?id=" UNITID)) {  // HTTPS
      https.addHeader("Content-Type", "application/json");

      LOG.print("[HTTPS] POST...\n");
      // start connection and send HTTP header
      int httpCode = https.POST(createJSON());

      // httpCode will be negative on error
      if (httpCode > 0) {
        // HTTP header has been send and Server response header has been handled
        LOG.printf("[HTTPS] POST... code: %d\n", httpCode);

        // file found at server
        if (httpCode == HTTP_CODE_OK || httpCode == HTTP_CODE_MOVED_PERMANENTLY) {
          DynamicJsonDocument doc(2048);
          String pload = https.getString();
          LOG.print("[HTTPS] Payload from server: ");
          LOG.println(pload);
          deserializeJson(doc, pload);
          
          ret = doc["commandtoexecute"].as<String>();

        }
      } else {
        LOG.printf("[HTTPS] POST... failed, error: %s\n", https.errorToString(httpCode).c_str());
      }

      https.end();
    } else {
      LOG.printf("[HTTPS] Unable to connect\n");
    }  

    return ret;
}

void executeCMD(String cmd){
  AlarmCommands c = ALARMCMD_DISARM;
  if(cmd=="ARM"){
    c = ALARMCMD_ARM;
  }else if(cmd="DISARM"){
    c = ALARMCMD_DISARM;
  }else if(cmd="ARM_A"){
    c = ALARMCMD_ARM_A;
  }else if(cmd="ARM_B"){
    c = ALARMCMD_ARM_B;
  }else if(cmd="ARM_ABC"){
    c = ALARMCMD_ARM_ABC;
  }
  xQueueSend(queue, &c, 500);
}

// MAIN Loop!
int iter = 0;

void loop(void) {

  iter++;

  if ((WiFiMulti.run() != WL_CONNECTED)) {
    LOG.println("WiFi error - trying to reset ..."); 
    ESP.restart();   
  }

  delay(10);
  // once per 5 seconds
  if(iter > sslSendDelay){

    sslSendDelay += 100;
    if(sslSendDelay>500){
      sslSendDelay = 0;
    }

    iter = 0;
    LOG.print("loop() running on core ");
    LOG.println(xPortGetCoreID());
    digitalClockDisplay();

    setNTP();

    String cmd = sendSSL();
    if(cmd != ""){
      LOG.print("Server command to execute: ");
      LOG.println(cmd);
      executeCMD(cmd);
    }
  }
  
  server.handleClient();

}

// ################## Task in CORE 0 -> RS485 communication - Jablotron JA82
typedef enum {
    JAB_IDLE,
    JAB_ARMED,
    JAB_OUTCOMMINGDELAY,
    JAB_INCOMMINGDELAY,
    JAB_ALARM,
    JAB_DISCONNECTED
} JablotronAlarmState;

typedef enum {
    JAB_ZONE_A,
    JAB_ZONE_B,
    JAB_ZONE_ABC,
    JAB_ZONE_NONE
} JablotronAlarmZone;

struct JABLOTRONDATA {
  bool ledA;
  bool ledB;
  bool ledC;
  bool ledWarning;
  bool ledBacklight;
  time_t lastContact;
  /// 0x00 - Idle
  /// 0x06 - Alarm
  /// 0x0C - Outcomming timeout
  /// 0x0D - Incomming timeout
  /// 0x10 - Active detector
  uint8_t messageId;
  uint8_t deviceId;
  uint8_t msgCRC;
  JablotronAlarmState state;
  JablotronAlarmZone zone;
  bool isMovementThere;
  time_t lastMove;
};

JABLOTRONDATA jadata = {false,false,false,false,false,0,0,0,0,JAB_DISCONNECTED, JAB_ZONE_NONE, 0};

uint8_t jabRecBuf[10] = {0,0,0,0,0,0,0,0,0,0};
int jabBufIndex = 0;
AlarmCommands jabCmd;

void jabSetLastMove(){
  time_t t = UTC.now();

  LOG.println("JAB -> jabSetLastMove");
  if(!((jadata.lastMove+600)<t && (jadata.lastMove+300)>t)){
    LOG.println("JAB -> jabSetLastMove : SET lastMove");
    jadata.lastMove = t;
  }
}

bool jabIsMoving(){
  time_t t = UTC.now();
  return ((jadata.lastMove+600)>t);
}

void jabSetArmA(){
    uint8_t x[8] = { 0x8F, 0xFF, 0xA0, 0xFF, 0x82, 0xFF, 0xA1, 0xFF };
    hwSerial.write(x, 8);
}

void jabSetArmB(){
    uint8_t x[8] = { 0x8F, 0xFF, 0xA0, 0xFF, 0x83, 0xFF, 0xA1, 0xFF };
    hwSerial.write(x, 8);
}

void jabSetArmABC(){
    uint8_t x[8] = { 0x8F, 0xFF, 0xA0, 0xFF, 0x81, 0xFF, 0xA1, 0xFF };
    hwSerial.write(x, 8);
}

void jabChangeArm(){
  uint8_t x;
  uint8_t pin[JABPINLEN] = JABPIN;
  int ilen = 0;
  int i;
  for(i=0; i<JABPINLEN; i++){
    //format PIN and put to buffer
    x = pin[i] + 0x80;
    hwSerial.write(x);
    if ((i + 1) < JABPINLEN)
    {
        x = 0xA0;
    }
    else
    {
        x = 0xA2;
    }
    hwSerial.write(x);
  }
}

// process message
// ED 51 0C 00 09 04 00 00 73 FF
void jaProcessMsg(){
  // we have message (10 bytes long)
  jadata.lastContact = UTC.now();

  //LEDs
  jadata.ledA = ((jabRecBuf[4] & 0x08) == 0x08);
  jadata.ledB = ((jabRecBuf[4] & 0x04) == 0x04);
  jadata.ledC = ((jabRecBuf[4] & 0x02) == 0x02);
  jadata.ledBacklight = ((jabRecBuf[4] & 0x01) == 0x01);
  jadata.ledWarning = ((jabRecBuf[4] & 0x10) == 0x10);

  //messages
  jadata.messageId = jabRecBuf[2];
  jadata.deviceId = jabRecBuf[3];

  // exception - in case of alarm waiting for device ID
  JablotronAlarmState newState = JAB_IDLE;
    /*
      2nd byte (status):
        bit 0: A armed
        bit 1: B  armed
        bit 2: ALARM
        bit 3: Incomming Wait period raised
        bit 4: Otcomming wait period
        bit 5:
        bit 6:
        bit 7:
      */
  switch (jabRecBuf[1] & 0x03)
  {
      case 0x00:
          jadata.zone = JAB_ZONE_NONE;
          break;
      case 0x01:
          jadata.zone = JAB_ZONE_A;
          break;
      case 0x02:
          jadata.zone = JAB_ZONE_B;
          break;
      case 0x03:
          jadata.zone = JAB_ZONE_ABC;
          break;
  }

  if ((jabRecBuf[1] & 0x1F) == 0x00)
  {
      newState = JAB_IDLE;
  }
  else
  {
      if ((jabRecBuf[1] & 0x04) == 0x04)
      {
          newState = JAB_ALARM;
      }else if ((jabRecBuf[1] & 0x08) == 0x08)
      {
          newState = JAB_INCOMMINGDELAY;
      }
      else if ((jabRecBuf[1] & 0x10) == 0x10)
      {
          newState = JAB_OUTCOMMINGDELAY;
      }
      else
      {
          newState = JAB_ALARM;
      }
  }

  if(newState == JAB_ALARM && jadata.deviceId == 0x00)
  {
      //ignore this state ... waiting for alarm message with device ID
  }
  else
  {
      if (jadata.state != newState)
      {
          jadata.state = newState;
          
          // timeout for sending has to be shorter
          sslSendDelay = 0;
      }
  }

  if (jadata.msgCRC != jabRecBuf[8])
  {
      jadata.msgCRC = jabRecBuf[8];
      jabSetLastMove();
  }

  // send command if there is any
  if(xQueueReceive(queue, &jabCmd, 0)){
    // sending right command

    switch (jabCmd)
    {
      case ALARMCMD_ARM_A:
        LOG.println("JAB -> ALARMCMD_ARM_A");
        jabSetArmA();
        break;
      case ALARMCMD_ARM_B:
        LOG.println("JAB -> ALARMCMD_ARM_B");
        jabSetArmB();
        break;
      case ALARMCMD_ARM_ABC:
        LOG.println("JAB -> ALARMCMD_ARM_ABC");
        jabSetArmABC();
        break;

      case ALARMCMD_ARM:
        LOG.println("JAB -> ALARMCMD_ARM");
        jabChangeArm();
        break;
      case ALARMCMD_DISARM:
        LOG.println("JAB -> ALARMCMD_DISARM");
        jabChangeArm();
        break;
      
      default:
        break;
    }

    sslSendDelay = 100;
  }
}

void Task0code( void * pvParameters ){
  LOG.print("Task0 running on core ");
  LOG.println(xPortGetCoreID());

  hwSerial.setTimeout(500);

  for(;;){
    if(hwSerial.readBytes(&jabRecBuf[jabBufIndex], 1)>0){
      LOG.print(">");
      LOG.print(jabRecBuf[jabBufIndex], HEX);
      // data readed
      if(jabRecBuf[jabBufIndex] == 0xFF){
        // we have full datagram (END char)
        // if len == 10 we are OK
        if(jabBufIndex==9){
          LOG.println("  -> jabProcessMsg");
          jaProcessMsg();
          jabBufIndex = 0;
        }else{
          //packet to short
          LOG.println("  -> JAB-ERR: packet too short");
          jabBufIndex = 0;
        }
      }else{
        // regular char
        jabBufIndex++;
        if(jabBufIndex>=10){
          // strange data, packet too long
          LOG.println("  -> JAB-ERR: packet out of range");
          jabBufIndex = 0;
        }
      }
    }else{
      // no data, starting from zero ..
      jabBufIndex = 0;
      LOG.println("  -> JAB-ERR: No data received");
    }
  } 
}
