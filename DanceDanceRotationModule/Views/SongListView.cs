using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Settings.UI.Views;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.NoteDisplay;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace DanceDanceRotationModule.Storage
{

    public class SongListWindow : StandardWindow
    {
        public SongListWindow() :
            this(
                Resources.Instance.WindowBackgroundTexture,
                new Rectangle(40, 26, 913, 691),
                new Rectangle(40, 26, 913, 691)
            )
        {
            Parent = GameService.Graphics.SpriteScreen;
            Title = "Song List";
            Subtitle = "Dance Dance Rotation";
            Emblem = Resources.Instance.DdrLogoEmblemTexture;
            CanResize = true;
            CanCloseWithEscape = true;
            Size = new Point(500, 400);
            SavesPosition = true;
            SavesSize = true;
            Id = "DDR_SongList_ID";
        }

        public SongListWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {

        }

        public SongListWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
        }

        public SongListWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize)
        {
        }

        public SongListWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize)
        {
        }

        protected override Point HandleWindowResize(Point newSize) =>
          new Point(
            MathHelper.Clamp(newSize.X, 500, 500),
            MathHelper.Clamp(newSize.Y, 280, 800)
          );

    }

    public class SongListView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<SongListView>();

        protected override void Build(Container buildPanel)
        {
            FlowPanel rootPanel = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = false,
                Parent = buildPanel
            };

            FlowPanel topPanel = new FlowPanel()
            {
                // Height = 30,
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                Parent = rootPanel
            };

            StandardButton addSongButton = new StandardButton()
            {
                Text = "Add From Clipboard",
                Parent = topPanel
            };
            addSongButton.Click += delegate
            {
                Logger.Info("Attempting to read in clipboard contents");
                string clipboardContents = ClipboardUtil.WindowsClipboardService.GetTextAsync().Result;
                DanceDanceRotationModule.Instance.SongRepo.AddSong(clipboardContents);
            };
            StandardButton findSongsButton = new StandardButton()
            {
                Text = "Find Songs",
                Parent = topPanel
            };
            findSongsButton.Click += delegate
            {
                UrlHelper.OpenUrl("http://45.37.87.70:8080/songs/view#");
            };

            _songsListPanel = new FlowPanel()
            {
                // Width = buildPanel.Width,
                // Height = 700,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                Parent = rootPanel
            };
            _rows = new List<SongListRow>();

            DanceDanceRotationModule.Instance.SongRepo.OnSelectedSongChanged += delegate
            {
                BuildSongList();
            };
            DanceDanceRotationModule.Instance.SongRepo.OnSongsChanged += delegate
            {
                BuildSongList();
            };
        }

        private void BuildSongList()
        {
            _songsListPanel.ClearChildren();
            _rows.Clear();

            var selectedSong = DanceDanceRotationModule.Instance.SongRepo.GetSelectedSongId();
            var songList = DanceDanceRotationModule.Instance.SongRepo.GetAllSongs();

            // Sort song list by Profession, then by Song title
            songList.Sort(delegate(Song song1, Song song2)
            {
                if (song1.Profession != song2.Profession)
                {
                    return song1.Profession.CompareTo(song2.Profession);
                }
                return song1.Name.CompareTo(song2.Name);
            });

            foreach (var song in songList)
            {
                new SongListRow(song, selectedSong)
                {
                    WidthSizingMode = SizingMode.Fill,
                    HeightSizingMode = SizingMode.AutoSize,
                    Parent = _songsListPanel
                };
            }
        }

        private FlowPanel _songsListPanel;
        private List<SongListRow> _rows;
    }

    public class SongListRow : Panel
    {
        private Song song { get; set; }

        private Checkbox Checkbox { get; }
        private Label NameLabel { get; }
        // private Label DescriptionLabel { get; }
        private Label ProfessionLabel { get; }
        private Image DeleteButton { get; }

        public SongListRow(Song song, Song.ID checkedID)
        {
            Padding = new Thickness(12, 12, 12, 0);
            // FlowDirection = ControlFlowDirection.SingleLeftToRight;
            CanScroll = false;

            var isSelectedSong = song.Id.Equals(checkedID);
            Checkbox = new Checkbox()
            {
                Padding = new Thickness(20, 20),
                Checked = isSelectedSong,
                Enabled = isSelectedSong == false,
                Location = new Point(10, 28),
                Parent = this
            };
            Checkbox.CheckedChanged += delegate
            {
                DanceDanceRotationModule.Instance.SongRepo.SetSelectedSong(
                    song.Id
                );
            };

            NameLabel = new Label()
            {
                Text = song.Name,
                Font = GameService.Content.DefaultFont18,
                Location = new Point(Checkbox.Width + 20, 8),
                Parent = this
            };
            ProfessionLabel = new Label()
            {
                Text = ProfessionExtensions.GetProfessionDisplayText(song.Profession),
                AutoSizeWidth = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = ProfessionExtensions.GetProfessionColor(song.Profession),
                Location = new Point(
                    NameLabel.Left,
                    NameLabel.Bottom
                ),
                Parent = this
            };

            DeleteButton = new Image(
                Resources.Instance.ButtonDelete
            )
            {
                Size = ControlExtensions.ImageButtonSmallSize,
                Parent = this
            };
            ControlExtensions.ConvertToButton(DeleteButton);
            DeleteButton.Click += delegate
            {
                DanceDanceRotationModule.Instance.SongRepo.DeleteSong(song.Id);
            };

            Height = CalculateHeight();
        }

        private int CalculateHeight()
        {
            // return 16 + NameLabel.Height + DescriptionLabel.Height + ProfessionLabel.Height;
            return 16 + NameLabel.Height + ProfessionLabel.Height;
        }

        protected override void OnContentResized(RegionChangedEventArgs e)
        {
            base.OnContentResized(e);

            if (NameLabel == null)
                return;

            var actualHeight = CalculateHeight();
            var centerY = actualHeight / 2;

            // Delete Button
            DeleteButton.Location = new Point(
                Width - DeleteButton.Width - 8,
                centerY - (DeleteButton.Height / 2)
            );

            // Center Checkbox
            Checkbox.Location = new Point(
                Checkbox.Location.X,
                centerY - (Checkbox.Height / 2)
            );

            NameLabel.Location = new Point(
                Checkbox.Width + 20,
                8
            );
            NameLabel.Width = Width - 8 - NameLabel.Left - DeleteButton.Width;

            ProfessionLabel.Width = NameLabel.Width;
        }
    }
}