using System.Collections.Generic;
using System.IO;
using System.Text;
using Blish_HUD;
using Blish_HUD.Modules.Managers;
using DanceDanceRotationModule.Model;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace DanceDanceRotationModule.Util
{
    public class Resources
    {
        private static readonly Logger Logger = Logger.GetLogger<Resources>();

        public static Resources Instance = new Resources();

        public Resources()
        {
        }

        public void LoadResources(ContentsManager contentsManager)
        {
            // Store contents manager for later on demand loading of ability icons
            ContentsManager = contentsManager;

            // Load the abilityIds lookup table for later abilityId -> image loading
            var fileStream = ContentsManager.GetFileStream("abilityIdsToImageId.json");
            var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            string content = streamReader.ReadToEnd();
            var rawJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                content
            );
            foreach (KeyValuePair<string, string> entry in rawJson)
            {
                var abilityId = new AbilityId(entry.Key);
                AbilityToIconNames[abilityId] = entry.Value;
            }

            MugTexture = contentsManager.GetTexture("mug.png");
            WindowBackgroundEmptyTexture = contentsManager.GetTexture("windowBgEmpty.png");
            WindowBackgroundTexture = contentsManager.GetTexture("windowBg.png");
            WindowBackground2Texture = contentsManager.GetTexture("windowBg2.png");
            ButtonCopy = contentsManager.GetTexture("buttons/copyIcon.png");
            ButtonDelete = contentsManager.GetTexture("buttons/deleteIcon.png");
            ButtonDownload = contentsManager.GetTexture("buttons/downloadIcon.png");
            ButtonList = contentsManager.GetTexture("buttons/menuIcon.png");
            ButtonPause = contentsManager.GetTexture("buttons/pauseIcon.png");
            ButtonPlay = contentsManager.GetTexture("buttons/playIcon.png");
            ButtonStop = contentsManager.GetTexture("buttons/stopIcon.png");
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
            SongListIcon = contentsManager.GetTexture("songListIcon.png");
            UnknownAbilityIcon = contentsManager.GetTexture("unknownAbilityIcon.png");
        }

        public Texture2D GetAbilityIcon(AbilityId abilityId)
        {
            if (AbilityToIconNames.ContainsKey(abilityId))
            {
                var iconName = AbilityToIconNames[abilityId];
                if (AbilityIcons.ContainsKey(iconName))
                {
                    // Already loaded this icon
                    return AbilityIcons[iconName];
                }
                else
                {
                    // Load it
                    string imageFileName = "abilityIcons/" + AbilityToIconNames[abilityId];
                    var icon = ContentsManager.GetTexture(imageFileName);
                    if (icon == null)
                    {
                        // Shouldn't happen if the lookup table is accurate
                        Logger.Warn("Failed to load icon name from lookup table: " + imageFileName);
                        return UnknownAbilityIcon;
                    }
                    else
                    {
                        AbilityIcons[imageFileName] = icon;
                        return icon;
                    }
                }
            }
            else
            {
                // Unknown Ability ID
                Logger.Debug("Unknown Ability ID: " + abilityId.Raw);
                return UnknownAbilityIcon;
            }
        }

        public void Unload()
        {
            foreach (Texture2D Icon in AbilityIcons.Values)
            {
                Icon.Dispose();
            }
            AbilityIcons.Clear();
            AbilityToIconNames.Clear();

            WindowBackgroundEmptyTexture?.Dispose();
            WindowBackgroundTexture?.Dispose();
            WindowBackground2Texture?.Dispose();
            MugTexture?.Dispose();
            ButtonCopy?.Dispose();
            ButtonDelete?.Dispose();
            ButtonDownload?.Dispose();
            ButtonList?.Dispose();
            ButtonPause?.Dispose();
            ButtonPlay?.Dispose();
            ButtonStop?.Dispose();
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
            SongListIcon?.Dispose();
            UnknownAbilityIcon?.Dispose();
        }

        private ContentsManager ContentsManager;
        private IDictionary<AbilityId, string> AbilityToIconNames = new Dictionary<AbilityId, string>();
        private IDictionary<string, Texture2D> AbilityIcons = new Dictionary<string, Texture2D>();

        public Texture2D MugTexture { get; private set; }
        public Texture2D WindowBackgroundEmptyTexture { get; private set; }
        public Texture2D WindowBackgroundTexture { get; private set; }
        public Texture2D WindowBackground2Texture { get; private set; }
        // DDR Icons in the notes container
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
        // Buttons in the app
        public Texture2D ButtonCopy { get; private set; }
        public Texture2D ButtonDelete { get; private set; }
        public Texture2D ButtonDownload { get; private set; }
        public Texture2D ButtonList { get; private set; }
        public Texture2D ButtonPause { get; private set; }
        public Texture2D ButtonPlay { get; private set; }
        public Texture2D ButtonStop { get; private set; }
        public Texture2D NotesControlsBg { get; private set; }
        public Texture2D NotesBg { get; private set; }
        public Texture2D SongListIcon { get; private set; }
        public Texture2D UnknownAbilityIcon { get; private set; }
    }
}