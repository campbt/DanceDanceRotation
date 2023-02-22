using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DanceDanceRotationModule.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DanceDanceRotationModule.Storage
{
    public class SongTranslator
    {

        /**
         * Raw data representation of the JSON sent to the module
         */
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        protected struct SongJson
        {
            public string name { get; set; }
            public string description { get; set; }
            public List<Note> notes { get; set; }

            public struct Note
            {
                public int time { get; set; }
                // public int duration { get; set; }
                public string noteType { get; set; }
                // public string abilityId { get; set; }
            }
        }

        /**
         * Converts a Song to JSON and returns it as a string
         */
        public static string ToJson(Song song)
        {
            SongJson songJson = new SongJson()
            {
                name = song.Name,
                description = song.Description,
                notes = song.Notes.Select( note =>
                    new SongJson.Note()
                    {
                        time = note.TimeInRotation.Milliseconds,
                        noteType = note.NoteType.ToString()
                    }
                ).ToList()
            };
            return JsonConvert.SerializeObject(songJson);
        }

        /**
         * Returns a Song? from the provided json
         * @throws if the deserialize fails
         */
        public static Song FromJson(string json)
        {
            SongJson songJson = JsonConvert.DeserializeObject<SongJson>(json);
            return new Song()
            {
                Id = new Song.ID(songJson.name),
                Description = songJson.description,
                Notes = songJson.notes.Select(noteJson =>
                {
                    if (NoteType.TryParse(noteJson.noteType, out NoteType noteType) == false)
                    {
                        throw new InvalidDataException("Unknown Note Type: " + noteJson.noteType);
                    }

                    return new Note()
                    {
                        TimeInRotation = TimeSpan.FromMilliseconds(noteJson.time),
                        NoteType = noteType,
                    };
                }).ToList()
            };
        }
    }
}