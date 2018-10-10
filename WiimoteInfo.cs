using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
//using WiimoteLib;
//using ArduinoIR;
using SerialPortListener;
using SerialPortListener.Serial;
using WiiWandz.Spells;
using WiiWandz.Strokes;
using System.Collections.Generic;
using System.Text;
using WiiWandz.Positions;
using WiiWandz.ini;
using System.IO;
using System.Reflection;
using System.Speech.Recognition;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace WiiWandz
{
    public partial class WiimoteInfo : UserControl
    {



        static bool _continue;
        static SerialPort _serialPort;


        public static SerialPortManager _spManager;

#if MSRS
	using Microsoft.Dss.Core.Attributes;
#else
        sealed class DataContract : Attribute
        {
        }

        sealed class DataMember : Attribute
        {
        }
#endif
        // current state of controller
        private readonly IRComState mComState = new IRComState();

        /// <summary>
        /// Point structure for floating point 2D positions (X, Y)
        /// </summary>
        [Serializable]
        [DataContract]
        public struct PointF
        {
            /// <summary>
            /// X, Y coordinates of this point
            /// </summary>
            [DataMember]
            public float X, Y;

            /// <summary>
            /// Convert to human-readable string
            /// </summary>
            /// <returns>A string that represents the point</returns>
            public override string ToString()
            {
                return string.Format("{{X={0}, Y={1}}}", X, Y);
            }

        }

        /// <summary>
        /// Point structure for int 2D positions (X, Y)
        /// </summary>
        [Serializable]
        [DataContract]
        public struct Point
        {
            /// <summary>
            /// X, Y coordinates of this point
            /// </summary>
            [DataMember]
            public int X, Y;

            /// <summary>
            /// Convert to human-readable string
            /// </summary>
            /// <returns>A string that represents the point.</returns>
            public override string ToString()
            {
                return string.Format("{{X={0}, Y={1}}}", X, Y);
            }
        }

        /// <summary>
        /// Point structure for floating point 3D positions (X, Y, Z)
        /// </summary>
        [Serializable]
        [DataContract]
        public struct Point3F
        {
            /// <summary>
            /// X, Y, Z coordinates of this point
            /// </summary>
            [DataMember]
            public float X, Y, Z;

            /// <summary>
            /// Convert to human-readable string
            /// </summary>
            /// <returns>A string that represents the point</returns>
            public override string ToString()
            {
                return string.Format("{{X={0}, Y={1}, Z={2}}}", X, Y, Z);
            }

        }

        [Serializable]
        [DataContract]
        public struct IRSensorCOM
        {
            /// <summary>
            /// Raw values of individual sensor.  Values range between 0 - 1023 on the X axis and 0 - 767 on the Y axis.
            /// </summary>
            [DataMember]
            public Point RawPosition;
            /// <summary>
            /// Normalized values of the sensor position.  Values range between 0.0 - 1.0.
            /// </summary>
            [DataMember]
            public PointF Position;
            /// <summary>
            /// Size of IR Sensor.  Values range from 0 - 15
            /// </summary>
            [DataMember]
            public int Size;
            /// <summary>
            /// IR sensor seen
            /// </summary>
            [DataMember]
            public bool Found;
            /// <summary>
            /// Convert to human-readable string
            /// </summary>
            /// <returns>A string that represents the point.</returns>
            public override string ToString()
            {
                return string.Format("{{{0}, Size={1}, Found={2}}}", Position, Size, Found);
            }
        }

        /// <summary>
        /// Current state of the IR camera
        /// </summary>
        [Serializable]
        [DataContract]
        public struct IRStateCOM
        {
            /// <summary>
            /// Current mode of IR sensor data
            /// </summary>
            [DataMember]
            public IRMode Mode;
            /// <summary>
            /// Current state of IR sensors
            /// </summary>
            [DataMember]
            public IRSensorCOM[] IRSensors;
            /// <summary>
            /// Raw midpoint of IR sensors 1 and 2 only.  Values range between 0 - 1023, 0 - 767
            /// </summary>
            [DataMember]
            public Point RawMidpoint;
            /// <summary>
            /// Normalized midpoint of IR sensors 1 and 2 only.  Values range between 0.0 - 1.0
            /// </summary>
            [DataMember]
            public PointF Midpoint;
        }
        /// <summary>
        /// The mode of data reported for the IR sensor
        /// </summary>
        [DataContract]
        public enum IRMode : byte
        {
            /// <summary>
            /// IR sensor off
            /// </summary>
            Off = 0x00,
            /// <summary>
            /// Basic mode
            /// </summary>
            Basic = 0x01,   // 10 bytes
                            /// <summary>
                            /// Extended mode
                            /// </summary>
            Extended = 0x03,    // 12 bytes
                                /// <summary>
                                /// Full mode (unsupported)
                                /// </summary>
            Full = 0x05,    // 16 bytes * 2 (format unknown)
        }

        [Serializable]
        [DataContract]
        public class IRComState
        {
            [DataMember]
            public IRStateCOM IRStateCOM;


            public IRComState()
                {
                IRStateCOM.IRSensors = new IRSensorCOM[4];
            }
        }

        private void UserInitialization()
        {
            string IniCOMPort = GetIniValue("System", "ReadCOM", "");
            List<string> result = IniCOMPort.Split(',').ToList();
            

            _spManager = new SerialPortManager();
            SerialSettings mySerialSettings = _spManager.CurrentSerialSettings;
            serialSettingsBindingSource.DataSource = mySerialSettings;
            portNameComboBox.DataSource = mySerialSettings.PortNameCollection;
            baudRateComboBox.DataSource = mySerialSettings.BaudRateCollection;
            dataBitsComboBox.DataSource = mySerialSettings.DataBitsCollection;
            parityComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.Parity));
            stopBitsComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.StopBits));

            _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
            //this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _spManager.Dispose();
        }

        // Handles the "Start Listening"-buttom click event
        private void btnStart_Click(object sender, EventArgs e)
        {
            _spManager.StartListening();
        }

        // Handles the "Stop Listening"-buttom click event
        private void btnStop_Click(object sender, EventArgs e)
        {
            _spManager.StopListening();
        }

       


        void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {


            if (this.InvokeRequired)
            {
                // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.
                this.BeginInvoke(new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved), new object[] { sender, e });
                return;
            }

            int maxTextLength = 50; // maximum text length in text box
            if (tbData.TextLength > maxTextLength)
                tbData.Text = tbData.Text.Remove(0, tbData.TextLength - maxTextLength);

            // This application is connected to a Arduino sending ASCCI characters, so data is converted to text
            
            string RawStr = e.Data;
            string openStr = "";
            

            int lengthStr = RawStr.Split(',').Length;
            if (lengthStr == 8)
            {
                openStr = RawStr;
            }
            else
            {
                openStr = "1023,1023,1023,1023,1023,1023,1023,1023";
            }
                       
     
            string[] message = openStr.Split(',');
            

            

            int x = int.TryParse(message[0], out x) ? x : 1023;
            int y = int.TryParse(message[1], out y) ? y : 700;
            int xx = int.TryParse(message[2], out xx) ? xx : 1023;
            int yy = int.TryParse(message[3], out yy) ? yy : 700;
            int xxx = int.TryParse(message[4], out xxx) ? xxx : 1023;
            int yyy = int.TryParse(message[5], out yyy) ? yyy : 700;
            int xxxx = int.TryParse(message[4], out xxxx) ? xxxx : 1023;
            int yyyy = int.TryParse(message[5], out yyyy) ? yyyy : 700;

            tbData.AppendText(openStr);
            tbData.ScrollToCaret();




          

            mComState.IRStateCOM.IRSensors[0].RawPosition.X = x;
            mComState.IRStateCOM.IRSensors[0].RawPosition.Y = y;

           
                    mComState.IRStateCOM.IRSensors[1].RawPosition.X = xx;
                    mComState.IRStateCOM.IRSensors[1].RawPosition.Y = yy;

                    mComState.IRStateCOM.IRSensors[2].RawPosition.X = xxx;
                    mComState.IRStateCOM.IRSensors[2].RawPosition.Y = yyy;

                    mComState.IRStateCOM.IRSensors[3].RawPosition.X = xxxx;
                    mComState.IRStateCOM.IRSensors[3].RawPosition.Y = yyyy;
              
                    mComState.IRStateCOM.IRSensors[0].Size = 0x00;
                    mComState.IRStateCOM.IRSensors[1].Size = 0x00;
                    mComState.IRStateCOM.IRSensors[2].Size = 0x00;
                    mComState.IRStateCOM.IRSensors[3].Size = 0x00;

                    mComState.IRStateCOM.IRSensors[0].Found = !(x == 1023 & y == 1023);
                    mComState.IRStateCOM.IRSensors[1].Found = !(xx == 1023 & yy == 1023);
                    mComState.IRStateCOM.IRSensors[2].Found = !(xxx == 1023 & yyy == 1023);
                    mComState.IRStateCOM.IRSensors[3].Found = !(xxxx == 1023 & yyyy == 1023);
                  

            mComState.IRStateCOM.IRSensors[0].Position.X = (float)(mComState.IRStateCOM.IRSensors[0].RawPosition.X / 1023.5f);
            mComState.IRStateCOM.IRSensors[1].Position.X = (float)(mComState.IRStateCOM.IRSensors[1].RawPosition.X / 1023.5f);
            mComState.IRStateCOM.IRSensors[2].Position.X = (float)(mComState.IRStateCOM.IRSensors[2].RawPosition.X / 1023.5f);
            mComState.IRStateCOM.IRSensors[3].Position.X = (float)(mComState.IRStateCOM.IRSensors[3].RawPosition.X / 1023.5f);

            mComState.IRStateCOM.IRSensors[0].Position.Y = (float)(mComState.IRStateCOM.IRSensors[0].RawPosition.Y / 767.5f);
            mComState.IRStateCOM.IRSensors[1].Position.Y = (float)(mComState.IRStateCOM.IRSensors[1].RawPosition.Y / 767.5f);
            mComState.IRStateCOM.IRSensors[2].Position.Y = (float)(mComState.IRStateCOM.IRSensors[2].RawPosition.Y / 767.5f);
            mComState.IRStateCOM.IRSensors[3].Position.Y = (float)(mComState.IRStateCOM.IRSensors[3].RawPosition.Y / 767.5f);

            if (mComState.IRStateCOM.IRSensors[0].Found && mComState.IRStateCOM.IRSensors[1].Found)
            {
                mComState.IRStateCOM.RawMidpoint.X = (mComState.IRStateCOM.IRSensors[1].RawPosition.X + mComState.IRStateCOM.IRSensors[0].RawPosition.X) / 2;
                mComState.IRStateCOM.RawMidpoint.Y = (mComState.IRStateCOM.IRSensors[1].RawPosition.Y + mComState.IRStateCOM.IRSensors[0].RawPosition.Y) / 2;

                mComState.IRStateCOM.Midpoint.X = (mComState.IRStateCOM.IRSensors[1].Position.X + mComState.IRStateCOM.IRSensors[0].Position.X) / 2.0f;
                mComState.IRStateCOM.Midpoint.Y = (mComState.IRStateCOM.IRSensors[1].Position.Y + mComState.IRStateCOM.IRSensors[0].Position.Y) / 2.0f;
            }
            else
                mComState.IRStateCOM.Midpoint.X = mComState.IRStateCOM.Midpoint.Y = 0.0f;





            irGraphics.Clear(Color.Black);
            strokesGraphics.Clear(Color.Black);

            UpdateIR(mComState.IRStateCOM.IRSensors[0], lblIR1, lblIR1Raw, chkFound1, Color.Red);
            UpdateIR(mComState.IRStateCOM.IRSensors[1], lblIR2, lblIR2Raw, chkFound2, Color.Blue);
            UpdateIR(mComState.IRStateCOM.IRSensors[2], lblIR3, lblIR3Raw, chkFound3, Color.Yellow);
            UpdateIR(mComState.IRStateCOM.IRSensors[3], lblIR4, lblIR4Raw, chkFound4, Color.Orange);

            

            if (mComState.IRStateCOM.IRSensors[0].Found && mComState.IRStateCOM.IRSensors[1].Found)
            {
                irGraphics.DrawEllipse(new Pen(Color.Green), (int)(mComState.IRStateCOM.RawMidpoint.X / 4), (int)(mComState.IRStateCOM.RawMidpoint.Y / 4), 2, 2);

            }

            if (GetIniValue("System", "speachRec", "") == "true")
            {
                if (!mComState.IRStateCOM.IRSensors[0].Found)
                {
                    if (recEngineRunning)
                    {
                        recEngineRunning = false;
                        recEngine.RecognizeAsyncStop();

                        voiceActiveChBx.Checked = false;
                    }
                }

                if (mComState.IRStateCOM.IRSensors[0].Found)
                {

                    if (!recEngineRunning)
                    {
                        recEngineRunning = true;
                        recEngine.RecognizeAsync(RecognizeMode.Multiple);

                        voiceActiveChBx.Checked = true;
                    }



                }
            }

            for (int i = 0; i < 4; i++)
                    {
                if (mComState.IRStateCOM.IRSensors[i].Found)
                {
                    
                   
                    trigger = wandTracker.addPosition(mComState.IRStateCOM.IRSensors[i].RawPosition, DateTime.Now);
                    break;


                }
            }

            if (trigger != null) // && trigger.casting())
            {
                if (trigger.getConfidence() > maxConfidence)
                {
                    maxConfidence = trigger.getConfidence();
                }
                if (trigger.getConfidence() < minConfidence)
                {
                    minConfidence = trigger.getConfidence();
                }
                lblSpellName.Text = trigger.GetType().Name + " - " + DateTime.Now.Subtract(wandTracker.startSpell).Seconds + " seconds";
                lblMaxConfidence.Text = maxConfidence.ToString();
                lblMinConfidence.Text = minConfidence.ToString();
                lblCurrentConfidence.Text = trigger.getConfidence().ToString();



            }
            else
            {
                lblSpellName.Text = "No spell";



            }

            Position previous = null;
            foreach (Position p in wandTracker.positions)
            {
                if (previous == null)
                {
                    previous = p;
                    continue;
                }
                System.Drawing.Point pointA = new System.Drawing.Point();
                System.Drawing.Point pointB = new System.Drawing.Point();
                pointA.X = (PositionStatistics.MAX_X - previous.point.X) / 4;
                pointA.Y = (PositionStatistics.MAX_Y - previous.point.Y) / 4;
                pointB.X = (PositionStatistics.MAX_X - p.point.X) / 4;
                pointB.Y = (PositionStatistics.MAX_Y - p.point.Y) / 4;

                strokesGraphics.DrawLine(new Pen(Color.Yellow), pointA, pointB);



                previous = p;
            }





            if (wandTracker.strokes != null)
            {
                foreach (Stroke stroke in wandTracker.strokes)
                {

                }

            }

            pbIR.Image = irBitmap;
            pbStrokes.Image = strokesBitmap;








        }


        public void SendCommand()
        {
            _spManager.StopListening();
            //_serialPort.Open();
            //_serialPort.Write(C);

        }



        private Bitmap irBitmap = new Bitmap(256, 192, PixelFormat.Format24bppRgb);
        private Bitmap strokesBitmap = new Bitmap(256, 192, PixelFormat.Format24bppRgb);
        private Graphics irGraphics;
        private Graphics strokesGraphics;
        //private Wiimote mWiimote;
        private WandTracker wandTracker;
        private Spell trigger;

        private double maxConfidence;
        private double minConfidence;

        //Speech serial command strings
        string secretKey;
        string eventName;
        readonly string TriggerWord1;
        readonly string TriggerWord2;
        readonly string Word1SerialCommand;
        readonly string Word2SerialCommand;

        bool recEngineRunning = false;
        
        //Speech recognition enginge setup
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();

       


        public WiimoteInfo()
        {
            InitializeComponent();

            UserInitialization();
            


            irGraphics = Graphics.FromImage(irBitmap);
            strokesGraphics = Graphics.FromImage(strokesBitmap);
            wandTracker = new WandTracker();

            this.maxConfidence = 0.0;
            this.minConfidence = 1.0;

            string COMSend = GetIniValue("System", "SendToCOM", "");
            string Spell1 = GetIniValue("System", "Spell1", "");
            string Spell2 = GetIniValue("System", "Spell2", "");
            string Spell3 = GetIniValue("System", "Spell3", "");
            string Spell1Send = GetIniValue("System", "Spell1Send", "");
            string Spell2Send = GetIniValue("System", "Spell2Send", "");
            string Spell3Send = GetIniValue("System", "Spell3Send", "");

            iftttUserKey.Text = COMSend;

            iftttEvent1.Text = Spell1Send;
            iftttEvent2.Text = Spell2Send;
            iftttEvent3.Text = Spell3Send;

            spellBox1.Text = Spell1;
            spellBox2.Text = Spell2;
            spellBox3.Text = Spell3;


            if (GetIniValue("System", "speachRec", "") == "true")
            {
                Choices commands = new Choices();
                commands.Add(new string[] { "Lumos", "knocks" });
                GrammarBuilder gBuilder = new GrammarBuilder();
                gBuilder.Append(commands);
                Grammar grammar = new Grammar(gBuilder);

                recEngine.LoadGrammarAsync(grammar);
                recEngine.SetInputToDefaultAudioDevice();
                recEngine.SpeechRecognized += recEngine_SpeechRecognized;

            }

            secretKey = COMSend;
            eventName = Spell1Send;

            TriggerWord1 = GetIniValue("System", "TriggerWord1", "");
            TriggerWord2 = GetIniValue("System", "TriggerWord2", "");
            Word1SerialCommand = GetIniValue("System", "Word1SerialCommand", "");
            Word2SerialCommand = GetIniValue("System", "Word2SerialCommand", "");

            LumosSpeachText.Text = Word1SerialCommand;
            NoxSpeachText.Text = Word2SerialCommand;

            _spManager.StartListening();

            

        }

       

        public static string GetIniValue(string MainSection, string key, string defaultValue)
        {
            IniFile inif = new IniFile(AppDataPath() + @"\config.ini");
            string value = "";

            value = (inif.IniReadValue(MainSection, key, defaultValue));
            return value;
        }

        public static string AppDataPath()
        {
            String gCommonAppDataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); ; // your config file location path
            return gCommonAppDataPath;
        }
        
        public void UpdateWiimote()
        {
            

            //pbBattery.Value = (ws.Battery > 0xc8 ? 0xc8 : (int)ws.Battery);
            //lblBattery.Text = ws.Battery.ToString();
            //lblDevicePath.Text = "Device Path: " + mWiimote.HIDDevicePath;

        }

        //private void UpdateIR()
        //{

        //}
         
        private void UpdateIR(IRSensorCOM irSensor, Label lblNorm, Label lblRaw, CheckBox chkFound, Color color)
        {
            chkFound.Checked = irSensor.Found;
            
               

            if (irSensor.Found)
            {
                lblNorm.Text = irSensor.Position.ToString() + ", " + irSensor.Size;
                lblRaw.Text = irSensor.RawPosition.ToString();
                irGraphics.DrawEllipse(new Pen(color), (int)(irSensor.RawPosition.X / 4), (int)(irSensor.RawPosition.Y / 4),
                             irSensor.Size + 1, irSensor.Size + 1);
            }
        }
        
        private void recEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            switch (e.Result.Text)
            {
                case "Lumos":
                    //SerialPort mySerialPort = new SerialPort(secretKey, 250000);
                    //MessageBox.Show("You said: Lumos");
                    //mySerialPort.Open();
                    //mySerialPort.Write(eventName);
                    //mySerialPort.Close();
                    _spManager.sendCommand(LumosSpeachText.Text);
                     break;

                case "knocks":
                    //  SerialPort mySerialPort1 = new SerialPort(secretKey, 115200);
                    //MessageBox.Show("You said: Nox");
                    // mySerialPort1.Open();
                    //mySerialPort1.Write(eventName1);
                    // mySerialPort1.Close();
                    _spManager.sendCommand(NoxSpeachText.Text);
                    break;

            }
        }
       
        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        
        private void spellBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            setSpells();
        }
       
        private void spellBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            setSpells();
        }


        private void setSpells()
        {
            List<String> spells = new List<string>();
            List<int> durations = new List<int>();
            List<int> voltages = new List<int>();
            List<String> events = new List<String>();


            if (!String.IsNullOrEmpty(spellBox1.Text))
            {
                spells.Add(spellBox1.Text.Replace(" ", ""));
                
                events.Add(iftttEvent1.Text);
            }

            if (!String.IsNullOrEmpty(spellBox2.Text))
            {
                spells.Add(spellBox2.Text.Replace(" ", ""));
                
                events.Add(iftttEvent2.Text);
            }

            if (!String.IsNullOrEmpty(spellBox3.Text))
            {
                spells.Add(spellBox3.Text.Replace(" ", ""));
                
                events.Add(iftttEvent3.Text);
            }

           

            wandTracker.setSpells(spells, durations, voltages, events);
        }
       
        private void spellBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            setSpells();
        }
        
        private void recordButton_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(spellBox1.Text.Replace(" ", ""));
            sb.Append(";");
            foreach (Position p in wandTracker.positions)
            {
                sb.Append(p.point.X);
                sb.Append(",");
                sb.Append(p.point.Y);
                sb.Append(";");
            }
            sb.Append("\n");

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Public\wiiwands-test-data.txt", true))
            {
                file.WriteLine(sb.ToString());
            }

        }

        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            TrainingDataForm f2 = new TrainingDataForm();
            f2.ShowDialog();
        }
        
        private void WiimoteInfo_Load(object sender, SpeechRecognizedEventArgs e)
        {
            
        }

        private void iftttEvent1_TextChanged(object sender, EventArgs e)
        {
            setSpells();
        }

        private void iftttEvent2_TextChanged(object sender, EventArgs e)
        {
            setSpells();
        }

        private void iftttEvent3_TextChanged(object sender, EventArgs e)
        {
            setSpells();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            wandTracker.setDeviceInfo("", "", iftttUserKey.Text);
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {
            
        }

        private void LumosSpeachText_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void NoxSpeachText_TextChanged(object sender, EventArgs e)
        {
            
        }

    }
}
