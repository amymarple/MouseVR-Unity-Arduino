/*
 * Water Reward Serial Test
 * 
 * Tests the water reward solenoid by responding to serial commands.
 * Send 'h' via serial monitor to trigger 3 reward pulses.
 * 
 * Hardware: Water reward solenoid connected to pin 7
 */

int solenoidPin = 7;
int incomingByte;

void setup() {
  Serial.begin(9600);
  pinMode(solenoidPin, OUTPUT);
  digitalWrite(solenoidPin, LOW);
}

void loop() {
  if (Serial.available() > 0) {
    // Read the incoming byte:
    incomingByte = Serial.read();
    if (incomingByte == 'h') {
      // Deliver 3 water pulses
      for (int i = 0; i <= 2; i++) {
        digitalWrite(solenoidPin, LOW);
        delay(1000);
        digitalWrite(solenoidPin, HIGH);
        delay(20);
      }
      digitalWrite(solenoidPin, LOW);
    }
    if (incomingByte == 'l') { 
      digitalWrite(solenoidPin, LOW);
    }
  }
}
