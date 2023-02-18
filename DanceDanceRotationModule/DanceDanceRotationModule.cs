using System;
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
using DanceDanceRotationModule.Util;
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

        // MARK: Hotkeys

        internal SettingEntry<KeyBinding> SwapWeapons { get; private set; }
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

        // // public static readonly KeyBinding CustomKeyBinding = GameService.Overlay.InteractKey.Value;
        // internal readonly KeyBinding CustomKeyBinding = CustomKey.Value;

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
            Weapon1          = DefineHotkeySetting(settings, NoteType.Weapon1);
            Weapon2          = DefineHotkeySetting(settings, NoteType.Weapon2);
            Weapon3          = DefineHotkeySetting(settings, NoteType.Weapon3);
            Weapon4          = DefineHotkeySetting(settings, NoteType.Weapon4);
            Weapon5          = DefineHotkeySetting(settings, NoteType.Weapon5);
            HealingSkill     = DefineHotkeySetting(settings, NoteType.HealingSkill);
            UtilitySkill1    = DefineHotkeySetting(settings, NoteType.UtilitySkill1);
            UtilitySkill2    = DefineHotkeySetting(settings, NoteType.UtilitySkill2 );
            UtilitySkill3    = DefineHotkeySetting(settings, NoteType.UtilitySkill3);
            EliteSkill       = DefineHotkeySetting(settings, NoteType.EliteSkill );
            ProfessionSkill1 = DefineHotkeySetting(settings, NoteType.ProfessionSkill1);
            ProfessionSkill2 = DefineHotkeySetting(settings, NoteType.ProfessionSkill2);
            ProfessionSkill3 = DefineHotkeySetting(settings, NoteType.ProfessionSkill3);
            ProfessionSkill4 = DefineHotkeySetting(settings, NoteType.ProfessionSkill4);
            ProfessionSkill5 = DefineHotkeySetting(settings, NoteType.ProfessionSkill5);
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
                _mainView.OnHotkeyPressed(noteType);
            };
            return retval;
        }

        public SettingEntry<KeyBinding> GetKeyBindingForNoteType(NoteType noteType)
        {
            SettingEntry<KeyBinding> retval = new SettingEntry<KeyBinding>();
            SettingsManager.ModuleSettings.TryGetSetting<KeyBinding>(noteType.ToString(), out retval);
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
                string fullDirectoryPath = DirectoriesManager.GetFullDirectoryPath(directoryName);
                var allFiles = Directory.EnumerateFiles(fullDirectoryPath, "*", SearchOption.AllDirectories).ToList();

                // example of how to log something in the blishhud.XXX-XXX.log file in %userprofile%\Documents\Guild Wars 2\addons\blishhud\logs
                // Logger.Info($"'{directoryName}' can be found at '{fullDirectoryPath}' and has {allFiles.Count} total files within it.");
            }

            // Load content from the ref directory in the module.bhm automatically with the ContentsManager
            Resources.Instance.LoadResources(ContentsManager);

            _mainWindow = new StandardWindow(
                Resources.Instance.WindowBackgroundTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(40, 26, 913, 691)
            )
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Dance Dance Rotation",
                Subtitle = "v0.0.1",
                Emblem = Resources.Instance.MugTexture,
                Location = new Point(300, 300),
                CanResize = true,
                CanCloseWithEscape = false,
                SavesPosition = true,
                SavesSize = true,
                Id = $"{nameof(DanceDanceRotationModule)}_ID"
            };

            _mainView = new MainView();

            // show blish hud overlay settings content inside the window
            _mainWindow.Show(_mainView);
        }

        // Allows you to perform an action once your module has finished loading (once
        // <see cref="LoadAsync"/> has completed).  You must call "base.OnModuleLoaded(e)" at the
        // end for the <see cref="DanceDanceRotationModule.ModuleLoaded"/> event to fire.
        protected override void OnModuleLoaded(EventArgs e)
        {
            // Add a mug corner icon in the top left next to the other icons in guild wars 2 (e.g. inventory icon, Mail icon)
            _cornerIcon = new CornerIcon()
            {
                Icon             = Resources.Instance.MugTexture,
                BasicTooltipText = $"Dance Dance Rotation",
                Parent           = GameService.Graphics.SpriteScreen
            };

            _cornerIcon.Click += delegate
            {
                _mainWindow.ToggleWindow();
            };

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
            // _notesContainer?.Destroy();

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            DanceDanceRotationModuleInstance = null;
        }


        internal static DanceDanceRotationModule DanceDanceRotationModuleInstance;
        private CornerIcon _cornerIcon;
        private StandardWindow _mainWindow;
        private MainView _mainView;

    }
}