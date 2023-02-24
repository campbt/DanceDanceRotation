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


        public SongData(
            Song.ID id,
            UtilitySkillMapping utility1Mapping,
            UtilitySkillMapping utility2Mapping,
            UtilitySkillMapping utility3Mapping
        )
        {
            Id = id;
            Utility1Mapping = utility1Mapping;
            Utility2Mapping = utility2Mapping;
            Utility3Mapping = utility3Mapping;
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
                id,
                UtilitySkillMapping.One,
                UtilitySkillMapping.Two,
                UtilitySkillMapping.Three
            );
        }
    }
}