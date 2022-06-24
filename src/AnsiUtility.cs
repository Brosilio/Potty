using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Potty
{
    internal static class AnsiUtility
    {
        internal const char NUL = (char)0;
        internal const char ESC = (char)0x1B;
        internal const char LBK = '[';

        internal static readonly Dictionary<ConsoleKeyInfo, char[]> AnsiSeqs = new Dictionary<ConsoleKeyInfo, char[]>()
        {
            // arrow keys, home, end
            { new ConsoleKeyInfo(NUL, ConsoleKey.UpArrow, false, false, false), new[] { ESC, LBK, 'A' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.DownArrow, false, false, false), new[] { ESC, LBK, 'B' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.RightArrow, false, false, false), new[] { ESC, LBK, 'C' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.LeftArrow, false, false, false), new[] { ESC, LBK, 'D' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.Home, false, false, false), new[] { ESC, LBK, 'H' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.End, false, false, false), new[] { ESC, LBK, 'F' } },

            // ctrl+arrow keys
            { new ConsoleKeyInfo(NUL, ConsoleKey.UpArrow, false, false, true), new[] { ESC, LBK, '1', ';', '5', 'A' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.DownArrow, false, false, true), new[] { ESC, LBK, '1', ';', '5', 'B' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.RightArrow, false, false, true), new[] { ESC, LBK, '1', ';', '5', 'C' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.LeftArrow, false, false, true), new[] { ESC, LBK, '1', ';', '5', 'D' } },

            // ctrl+space
            { new ConsoleKeyInfo(NUL, ConsoleKey.Spacebar, false, false, true), new[] { NUL } },

            // numpad
            { new ConsoleKeyInfo(NUL, ConsoleKey.Insert, false, false, false), new[] { ESC, LBK, '2', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.Delete, false, false, false), new[] { ESC, LBK, '3', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.PageUp, false, false, false), new[] { ESC, LBK, '5', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.PageDown, false, false, false), new[] { ESC, LBK, '6', '~' } },

            // fn keys
            { new ConsoleKeyInfo(NUL, ConsoleKey.F1, false, false, false), new[] { 'O', 'P' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F2, false, false, false), new[] { 'O', 'Q' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F3, false, false, false), new[] { 'O', 'R' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F4, false, false, false), new[] { 'O', 'S' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F5, false, false, false), new[] { ESC, LBK, '1', '5', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F6, false, false, false), new[] { ESC, LBK, '1', '7', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F7, false, false, false), new[] { ESC, LBK, '1', '8', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F8, false, false, false), new[] { ESC, LBK, '1', '9', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F9, false, false, false), new[] { ESC, LBK, '2', '0', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F10, false, false, false), new[] { ESC, LBK, '2', '1', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F11, false, false, false), new[] { ESC, LBK, '2', '3', '~' } },
            { new ConsoleKeyInfo(NUL, ConsoleKey.F12, false, false, false), new[] { ESC, LBK, '2', '4', '~' } },
        };


        internal static int GetAnsiSequence(char[] buffer, ConsoleKeyInfo key)
        {
            // check if the character is just a normal character
            if ((int)key.KeyChar != 0)
            {
                buffer[0] = key.KeyChar;
                return 1;
            }

            // if we have a sequence for this, use it
            if (AnsiSeqs.ContainsKey(key))
            {
                char[] seq = AnsiSeqs[key];

                Array.Copy(seq, buffer, seq.Length);
                return seq.Length;
            }

            return 0;
        }
    }
}
