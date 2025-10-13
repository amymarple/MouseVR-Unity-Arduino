#include <SPI.h>
#define SERIAL                         SerialUSB
#define SOLENOID_PIN                   7

#define PIN_MOUSECAM_RESET_1           11
#define PIN_SS_1                       10
#define PIN_MOUSECAM_CS_1              10

#define PIN_MOUSECAM_RESET_2           5
#define PIN_SS_2                       4
#define PIN_MOUSECAM_CS_2              4


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



struct Offset
{
  int32_t x_1;
  int32_t y_1;
  int32_t x_2;
  int32_t y_2;
};

Offset offset;

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


// pdata must point to an array of size ADNS3080_PIXELS_X x ADNS3080_PIXELS_Y
// you must call mousecam_reset() after this if you want to go back to normal operation
int mousecam_frame_capture(int cs_pin, byte *pdata)
{
  mousecam_write_reg(cs_pin, ADNS3080_FRAME_CAPTURE,0x83);
  delayMicroseconds(20);
  SPI.transfer(cs_pin, ADNS3080_PIXEL_BURST, SPI_CONTINUE);
  delayMicroseconds(50);
 
  int pix;
  byte started = 0;
  int count;
  int timeout = 0;
  int ret = 0;
  for(count = 0; count < ADNS3080_PIXELS_X * ADNS3080_PIXELS_Y; )
  {
    pix = SPI.transfer(cs_pin, 0xff, SPI_CONTINUE);
   
    delayMicroseconds(10);
    if(started==0)
    {
      if(pix&0x40)
        started = 1;
      else
      {
        timeout++;
        if(timeout==1000)
        {
          ret = -1;
          break;
        }
      }
    }
    if(started==1)
    {
      pdata[count++] = (pix & 0x3f)<<2; // scale to normal grayscale byte range
    }
  }

  SPI.transfer(cs_pin, 0xff);
 
  delayMicroseconds(14);
 
  return ret;
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
  SPI.transfer(cs_pin, ADNS3080_MOTION_BURST, SPI_CONTINUE);
  delayMicroseconds(75);
  p->motion =  SPI.transfer(cs_pin, 0xff, SPI_CONTINUE);
  p->dx =  SPI.transfer(cs_pin, 0xff, SPI_CONTINUE);
  p->dy =  SPI.transfer(cs_pin, 0xff, SPI_CONTINUE);
  p->squal =  SPI.transfer(cs_pin, 0xff, SPI_CONTINUE);
  p->shutter =  SPI.transfer(cs_pin, 0xff, SPI_CONTINUE)<<8;
  p->shutter |=  SPI.transfer(cs_pin, 0xff, SPI_CONTINUE);
  p->max_pix =  SPI.transfer(cs_pin, 0xff);
  delayMicroseconds(5);
}


void setup() {

  pinMode(SOLENOID_PIN,OUTPUT);
  digitalWrite(SOLENOID_PIN,LOW);

  SPI.begin(PIN_SS_1);
  SPI.begin(PIN_SS_2);
  SPI.setClockDivider(PIN_SS_1, SPI_CLOCK_DIV32);
  SPI.setClockDivider(PIN_SS_2, SPI_CLOCK_DIV32);

  SPI.beginTransaction(spi_settings);

  SERIAL.begin(115200);
 
  offset.x_1 = 0;
  offset.x_2 = 0;
  offset.y_1 = 0;
  offset.y_2 = 0;

  while(mousecam_init() == -1){
    //SERIAL.println("Mouse cam failed to init");
  }
 
}



void loop() {

  MD md;
  mousecam_read_motion(PIN_SS_1, &md);
  offset.x_1 += (int) (signed char) md.dx;
  offset.y_1 += (int) (signed char) md.dy;

  mousecam_read_motion(PIN_SS_2, &md);
  offset.x_2 += (int) (signed char) md.dx;
  offset.y_2 += (int) (signed char) md.dy;


  if(SERIAL.available()){
    byte a = SERIAL.read();
    if(a == 'h'){
      
      SERIAL.write((byte *) &offset, 16);
      offset.x_1 = 0;
      offset.y_1 = 0;
      offset.x_2 = 0;
      offset.y_2 = 0;
    }
  }

  

}
