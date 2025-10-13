/* 
 * VR Mouse Trackball Interface with Reward/Punishment System
 * 
 * This Arduino sketch interfaces with Unity VR via serial communication,
 * reading motion data from dual ADNS3080 optical sensors mounted on a
 * trackball and controlling water reward and air puff systems.
 * 
 * Hardware:
 * - 2x ADNS3080 optical flow sensors (for X/Y motion detection)
 * - Lick sensor (digital input)
 * - Water solenoid (digital output)
 * - Air puff solenoid (digital output)
 * 
 * Communication Protocol:
 * - 'h' command: Request motion and lick data
 * - 'w' command: Trigger water reward
 * - 'a' command: Trigger air puff
 */

#include <SPI.h>
#define WATER_OPEN_TIME 60
#define AIRPUFF_OPEN_TIME 20

#define SERIAL                         Serial
#define SOLENOID_PIN                   7

#define PIN_MOUSECAM_RESET_1           11
#define PIN_SS_1                       10
#define PIN_MOUSECAM_CS_1              10

#define PIN_MOUSECAM_RESET_2           5
#define PIN_SS_2                       4
#define PIN_MOUSECAM_CS_2              4

#define LICK_PIN                       2
#define AIRPUFF_PIN                    7
#define WATER_PIN                      8

#define ADNS3080_PIXELS_X              30
#define ADNS3080_PIXELS_Y              30

#define ADNS3080_PRODUCT_ID            0x00
#define ADNS3080_PRODUCT_ID_VAL        0x17
#define ADNS3080_CONFIGURATION_BITS    0x0a
#define ADNS3080_MOTION_BURST          0x50
#define ADNS3080_PIXEL_SUM             0x06
#define ADNS3080_FRAME_CAPTURE         0x13
#define ADNS3080_PIXEL_BURST           0x40


SPISettings spi_settings(20000000, MSBFIRST, SPI_MODE3);
byte frame[ADNS3080_PIXELS_X * ADNS3080_PIXELS_Y];

int reads_since_last_write=0;
int sensed = 4;
int rawVal;
int waitTime = 100;

bool water_open = false;
bool airpuff_open = false;
unsigned long water_opened;
unsigned long airpuff_opened;

unsigned long t = millis();
unsigned long t_last = t;


int incomingByte;
int incomingByte_1;


unsigned long lastSense;

unsigned long lastPrint;
bool was_not_licking = true;

struct MotionAndLickData
{
  int32_t x_1;
  int32_t y_1;
  int32_t x_2;
  int32_t y_2;
  int32_t lick_count;
  int32_t dt;
};

MotionAndLickData motionAndLickData;

struct MD
{
 byte motion;
 char dx, dy;
 byte squal;
 word shutter;
 byte max_pix;
};


int mousecam_read_reg(int cs_pin, int reg)
{
  SPI.transfer(cs_pin, reg, SPI_CONTINUE);
  delayMicroseconds(75);
  int ret = SPI.transfer(cs_pin, 0xff);
  delayMicroseconds(1);
  return ret;
}

void mousecam_write_reg(int cs_pin, int reg, int val)
{
  SPI.transfer(cs_pin, reg | 0x80, SPI_CONTINUE);
  SPI.transfer(cs_pin, val);
  delayMicroseconds(50);
}

int mousecam_init()
{
  pinMode(PIN_MOUSECAM_RESET_1,OUTPUT);
  pinMode(PIN_MOUSECAM_CS_1, OUTPUT);
  digitalWrite(PIN_MOUSECAM_RESET_1,LOW);

  pinMode(PIN_MOUSECAM_RESET_2,OUTPUT);
  pinMode(PIN_MOUSECAM_CS_2, OUTPUT);
  digitalWrite(PIN_MOUSECAM_RESET_2,LOW);
 
  delay(10);
 
  mousecam_reset(PIN_MOUSECAM_RESET_1);
  mousecam_reset(PIN_MOUSECAM_RESET_2);

 
  int pid_1 = mousecam_read_reg(PIN_MOUSECAM_CS_1, ADNS3080_PRODUCT_ID);
  int pid_2 = mousecam_read_reg(PIN_MOUSECAM_CS_2, ADNS3080_PRODUCT_ID);

  if(pid_1 != ADNS3080_PRODUCT_ID_VAL || pid_2 != ADNS3080_PRODUCT_ID_VAL)
    return -1;

  // turn on sensitive mode
  mousecam_write_reg(PIN_MOUSECAM_CS_1, ADNS3080_CONFIGURATION_BITS, 0x19);
  mousecam_write_reg(PIN_MOUSECAM_CS_2, ADNS3080_CONFIGURATION_BITS, 0x19);

  return 0;
}

void mousecam_reset(int reset_pin)
{
  digitalWrite(reset_pin,HIGH);
  delay(1); // reset pulse > 10us
  digitalWrite(reset_pin,LOW);
  delay(35); // 35ms from reset to functional
}

void mousecam_read_motion(int cs_pin, struct MD *p)
{
  delayMicroseconds(10);
  SPI.transfer(cs_pin, ADNS3080_MOTION_BURST, SPI_CONTINUE);
  delayMicroseconds(75);
  p->motion =  SPI.transfer(cs_pin, 0xff, SPI_CONTINUE);
  p->dx =  SPI.transfer(cs_pin, 0xff, SPI_CONTINUE);
  p->dy =  SPI.transfer(cs_pin, 0xff, SPI_CONTINUE);
  delayMicroseconds(5);
}

void setup() {
  Serial.begin(9600);
  pinMode(WATER_PIN, OUTPUT);
  pinMode(AIRPUFF_PIN, OUTPUT);
  pinMode(sensed,OUTPUT);
  pinMode(LICK_PIN, INPUT);

  digitalWrite(WATER_PIN, LOW);
  digitalWrite(AIRPUFF_PIN, LOW);

  water_open = false;
  airpuff_open = false;

  SPI.begin(PIN_SS_1);
  SPI.begin(PIN_SS_2);
  SPI.setClockDivider(PIN_SS_1, SPI_CLOCK_DIV32);
  SPI.setClockDivider(PIN_SS_2, SPI_CLOCK_DIV32);

  SPI.beginTransaction(spi_settings);

  SERIAL.begin(115200);
 
  motionAndLickData.x_1 = 0;
  motionAndLickData.x_2 = 0;
  motionAndLickData.y_1 = 0;
  motionAndLickData.y_2 = 0;
  motionAndLickData.lick_count = 0;
  reads_since_last_write = 0;
  
  while(mousecam_init() == -1){
    // Wait for sensor initialization
  }
}


// *****************************************************************//
// ************************** MAIN LOOP ****************************//
// *****************************************************************//
  
void loop() {

  motionAndLickData.lick_count += 1;

  MD md;
  
  // Read motion from first sensor
  mousecam_read_motion(PIN_SS_1, &md);
  if(md.motion & 0x80){
    motionAndLickData.x_1 += (int) (signed char) md.dx;
    motionAndLickData.y_1 += (int) (signed char) md.dy;
  }

  // Read motion from second sensor
  mousecam_read_motion(PIN_SS_2, &md);
  if(md.motion & 0x80){
    motionAndLickData.x_2 += (int) (signed char) md.dx;
    motionAndLickData.y_2 += (int) (signed char) md.dy;
  }

  // Handle serial commands from Unity
  if(SERIAL.available()){
    byte a = SERIAL.read();
    if(a == 'h'){           // DATA REQUESTED
      t = millis();
      int dt = (int) (t - t_last);
      t_last = t;
      
      // Output the amount of time passed since the last read
      motionAndLickData.dt = dt;
      
      SERIAL.write((byte *) &motionAndLickData, 6*4);
      motionAndLickData.x_1 = 0;
      motionAndLickData.y_1 = 0;
      motionAndLickData.x_2 = 0;
      motionAndLickData.y_2 = 0;
      motionAndLickData.lick_count = 0;
      motionAndLickData.dt = 0;
    } else if(a == 'w'){    // WATER DELIVERY REQUESTED
      water_open=true;
      water_opened = millis();
      digitalWrite(WATER_PIN, HIGH);
    } else if(a == 'a'){    // AIRPUFF REQUESTED
      airpuff_open = true;
      airpuff_opened = millis();
      digitalWrite(AIRPUFF_PIN, HIGH);
    }
  }
  

  // CLOSE THE WATER OR AIRPUFF IF THEY ARE OPEN AND ENOUGH TIME HAS PASSED

  // If the port is open and has been open longer that WATER_OPEN_TIME, close it
  if(water_open == true && millis() - water_opened >= WATER_OPEN_TIME){
    water_open = false;
    digitalWrite(WATER_PIN, LOW);
   }

  // If the port is open and has been open longer that AIRPUFF_OPEN_TIME, close it
  if(airpuff_open == true && millis() - airpuff_opened >= AIRPUFF_OPEN_TIME){
    airpuff_open = false;
    digitalWrite(AIRPUFF_PIN, LOW);
   }

   // UPDATE THE LICK COUNTER
    int lick_pin_value = digitalRead(LICK_PIN);
    if(lick_pin_value == 1){
      if(was_not_licking == true){
        motionAndLickData.lick_count += 1;
        was_not_licking = false;
      }
    } else {
      was_not_licking = true;
    }
}
