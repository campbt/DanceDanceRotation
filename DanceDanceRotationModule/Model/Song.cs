using System;
using System.Collections.Generic;

namespace DanceDanceRotationModule.Model
{
    /**
     * Represents a "Song" - a full rotation with note sequence and metadata
     */
    public class Song
    {
        // ReSharper disable once InconsistentNaming
        public struct ID : IEquatable<ID>
        {
            public ID(string name)
            {
                Name = name;
            }

            public string Name { get; set; }

            public bool IsValid()
            {
                return Name.Length > 0;
            }

            public bool Equals(ID other)
            {
                return Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                return obj is ID other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (Name != null ? Name.GetHashCode() : 0);
            }
        }

        /** User defined name of the song. Used for uniquely identifying a song. */
        public ID Id { get; set; }

        /** User defined description of the song */
        public string Description { get; set; }

        /** Ex: https://snowcrows.com/en/builds/elementalist/weaver/condition-weaver */
        public string BuildUrl { get; set; }

        // ReSharper disable once InvalidXmlDocComment
        /** Ex: [&DQYfFRomOBV0AAAAcwAAAMsAAAA1FwAAEhcAAAAAAAAAAAAAAAAAAAAAAAA=] */
        public string BuildTemplateCode { get; set; }

        /** The Ability in the first Utility slot. */
        public PaletteId Utility1 { get; set; }
        /** The Ability in the second Utility slot. */
        public PaletteId Utility2 { get; set; }
        /** The Ability in the third Utility slot. */
        public PaletteId Utility3 { get; set; }
        /** The Profession that plays this song */
        public Profession Profession { get; set; }
        /** The Elite Spec Name (or profession name if not elite) */
        public string EliteName { get; set; }

        /** The notes of the song, sorted by time in rotation. */
        public List<Note> Notes { get; set; }

        public string Name => Id.Name;
    }
}