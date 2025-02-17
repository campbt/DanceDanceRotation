﻿using System;

namespace DanceDanceRotationModule.Model
{
    /**
     * Settings and other information specific to one song
     */
    public struct SongData
    {
        public const int MinimumNotePositionChangePerSecond = 75;
        public const int MaximumNotePositionChangePerSecond = 600;
        public const int DefaultNotePositionChangePerSecond = 300;

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
        /* Auto-pause notes on perfect and play when pressed */
        public bool NoMissMode { get; set; }
        /* Should adjust the speed of the notes based this rate. 1.0 == normal speed. 0.5 is slower. */
        public float PlaybackRate { get; set; }
        /* Should start with notes at this second. */
        public int StartAtSecond { get; set; }
        /* Determines how fast notes move */
        public int NotePositionChangePerSecond { get; set; }


        public SongData(
            Song.ID id
        )
        {
            Id = id;
            Utility1Mapping = UtilitySkillMapping.One;
            Utility2Mapping = UtilitySkillMapping.Two;
            Utility3Mapping = UtilitySkillMapping.Three;
            NoMissMode = false;
            PlaybackRate = 1.0f;
            StartAtSecond = 0;
            NotePositionChangePerSecond = DefaultNotePositionChangePerSecond;
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
                case NoteType.Utility1:
                    return RemapNoteType(Utility1Mapping);
                case NoteType.Utility2:
                    return RemapNoteType(Utility2Mapping);
                case NoteType.Utility3:
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
                    return NoteType.Utility1;
                case UtilitySkillMapping.Two:
                    return NoteType.Utility2;
                case UtilitySkillMapping.Three:
                    return NoteType.Utility3;
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