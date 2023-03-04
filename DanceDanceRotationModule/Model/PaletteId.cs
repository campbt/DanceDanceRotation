using System;

namespace DanceDanceRotationModule.Model
{
    /**
     * Represents a "Palette" Ability ID that GW2 uses to identify a skill
     * These IDs seem to be used by the build templates, and need a lookup table
     * to find the actual [AbilityId] they refer to.
     */
    public struct PaletteId : IEquatable<PaletteId>
    {
        public PaletteId(int raw)
        {
            Raw = raw;
        }

        public int Raw { get; set; }

        public bool Equals(PaletteId other)
        {
            return Raw == other.Raw;
        }

        public override bool Equals(object obj)
        {
            return obj is PaletteId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Raw;
        }
    }
}