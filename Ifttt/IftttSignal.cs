using System;
using System.Net;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using SerialPortListener.Serial;
using WiiWandz;


namespace WiiWandz.Ifttt
{
	public class IftttSignal
	{
		public String secretKey;
        public String eventName;
        
       
        

        public IftttSignal (String secretKey, String eventName)
		{
			this.secretKey = secretKey;
            this.eventName = eventName;
            
        }

        public void sendSignal()
        {
            
            WiimoteInfo._spManager.sendCommand(eventName);


            //SerialPort mySerialPort = new SerialPort(secretKey, 115200);

            //mySerialPort.Open();
            //mySerialPort.Write(eventName);
            //mySerialPort.Close();
            

            
        }

        
			}
		}

