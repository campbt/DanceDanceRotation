using System;

namespace DanceDanceRotationModule.Model
{
    /**
     * Represents an Ability ID that GW2 uses to identify a skill
     */
    public struct AbilityId : IEquatable<AbilityId>
    {
        public AbilityId(string raw)
        {
            Raw = raw;
        }

        public string Raw { get; set; }

        public bool Equals(AbilityId other)
        {
            return Raw == other.Raw;
        }

        public override bool Equals(object obj)
        {
            return obj is AbilityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Raw != null ? Raw.GetHashCode() : 0);
        }
    }
}