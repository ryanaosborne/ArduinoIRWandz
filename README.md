# ArduinoIRWandz
Bring the magic home: Using the Universal Interactive Harry Potter Wands with an I2C IR Sensor and Arduino UNO to control the things around you. 

This project was heavily inspired and produced thanks to the work already done by bigokro and his WiiWandz project which, as he describes 
it, is "A project for tracking infrared inputs from a Wiimote, and triggering a signal when specific movements are detected" and his 
original project can be found here: https://github.com/bigokro/wiiwandz

Building off of what he made, this new version was geared towards removing the latency issues from his oringinal project by getting rid of any wireless transmissions (bluetooth and IFTTT). I removed the bluetooth latency by introducing an i2c IR sensor (same one found in the WiiRemote) controlled through an Arduino UNO and passed the data to this program via serial port communication. That same Arduino UNO was used to remove the IFTTT latency by retransmitting our spell command back through the serial port. This method would allow you to use simple relays to create "magic" via the Arduino UNO pins.

I also added the ability to use voice controls for spell commands rather than rely solely on movement of the IR source. 

Parts needed:

Windows Computer with available USB port
Arduino UNO
I2C IR Tracking Camera: https://www.robotshop.com/en/ir-tracking-camera.html or https://www.dfrobot.com/product-1088.html
   (This camera is the same camera/sensor that is found in the Original/Non-generic WiiMotes. Some people have successfully removed them 
   from the Wiimote and gotten them to work over I2C. This could be a viable way of getting an IR Tracker but that is not, and will 
   not be explored for this project.)
IR light source: Make your own or purchase an array such as this one: http://a.co/d/1pGBUIg
Interactive Wand (or alternative IR source for testing IR sensor)

Instructions:

Setting up the Arduino:
Wire the I2C IR tracking camera up to the Arduino UNO as pictured here: 
https://raw.githubusercontent.com/DFRobot/DFRobotMediaWikiImage/master/Image/Positioning_ir_camera_connection_diagram_1500.jpg
Upload the "Arduino_i2cReader.ino" scetch to your Arduino UNO board

Running the program for the first time:
Open the WiiWandz.sln and compile the program
You will need to create a "Config.ini" file that is located within the same directory that the WiiWandz.exe file is executing from. You 
can edit the Config file to preload the COM port you want to communicate with and which spells you want to activate
    
This program uses a connected microphone to your computer to send on/off commands to the connected Arduino for the "Lumos" and "Nox"
commands. The microphone is required at boot of the program. This can be toggled on and off via the Config.ini file by setting the
"speachRec" line to "true" or "false".
 
