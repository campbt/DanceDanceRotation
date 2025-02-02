namespace DanceDanceRotationModule.Model
{
    /**
     * Enum defining the direction the player wishes the notes to move in
     */
    public enum CompactStyle
    {
        /** 6 Lanes - Notes go on each lane */
        Regular,
        /** 3 Lanes - Notes try to go on first lane, but may be shifted down to prevent overlap */
        Compact,
        /** 1 Lane - All notes in one lane, no adjustments for overlap. */
        UltraCompact
    }
}