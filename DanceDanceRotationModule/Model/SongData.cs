using System;

namespace DanceDanceRotationModule.Model
{
    /**
     * Settings and other information specific to one song
     */
    public struct SongData
    {
        public enum UtilitySkillMapping
        {
            One,
            Two,
            Three
        }

        public Song.ID Id { get; set;  }
        public UtilitySkillMapping Utility1Mapping { get; set; }
        public UtilitySkillMapping Utility2Mapping { get; set; }
        public UtilitySkillMapping Utility3Mapping { get; set; }
        /* Should adjust the speed of the notes based this rate. 1.0 == normal speed. 0.5 is slower. */
        public float PlaybackRate { get; set; }
        /* Should start with notes at this second. */
        public int StartAtSecond { get; set; }


        public SongData(
            Song.ID id
        )
        {
            Id = id;
            Utility1Mapping = UtilitySkillMapping.One;
            Utility2Mapping = UtilitySkillMapping.Two;
            Utility3Mapping = UtilitySkillMapping.Three;
            PlaybackRate = 100;
            StartAtSecond = 0;
        }

        /**
         * Utility function that takes an input [NoteType] and potentially
         * returns a different one based on the [UtilitySkillMapping] settings
         * of this [SongData]
         */
        public NoteType RemapNoteType(NoteType noteType)
        {
            switch (noteType)
            {
                case NoteType.UtilitySkill1:
                    return RemapNoteType(Utility1Mapping);
                case NoteType.UtilitySkill2:
                    return RemapNoteType(Utility2Mapping);
                case NoteType.UtilitySkill3:
                    return RemapNoteType(Utility3Mapping);
                default:
                    // No remapping for non-utility types
                    return noteType;
            }
        }

        public static NoteType RemapNoteType(UtilitySkillMapping skillMapping)
        {
            switch (skillMapping)
            {
                case UtilitySkillMapping.One:
                    return NoteType.UtilitySkill1;
                case UtilitySkillMapping.Two:
                    return NoteType.UtilitySkill2;
                case UtilitySkillMapping.Three:
                    return NoteType.UtilitySkill3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillMapping), skillMapping, null);
            }
        }

        public static SongData DefaultSettings(
            Song.ID id
        )
        {
            return new SongData(
                id
            );
        }
    }
}