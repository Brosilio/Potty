using System;
using System.Collections.Generic;
using System.IO.Ports;

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
        }
    }
}
