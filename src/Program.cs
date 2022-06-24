using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Potty
{
    class Program : CliProgramBase
    {
        [CliArg(HelpText = "Show help text")]
        public bool Help = false;

        [CliArg(HelpText = "List available serial ports then quit")]
        public bool List = false;

        [CliArg(HelpText = "The port to open")]
        public string Port = null;

        [CliArg(HelpText = "The baud rate to use. Default: 9600")]
        public int Baud = 9600;

        [CliArg(HelpText = "The number of data bits. Default: 8")]
        public int Data = 8;

        [CliArg(HelpText = "The parity bits to use. Default: None")]
        public Parity Parity = Parity.None;

        [CliArg(HelpText = "The number of stop bits to use. Default: One")]
        public StopBits StopBits = StopBits.One;

        [CliArg(HelpText = "Enable ANSI control sequence support. Default: false")]
        public bool Ansi = false;

        [CliArg(HelpText = "Whether or not to echo keypresses locally. Default: false")]
        public bool Echo = false;

        [CliArg(HelpText = "Whether or not to emit CR and LF instead of just CR for the RETURN key. Default: false")]
        public bool Crlf = false;

        public Program(string[] args) : base(args)
        {
        }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments provided, use \"potty help\" for help");
                return 1;
            }

            // run
            Program p = new Program(args);
            if (p.ValidateCliArgs())
                p.CliMain();

            return 0;
        }

        protected override void CliMain()
        {
            // enable ANSI on windows if requested
            if (Ansi && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (!Utility.EnableAnsiOnWindows())
                {
                    Console.WriteLine("<POTTY>: INCOMING ANSI SUPPORT NOT AVAILABLE");
                }
            }

            // disable cursor
            Console.CursorVisible = false;

            // open serial port and do the stuff
            SerialPort sp = new SerialPort(Port, Baud, Parity, Data, StopBits);
            sp.Open();

            // reenable cursor
            Console.CursorVisible = true;

            // start reading and copying to stdout
            Task rTask = Task.Run(() =>
            {
                byte[] buffer = new byte[8192];
                int read = -1;
                Stream stdout = Console.OpenStandardOutput();

                while (read != 0 && sp.IsOpen)
                {
                    read = sp.Read(buffer, 0, buffer.Length);
                    stdout.Write(buffer, 0, read);
                    stdout.Flush();
                }
            });

            // read keys or characters from stdin
            Task wTask = Task.Run(() =>
            {
                char[] buffer = new char[16];
                ConsoleKeyInfo cki;
                int len;

                while (sp.IsOpen)
                {
                    len = 1;
                    cki = Console.ReadKey(true);

                    // set the character to send
                    buffer[0] = cki.KeyChar;

                    // overwrite the character with ansi sequence, if any
                    if (Ansi)
                        len = AnsiUtility.GetAnsiSequence(buffer, cki);

                    // append \n if requested
                    if (Crlf && cki.Key == ConsoleKey.Enter)
                        buffer[len++] = '\n';

                    // write out
                    sp.Write(buffer, 0, len);

                    // echo if requested
                    if (Echo)
                        Console.Write(buffer, 0, len);
                }
            });

            // go until something fails
            Task.WaitAny(rTask, wTask);
        }

        protected override bool ValidateCliArgs()
        {
            // print help and quit
            if (Help)
            {
                Console.WriteLine("Usage: potty arg=val ...\n");
                Console.WriteLine(GetCliUsageString());
                return false;
            }

            // list available serial ports and quit
            if (List)
            {
                foreach (string name in SerialPort.GetPortNames())
                    Console.WriteLine(name);

                return false;
            }

            // ensure data is within valid range
            if (Data < 5 || Data > 8)
                throw new ArgumentOutOfRangeException(nameof(Data), "Data must be 5, 6, 7, or 8");

            return true;
        }
    }
}
