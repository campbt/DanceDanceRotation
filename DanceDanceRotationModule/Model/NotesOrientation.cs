namespace DanceDanceRotationModule.Model
{
    /**
     * Enum defining the direction the player wishes the notes to move in
     */
    public enum NotesOrientation
    {
        RightToLeft,
        LeftToRight,
        TopToBottom,
        BottomToTop,
        // Special: This is basically TopToBottom, but with 10 lanes and spaced out in the center. Intended to be
        //          overlaid on top of the 10 ability icons themselves
        AbilityBarStyle
    }

    public static class OrientationExtensions
    {
        public static bool IsVertical(NotesOrientation orientation)
        {
            switch (orientation)
            {
                case NotesOrientation.TopToBottom:
                case NotesOrientation.BottomToTop:
                case NotesOrientation.AbilityBarStyle:
                    return true;
                case NotesOrientation.RightToLeft:
                case NotesOrientation.LeftToRight:
                default:
                    return false;
            }
        }
    }
}