using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using DanceDanceRotationModule.Storage;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework;
using Container = Blish_HUD.Controls.Container;
using Image = Blish_HUD.Controls.Image;
using Point = Microsoft.Xna.Framework.Point;

namespace DanceDanceRotationModule.Views
{
    public class NotesView : View
    {
        protected override void Build(Container buildPanel)
        {
            Panel rootPanel = new Panel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = false,
                Parent = buildPanel
            };

            // Made this a separate panel so that the opacity could be controlled by the settings, without changing anything else
            Panel backgroundPanel = new Panel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                CanScroll = false,
                BackgroundTexture = Resources.Instance.NotesBg,
                ZIndex = -2,
                Parent = buildPanel
            };
            // Also make the top panel a separate one so that its opacity can only show up when the main view's opacity goes down
            // It looked a bit silly having this always on
            Panel topPanelBackground = new Panel()
            {
                WidthSizingMode = SizingMode.Fill,
                CanScroll = false,
                BackgroundTexture = Resources.Instance.NotesControlsBg,
                ZIndex = -1,
                Parent = buildPanel
            };
            DanceDanceRotationModule.Settings.BackgroundOpacity.SettingChanged += delegate(object sender, ValueChangedEventArgs<float> args)
            {
                backgroundPanel.Opacity = args.NewValue;
                topPanelBackground.Opacity = 1 - args.NewValue;
            };
            backgroundPanel.Opacity = DanceDanceRotationModule.Settings.BackgroundOpacity.Value;
            topPanelBackground.Opacity = 1 - backgroundPanel.Opacity;

            FlowPanel flowPanel = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = false,
                ZIndex = 5,
                Parent = rootPanel
            };


            _topPanel = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                // OuterControlPadding+AutoSizePadding: effectively form the full 4 point padding of the parent view
                OuterControlPadding = new Vector2(10, 10),
                AutoSizePadding = new Point(10, 10),
                // ControlPadding is padding in between the elements
                ControlPadding = new Vector2(4, 0),
                CanScroll = false,
                Parent = flowPanel
            };
            _topPanel.Resized += delegate
            {
                topPanelBackground.Location = _topPanel.Location;
                topPanelBackground.Height = _topPanel.Height;
            };

            // MARK: Song List Button

            var songListButton = new Image(
                Resources.Instance.ButtonList
            )
            {
                Size = ControlExtensions.ImageButtonSmallSize,
                Parent = _topPanel
            };
            ControlExtensions.ConvertToButton(songListButton);
            songListButton.BasicTooltipText = "Song List";
            songListButton.Click += delegate
            {
                DanceDanceRotationModule.Instance.ToggleSongList();
            };

            // MARK: Song Detail Button

            var songInfoButton = new Image(
                Resources.Instance.ButtonDetails
            )
            {
                Size = ControlExtensions.ImageButtonSmallSize,
                Parent = _topPanel
            };
            ControlExtensions.ConvertToButton(songInfoButton);
            songInfoButton.BasicTooltipText = "Song Info";
            songInfoButton.Click += delegate
            {
                DanceDanceRotationModule.Instance.ToggleSongInfo();
            };


            // MARK: Stop Button

            var stopButton = new Image(
                Resources.Instance.ButtonStop
            )
            {
                Size = ControlExtensions.ImageButtonSmallSize,
                Parent = _topPanel
            };
            ControlExtensions.ConvertToButton(stopButton);
            stopButton.BasicTooltipText = "Reset";
            stopButton.Click += delegate
            {
                _notesContainer.Stop();
            };

            // MARK: Play/Pause Button

            var playPauseButton = new Image(
                Resources.Instance.ButtonPlay
            )
            {
                Size = ControlExtensions.ImageButtonSmallSize,
                Parent = _topPanel
            };
            ControlExtensions.ConvertToButton(playPauseButton);
            playPauseButton.BasicTooltipText = "Play";
            playPauseButton.Click += delegate
            {
                if (_notesContainer.IsStarted() == false || _notesContainer.IsPaused())
                {
                    _notesContainer.Play();
                    playPauseButton.Texture = Resources.Instance.ButtonPause;
                    playPauseButton.BasicTooltipText = "Pause";
                }
                else
                {
                    _notesContainer.Pause();
                    playPauseButton.Texture = Resources.Instance.ButtonPlay;
                    playPauseButton.BasicTooltipText = "Play";
                }
            };

            // MARK: Song Title

            var activeSongName = new Label()
            {
                Text = "",
                Width = 1000,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont18,
                Parent = _topPanel
            };
            ControlExtensions.ConvertToButton(activeSongName);
            activeSongName.Click += delegate
            {
                DanceDanceRotationModule.Instance.ToggleSongInfo();
            };
            // Make value equal to currently selected song
            DanceDanceRotationModule.SongRepo.OnSelectedSongChanged +=
                delegate(object sender, SelectedSongInfo songInfo)
                {
                    activeSongName.Text =
                        songInfo.Song != null
                            ? songInfo.Song.Name
                            : "--";
                };

            _notesContainer= new NotesContainer()
            {
                // Main Window Settings
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                AutoSizePadding = new Point(12, 12),
                Parent = flowPanel
            };

            _notesContainer.OnStartStop +=
                delegate(object sender, bool isStarted)
                {
                    if (isStarted == false)
                    {
                        playPauseButton.Texture = Resources.Instance.ButtonPlay;
                    }
                };
        }

        public void Update(GameTime gameTime)
        {
            _notesContainer?.UpdateNotes(gameTime);
        }

        public NotesContainer GetNotesContainer()
        {
            return _notesContainer;
        }

        protected override void Unload()
        {
            base.Unload();

            _notesContainer.Dispose();
            _topPanel.Dispose();
        }

        private NotesContainer _notesContainer;
        private FlowPanel _topPanel;
    }
}