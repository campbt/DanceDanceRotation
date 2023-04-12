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
        BottomToTop
    }

    public static class OrientationExtensions
    {
        public static bool IsVertical(NotesOrientation orientation)
        {
            switch (orientation)
            {
                case NotesOrientation.TopToBottom:
                case NotesOrientation.BottomToTop:
                    return true;
                case NotesOrientation.RightToLeft:
                case NotesOrientation.LeftToRight:
                default:
                    return false;
            }
        }
    }
}