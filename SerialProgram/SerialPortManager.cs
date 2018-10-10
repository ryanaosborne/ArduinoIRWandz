using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Reflection;
using System.ComponentModel;
using System.Threading;
using System.IO;
using WiiWandz.ini;
using System.Windows.Forms;

namespace SerialPortListener.Serial
{
    /// <summary>
    /// Manager for serial port data
    /// </summary>
    public class SerialPortManager : IDisposable
    {

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

        public SerialPortManager()
        {
            // Finding installed serial ports on hardware

            string IniCOMPort = GetIniValue("System", "ReadCOM", "");

            _currentSerialSettings.PortNameCollection = SerialPort.GetPortNames();
            _currentSerialSettings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_currentSerialSettings_PropertyChanged);

            
            //Search open COM ports for value specified in INI file
            var results = Array.FindIndex(_currentSerialSettings.PortNameCollection, s => s.Equals(IniCOMPort));
            if (results >= 0 && _currentSerialSettings.PortNameCollection.Length > 0)
            {
                _currentSerialSettings.PortName = _currentSerialSettings.PortNameCollection[results];
            }
            // If INI COM port is not found or not specified, we select the first found
            else if (_currentSerialSettings.PortNameCollection.Length > 0)
            {
                MessageBox.Show("Cound not find open COM Port from INI specified COM port. Setting COM port to first open COM port available");
                _currentSerialSettings.PortName = _currentSerialSettings.PortNameCollection[0]; ;
            }

            //if (_currentSerialSettings.PortNameCollection.Length > 0)
                //_currentSerialSettings.PortName = _currentSerialSettings.PortNameCollection[0];
        }


        ~SerialPortManager()
        {
            Dispose(false);
        }


        #region Fields
        private SerialPort _serialPort;
        private SerialSettings _currentSerialSettings = new SerialSettings();
        private string _latestRecieved = String.Empty;
        public event EventHandler<SerialDataEventArgs> NewSerialDataRecieved;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current serial port settings
        /// </summary>
        public SerialSettings CurrentSerialSettings
        {
            get { return _currentSerialSettings; }
            set { _currentSerialSettings = value; }
        }

        #endregion

        #region Event handlers

        void _currentSerialSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // if serial port is changed, a new baud query is issued
            if (e.PropertyName.Equals("PortName"))
                UpdateBaudRateCollection();
        }


        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            /*
            int dataLength = _serialPort.BytesToRead;
            byte[] data = new byte[dataLength];
            int nbrDataRead = _serialPort.Read(data, 0, dataLength);
            if (nbrDataRead == 0)
                return;
                */
            string data = _serialPort.ReadLine();

                      

            // Send data to whom ever is interested
            if (NewSerialDataRecieved != null)
                NewSerialDataRecieved(this, new SerialDataEventArgs(data));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connects to a serial port defined through the current settings
        /// </summary>
        public void StartListening()
        {
            
                // Closing serial port if it is open
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();

            // Setting serial port settings
            _serialPort = new SerialPort(
                _currentSerialSettings.PortName,
                _currentSerialSettings.BaudRate,
                _currentSerialSettings.Parity,
                _currentSerialSettings.DataBits,
                _currentSerialSettings.StopBits);


          

            // Subscribe to event and open serial port for data
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            _serialPort.Open();
        }

        public void sendCommand(string c)
        {
            _serialPort.Write(c);
        }

        /// <summary>
        /// Closes the serial port
        /// </summary>
        public void StopListening()
        {
            _serialPort.Close();
        }


        /// <summary>
        /// Retrieves the current selected device's COMMPROP structure, and extracts the dwSettableBaud property
        /// </summary>
        private void UpdateBaudRateCollection()
        {
            _serialPort = new SerialPort(_currentSerialSettings.PortName);
            _serialPort.Open();
            object p = _serialPort.BaseStream.GetType().GetField("commProp", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_serialPort.BaseStream);
            Int32 dwSettableBaud = (Int32)p.GetType().GetField("dwSettableBaud", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(p);

            _serialPort.Close();
            _currentSerialSettings.UpdateBaudRateCollection(dwSettableBaud);
        }

        // Call to release serial port
        public void Dispose()
        {
            Dispose(true);
        }

        // Part of basic design pattern for implementing Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            }
            // Releasing serial port (and other unmanaged objects)
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                _serialPort.Dispose();
            }
        }


        #endregion

    }

    /// <summary>
    /// EventArgs used to send bytes recieved on serial port
    /// </summary>
    /// 
    //CHANGED ALL BELOW!!!!!!!!!!!!!
    public class SerialDataEventArgs : EventArgs
    {
        public SerialDataEventArgs(string dataInByteArray)
        {
            Data = dataInByteArray;
        }

        /// <summary>
        /// Byte array containing data from serial port
        /// </summary>
        public string Data;
    }
}
