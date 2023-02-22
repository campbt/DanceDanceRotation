using System;
using System.Collections.Generic;

namespace DanceDanceRotationModule.Model
{
    /**
     * Represents a "Song" - a full rotation with note sequence and metadata
     */
    public class Song
    {
        public struct ID : IEquatable<ID>
        {
            public ID(string name)
            {
                Name = name;
            }

            public string Name { get; set; }

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

        public ID Id { get; set; }
        public string Description { get; set; }
        public List<Note> Notes { get; set; }

        public string Name
        {
            get => Id.Name;
        }
    }
}