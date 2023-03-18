using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Blish_HUD;
using DanceDanceRotationModule.Model;
using Newtonsoft.Json;

namespace DanceDanceRotationModule.Storage
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class SongTranslator
    {
        private static readonly Logger Logger = Logger.GetLogger<SongTranslator>();

        /**
         * Raw data representation of the JSON sent to the module
         */
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        protected struct SongJson
        {
            public string name { get; set; }
            public string description { get; set; }
            public List<Note> notes { get; set; }
            public string logUrl { get; set; }
            public string buildUrl { get; set; }
            public string buildChatCode { get; set; }
            public BuildTemplate decodedBuildTemplate { get; set; }

            public struct BuildTemplate
            {
                public int profession { get; set; }
                public List<Specialization> specializations { get; set; }
                public Skills skills { get; set; }

                public struct Specialization
                {
                    public int id { get; set; }
                }

                public struct Skills
                {
                    public UtilitySkills terrestrial { get; set; }
                    public UtilitySkills aquatic { get; set; }

                    public struct UtilitySkills
                    {
                        public int heal { get; set; }
                        public List<int> utilities { get; set; }
                        public int elite { get; set; }
                    }
                }
            }

            public struct Note
            {
                public double time { get; set; }
                public double duration { get; set; }
                public string noteType { get; set; }
                public int abilityId { get; set; }
                public bool overrideAuto { get; set; }
            }
        }

        /**
         * Returns a Song? from the provided json
         * @throws if the deserialize fails
         */
        public static Song FromJson(string json)
        {
            SongJson songJson = JsonConvert.DeserializeObject<SongJson>(json);

            List<int> utilities = songJson.decodedBuildTemplate.skills.terrestrial.utilities;
            if (utilities == null)
            {
                utilities = new List<int>(3);
            }
            while (utilities.Count < 3)
            {
                utilities.Add(0);
            }

            var profession = ProfessionExtensions.ProfessionFromBuildTemplate(
                songJson.decodedBuildTemplate.profession
            );
            var eliteName = EliteNameFromBuildTemplate(
                profession,
                songJson.decodedBuildTemplate.specializations.Last().id
            );


            return new Song()
            {
                Id = new Song.ID(songJson.name),
                Description = songJson.description,
                BuildUrl = songJson.buildUrl,
                BuildTemplateCode = songJson.buildChatCode,
                Utility1 = new PaletteId(utilities[0]),
                Utility2 = new PaletteId(utilities[1]),
                Utility3 = new PaletteId(utilities[2]),
                Profession = profession,
                EliteName = eliteName,
                Notes = songJson.notes.Select(noteJson =>
                {
                    if (NoteType.TryParse(noteJson.noteType, out NoteType noteType) == false)
                    {
                        Logger.Warn("Unknown Note Type: '" + noteJson.noteType + "'");
                        noteType = NoteType.Unknown;
                    }

                    return new Note(
                        noteType: noteType,
                        timeInRotation: TimeSpan.FromMilliseconds(Math.Round(noteJson.time)),
                        duration: TimeSpan.FromMilliseconds(Math.Round(noteJson.duration)),
                        abilityId: new AbilityId(noteJson.abilityId),
                        overrideAuto: noteJson.overrideAuto
                    );
                }).ToList()
            };
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private static string EliteNameFromBuildTemplate(
            Profession profession,
            int buildTemplateCode
        )
        {
            switch (buildTemplateCode)
            {
                case 5: return "Druid";
                case 7: return "Daredevil";
                case 18: return "Berserker";
                case 27: return "Dragonhunter";
                case 34: return "Reaper";
                case 40: return "Chronomancer";
                case 43: return "Scrapper";
                case 48: return "Tempest";
                case 52: return "Herald";
                case 55: return "Soulbeast";
                case 56: return "Weaver";
                case 57: return "Holosmith";
                case 58: return "Deadeye";
                case 59: return "Mirage";
                case 60: return "Scourge";
                case 61: return "Spellbreaker";
                case 62: return "Firebrand";
                case 63: return "Renegade";
                case 64: return "Harbinger";
                case 65: return "Willbender";
                case 66: return "Virtuoso";
                case 67: return "Catalyst";
                case 68: return "Bladesworn";
                case 69: return "Vindicator";
                case 70: return "Mechanist";
                case 71: return "Specter";
                default:
                    // Not an elite spec. Just use the profession text
                    return ProfessionExtensions.GetProfessionDisplayText(profession);
            }
        }
    }
}