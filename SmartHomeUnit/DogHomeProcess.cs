using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SMartHomeUnit
{
    /*
     * 
     * |ERR;Error NFR Timeout!|OK;1;5.00;5.50;24.12;0|
     */

    class DogHomeProcess
    {
        private MySmartHomeConfig conf;

        public double Temp { get; set; }
        public double Templimit { get; set; }
        public bool Heating { get; set; }

        private string ttyname;

        public DogHomeProcess(MySmartHomeConfig homeConf, string tty)
        {
            conf = homeConf;
            Templimit = 5.0;
            ttyname = tty;
        }

        public void CollectData()
        {
            // send and receive data to/from serial port
            SerialPort stty = new SerialPort(ttyname, 9600, Parity.None, 8, StopBits.One);

            try
            {
                setupPort(ttyname);
                stty.Open();
                stty.ReadTimeout = 750;
                cleanup(stty);
                for(int i = 0; i < 10; i++)
                {
                    byte[] arr;
                    myWrite("QUERY|", ttyname);
                    Thread.Sleep(100);
                    var rd = readDatagram(stty);
                    Console.WriteLine("    ... serial data: " + rd);
                    if (rd.ToUpper().StartsWith("OK;"))
                    {
                        //parse output from dockouse
                        // OK; 1; 5.00; 5.50; 24.12; 0 |
                        // status; sensor cout; on temp; off temp; temp; heating
                        var data = rd.Split(';');
                        Temp = double.Parse(data[4].Trim(), CultureInfo.InvariantCulture.NumberFormat);
                        Templimit = double.Parse(data[2].Trim(), CultureInfo.InvariantCulture.NumberFormat);
                        Heating = (data[5].Trim() == "1");

                        // if there is change of temperature - update it
                        if (Templimit != conf.dogontemp)
                        {
                            Console.WriteLine("    ... DogHouse new Temp ON/OFF");
                            for (int j = 0; j < 10; j++)
                            {
                                // SET;on;off;|
                                var str = (string.Format(CultureInfo.InvariantCulture.NumberFormat, "SET;{0};{1};|", conf.dogontemp, conf.dogontemp + 0.5));
                                myWrite(str, ttyname);
                                Thread.Sleep(100);
                                rd = readDatagram(stty);
                                Console.WriteLine("    ... serial data: " + rd);
                                if (rd.ToUpper().StartsWith("OK;"))
                                {
                                    break;
                                }
                                Thread.Sleep(100);
                            }
                        }

                        break;
                    }

                    Thread.Sleep(100);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("SERIAL PORT err: {0}\n", ex.Message);
            }
            finally
            {
                try
                {
                    stty.Close();                       // Close port
                }
                catch { }
            }
        }

        private string readDatagram(SerialPort stty)
        {
            var sb = new StringBuilder();

            try
            {
                for (;;)
                {
                    char c = (char)stty.ReadByte();
                    if (c == '|')
                    {
                        break;
                    }
                    sb.Append(c);
                }
            }
            catch
            {
                return "ERR;systimeout";
            }

            return sb.ToString();
        }

        private void cleanup(SerialPort stty)
        {
            Thread.Sleep(10);
            while (stty.BytesToRead > 0)
            {
                stty.ReadByte();
            }
        }

        private void myWrite(string text, string tty)
        {
            var info = new ProcessStartInfo();
            info.FileName = "/opt/smarthome/sp.sh";
            info.Arguments = "\"" + text + "\" " + tty;

            info.UseShellExecute = true;
            info.CreateNoWindow = true;

            info.RedirectStandardOutput = false;
            info.RedirectStandardError = false;

            var p = Process.Start(info);
            p.WaitForExit();
        }
        private void setupPort(string tty)
        {
            var info = new ProcessStartInfo();
            info.FileName = "stty";
            info.Arguments = "-F " + tty + " 9600 clocal cread cs8 -cstopb";

            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            var p = Process.Start(info);
            p.WaitForExit();
        }
    }
}
