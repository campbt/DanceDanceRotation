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