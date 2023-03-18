using System;
using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.Storage;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DanceDanceRotationModule.Views
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

        public SongListWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
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
                ControlPadding = new Vector2(4, 0),
                Parent = rootPanel
            };

            StandardButton addSongButton = new StandardButton()
            {
                Text = "Add From Clipboard",
                Parent = topPanel
            };
            addSongButton.Click += delegate
            {
                Logger.Info("AddFromClipboardButton Clicked");
                Logger.Info("Attempting to read in clipboard contents");
                string clipboardContents = ClipboardUtil.WindowsClipboardService.GetTextAsync().Result;
                var song = DanceDanceRotationModule.SongRepo.AddSong(
                    clipboardContents,
                    showNotification: true
                );

                if (song != null)
                {
                    DanceDanceRotationModule.SongRepo.SetSelectedSong(
                        song.Id
                    );
                }
            };
            StandardButton createSongButton = new StandardButton()
            {
                Text = "Create Song",
                Parent = topPanel
            };
            createSongButton.Click += delegate
            {
                Logger.Info("CreateSongButton Clicked");
                UrlHelper.OpenUrl("https://campbt.github.io/DanceDanceRotationComposer/create.html");
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

            DanceDanceRotationModule.SongRepo.OnSelectedSongChanged +=
                delegate(object sender, SelectedSongInfo info)
                {
                    if (_rows.Count == 0)
                    {
                        BuildSongList();
                    }
                    else if (info.Song != null)
                    {
                        // Selected song may change the display of rows, but it doesn't
                        // need to rebuild the song
                        foreach (var row in _rows)
                        {
                            row.OnSelectedSongInfoChanged(info.Song.Id);
                        }
                    }
                };
            DanceDanceRotationModule.SongRepo.OnSongsChanged +=
                delegate
                {
                    Logger.Trace("OnSongsChanged - Rebuilding Song List");
                    BuildSongList();
                };

            DanceDanceRotationModule.Settings.ShowOnlyCharacterClassSongs.SettingChanged +=
                delegate
                {
                    Logger.Trace("ShowOnlyCharacterClassSongs.SettingsChanged - Rebuilding Song List");
                    BuildSongList();
                };

            // Listen for profession change
            GameService.Gw2Mumble.PlayerCharacter.NameChanged += delegate
            {
                Logger.Trace("PlayerCharacter.NameChanged - Rebuilding Song List");
                BuildSongList();
            };
        }

        private void BuildSongList()
        {
            _songsListPanel.ClearChildren();
            _rows.Clear();

            var selectedSongId = DanceDanceRotationModule.SongRepo.GetSelectedSongId();
            var songList = DanceDanceRotationModule.SongRepo.GetAllSongs();

            // Get the profession of currently logged in character
            Profession playerCurrentProfession = ProfessionExtensions.ProfessionFromBuildTemplate(
                (int)GameService.Gw2Mumble.PlayerCharacter.Profession
            );

            var showOnlyCharacterClassSongs =
                DanceDanceRotationModule.Settings.ShowOnlyCharacterClassSongs.Value;

            List<Song> filteredSongs;
            if (
                showOnlyCharacterClassSongs &&
                playerCurrentProfession != Profession.Unknown // Unknown may mean the player is on the initial character select screen.
            )
            {
                filteredSongs = new List<Song>();
                foreach (var song in songList)
                {
                    //only add SongListRow for songs in the repo that match the current profession
                    if (song.Profession == playerCurrentProfession)
                    {
                        filteredSongs.Add(song);
                    }
                }
            }
            else
            {
                filteredSongs = songList;
            }

            // Sort song list by Profession, then by Elite, then by Song title
            filteredSongs.Sort(delegate(Song song1, Song song2)
            {
                if (song1.Profession != song2.Profession)
                {
                    return song1.Profession.CompareTo(song2.Profession);
                }
                if (song1.EliteName != song2.EliteName)
                {
                    return String.Compare(song1.EliteName, song2.EliteName, StringComparison.Ordinal);
                }
                return String.Compare(song1.Name, song2.Name, StringComparison.Ordinal);
            });

            Logger.Trace($"BuildSongList | {filteredSongs.Count}/{songList.Count} songs shown");

            foreach (var song in filteredSongs)
            {
                _rows.Add(
                    new SongListRow(song, selectedSongId)
                    {
                        WidthSizingMode = SizingMode.Fill,
                        HeightSizingMode = SizingMode.AutoSize,
                        Parent = _songsListPanel
                    }
                );
            }
        }

        private FlowPanel _songsListPanel;
        private List<SongListRow> _rows;
    }

    public class SongListRow : Panel
    {
        private static readonly Logger Logger = Logger.GetLogger<SongListRow>();

        internal Song Song { get; }

        private Checkbox Checkbox { get; }
        private Label NameLabel { get; }
        // private Label DescriptionLabel { get; }
        private Label ProfessionLabel { get; }
        private Image DeleteButton { get; }

        public SongListRow(Song song, Song.ID checkedId)
        {
            Song = song;
            Padding = new Thickness(12, 12, 12, 0);
            // FlowDirection = ControlFlowDirection.SingleLeftToRight;
            CanScroll = false;

            Checkbox = new Checkbox()
            {
                Padding = new Thickness(20, 20),
                Location = new Point(10, 28),
                Parent = this
            };
            Checkbox.CheckedChanged +=
                delegate(object sender, CheckChangedEvent checkChangedEvent)
                {
                    if (checkChangedEvent.Checked)
                    {
                        DanceDanceRotationModule.SongRepo.SetSelectedSong(
                            song.Id
                        );
                    }
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
                Text = song.EliteName,
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
                Logger.Info($"DeleteButton Clicked: song={song.Id.Name}");
                DanceDanceRotationModule.SongRepo.DeleteSong(song.Id);
            };

            Height = CalculateHeight();

            OnSelectedSongInfoChanged(checkedId);
        }

        public void OnSelectedSongInfoChanged(Song.ID checkedId)
        {
            var isSelectedSong = Song.Id.Equals(checkedId);
            Checkbox.Enabled = isSelectedSong == false;
            Checkbox.Checked = isSelectedSong;
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
                Width - DeleteButton.Width - 20,
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