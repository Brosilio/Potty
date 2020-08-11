using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Text;

namespace Coral
{
    class Program
    {
        static void Main(string[] rawArgs)
        {
            Dictionary<string, string> argDict = new Dictionary<string, string>();
            foreach (string str in rawArgs)
            {
                if (!string.IsNullOrWhiteSpace(str) && str.Contains('-'))
                {
                    string arg = str.TrimStart('-');
                    int end = arg.IndexOf("=");

                    if (end == -1)
                        argDict.Add(arg, null);
                    else
                        argDict.Add(arg.Substring(0, arg.IndexOf('=')), arg.Substring(arg.IndexOf('=') + 1));
                }
            }

            if (argDict.Count == 0 || argDict.ContainsKey("help") || argDict.ContainsKey("usage"))
            {
                PrintUsage();
                return;
            }

            if (argDict.ContainsKey("list"))
            {
                int index = 0;
                foreach (string s in SerialPort.GetPortNames())
                {
                    Console.WriteLine($"{index}: {s}");
                    index++;
                }

                return;
            }

            if (!argDict.ContainsKey("port"))
            {
                Console.WriteLine("No port specified. Use --help or --usage.");
                return;
            }


            int baudRate = 9600;
            StopBits stop = StopBits.One; // TODO
            int data = 8;
            Parity parity = Parity.None; // TODO

            if (argDict.ContainsKey("baud"))
            {
                if (!int.TryParse(argDict["baud"], out baudRate))
                {
                    Console.WriteLine("Invalid value specified for --baud");
                    return;
                }
            }

            if (argDict.ContainsKey("data"))
            {
                if (!int.TryParse(argDict["data"], out data))
                {
                    Console.WriteLine("Invalid value specified for --data");
                    return;
                }

                if (data < 5 || data > 8)
                {
                    Console.WriteLine("Invalid value specified for --data. Must be >=5 and <=8");
                    return;
                }
            }

            try
            {
                SerialPort sp = new SerialPort(argDict["port"], baudRate, parity, data, stop);
                try
                {
                    sp.Open();
                    if (!sp.IsOpen)
                    {
                        Console.WriteLine($"Could not open serial port ({argDict["port"]}).");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"Could not open serial port ({argDict["port"]}).");
                    return;
                }

                bool fuckOff = false;
                new Thread(() =>
                {
                    byte[] b = new byte[8192]; // TODO: acknowledge presence of --buffer argument
                    int read = 0;
                    do
                    {
                        sp.Read(b, 0, b.Length);
                        if (read == 0)
                        {
                            Console.WriteLine($"Connection to {argDict["port"]} closed.");
                            fuckOff = true;
                            Environment.Exit(1);
                            return;
                        }

                        // TODO: acknowledge presence of --log argument
                        Console.Write(Encoding.ASCII.GetString(b, 0, read));
                    } while (read != 0);
                }).Start();

                while (!fuckOff)
                {
                    string r = Console.ReadLine();
                    sp.WriteLine(r);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Error on ({argDict["port"]}).");
            }

            Environment.Exit(0);
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Coral - Usage");
            Console.WriteLine("    Coral --port=<port> --baud=<baud rate> --stop=<stop bits>");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("    --list: list available serial ports");
            Console.WriteLine("    --port: the name or index of the serial port to connect to");
            Console.WriteLine("    --baud: the baud rate to use (default 9600)");
            Console.WriteLine("    --data: the number of data bits [5|6|7|8] (default 8)");
            Console.WriteLine("    --stop: the number of stop bits [0|1|1.5|2] (default 0)");
            Console.WriteLine("    --parity: the parity (default none)");
            Console.WriteLine("    --buffer: the size of I/O buffers, in bytes (default 8192)");
            Console.WriteLine("    --log: a filename to log output to");
        }
    }
}
