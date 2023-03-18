using System;
using Color = Microsoft.Xna.Framework.Color;

namespace DanceDanceRotationModule.Model
{
    /**
     * Represents one of the professions in GW2
     * Note: Order is used in SongList
     */
    public enum Profession
    {
        Guardian,
        Revenant,
        Warrior,
        Engineer,
        Ranger,
        Thief,
        Elementalist,
        Mesmer,
        Necromancer,
        Unknown
    }

    public static class ProfessionExtensions
    {
        public static Profession ProfessionFromBuildTemplate(int buildTemplateCode)
        {
            switch (buildTemplateCode)
            {
                case 1:
                    return Profession.Guardian;
                case 2:
                    return Profession.Warrior;
                case 3:
                    return Profession.Engineer;
                case 4:
                    return Profession.Ranger;
                case 5:
                    return Profession.Thief;
                case 6:
                    return Profession.Elementalist;
                case 7:
                    return Profession.Mesmer;
                case 8:
                    return Profession.Necromancer;
                case 9:
                    return Profession.Revenant;
                default:
                    return Profession.Unknown;
            }
        }

        public static string GetProfessionDisplayText(Profession profession)
        {
            switch (profession)
            {
                case Profession.Guardian:
                    return "Guardian";
                case Profession.Warrior:
                    return "Warrior";
                case Profession.Engineer:
                    return "Engineer";
                case Profession.Ranger:
                    return "Ranger";
                case Profession.Thief:
                    return "Thief";
                case Profession.Elementalist:
                    return "Elementalist";
                case Profession.Mesmer:
                    return "Mesmer";
                case Profession.Necromancer:
                    return "Necromancer";
                case Profession.Revenant:
                    return "Revenant";
                case Profession.Unknown:
                default:
                    return "Unknown";
            }

        }

        public static Color GetProfessionColor(Profession profession)
        {
            string hexValue;
            switch (profession)
            {
                case Profession.Guardian:
                    hexValue = "#72C1D9";
                    break;
                case Profession.Warrior:
                    hexValue = "#FFD166";
                    break;
                case Profession.Engineer:
                    hexValue = "#D09C59";
                    break;
                case Profession.Ranger:
                    hexValue = "#8CDC82";
                    break;
                case Profession.Thief:
                    hexValue = "#C08F95";
                    break;
                case Profession.Elementalist:
                    hexValue = "#F68A87";
                    break;
                case Profession.Mesmer:
                    hexValue = "#B679D5";
                    break;
                case Profession.Necromancer:
                    hexValue = "#52A76F";
                    break;
                case Profession.Revenant:
                    hexValue = "#D16E5A";
                    break;
                case Profession.Unknown:
                default:
                    // Shouldn't hit this
                    hexValue = "#BBBBBB";
                    break;
            }

            return FromRgbHex(hexValue);
        }

        private static Color FromRgbHex(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            if (hex.Length != 6)
                throw new Exception("Color not valid");

            return new Color(
                int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber)
            );
        }
    }
}