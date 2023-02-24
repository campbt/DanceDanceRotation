using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;

namespace DanceDanceRotationModule.Util
{
    public class Resources
    {
        public static Resources Instance = new Resources();

        public Resources()
        {
        }

        public void LoadResources(ContentsManager contentsManager)
        {
            MugTexture = contentsManager.GetTexture("mug.png");
            WindowBackgroundEmptyTexture = contentsManager.GetTexture("windowBgEmpty.png");
            WindowBackgroundTexture = contentsManager.GetTexture("windowBg.png");
            WindowBackground2Texture = contentsManager.GetTexture("windowBg2.png");
            DdrLogoTexture = contentsManager.GetTexture("ddr_logo.png");
            DdrLogoEmblemTexture = contentsManager.GetTexture("ddr_logo_emblem.png");
            DdrNoteGreenTexture = contentsManager.GetTexture("ddr_note_green.png");
            DdrNotePurpleTexture = contentsManager.GetTexture("ddr_note_purple.png");
            DdrNoteRedTexture = contentsManager.GetTexture("ddr_note_red.png");
            DdrStar = contentsManager.GetTexture("ddr_star.png");
            DdrTargetBottom = contentsManager.GetTexture("ddr_target_bottom.png");
            DdrTargetCircle = contentsManager.GetTexture("ddr_target_circle.png");
            DdrTargetSpacer = contentsManager.GetTexture("ddr_target_spacer.png");
            DdrTargetTop = contentsManager.GetTexture("ddr_target_top.png");
            NotesControlsBg = contentsManager.GetTexture("notesControlsBg.png");
            NotesBg = contentsManager.GetTexture("notesBg.png");
        }

        public void Unload()
        {
            WindowBackgroundEmptyTexture?.Dispose();
            WindowBackgroundTexture?.Dispose();
            WindowBackground2Texture?.Dispose();
            MugTexture?.Dispose();
            DdrLogoTexture?.Dispose();
            DdrLogoEmblemTexture?.Dispose();
            DdrNoteGreenTexture?.Dispose();
            DdrNotePurpleTexture?.Dispose();
            DdrNoteRedTexture?.Dispose();
            DdrStar?.Dispose();
            DdrTargetBottom?.Dispose();
            DdrTargetCircle?.Dispose();
            DdrTargetSpacer?.Dispose();
            DdrTargetTop?.Dispose();
            NotesControlsBg?.Dispose();
            NotesBg?.Dispose();
        }

        public Texture2D MugTexture { get; private set; }
        public Texture2D WindowBackgroundEmptyTexture { get; private set; }
        public Texture2D WindowBackgroundTexture { get; private set; }
        public Texture2D WindowBackground2Texture { get; private set; }
        public Texture2D DdrLogoTexture { get; private set; }
        public Texture2D DdrLogoEmblemTexture { get; private set; }
        public Texture2D DdrNotePurpleTexture { get; private set; }
        public Texture2D DdrNoteRedTexture { get; private set; }
        public Texture2D DdrNoteGreenTexture { get; private set; }
        public Texture2D DdrStar { get; private set; }
        public Texture2D DdrTargetBottom { get; private set; }
        public Texture2D DdrTargetCircle { get; private set; }
        public Texture2D DdrTargetSpacer { get; private set; }
        public Texture2D DdrTargetTop { get; private set; }
        public Texture2D NotesControlsBg { get; private set; }
        public Texture2D NotesBg { get; private set; }
    }
}