using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Potty
{
    public static class SafeConsole
    {
        /// <summary>
        /// Whether or not the cursor is visible.
        /// Does nothing if not supported on current platform, or if
        /// no console is available.
        /// </summary>
        public static bool CursorVisible
        {
            set
            {
                try
                {
                    Console.CursorVisible = value;
                } catch { }
            }

            get => Console.CursorVisible;
        }

        /// <summary>
        /// Read either the next key press or fill the buffer with data from stdin.
        /// </summary>
        /// <param name="buffer">The buffer to write data to.</param>
        /// <param name="processAnsi">Whether or not to generate ANSI sequences. Only applies when stdin is not redirected.</param>
        /// <param name="transformCrlf">Whether or not to convert \r to \r\n. Only applies when stdin is not redirected.</param>
        /// <returns>The number of characters written to the buffer, or zero if no more data is available from stdin.</returns>
        public static int ReadNext(char[] buffer, bool processAnsi, bool transformCrlf)
        {
            // read stdin if it's redirected
            if(Console.IsInputRedirected)
            {
                return Console.In.Read(buffer, 0, buffer.Length);
            }

            // stdin not redirected, we can use the console

            int len = 1;
            ConsoleKeyInfo cki = Console.ReadKey(true);
            buffer[0] = cki.KeyChar;

            // get ansi sequence if available
            if (processAnsi)
                len = AnsiUtility.GetAnsiSequence(buffer, cki);

            // transform \r to \r\n if requested
            if (transformCrlf)
                buffer[len++] = '\n';

            return len;
        }
    }
}
