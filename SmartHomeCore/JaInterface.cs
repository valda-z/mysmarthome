using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace JaLib
{
    public enum AlarmState
    {
        IDLE,
        ARMED,
        OUTCOMMINGDELAY,
        INCOMMINGDELAY,
        ALARM,
        DISCONNECTED
    }

    public enum ArmedZones
    {
        A,
        B,
        ABC,
        NONE
    }

    /// <summary>
    /// Event fired when state of Alarm system is changed.
    /// </summary>
    /// <param name="sender">this</param>
    /// <param name="state">New state of system</param>
    /// <param name="deviceId">In case of alarm - ID of device which triggers alarm</param>
    public delegate void AlarmStateChangedHandler(JaInterface sender, AlarmState state, byte deviceId);
    /// <summary>
    /// Event in case that any status data of Alarm keyboard is triggered.
    /// </summary>
    /// <param name="sender">this</param>
    public delegate void AlarmDataChangedHandler(JaInterface sender);

    public class JaInterface
    {
        /// <summary>
        /// Alarm system changes state.
        /// </summary>
        public event AlarmStateChangedHandler AlarmStateChanged;
        /// <summary>
        /// Alarm keyboard changes any data (needed for signalization to user)
        /// </summary>
        public event AlarmDataChangedHandler AlarmDataChanged;

        private bool _LED_A = false;
        private bool _LED_B = false;
        private bool _LED_C = false;
        private bool _LED_Warning = false;
        private bool _LED_Backlight = false;

        private DateTime _lastContact = new DateTime(2000, 1, 1);

        /// <summary>
        /// LED "A"
        /// </summary>
        public bool LED_A { get { return _LED_A; } }
        /// <summary>
        /// LED "B"
        /// </summary>
        public bool LED_B { get { return _LED_B; } }
        /// <summary>
        /// LED "C"
        /// </summary>
        public bool LED_C { get { return _LED_C; } }
        /// <summary>
        /// LED "Warning"
        /// </summary>
        public bool LED_Warning { get { return _LED_Warning; } }
        /// <summary>
        /// Backlight
        /// </summary>
        public bool LED_Backlight { get { return _LED_Backlight; } }

        public bool jablotronAlive
        {
            get
            {
                return (_lastContact > DateTime.Now.AddSeconds(-10));
            }
        }

        /// <summary>
        /// 0x00 - Idle
        /// 0x06 - Alarm
        /// 0x0C - Outcomming timeout
        /// 0x0D - Incomming timeout
        /// 0x10 - Active detector
        /// </summary>
        private byte messageId;

        private byte deviceId;
        private byte msgCRC;

        /// <summary>
        /// Device ID which triggers alarm (or in case of displaying status messages means second line ...)
        /// </summary>
        public byte DeviceId { get { return deviceId; } }
        /// <summary>
        /// ID of message on first line of keyboard
        /// 
        /// 0x00 - Idle
        /// 0x06 - Alarm
        /// 0x0C - Outcomming timeout
        /// 0x0D - Incomming timeout
        /// 0x10 - Active detector
        /// </summary>
        public byte MessageId { get { return messageId; } }

        private bool _isOpen = false;
        private SerialDevice port;

        private bool _protoIsED = false;

        private List<byte> data = new List<byte>();
        private List<byte> cmd = new List<byte>();

        private AlarmState _state = AlarmState.IDLE;
        private ArmedZones _zones = ArmedZones.NONE;

        /// <summary>
        /// Get current AlarmState
        /// </summary>
        public AlarmState State
        {
            get
            {
                if (jablotronAlive)
                {
                    return _state;
                }
                else
                {
                    return AlarmState.DISCONNECTED;
                }
            }
        }
        /// <summary>
        /// Get current armed zone (A, B, ABC, NONE)
        /// </summary>
        public ArmedZones Zones { get { return _zones; } }

        /// <summary>
        /// Returns if serial COM port communication is in open state
        /// </summary>
        public bool IsOpen { get { return _isOpen; } }

        private string _PIN;

        /// <summary>
        /// Set PIN which is used for Arming and Disarming - any valid PIN from your alarm system.
        /// </summary>
        public string PIN
        {
            get { return _PIN; }
            set
            {
                //TODO: some format check needed!
                _PIN = value;
            }
        }

        public JaInterface()
        {

        }

        /// <summary>
        /// Change armed status (equivalent to entering valid PIN on keyboard)
        /// </summary>
        public void ChangeArmedStatus()
        {
            cmd.Clear();
            //format PIN and put to buffer
            for(int i = 0; i < _PIN.Length; i++)
            {
                byte x = Convert.ToByte(_PIN.Substring(i, 1), 16);
                x += 0x80;
                cmd.Add(x);
                if ((i + 1) < _PIN.Length)
                {
                    cmd.Add(0xA0);
                }
                else
                {
                    cmd.Add(0xA2);
                }
                cmd.Add(0xFF);
            }
        }

        /// <summary>
        /// Press "A" button on alarm keyboard
        /// </summary>
        public void SetArmA()
        {
            cmd.Clear();
            byte[] x = { 0x8F, 0xFF, 0xA0, 0xFF, 0x82, 0xFF, 0xA1, 0xFF };
            cmd.AddRange(x);
        }

        /// <summary>
        /// Press "B" button on alarm keyboard
        /// </summary>
        public void SetArmB()
        {
            cmd.Clear();
            byte[] x = { 0x8F, 0xFF, 0xA0, 0xFF, 0x83, 0xFF, 0xA1, 0xFF };
            cmd.AddRange(x);
        }

        /// <summary>
        /// Press "C" button on alarm keyboard
        /// </summary>
        public void SetArmABC()
        {
            cmd.Clear();
            byte[] x = { 0x8F, 0xFF, 0xA0, 0xFF, 0x81, 0xFF, 0xA1, 0xFF };
            cmd.AddRange(x);
        }

        /// <summary>
        /// Set Armed state by PIN
        /// </summary>
        public void SetArmed()
        {
            if(_state!= AlarmState.IDLE)
            {
                throw new JaExceptionAlreadyArmed();
            }
            ChangeArmedStatus();
        }

        /// <summary>
        /// Set Disarmed state (IDLE) by PIN
        /// </summary>
        public void SetDisArmed()
        {
            if (_state == AlarmState.IDLE)
            {
                throw new JaExceptionAlreadyDisArmed();
            }
            ChangeArmedStatus();
        }

        /// <summary>
        /// Open COM port and start working thread
        /// </summary>
        /// <param name="portName"></param>
        public void Open(string portName)
        {
            if (_isOpen)
            {
                port.Close();
            }

            port = new SerialDevice(portName, BaudRate.B9600);
            port.DataReceived += Port_DataReceived;
            port.Open();

            _isOpen = true;
        }

        private void processMsg(byte[] msg)
        {
            // send message (if there is any)
            if (cmd.Count > 0)
            {
                byte[] wr = cmd.ToArray();
                port.Write(wr);
                cmd.Clear();
            }

            // process message
            // ED 51 0C 00 09 04 00 00 73 FF
            if (msg.Length == 10)
            {
                //Console.WriteLine(">>>>> {0}", BitConverter.ToString(msg));

                _lastContact = DateTime.Now;

                //LEDs
                _LED_A = ((msg[4] & 0x08) == 0x08);
                _LED_B = ((msg[4] & 0x04) == 0x04);
                _LED_C = ((msg[4] & 0x02) == 0x02);
                _LED_Backlight = ((msg[4] & 0x01) == 0x01);
                _LED_Warning = ((msg[4] & 0x10) == 0x10);

                //messages
                messageId = msg[2];
                deviceId = msg[3];

                // exception - in case of alarm waiting for device ID
                AlarmState newState = AlarmState.IDLE;
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
                switch (msg[1] & 0x03)
                {
                    case 0x00:
                        _zones = ArmedZones.NONE;
                        break;
                    case 0x01:
                        _zones = ArmedZones.A;
                        break;
                    case 0x02:
                        _zones = ArmedZones.B;
                        break;
                    case 0x03:
                        _zones = ArmedZones.ABC;
                        break;
                }
                if ((msg[1] & 0x1F) == 0x00)
                {
                    newState = AlarmState.IDLE;
                }
                else
                {
                    if ((msg[1] & 0x04) == 0x04)
                    {
                        newState = AlarmState.ALARM;
                    }else if ((msg[1] & 0x08) == 0x08)
                    {
                        newState = AlarmState.INCOMMINGDELAY;
                    }
                    else if ((msg[1] & 0x10) == 0x10)
                    {
                        newState = AlarmState.OUTCOMMINGDELAY;
                    }
                    else
                    {
                        newState = AlarmState.ARMED;
                    }
                }

                if(newState == AlarmState.ALARM && deviceId == 0x00)
                {
                    //ignore this state ... waiting for alarm message with device ID
                }
                else
                {
                    if (_state != newState)
                    {
                        _state = newState;
                        //emmit event
                        AlarmStateChanged?.Invoke(this, _state, deviceId);
                    }
                }

                if (msgCRC != msg[8])
                {
                    msgCRC = msg[8];
                    //emmit event
                    AlarmDataChanged?.Invoke(this);
                }
            }
        }

        private void Port_DataReceived(object sender, byte[] rd)
        {
            foreach(byte b in rd)
            {
                // process bytes
                if (!_protoIsED)
                {
                    if (b == 0xED)
                    {
                        data.Clear();
                        data.Add(b);
                        _protoIsED = true;
                    }
                }
                else
                {
                    data.Add(b);
                    if (b == 0xFF)
                    {
                        // process message
                        processMsg(data.ToArray());
                        _protoIsED = false;
                    }
                }
            }
        }

        /// <summary>
        /// Close COM port communication
        /// </summary>
        public void Close()
        {
            if (_isOpen)
            {
                port.Close();
                port = null;
            }
            _isOpen = false;
        }
    }
}
