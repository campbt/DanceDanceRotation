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
         /** Specifically used to override the auto-hit effect for Weapon1 */
         public bool OverrideAuto { get; set; }

         public Note(
             NoteType noteType,
             TimeSpan timeInRotation,
             TimeSpan duration,
             AbilityId abilityId,
             bool overrideAuto
         )
         {
             NoteType = noteType;
             TimeInRotation = timeInRotation;
             Duration = duration;
             AbilityId = abilityId;
             OverrideAuto = overrideAuto;
         }
    }

}