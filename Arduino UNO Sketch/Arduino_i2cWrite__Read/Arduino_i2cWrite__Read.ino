#include <Wire.h>
int IRsensorAddress = 0xB0;
//int IRsensorAddress = 0x58;

const int led = 5;
String readString;

int slaveAddress;
int ledPin = 13;
boolean ledState = false;
byte data_buf[16];
int i;
int Ix[4];
int Iy[4];
int s;
byte test = 1023.5f;

void Write_2bytes(byte d1, byte d2)
  {
      Wire.beginTransmission(slaveAddress);
      Wire.write(d1); Wire.write(d2);
      Wire.endTransmission();
  }


void setup()
{
      slaveAddress = IRsensorAddress >> 1; // This results in 0x21 as the address to pass to TWI
      Serial.begin(115200);
      pinMode(ledPin, OUTPUT); // Set the LED pin as output
      pinMode(led, OUTPUT);
      digitalWrite(led, HIGH);
      Wire.begin();

      Wire.beginTransmission(slaveAddress);
      Wire.write(0x30); Wire.write(0x01);
      Wire.endTransmission(); delay(10);

      Wire.beginTransmission(slaveAddress);
      //Wire.write(0x00); Wire.write(0x00);Wire.write(0x00); Wire.write(0x00);Wire.write(0x00); Wire.write(0x00);Wire.write(0x00); Wire.write(0x09);
      Wire.write(0x00); Wire.write(0x00);Wire.write(0x00); Wire.write(0x00);Wire.write(0x00); Wire.write(0x00);Wire.write(0x00); Wire.write(0x90);
      Wire.endTransmission(); delay(10);

      Wire.beginTransmission(slaveAddress);
      Wire.write(0x07); Wire.write(0x00);Wire.write(0x41);
      //Wire.write(0x07); Wire.write(0x00);Wire.write(0x41);
      Wire.endTransmission(); delay(10);

      Wire.beginTransmission(slaveAddress);
      Wire.write(0x1A); Wire.write(0x40);Wire.write(0x00);
      Wire.endTransmission(); delay(10);

      Wire.beginTransmission(slaveAddress);
      Wire.write(0x33); Wire.write(0x03);
      Wire.endTransmission(); delay(10);

      Wire.beginTransmission(slaveAddress);
      Wire.write(0x30); Wire.write(0x08);
      Wire.endTransmission(); delay(10);
      delay(100);
      /* IR sensor initialize
      Write_2bytes(0x30,0x01); delay(10);
      Write_2bytes(0x30,0x08); delay(10);
      Write_2bytes(0x06,0x90); delay(10);
      Write_2bytes(0x08,0xC0); delay(10);
      Write_2bytes(0x1A,0x40); delay(10);
      Write_2bytes(0x33,0x33); delay(10);
      delay(100);
      */
}    
void loop()
    {
        ledState = !ledState;
        if (ledState) { digitalWrite(ledPin,HIGH); } else { digitalWrite(ledPin,LOW); }

        //IR sensor read
        Wire.beginTransmission(slaveAddress);
        Wire.write(0x36);
        Wire.endTransmission();
        Wire.requestFrom(slaveAddress, 16); // Request the 2 byte heading (MSB comes first)

        for (i=0;i<16;i++) { data_buf[i]=0; }
        i=0;

        while(Wire.available() && i < 16) {
            data_buf[i] = Wire.read();
            i++;
        }

        Ix[0] = data_buf[1];
        Iy[0] = data_buf[2];
        s = data_buf[3];
        //Ix[0] += (s & 0x30) <<4;
        Ix[0] = data_buf[1] | ((data_buf[3] >> 4) & 0x03) << 8;
        Iy[0] = data_buf[2] | ((data_buf[3] >> 6) & 0x03) << 8;
        //Ix[0] = Ix[0] / test;

        Ix[1] = data_buf[4];
        Iy[1] = data_buf[5];
        s = data_buf[6];
        Ix[1] += (s & 0x30) <<4;
        Iy[1] += (s & 0xC0) <<2;

        Ix[2] = data_buf[7];
        Iy[2] = data_buf[8];
        s = data_buf[9];
        Ix[2] += (s & 0x30) <<4;
        Iy[2] += (s & 0xC0) <<2;

        Ix[3] = data_buf[10];
        Iy[3] = data_buf[11];
        s = data_buf[12];
        Ix[3] += (s & 0x30) <<4;
        Iy[3] += (s & 0xC0) <<2;

        for(i=0; i<4; i++)
        {
            if (Ix[i] < 1000)
                Serial.print("");
            if (Ix[i] < 100)
                Serial.print("");
            if (Ix[i] < 10)
                Serial.print("");
            Serial.print( int(Ix[i]) );
            Serial.print(",");
            if (Iy[i] < 1000)
                Serial.print("");
            if (Iy[i] < 100)
                Serial.print("");
            if (Iy[i] < 10)
                Serial.print("");
            Serial.print( int(Iy[i]) );
            if (i<3)
                Serial.print(",");
        }

        //Serial.print(",");
        Serial.println("");
        delay(15);


        while (Serial.available()) {
    delay(3);  
    char c = Serial.read();
    readString += c; 
  }
  readString.trim();
  if (readString.length() >0) {
    if (readString == "on"){
      //Serial.println("switching on");
      digitalWrite(led, HIGH);
    }
    if (readString == "off")
    {
      //Serial.println("switching off");
      digitalWrite(led, LOW);
    }

    readString="";
  
  }
    }





        
    
