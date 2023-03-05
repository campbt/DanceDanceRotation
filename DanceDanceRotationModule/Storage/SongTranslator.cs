using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Blish_HUD;
using DanceDanceRotationModule.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DanceDanceRotationModule.Storage
{
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
                public Skills skills { get; set; }

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

            return new Song()
            {
                Id = new Song.ID(songJson.name),
                Description = songJson.description,
                BuildUrl = songJson.buildUrl,
                BuildTemplateCode = songJson.buildChatCode,
                Utility1 = new PaletteId(utilities[0]),
                Utility2 = new PaletteId(utilities[1]),
                Utility3 = new PaletteId(utilities[2]),
                Profession = ProfessionExtensions.ProfessionFromBuildTemplate(
                    songJson.decodedBuildTemplate.profession
                ),
                Notes = songJson.notes.Select(noteJson =>
                {
                    if (NoteType.TryParse(noteJson.noteType, out NoteType noteType) == false)
                    {
                        Logger.Warn("Unknown Note Type: '" + noteJson.noteType + "'");
                        noteType = NoteType.Unknown;
                    }

                    return new Note()
                    {
                        TimeInRotation = TimeSpan.FromMilliseconds(Math.Round(noteJson.time)),
                        NoteType = noteType,
                        Duration = TimeSpan.FromMilliseconds(Math.Round(noteJson.duration)),
                        AbilityId = new AbilityId(noteJson.abilityId)
                    };
                }).ToList()
            };
        }
    }
}