using System;

namespace DanceDanceRotationModule.Model
{
    /**
     * Represents an Ability ID that GW2 uses to identify a skill
     */
    public readonly struct AbilityId : IEquatable<AbilityId>
    {
        /** Special ID used to indicate an unknown ability */
        public static readonly AbilityId Unknown = new AbilityId(-9999999);

        public AbilityId(int raw)
        {
            Raw = raw;
        }

        public int Raw { get; }

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
            return Raw.GetHashCode();
        }
    }
}