using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using DanceDanceRotationModule.Storage;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DanceDanceRotationModule.Views
{
    public class HelpWindow : StandardWindow
    {
        public const int InitialWidth = 830;
        public const int InitialHeight = 360;

        public HelpWindow() :
            this(
                Resources.Instance.WindowBackgroundTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(40, 26, 913, 691)
                // Resources.Instance.WindowBackground2Texture,
                // new Rectangle(40, 26, 550, 540),
                // new Rectangle(40, 26, 550, 540)
            )
        {
            Parent = GameService.Graphics.SpriteScreen;
            Title = "Welcome to Dance Dance Rotation!";
            Emblem = Resources.Instance.DdrLogoEmblemTexture;
            CanResize = true;
            CanCloseWithEscape = false;
            Size = new Point(
                InitialWidth,
                InitialHeight
            );
            SavesPosition = false;
            SavesSize = false;
            Id = "DDR_HelpWindow_ID";
        }

        public HelpWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
        }

        protected override Point HandleWindowResize(Point newSize) =>
          new Point(
                InitialWidth,
                InitialHeight
          );

    }


    /**
     * A view that is presented to the user only at the very start, and provides initial set up instructions
     */
    public class HelpView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<HelpView>();

        protected override void Build(Container buildPanel)
        {
            Logger.Info("Loading Help View");

            Panel rootPanel = new Panel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                // FlowDirection = ControlFlowDirection.SingleLeftToRight,
                CanScroll = false,
                Parent = buildPanel
            };

            CreateHelpSetion(
                rootPanel: rootPanel,
                imageLocation: new Point(10, 10),
                imageTextureName: "help/setHotkeys.png",
                titleString: "Set Hotkeys",
                subtitleString: "in Blish Settings",
                buttonString: ""
            );
            // Not sure if it's actually possible to open the blish settings
            // openBlishSettingsButton.Click += delegate
            // {
            // };

            var openSongListButton = CreateHelpSetion(
                rootPanel: rootPanel,
                imageLocation: new Point(280, 10),
                imageTextureName: "help/selectSong.png",
                titleString: "Select Song",
                subtitleString: "",
                buttonString: "Open"
            );
            openSongListButton.Click += delegate
            {
                Logger.Info($"HelpView - ShowSongList selected");
                DanceDanceRotationModule.Instance.ShowSongList();
            };

            var openNotesContainerButton = CreateHelpSetion(
                rootPanel: rootPanel,
                imageLocation: new Point(550, 10),
                imageTextureName: "help/pressPlay.png",
                titleString: "Press Play",
                subtitleString: "",
                buttonString: "Open"
            );
            openNotesContainerButton.Click += delegate
            {
                Logger.Info($"HelpView - ShowNotesWindow selected");
                DanceDanceRotationModule.Instance.ShowNotesWindow();
            };
            // Only enable this button once the user has selected a song
            openNotesContainerButton.Enabled = false;

            void SettingsChangedDelegate(object sender, SelectedSongInfo info)
            {
                if (
                    info.Song != null &&
                    DanceDanceRotationModule.Settings.HasShownInitialSongInfo.Value == false
               )
                {
                    Logger.Info(
                        $"The first ever song was selected ({info.Song.Id.Name})." +
                        " Showing song info and enabling the open notes helper button."
                    );
                    openNotesContainerButton.Enabled = info.Song != null;

                    // Show the SongInfo pane once the first song is selected
                    DanceDanceRotationModule.Settings.HasShownInitialSongInfo.Value = true;
                    DanceDanceRotationModule.Instance.ShowSongInfo();

                    // Only need to register for this one time
                    DanceDanceRotationModule.SongRepo.OnSelectedSongChanged -= SettingsChangedDelegate;
                }
            }
            DanceDanceRotationModule.SongRepo.OnSelectedSongChanged += SettingsChangedDelegate;
        }

        private StandardButton CreateHelpSetion(
            Container rootPanel,
            Point imageLocation,
            string imageTextureName,
            string titleString,
            string subtitleString,
            string buttonString
        )
        {
            var titleTextColor = ContentService.Colors.ColonialWhite;
            var titleFont = GameService.Content.DefaultFont32;
            var subtitleTextColor = Color.White;
            var subtitleFont = GameService.Content.DefaultFont18;
            var centerPoint = imageLocation.X + 125;

            var image = new Image(
                // Loaded dynamically here because this view is often never loaded at all
                DanceDanceRotationModule.Instance.ContentsManager.GetTexture(imageTextureName)
            )
            {
                Size = new Point(250, 150),
                Location = imageLocation,
                Parent = rootPanel
            };

            var yPosition = image.Bottom + 20;

            var title = new Label()
            {
                Text = titleString,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Location = new Point(
                     centerPoint,
                     yPosition
                ),
                Font = titleFont,
                TextColor = titleTextColor,
                Parent = rootPanel
            };
            title.Left -= title.Width / 2;
            yPosition = title.Bottom + 8;

            if (subtitleString.Length > 0)
            {
                var subtitle = new Label()
                {
                    Text = subtitleString,
                    Height = 28,
                    AutoSizeWidth = true,
                    // AutoSizeHeight = true,
                    ClipsBounds = true,
                    Location = new Point(
                         centerPoint,
                         yPosition
                    ),
                    Font = subtitleFont,
                    TextColor = subtitleTextColor,
                    Parent = rootPanel
                };
                subtitle.Left -= subtitle.Width / 2;
            }

            if (buttonString.Length > 0)
            {
                var button = new StandardButton()
                {
                    Text = buttonString,
                    Location = new Point(
                         centerPoint,
                         yPosition
                    ),
                    Parent = rootPanel
                };
                button.Left -= button.Width / 2;
                return button;
            }

            return null;
        }
    }
}