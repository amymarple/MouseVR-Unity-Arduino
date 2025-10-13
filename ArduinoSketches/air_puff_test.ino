/*
 * Simple Air Puff Test
 * 
 * Tests the air puff solenoid by cycling it on and off
 * at regular intervals for validation.
 * 
 * Hardware: Air puff solenoid connected to pin 7
 */

int solenoidPin = 7;  // This is the output pin on the Arduino

void setup() 
{
  pinMode(solenoidPin, OUTPUT);  // Sets that pin as an output
}

void loop() 
{
  digitalWrite(solenoidPin, HIGH);  // Switch Solenoid ON
  delay(250);                       // Wait 0.25 Second
  digitalWrite(solenoidPin, LOW);   // Switch Solenoid OFF
  delay(1000);                      // Wait 1 Second
}
// Recommended: 20-25 psi air pressure
