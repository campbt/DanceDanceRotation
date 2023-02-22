using System;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DanceDanceRotationModule.Model
{
    public enum NoteType
    {
        Dodge,
        WeaponSwap,
        // Weapon Skills
        Weapon1,
        Weapon2,
        Weapon3,
        Weapon4,
        Weapon5,
        // Healing/Utility/Elite
        HealingSkill,
        UtilitySkill1,
        UtilitySkill2,
        UtilitySkill3,
        EliteSkill,
        // Profession Skills
        ProfessionSkill1,
        ProfessionSkill2,
        ProfessionSkill3,
        ProfessionSkill4,
        ProfessionSkill5,
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
                case NoteType.HealingSkill:
                    return Keys.D6;
                case NoteType.UtilitySkill1:
                    return Keys.D7;
                case NoteType.UtilitySkill2:
                    return Keys.D8;
                case NoteType.UtilitySkill3:
                    return Keys.D9;
                case NoteType.EliteSkill:
                    return Keys.D0;
                case NoteType.ProfessionSkill1:
                    return Keys.F1;
                case NoteType.ProfessionSkill2:
                    return Keys.F2;
                case NoteType.ProfessionSkill3:
                    return Keys.F3;
                case NoteType.ProfessionSkill4:
                    return Keys.F4;
                case NoteType.ProfessionSkill5:
                    return Keys.F4;
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
                case NoteType.HealingSkill:
                    return "Healing Skill";
                case NoteType.UtilitySkill1:
                    return "Utility Skill 1";
                case NoteType.UtilitySkill2:
                    return "Utility Skill 2";
                case NoteType.UtilitySkill3:
                    return "Utility Skill 3";
                case NoteType.EliteSkill:
                    return "Elite Skill";
                case NoteType.ProfessionSkill1:
                    return "Profession Skill 1";
                case NoteType.ProfessionSkill2:
                    return "Profession Skill 2";
                case NoteType.ProfessionSkill3:
                    return "Profession Skill 3";
                case NoteType.ProfessionSkill4:
                    return "Profession Skill 4";
                case NoteType.ProfessionSkill5:
                    return "Profession Skill 5";
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
                case NoteType.HealingSkill:
                case NoteType.ProfessionSkill1:
                    return 0;
                case NoteType.Weapon2:
                case NoteType.UtilitySkill1:
                case NoteType.ProfessionSkill2:
                    return 1;
                case NoteType.Weapon3:
                case NoteType.UtilitySkill2:
                case NoteType.ProfessionSkill3:
                    return 2;
                case NoteType.Weapon4:
                case NoteType.UtilitySkill3:
                case NoteType.ProfessionSkill4:
                    return 3;
                case NoteType.Weapon5:
                case NoteType.EliteSkill:
                case NoteType.ProfessionSkill5:
                    return 4;
                case NoteType.Dodge:
                case NoteType.WeaponSwap:
                    return 5;
                default:
                    return 0;
            }
        }

        public static Texture2D NoteImage(NoteType noteType)
        {
            switch (noteType)
            {
                case NoteType.Dodge:
                case NoteType.WeaponSwap:
                case NoteType.Weapon1:
                case NoteType.Weapon2:
                case NoteType.Weapon3:
                case NoteType.Weapon4:
                case NoteType.Weapon5:
                    return Resources.Instance.DdrNoteRedTexture;
                case NoteType.HealingSkill:
                case NoteType.UtilitySkill1:
                case NoteType.UtilitySkill2:
                case NoteType.UtilitySkill3:
                case NoteType.EliteSkill:
                    return Resources.Instance.DdrNotePurpleTexture;
                case NoteType.ProfessionSkill1:
                case NoteType.ProfessionSkill2:
                case NoteType.ProfessionSkill3:
                case NoteType.ProfessionSkill4:
                case NoteType.ProfessionSkill5:
                    return Resources.Instance.DdrNoteGreenTexture;
                default:
                    return Resources.Instance.DdrNoteRedTexture;
            }

        }
    }
}