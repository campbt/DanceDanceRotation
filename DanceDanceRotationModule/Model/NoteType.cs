using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DanceDanceRotationModule.Model
{
    public enum NoteType
    {
        // Special
        Dodge,
        WeaponSwap,
        WeaponStow,
        // Weapon Skills
        Weapon1,
        Weapon2,
        Weapon3,
        Weapon4,
        Weapon5,
        // Healing/Utility/Elite
        Heal,
        Utility1,
        Utility2,
        Utility3,
        Elite,
        // Profession Skills
        Profession1,
        Profession2,
        Profession3,
        Profession4,
        Profession5,
        // Unknown is just used as a placeholder for development.
        // It will display a note on the 6th track that always fails
        Unknown
    }

    public static class NoteTypeExtensions
    {

        public static Keys DefaultHotkey(NoteType noteType)
        {
            switch (noteType)
            {
                case NoteType.Dodge:
                    return Keys.V;
                case NoteType.WeaponSwap:
                    return Keys.OemTilde;
                case NoteType.WeaponStow:
                    return Keys.None;
                case NoteType.Weapon1:
                    return Keys.D1;
                case NoteType.Weapon2:
                    return Keys.D2;
                case NoteType.Weapon3:
                    return Keys.D3;
                case NoteType.Weapon4:
                    return Keys.D4;
                case NoteType.Weapon5:
                    return Keys.D5;
                case NoteType.Heal:
                    return Keys.D6;
                case NoteType.Utility1:
                    return Keys.D7;
                case NoteType.Utility2:
                    return Keys.D8;
                case NoteType.Utility3:
                    return Keys.D9;
                case NoteType.Elite:
                    return Keys.D0;
                case NoteType.Profession1:
                    return Keys.F1;
                case NoteType.Profession2:
                    return Keys.F2;
                case NoteType.Profession3:
                    return Keys.F3;
                case NoteType.Profession4:
                    return Keys.F4;
                case NoteType.Profession5:
                    return Keys.F4;
                case NoteType.Unknown:
                    return Keys.None;
                default:
                    return Keys.None;
            }
        }

        public static string HotkeyDescription(NoteType noteType)
        {
            switch (noteType)
            {
                case NoteType.Dodge:
                    return "Dodge";
                case NoteType.WeaponSwap:
                    return "Swap Weapons";
                case NoteType.WeaponStow:
                    return "Stow Weapons";
                case NoteType.Weapon1:
                    return "Weapon Skill 1";
                case NoteType.Weapon2:
                    return "Weapon Skill 2";
                case NoteType.Weapon3:
                    return "Weapon Skill 3";
                case NoteType.Weapon4:
                    return "Weapon Skill 4";
                case NoteType.Weapon5:
                    return "Weapon Skill 5";
                case NoteType.Heal:
                    return "Healing Skill";
                case NoteType.Utility1:
                    return "Utility Skill 1";
                case NoteType.Utility2:
                    return "Utility Skill 2";
                case NoteType.Utility3:
                    return "Utility Skill 3";
                case NoteType.Elite:
                    return "Elite Skill";
                case NoteType.Profession1:
                    return "Profession Skill 1";
                case NoteType.Profession2:
                    return "Profession Skill 2";
                case NoteType.Profession3:
                    return "Profession Skill 3";
                case NoteType.Profession4:
                    return "Profession Skill 4";
                case NoteType.Profession5:
                    return "Profession Skill 5";
                case NoteType.Unknown:
                    return "Unknown";
                default:
                    return "";
            }
        }

        /**
         * Returns the lane this note type will be put in
         */
        public static int NoteLane(NoteType noteType)
        {
            switch (noteType)
            {
                case NoteType.Weapon1:
                case NoteType.Heal:
                case NoteType.Profession1:
                    return 0;
                case NoteType.Weapon2:
                case NoteType.Utility1:
                case NoteType.Profession2:
                    return 1;
                case NoteType.Weapon3:
                case NoteType.Utility2:
                case NoteType.Profession3:
                    return 2;
                case NoteType.Weapon4:
                case NoteType.Utility3:
                case NoteType.Profession4:
                    return 3;
                case NoteType.Weapon5:
                case NoteType.Elite:
                case NoteType.Profession5:
                    return 4;
                case NoteType.Dodge:
                case NoteType.WeaponSwap:
                case NoteType.WeaponStow:
                case NoteType.Unknown:
                    return 5;
                default:
                    return 0;
            }
        }

        public static Texture2D NoteImage(NoteType noteType)
        {
            switch (noteType)
            {
                case NoteType.WeaponSwap:
                case NoteType.Weapon1:
                case NoteType.Weapon2:
                case NoteType.Weapon3:
                case NoteType.Weapon4:
                case NoteType.Weapon5:
                    return Resources.Instance.DdrNoteRedTexture;
                case NoteType.Dodge:
                case NoteType.Heal:
                case NoteType.Utility1:
                case NoteType.Utility2:
                case NoteType.Utility3:
                case NoteType.Elite:
                    return Resources.Instance.DdrNotePurpleTexture;
                case NoteType.WeaponStow:
                case NoteType.Profession1:
                case NoteType.Profession2:
                case NoteType.Profession3:
                case NoteType.Profession4:
                case NoteType.Profession5:
                case NoteType.Unknown:
                    return Resources.Instance.DdrNoteGreenTexture;
                default:
                    return Resources.Instance.DdrNoteRedTexture;
            }

        }
    }
}