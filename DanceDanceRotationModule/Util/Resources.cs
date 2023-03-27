using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Gw2Mumble;
using Blish_HUD.Modules.Managers;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.Storage;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace DanceDanceRotationModule.Util
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Resources
    {
        // MARK: Statics

        private static readonly Logger Logger = Logger.GetLogger<Resources>();

        public static Resources Instance = new Resources();

        // MARK: Inner Types

        private struct AbilityInfo
        {
            /** The asset ID of the icon. This is used to look up the asset in the DAC */
            public int assetId { get; }
            /** Path to a custom icon asset stored in ref/ */
            public string icon { get; }
            public string name { get; }

            public AbilityInfo(int assetId, string icon, string name)
            {
                this.assetId = assetId;
                this.icon = icon;
                this.name = name;

                if (name == null)
                {
                    throw new Exception($"AbilityInfo was read in wrong. Missing name! ${this}");
                }
            }
        }

        // MARK: Resource Loading

        public void LoadResources(ContentsManager contentsManager)
        {
            // Store contents manager for later on demand loading of ability icons
            ContentsManager = contentsManager;

            // Load the abilityIds lookup table for later abilityId -> image loading
            LoadAbilityInfoFile("abilityInfoApi.json");
            // Load custom ones not provided by the API
            LoadAbilityInfoFile("abilityInfoCustom.json");

            // Load the PaletteId -> AbilityId table
            LoadPaletteIdLookup("paletteSkillLookup.json");

            ButtonCopy = contentsManager.GetTexture("buttons/copyIcon.png");
            ButtonDelete = contentsManager.GetTexture("buttons/deleteIcon.png");
            ButtonDownload = contentsManager.GetTexture("buttons/downloadIcon.png");
            ButtonList = contentsManager.GetTexture("buttons/menuIcon.png");
            ButtonOpenUrl = contentsManager.GetTexture("buttons/openLinkIcon.png");
            ButtonPause = contentsManager.GetTexture("buttons/pauseIcon.png");
            ButtonPlay = contentsManager.GetTexture("buttons/playIcon.png");
            ButtonReload = contentsManager.GetTexture("buttons/reloadIcon.png");
            ButtonStop = contentsManager.GetTexture("buttons/stopIcon.png");
            DdrLogoTexture = contentsManager.GetTexture("notes/ddr_logo.png");
            DdrLogoEmblemTexture = contentsManager.GetTexture("ddr_logo_emblem.png");
            DdrNoteGreenTexture = contentsManager.GetTexture("ddr_note_green.png");
            DdrNotePurpleTexture = contentsManager.GetTexture("notes/ddr_note_purple.png");
            DdrNoteRedTexture = contentsManager.GetTexture("notes/ddr_note_red.png");
            DdrTargetBottom = contentsManager.GetTexture("notes/ddr_target_bottom.png");
            DdrTargetCircle = contentsManager.GetTexture("notes/ddr_target_circle.png");
            DdrTargetSpacer = contentsManager.GetTexture("notes/ddr_target_spacer.png");
            DdrTargetTop = contentsManager.GetTexture("notes/ddr_target_top.png");
            NotesControlsBg = contentsManager.GetTexture("notes/notesControlsBg.png");
            NotesBg = contentsManager.GetTexture("notes/notesBg.png");
            SongInfoBackground = contentsManager.GetTexture("windows/songInfoBg.png");
            UnknownAbilityIcon = contentsManager.GetTexture("abilityIcons/special/unknownAbilityIcon.png");
            WindowBackgroundEmptyTexture = contentsManager.GetTexture("windows/windowBgEmpty.png");
            WindowBackgroundTexture = contentsManager.GetTexture("windows/windowBg.png");
            WindowBackground2Texture = contentsManager.GetTexture("windows/windowBg2.png");
        }

        /**
         * Reads in [AbilityInfo] from the json file supplied
         * Icons in the files are just the file name, but each .json file
         * is referring to different sub-folders in abilityIcons/ for organizational purposes,
         * so the icon name needs to be prefixed with this
         */
        private void LoadAbilityInfoFile(string fileName)
        {
            var fileStream = ContentsManager.GetFileStream(fileName);
            var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            string content = streamReader.ReadToEnd();
            var rawJson = JsonConvert.DeserializeObject<Dictionary<string, AbilityInfo>>(
                content
            );
            foreach (KeyValuePair<string, AbilityInfo> entry in rawJson)
            {
                try
                {
                    var raw = int.Parse(entry.Key);
                    var abilityId = new AbilityId(raw);
                    var abilityInfo = entry.Value;
                    AbilityInfos[abilityId] = abilityInfo;
                }
                catch
                {
                    Logger.Warn("Failed to decode an entry in " + fileName + ": " + entry);
                }
            }
        }

        /**
         * Reads in the paletteSkillLookup.json file into the lookup table, PaletteIdLookup
         */
        private void LoadPaletteIdLookup(string fileName)
        {
            Logger.Info("Loading Palette ID lookup tables from: " + fileName);

            var fileStream = ContentsManager.GetFileStream(fileName);
            var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            string content = streamReader.ReadToEnd();
            var rawJson = JsonConvert.DeserializeObject<Dictionary<string, int>>(
                content
            );
            foreach (KeyValuePair<string, int> entry in rawJson)
            {
                try
                {
                    var raw = int.Parse(entry.Key);
                    var paletteId = new PaletteId(raw);
                    var abilityId = new AbilityId(entry.Value);
                    PaletteIdLookup[paletteId] = abilityId;
                }
                catch
                {
                    Logger.Warn("Failed to decode an entry in " + fileName + ": " + entry);
                }
            }
        }

        // MARK: Information Retrieval

        public AbilityId GetAbilityIdForPaletteId(PaletteId paletteId)
        {
            if (PaletteIdLookup.ContainsKey(paletteId))
            {
                return PaletteIdLookup[paletteId];
            }
            else
            {
                return AbilityId.Unknown;
            }
        }

        public AsyncTexture2D GetAbilityIcon(PaletteId paletteId)
        {
            AbilityId abilityId = GetAbilityIdForPaletteId(paletteId);
            return GetAbilityIcon(
                abilityId
            );
        }

        public AsyncTexture2D GetAbilityIcon(AbilityId abilityId)
        {
            if (AbilityInfos.ContainsKey(abilityId))
            {
                var abilityInfo = AbilityInfos[abilityId];
                string assetCacheKey =
                    (string.IsNullOrEmpty(abilityInfo.icon))
                        ? abilityInfo.assetId.ToString()
                        : abilityInfo.icon;

                if (AbilityIconTextureCache.ContainsKey(assetCacheKey))
                {
                    // Already loaded this icon
                    return AbilityIconTextureCache[assetCacheKey];
                }
                else
                {
                    // Load it
                    if (string.IsNullOrEmpty(abilityInfo.icon) == false)
                    {
                        // This is a custom icon that is stored in ref
                        string imageFileName = "abilityIcons/" + abilityInfo.icon;
                        var texture = ContentsManager.GetTexture(imageFileName);
                        if (texture == null)
                        {
                            // Shouldn't happen if the lookup table is accurate
                            Logger.Warn("Failed to load icon name from lookup table: " + imageFileName);
                            return UnknownAbilityIcon;
                        }
                        else
                        {
                            AbilityIconTextureCache[assetCacheKey] = texture;
                            return texture;
                        }
                    } else if (GameService.Content.DatAssetCache.TryGetTextureFromAssetId(
                        abilityInfo.assetId,
                        out AsyncTexture2D texture
                    ))
                    {
                        AbilityIconTextureCache[assetCacheKey] = texture;
                        return texture;
                    }
                    else
                    {
                        // Shouldn't happen if the lookup table is accurate
                        Logger.Warn($"Failed to load icon name from lookup table: abilityId=${abilityId.Raw} assertId=${assetCacheKey}");
                        return UnknownAbilityIcon;
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

        public string GetAbilityName(AbilityId abilityId)
        {
            if (AbilityInfos.ContainsKey(abilityId))
            {
                return AbilityInfos[abilityId].name;
            }
            return "";
        }

        // MARK: Ability Icon Cache Management

        /**
         * Sets up a listener so the ability icon cache in Resources only caches the current song
         * It should also pre-fetch these icons as some may need to be downloaded
         */
        public void SetUpCurrentSongListener(SongRepo songRepo)
        {
            var lastSelectedSongId = new Song.ID();
            songRepo.OnSelectedSongChanged +=
                delegate(object sender, SelectedSongInfo info)
                {
                    if (info.Song != null && info.Song.Id.Equals(lastSelectedSongId) == false)
                    {
                        lastSelectedSongId = info.Song.Id;
                        Logger.Info("Selected Song Changed. Updating ability icon cache to just this song.");
                        PurgeAbilityIconCache();
                        if (info.Song != null)
                        {
                            PreloadSongAbilityIcons(info.Song);
                        }
                    }
                };
        }

        /**
         * Purges the cached ability icons. Blish should clean up storage
         */
        public void PurgeAbilityIconCache()
        {
            AbilityIconTextureCache.Clear();
        }

        /**
         * Helps avoid pop-in if the ability icon needs to be downloaded, or there is a slow disk read for some reason.
         */
        public void PreloadSongAbilityIcons(Song song)
        {
            var uniqueAbilityIds = new HashSet<AbilityId>();
            foreach (var note in song.Notes)
            {
                uniqueAbilityIds.Add(note.AbilityId);
            }
            foreach (var abilityId in uniqueAbilityIds)
            {
                GetAbilityIcon(abilityId);
            }
        }

        // MARK: Disposal

        public void Unload()
        {
            foreach (Texture2D Icon in AbilityIconTextureCache.Values)
            {
                Icon.Dispose();
            }
            AbilityIconTextureCache.Clear();
            AbilityInfos.Clear();

            WindowBackgroundEmptyTexture?.Dispose();
            WindowBackgroundTexture?.Dispose();
            WindowBackground2Texture?.Dispose();
            ButtonCopy?.Dispose();
            ButtonDelete?.Dispose();
            ButtonDownload?.Dispose();
            ButtonList?.Dispose();
            ButtonOpenUrl?.Dispose();
            ButtonPause?.Dispose();
            ButtonPlay?.Dispose();
            ButtonReload?.Dispose();
            ButtonStop?.Dispose();
            DdrLogoTexture?.Dispose();
            DdrLogoEmblemTexture?.Dispose();
            DdrNoteGreenTexture?.Dispose();
            DdrNotePurpleTexture?.Dispose();
            DdrNoteRedTexture?.Dispose();
            DdrTargetBottom?.Dispose();
            DdrTargetCircle?.Dispose();
            DdrTargetSpacer?.Dispose();
            DdrTargetTop?.Dispose();
            NotesControlsBg?.Dispose();
            NotesBg?.Dispose();
            SongInfoBackground?.Dispose();
            UnknownAbilityIcon?.Dispose();
            Logger.Info("Resourced Unloaded");
        }

        private ContentsManager ContentsManager;
        // Read in from .json files at load, this provides information ability each ability ID
        private IDictionary<AbilityId, AbilityInfo> AbilityInfos = new Dictionary<AbilityId, AbilityInfo>();
        // Read in from a .json file at load, this provides a table to convert PaletteId -> AbilityId
        private IDictionary<PaletteId, AbilityId> PaletteIdLookup = new Dictionary<PaletteId, AbilityId>();
        // A cache of loaded Texture2Ds given an ability icon image name.
        private IDictionary<string, AsyncTexture2D> AbilityIconTextureCache = new Dictionary<string, AsyncTexture2D>();

        public Texture2D WindowBackgroundEmptyTexture { get; private set; }
        public Texture2D WindowBackgroundTexture { get; private set; }
        public Texture2D WindowBackground2Texture { get; private set; }
        // DDR Icons in the notes container
        public Texture2D DdrLogoTexture { get; private set; }
        public Texture2D DdrLogoEmblemTexture { get; private set; }
        public Texture2D DdrNotePurpleTexture { get; private set; }
        public Texture2D DdrNoteRedTexture { get; private set; }
        public Texture2D DdrNoteGreenTexture { get; private set; }
        public Texture2D DdrTargetBottom { get; private set; }
        public Texture2D DdrTargetCircle { get; private set; }
        public Texture2D DdrTargetSpacer { get; private set; }
        public Texture2D DdrTargetTop { get; private set; }
        // Buttons in the app
        public Texture2D ButtonCopy { get; private set; }
        public Texture2D ButtonDelete { get; private set; }
        public Texture2D ButtonDownload { get; private set; }
        public Texture2D ButtonList { get; private set; }
        public Texture2D ButtonOpenUrl { get; private set; }
        public Texture2D ButtonPause { get; private set; }
        public Texture2D ButtonPlay { get; private set; }
        public Texture2D ButtonReload { get; private set; }
        public Texture2D ButtonStop { get; private set; }
        public Texture2D NotesControlsBg { get; private set; }
        public Texture2D NotesBg { get; private set; }
        public Texture2D SongInfoBackground { get; private set; }
        public Texture2D UnknownAbilityIcon { get; private set; }
    }
}