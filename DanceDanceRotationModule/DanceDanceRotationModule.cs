using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.NoteDisplay;
using DanceDanceRotationModule.Storage;
using DanceDanceRotationModule.Util;
using DanceDanceRotationModule.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DanceDanceRotationModule
{
    [Export(typeof(Module))]
    public class DanceDanceRotationModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<DanceDanceRotationModule>();

        #region Service Managers

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        #endregion

        // MARK: Settings - General

        internal SettingEntry<float> BackgroundOpacity { get; private set; }
        internal SettingEntry<bool> AutoHitWeapon1 { get; private set; }
        internal SettingEntry<bool> ShowAbilityIconsForNotes { get; private set; }
        internal SettingEntry<bool> ShowHotkeys { get; private set; }
        internal SettingEntry<bool> ShowNextAbilities { get; private set; }

        // MARK: Settings - Hotkeys

        internal SettingCollection AbilityHotkeysSettings { get; private set; }
        internal SettingEntry<KeyBinding> SwapWeapons { get; private set; }
        internal SettingEntry<KeyBinding> Dodge { get; private set; }
        internal SettingEntry<KeyBinding> Weapon1 { get; private set; }
        internal SettingEntry<KeyBinding> Weapon2 { get; private set; }
        internal SettingEntry<KeyBinding> Weapon3 { get; private set; }
        internal SettingEntry<KeyBinding> Weapon4 { get; private set; }
        internal SettingEntry<KeyBinding> Weapon5 { get; private set; }
        internal SettingEntry<KeyBinding> HealingSkill { get; private set; }
        internal SettingEntry<KeyBinding> UtilitySkill1 { get; private set; }
        internal SettingEntry<KeyBinding> UtilitySkill2 { get; private set; }
        internal SettingEntry<KeyBinding> UtilitySkill3 { get; private set; }
        internal SettingEntry<KeyBinding> EliteSkill { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill1 { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill2 { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill3 { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill4 { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill5 { get; private set; }

        // MARK: Control Hotkeys

        internal SettingEntry<KeyBinding> ToggleHotkey { get; private set; }
        internal SettingEntry<KeyBinding> PlayHotkey { get; private set; }
        internal SettingEntry<KeyBinding> PauseHotkey { get; private set; }
        internal SettingEntry<KeyBinding> StopHotkey { get; private set; }

        // MARK: Hidden Settings

        internal SettingEntry<List<SongData>> SongDatas { get; private set; }
        internal SettingEntry<Song.ID> SelectedSong { get; private set; }

        // Ideally you should keep the constructor as is.
        // Use <see cref="Initialize"/> to handle initializing the module.
        [ImportingConstructor]
        public DanceDanceRotationModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            DanceDanceRotationModuleInstance = this;
        }

        // Define the settings you would like to use in your module.  Settings are persistent
        // between updates to both Blish HUD and your module.
        protected override void DefineSettings(SettingCollection settings)
        {
            var generalSettings = settings.AddSubCollection(
                collectionKey: "general_settings",
                renderInUi: true,
                displayNameFunc: () => "General",
                lazyLoaded: false
            );

            BackgroundOpacity = generalSettings.DefineSetting("BackgroundOpacity",
                1.0f,
                () => "Background Transparency",
                () => "Sets the transparency of the notes background. Min=0% Max=100%");
            BackgroundOpacity.SetRange(0.0f, 1.0f);

            AutoHitWeapon1 = generalSettings.DefineSetting("AutoHitWeapon1",
                false,
                () => "Auto Hit Weapon 1",
                () => "If enabled, Weapon1 skills will automatically clear, instead of requiring hotkey presses, since they are probably on auto-cast.");

            ShowAbilityIconsForNotes = generalSettings.DefineSetting("ShowAbilityIconsForNotes",
                false,
                () => "Show ability icon as note",
                () => "If enabled, notes will use the actual ability icon instead of the generic.");

            ShowHotkeys = generalSettings.DefineSetting("ShowHotkeys",
                true,
                () => "Show ability hotkeys",
                () => "If enabled, notes will have the hotkeys displayed on top of them.");

            ShowNextAbilities = generalSettings.DefineSetting("ShowNextAbilities",
                false,
                () => "Show next abilities",
                () => "If enabled, the next few ability icons will be shown.");

            // MARK: Ability Hotkeys

            AbilityHotkeysSettings = settings.AddSubCollection(
                collectionKey: "hotkey_settings",
                renderInUi: true,
                displayNameFunc: () => "Ability Hotkeys",
                lazyLoaded: false
            );
            SwapWeapons      = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.WeaponSwap);
            Dodge            = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Dodge);
            Weapon1          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon1);
            Weapon2          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon2);
            Weapon3          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon3);
            Weapon4          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon4);
            Weapon5          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon5);
            HealingSkill     = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.HealingSkill);
            UtilitySkill1    = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.UtilitySkill1);
            UtilitySkill2    = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.UtilitySkill2 );
            UtilitySkill3    = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.UtilitySkill3);
            EliteSkill       = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.EliteSkill );
            ProfessionSkill1 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill1);
            ProfessionSkill2 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill2);
            ProfessionSkill3 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill3);
            ProfessionSkill4 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill4);
            ProfessionSkill5 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill5);

            // MARK: DDR Control Hotkeys

            var controlHotkeySettings = settings.AddSubCollection(
                collectionKey: "control_hotkey_settings",
                renderInUi: true,
                displayNameFunc: () => "Control Hotkeys",
                lazyLoaded: false
            );
            ToggleHotkey = controlHotkeySettings.DefineSetting("ToggleHotkey",
                new KeyBinding(Keys.None),
                () => "Toggle Play/Stop",
                () => "Will start or stop the DDR rotation when pressed based on if it is currently playing.");
            ToggleHotkey.Value.Enabled = true;
            ToggleHotkey.Value.Activated += delegate
            {
                _mainView.GetNotesContainer()?.ToggleStart();
            };
            PlayHotkey = controlHotkeySettings.DefineSetting("PlayHotkey",
                new KeyBinding(Keys.None),
                () => "Play",
                () => "Will start the DDR rotation when pressed, if not already playing.");
            PlayHotkey.Value.Enabled = true;
            PlayHotkey.Value.Activated += delegate
            {
                _mainView.GetNotesContainer()?.Play();
            };
            PauseHotkey = controlHotkeySettings.DefineSetting("PauseHotkey",
                new KeyBinding(Keys.None),
                () => "Pause",
                () => "Will pause the DDR rotation when pressed, if playing.");
            PauseHotkey.Value.Enabled = true;
            PauseHotkey.Value.Activated += delegate
            {
                _mainView.GetNotesContainer()?.Pause();
            };
            StopHotkey = controlHotkeySettings.DefineSetting("StopHotkey",
                new KeyBinding(Keys.None),
                () => "Stop",
                () => "Will stop the DDR rotation when pressed, if playing.");
            StopHotkey.Value.Enabled = true;
            StopHotkey.Value.Activated += delegate
            {
                _mainView.GetNotesContainer()?.Reset();
            };

            // MARK: Private settings (not visible to the user)

            var hiddenSettings = settings.AddSubCollection("hidden_settings");
            SongDatas = hiddenSettings.DefineSetting("SavedSongSettings", new List<SongData>());
            SelectedSong = hiddenSettings.DefineSetting("SelectedSong", new Song.ID());
        }

        private SettingEntry<KeyBinding> DefineHotkeySetting(SettingCollection settings, NoteType noteType)
        {
            SettingEntry<KeyBinding> retval = settings.DefineSetting(noteType.ToString(),
                new KeyBinding(NoteTypeExtensions.DefaultHotkey(noteType)),
                () => NoteTypeExtensions.HotkeyDescription(noteType),
                () => "Hotkey used for " + NoteTypeExtensions.HotkeyDescription(noteType) + "\nThis must match your in-game hotkeys to work!");
            retval.Value.Enabled = true;
            retval.Value.Activated += delegate
            {
                _mainView.GetNotesContainer()?.OnHotkeyPressed(noteType);
            };
            return retval;
        }

        public SettingEntry<KeyBinding> GetKeyBindingForNoteType(NoteType noteType)
        {
            SettingEntry<KeyBinding> retval = new SettingEntry<KeyBinding>();
            AbilityHotkeysSettings.TryGetSetting<KeyBinding>(noteType.ToString(), out retval);
            return retval;
        }

        // Allows your module to perform any initialization it needs before starting to run.
        // Please note that Initialize is NOT asynchronous and will block Blish HUD's update
        // and render loop, so be sure to not do anything here that takes too long.
        protected override void Initialize()
        {
        }

        // Load content and more here. This call is asynchronous, so it is a good time to run
        // any long running steps for your module including loading resources from file or ref.
        protected override async Task LoadAsync()
        {
            // Get your manifest registered directories with the DirectoriesManager
            foreach (string directoryName in this.DirectoriesManager.RegisteredDirectories)
            {
                // string fullDirectoryPath = DirectoriesManager.GetFullDirectoryPath(directoryName);
                // var allFiles = Directory.EnumerateFiles(fullDirectoryPath, "*", SearchOption.AllDirectories).ToList();

                // example of how to log something in the blishhud.XXX-XXX.log file in %userprofile%\Documents\Guild Wars 2\addons\blishhud\logs
                // Logger.Info($"'{directoryName}' can be found at '{fullDirectoryPath}' and has {allFiles.Count} total files within it.");
            }

            // Load content from the ref directory in the module.bhm automatically with the ContentsManager
            Resources.Instance.LoadResources(ContentsManager);

            // Load songs from settings
            SongRepo = new SongRepo();
            SongRepo.Load();

            // GraphicsDevice graphicsDevice = GameService.Graphics.LendGraphicsDeviceContext().GraphicsDevice;
            _mainWindow = new DdrNotesWindow(
                Resources.Instance.WindowBackgroundEmptyTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(40, 26, 913, 691)
            )
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Dance Dance Rotation",
                Subtitle = "v0.0.1",
                Emblem = Resources.Instance.DdrLogoEmblemTexture,
                CanResize = true,
                CanCloseWithEscape = false,
                SavesPosition = true,
                SavesSize = true,
                Id = "DDR_MainView_ID"
            };

            _songListWindow = new SongListWindow(
                Resources.Instance.WindowBackgroundTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(40, 26, 913, 691)
            )
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Song List",
                Subtitle = "Dance Dance Rotation",
                Emblem = Resources.Instance.DdrLogoEmblemTexture,
                CanResize = true,
                CanCloseWithEscape = true,
                SavesPosition = true,
                SavesSize = true,
                Id = "DDR_SongList_ID"
            };

            _songInfoWindow = new StandardWindow(
                Resources.Instance.SongInfoBackground,
                new Rectangle(40, 26, 333, 676),
                new Rectangle(40, 26, 333, 676)
            )
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Song Info",
                Emblem = Resources.Instance.DdrLogoEmblemTexture,
                CanResize = false,
                CanCloseWithEscape = true,
                SavesPosition = true,
                Id = "DDR_SongInfo_ID"
            };
        }

        // Allows you to perform an action once your module has finished loading (once
        // <see cref="LoadAsync"/> has completed).  You must call "base.OnModuleLoaded(e)" at the
        // end for the <see cref="DanceDanceRotationModule.ModuleLoaded"/> event to fire.
        protected override void OnModuleLoaded(EventArgs e)
        {
            // Add a mug corner icon in the top left next to the other icons in guild wars 2 (e.g. inventory icon, Mail icon)
            _cornerIcon = new CornerIcon()
            {
                Icon             = Resources.Instance.DdrLogoEmblemTexture,
                BasicTooltipText = $"Dance Dance Rotation",
                Parent           = GameService.Graphics.SpriteScreen
            };

            // Load these on OnModuleLoaded, otherwise SavesPosition seems to fail
            _mainView = new MainView();
            _songListView = new SongListView();
            _songInfoView = new SongInfoView();

            _mainWindow.Show(_mainView);

            _cornerIcon.Click += delegate
            {
                _mainWindow.ToggleWindow();
            };

            // Set up a listener for when the setting is changed.
            // This ID is then looked up in the Repo and broadcast as a Song for everything else.
            SelectedSong.SettingChanged +=
                delegate(object sender, ValueChangedEventArgs<Song.ID> args)
                {
                    SongRepo.SetSelectedSong(args.NewValue);
                };
            SongRepo.SetSelectedSong(SelectedSong.Value);

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        // Allows your module to run logic such as updating UI elements,
        // checking for conditions, playing audio, calculating changes, etc.
        // This method will block the primary Blish HUD loop, so any long
        // running tasks should be executed on a separate thread to prevent
        // slowing down the overlay.
        protected override void Update(GameTime gameTime)
        {
            _mainView?.Update(gameTime);
        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {
            Resources.Instance.Unload();

            _mainWindow?.Dispose();
            _cornerIcon?.Dispose();
            _songListWindow?.Dispose();
            _songInfoWindow?.Dispose();

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            DanceDanceRotationModuleInstance = null;
        }

        public void ToggleSongList()
        {
            if (_songListWindow.Visible)
            {
                _songListWindow.Hide();
            }
            else
            {
                _songListWindow.Show(_songListView);
            }
        }

        public void ToggleSongInfo()
        {
            if (_songInfoWindow.Visible)
            {
                _songInfoWindow.Hide();
            }
            else
            {
                _songInfoWindow.Show(_songInfoView);
            }
        }

        internal static DanceDanceRotationModule DanceDanceRotationModuleInstance;
        private CornerIcon _cornerIcon;

        // Windows
        private DdrNotesWindow _mainWindow;
        private SongListWindow _songListWindow;
        private StandardWindow _songInfoWindow;
        // Views
        private MainView _mainView;
        private SongListView _songListView;
        private SongInfoView _songInfoView;

        public SongRepo SongRepo { get; set; }
    }
}