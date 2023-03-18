using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using DanceDanceRotationModule.Storage;
using DanceDanceRotationModule.Util;
using DanceDanceRotationModule.Views;
using Microsoft.Xna.Framework;

namespace DanceDanceRotationModule
{
    [Export(typeof(Module))]
    public class DanceDanceRotationModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<DanceDanceRotationModule>();

        #region Service Managers

        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        // internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        #endregion

        // Ideally you should keep the constructor as is.
        // Use <see cref="Initialize"/> to handle initializing the module.
        [ImportingConstructor]
        public DanceDanceRotationModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Instance = this;
        }

        // Define the settings you would like to use in your module.  Settings are persistent
        // between updates to both Blish HUD and your module.
        protected override void DefineSettings(SettingCollection settings)
        {
            Settings = new ModuleSettings(settings);
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
            // Load content from the ref directory in the module.bhm automatically with the ContentsManager
            Resources.Instance.LoadResources(ContentsManager);

            // Load songs from settings
            SongRepo = new SongRepo();
            SongRepo.StartDirectoryWatcher();
            await SongRepo.Load();
        }

        // Allows you to perform an action once your module has finished loading (once
        // <see cref="LoadAsync"/> has completed).  You must call "base.OnModuleLoaded(e)" at the
        // end for the <see cref="DanceDanceRotationModule.ModuleLoaded"/> event to fire.
        protected override void OnModuleLoaded(EventArgs e)
        {
            LoadWindows();

            // Add a mug corner icon in the top left next to the other icons in guild wars 2 (e.g. inventory icon, Mail icon)
            _cornerIcon = new CornerIcon()
            {
                Icon             = Resources.Instance.DdrLogoEmblemTexture,
                BasicTooltipText = "Dance Dance Rotation",
                Parent           = GameService.Graphics.SpriteScreen
            };
            _cornerIcon.Click += delegate
            {
                ToggleNotesWindow();
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
            _notesView?.Update(gameTime);
        }

        // For a good module experience, your module should clean up ANY and ALL entities
        // and controls that were created and added to either the World or SpriteScreen.
        // Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        protected override void Unload()
        {
            Resources.Instance.Unload();

            _notesWindow?.Dispose();
            _cornerIcon?.Dispose();
            _songListWindow?.Dispose();
            _songInfoWindow?.Dispose();
            _helpWindow?.Dispose();
            SongRepo?.Dispose();

            // All static members must be manually unset
            // Static members are not automatically cleared and will keep a reference to your,
            // module unless manually unset.
            Instance = null;
            Settings = null;
            SongRepo = null;
        }

        /**
         * Creates the main windows and views for them
         * Load these on OnModuleLoaded, otherwise SavesPosition seems to fail
         */
        private void LoadWindows()
        {
            _notesWindow = new NotesWindow()
            {
                // This is just the initial location on first load of the module
                Location = new Point(
                    (GameService.Graphics.SpriteScreen.Width / 2) - (NotesWindow.InitialWidth / 2),
                    (GameService.Graphics.SpriteScreen.Height) - NotesWindow.InitialHeight - 180 /* 180 is trying to push this above the ability bar a bit */
                )
            };

            _songListWindow = new SongListWindow()
            {
                // This is just the initial location on first load of the module
                Location = new Point(
                    _notesWindow.Left,
                    _notesWindow.Top - 400 - 20
                )
            };

            _songInfoWindow = new SongInfoWindow()
            {
                // This is just the initial location on first load of the module
                Location = new Point(
                    _notesWindow.Left - 390,
                    _songListWindow.Top
                )
            };

            _notesView = new NotesView();
            _songListView = new SongListView();
            _songInfoView = new SongInfoView();

            // Show the Helper Menu
            if (Settings.HasShownHelpWindow.Value == false)
            {
                Logger.Info("Showing Help Window");
                Settings.HasShownHelpWindow.Value = true;
                _helpWindow = new HelpWindow()
                {
                    Location = new Point(
                        (GameService.Graphics.SpriteScreen.Width / 2) - (HelpWindow.InitialWidth / 2),
                        Math.Max(_songListWindow.Top - HelpWindow.InitialHeight, 100)
                    )
                };
                _helpWindow.Show(new HelpView());
                _helpWindow.CanResize = false; // Lets the background stayed resized
            }
        }

        public NotesContainer GetNotesContainer()
        {
            return _notesView?.GetNotesContainer();
        }

        public void ToggleNotesWindow()
        {
            Logger.Trace("ToggleNotesWindow");
            _notesWindow.ToggleWindow(_notesView);
        }

        public void ToggleSongList()
        {
            Logger.Trace("ToggleSongList");
            _songListWindow.ToggleWindow(_songListView);
        }

        public void ToggleSongInfo()
        {
            Logger.Trace("ToggleSongInfo");
            _songInfoWindow.ToggleWindow(_songInfoView);
        }

        public void ShowNotesWindow()
        {
            Logger.Trace("Showing Notes Window");
            _notesWindow.Show(_notesView);
        }

        public void ShowSongList()
        {
            Logger.Trace("Showing Song List Window");
            _songListWindow.Show(_songListView);
        }

        public void ShowSongInfo()
        {
            Logger.Trace("Showing Song Info Window");
            _songInfoWindow.Show(_songInfoView);
        }

        internal static DanceDanceRotationModule Instance;
        internal static ModuleSettings Settings;
        internal static SongRepo SongRepo { get; private set; }

        private CornerIcon _cornerIcon;

        // Windows
        private NotesWindow _notesWindow;
        private SongListWindow _songListWindow;
        private StandardWindow _songInfoWindow;
        private StandardWindow _helpWindow;
        // Views
        private NotesView _notesView;
        private SongListView _songListView;
        private SongInfoView _songInfoView;

    }
}