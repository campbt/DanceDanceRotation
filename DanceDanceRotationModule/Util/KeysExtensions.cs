using System.Diagnostics.CodeAnalysis;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;

namespace DanceDanceRotationModule.Util
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static class KeysExtensions
    {
        /**
         * Returns a condensed version of the hotkey, useful for overlaying on the notes themself
         */
        public static string NotesString(KeyBinding keyBinding)
        {
            var modifiers = NotesString(keyBinding.ModifierKeys);
            var primary = NotesString(keyBinding.PrimaryKey);

            return modifiers + primary;
        }

        public static string NotesString(ModifierKeys modifierKeys)
        {
            string retval = "";
            if (modifierKeys.HasFlag(ModifierKeys.Ctrl))
            {
                retval += "C+";
            }
            if (modifierKeys.HasFlag(ModifierKeys.Alt))
            {
                retval += "A+";
            }
            if (modifierKeys.HasFlag(ModifierKeys.Shift))
            {
                retval += "S+";
            }

            return retval;
        }

        public static string NotesString(Keys keys)
        {
            switch (keys)
            {
                case Keys.None:
                    return "";
                case Keys.Back:
                    return "Back";
                case Keys.Tab:
                    return "Tab";
                case Keys.Enter:
                    return "Enter";
                case Keys.CapsLock:
                    return "Caps";
                case Keys.Escape:
                    return "Escape";
                case Keys.Space:
                    return "Space";
                case Keys.PageUp:
                    return "PageUp";
                case Keys.PageDown:
                    return "PageDown";
                case Keys.End:
                    return "End";
                case Keys.Home:
                    return "Home";
                case Keys.Left:
                    return "Left";
                case Keys.Up:
                    return "Up";
                case Keys.Right:
                    return "Right";
                case Keys.Down:
                    return "Down";
                case Keys.Select:
                    return "Select";
                case Keys.Print:
                    return "Print";
                case Keys.Execute:
                    return "Execute";
                case Keys.PrintScreen:
                    return "PrintScreen";
                case Keys.Insert:
                    return "Insert";
                case Keys.Delete:
                    return "Delete";
                case Keys.Help:
                    return "Help";
                case Keys.D0:
                    return "0";
                case Keys.D1:
                    return "1";
                case Keys.D2:
                    return "2";
                case Keys.D3:
                    return "3";
                case Keys.D4:
                    return "4";
                case Keys.D5:
                    return "5";
                case Keys.D6:
                    return "6";
                case Keys.D7:
                    return "7";
                case Keys.D8:
                    return "8";
                case Keys.D9:
                    return "9";
                case Keys.A:
                    return "A";
                case Keys.B:
                    return "B";
                case Keys.C:
                    return "C";
                case Keys.D:
                    return "D";
                case Keys.E:
                    return "E";
                case Keys.F:
                    return "F";
                case Keys.G:
                    return "G";
                case Keys.H:
                    return "H";
                case Keys.I:
                    return "I";
                case Keys.J:
                    return "J";
                case Keys.K:
                    return "K";
                case Keys.L:
                    return "L";
                case Keys.M:
                    return "M";
                case Keys.N:
                    return "N";
                case Keys.O:
                    return "O";
                case Keys.P:
                    return "P";
                case Keys.Q:
                    return "Q";
                case Keys.R:
                    return "R";
                case Keys.S:
                    return "S";
                case Keys.T:
                    return "T";
                case Keys.U:
                    return "U";
                case Keys.V:
                    return "V";
                case Keys.W:
                    return "W";
                case Keys.X:
                    return "X";
                case Keys.Y:
                    return "Y";
                case Keys.Z:
                    return "Z";
                case Keys.LeftWindows:
                    return "LeftWindows";
                case Keys.RightWindows:
                    return "RightWindows";
                case Keys.Apps:
                    return "Apps";
                case Keys.Sleep:
                    return "Sleep";
                case Keys.NumPad0:
                    return "N0";
                case Keys.NumPad1:
                    return "N1";
                case Keys.NumPad2:
                    return "N2";
                case Keys.NumPad3:
                    return "N3";
                case Keys.NumPad4:
                    return "N4";
                case Keys.NumPad5:
                    return "N5";
                case Keys.NumPad6:
                    return "N6";
                case Keys.NumPad7:
                    return "N7";
                case Keys.NumPad8:
                    return "N8";
                case Keys.NumPad9:
                    return "N9";
                case Keys.Multiply:
                    return "*";
                case Keys.Add:
                    return "+";
                case Keys.Separator:
                    return "Separator";
                case Keys.Subtract:
                    return "-";
                case Keys.Decimal:
                    return ".";
                case Keys.Divide:
                    return "/";
                case Keys.F1:
                    return "F1";
                case Keys.F2:
                    return "F2";
                case Keys.F3:
                    return "F3";
                case Keys.F4:
                    return "F4";
                case Keys.F5:
                    return "F5";
                case Keys.F6:
                    return "F6";
                case Keys.F7:
                    return "F7";
                case Keys.F8:
                    return "F8";
                case Keys.F9:
                    return "F9";
                case Keys.F10:
                    return "F10";
                case Keys.F11:
                    return "F11";
                case Keys.F12:
                    return "F12";
                case Keys.F13:
                    return "F13";
                case Keys.F14:
                    return "F14";
                case Keys.F15:
                    return "F15";
                case Keys.F16:
                    return "F16";
                case Keys.F17:
                    return "F17";
                case Keys.F18:
                    return "F18";
                case Keys.F19:
                    return "F19";
                case Keys.F20:
                    return "F20";
                case Keys.F21:
                    return "F21";
                case Keys.F22:
                    return "F22";
                case Keys.F23:
                    return "F23";
                case Keys.F24:
                    return "F24";
                case Keys.NumLock:
                    return "NumLock";
                case Keys.Scroll:
                    return "Scroll";
                case Keys.LeftShift:
                    return "LeftShift";
                case Keys.RightShift:
                    return "RightShift";
                case Keys.LeftControl:
                    return "LeftControl";
                case Keys.RightControl:
                    return "RightControl";
                case Keys.LeftAlt:
                    return "LeftAlt";
                case Keys.RightAlt:
                    return "RightAlt";
                case Keys.BrowserBack:
                    return "BrowserBack";
                case Keys.BrowserForward:
                    return "BrowserForward";
                case Keys.BrowserRefresh:
                    return "BrowserRefresh";
                case Keys.BrowserStop:
                    return "BrowserStop";
                case Keys.BrowserSearch:
                    return "BrowserSearch";
                case Keys.BrowserFavorites:
                    return "BrowserFavorites";
                case Keys.BrowserHome:
                    return "BrowserHome";
                case Keys.VolumeMute:
                    return "VolumeMute";
                case Keys.VolumeDown:
                    return "VolumeDown";
                case Keys.VolumeUp:
                    return "VolumeUp";
                case Keys.MediaNextTrack:
                    return "MediaNextTrack";
                case Keys.MediaPreviousTrack:
                    return "MediaPreviousTrack";
                case Keys.MediaStop:
                    return "MediaStop";
                case Keys.MediaPlayPause:
                    return "MediaPlayPause";
                case Keys.LaunchMail:
                    return "LaunchMail";
                case Keys.SelectMedia:
                    return "SelectMedia";
                case Keys.LaunchApplication1:
                    return "LaunchApplication1";
                case Keys.LaunchApplication2:
                    return "LaunchApplication2";
                case Keys.OemSemicolon:
                    return ";";
                case Keys.OemPlus:
                    return "+";
                case Keys.OemComma:
                    return ",";
                case Keys.OemMinus:
                    return "-";
                case Keys.OemPeriod:
                    return ".";
                case Keys.OemQuestion:
                    return "?";
                case Keys.OemTilde:
                    return "~";
                case Keys.OemOpenBrackets:
                    return "{";
                case Keys.OemPipe:
                    return "|";
                case Keys.OemCloseBrackets:
                    return "}";
                case Keys.OemQuotes:
                    return "\"";
                case Keys.Oem8:
                    return "Oem8";
                case Keys.OemBackslash:
                    return "\\";
                case Keys.ProcessKey:
                    return "ProcessKey";
                case Keys.Attn:
                    return "Attn";
                case Keys.Crsel:
                    return "Crsel";
                case Keys.Exsel:
                    return "Exsel";
                case Keys.EraseEof:
                    return "EraseEof";
                case Keys.Play:
                    return "Play";
                case Keys.Zoom:
                    return "Zoom";
                case Keys.Pa1:
                    return "Pa1";
                case Keys.OemClear:
                    return "OemClear";
                case Keys.ChatPadGreen:
                    return "ChatPadGreen";
                case Keys.ChatPadOrange:
                    return "ChatPadOrange";
                case Keys.Pause:
                    return "Pause";
                case Keys.ImeConvert:
                    return "ImeConvert";
                case Keys.ImeNoConvert:
                    return "ImeNoConvert";
                case Keys.Kana:
                    return "Kana";
                case Keys.Kanji:
                    return "Kanji";
                case Keys.OemAuto:
                    return "OemAuto";
                case Keys.OemCopy:
                    return "OemCopy";
                case Keys.OemEnlW:
                    return "OemEnlW";
                default:
                    return "?";
            }
        }
    }
}