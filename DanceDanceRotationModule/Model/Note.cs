using System;

namespace DanceDanceRotationModule.Model
{

    /**
     * Basic structure that is fed into the NotesContainer to represent a single keypress
     * (like a weapon skill)
     */
    public struct Note
    {
         public NoteType NoteType { get; set; }
         public TimeSpan TimeInRotation { get; set; }
         public TimeSpan Duration { get; set; }
         /** @Nullable*/ public AbilityId AbilityId { get; set; }

         public Note(
             NoteType noteType,
             TimeSpan timeInRotation,
             TimeSpan duration,
             AbilityId abilityId
         )
         {
             NoteType = noteType;
             TimeInRotation = timeInRotation;
             Duration = duration;
             AbilityId = abilityId;
         }
    }

}