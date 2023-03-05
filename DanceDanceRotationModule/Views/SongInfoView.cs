using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.NoteDisplay;
using DanceDanceRotationModule.Storage;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace DanceDanceRotationModule.Views
{
    public class SongInfoWindow : StandardWindow
    {
        public SongInfoWindow() :
            this(
                Resources.Instance.SongInfoBackground,
                new Rectangle(40, 26, 333, 676),
                // The content region is slightly larger because of the scroll bar
                new Rectangle(40, 26, 349, 676)
            )
        {
            Parent = GameService.Graphics.SpriteScreen;
            Title = "Song Info";
            Emblem = Resources.Instance.DdrLogoEmblemTexture;
            Size = new Point(333, 676);
            CanResize = false;
            CanCloseWithEscape = true;
            SavesPosition = true;
            Id = "DDR_SongInfo_ID";
        }

        public SongInfoWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {

        }

        public SongInfoWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
        }

        public SongInfoWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize)
        {
        }

        public SongInfoWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion, Point windowSize) : base(background, windowRegion, contentRegion, windowSize)
        {
        }

        // protected override Point HandleWindowResize(Point newSize) =>
        //   new Point(
        //     MathHelper.Clamp(newSize.X, 333, 333),
        //     MathHelper.Clamp(newSize.Y, 500, 1000)
        //   );

    }

    public class SongInfoView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<SongInfoView>();

        private static readonly Point UtilityIconSize = new Point(72, 72);
        private static readonly Point UtilityRemapIconSize = new Point(48, 48);

        // Data
        private Song _song;
        private SongData _songData;

        // Views
        private Label _nameLabel;
        private Label _descriptionLabel;
        private TextBox _buildUrlTextBox;
        private Image _openBuildUrlBuildTemplateButton;
        private TextBox _buildTemplateTextBox;
        private Image _copyBuildTemplateButton;
        private Image _remapUtilityImage1;
        private Image _remapUtilityImage2;
        private Image _remapUtilityImage3;
        private Label _playbackRateLabel;
        private TrackBar _playbackRateTrackbar;
        private Label _startAtLabel;
        private TrackBar _startAtTrackbar;
        private Label _noteSpeedLabel;
        private TrackBar _noteSpeedTrackBar;

        public SongInfoView()
        {
        }

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

            FlowPanel infoPanel = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                // OuterControlPadding+AutoSizePadding: effectively form the full 4 point padding of the parent view
                OuterControlPadding = new Vector2(10, 10),
                AutoSizePadding = new Point(10, 10),
                // ControlPadding is padding in between the elements
                ControlPadding = new Vector2(0, 8),
                CanScroll = true,
                Parent = rootPanel
            };

            _nameLabel = new Label()
            {
                Text = "--",
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont18,
                Parent = infoPanel
            };
            _descriptionLabel = new Label()
            {
                Text = "--",
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Parent = infoPanel
            };

            // Spacer

            new Label()
            {
                Text = "",
                Height = 4,
                Parent = infoPanel
            };

            // MARK: Build Template

            new Label()
            {
                Text = "Build URL",
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont12,
                TextColor = Color.LightGray,
                Parent = infoPanel
            };
            FlowPanel buildUrlPanel = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(8, 0),
                CanScroll = false,
                Parent = infoPanel
            };
            _buildUrlTextBox = new TextBox()
            {
                Text = "https://snowcrows.com/en/builds/elementalist/weaver/condition-weaver",
                Enabled = false,
                Font = GameService.Content.DefaultFont12,
                Parent = buildUrlPanel
            };
            _buildUrlTextBox.TextChanged += delegate
            {
                // Prevent typing
                _buildTemplateTextBox.Text = "https://snowcrows.com/en/builds/elementalist/weaver/condition-weaver";
            };
            _openBuildUrlBuildTemplateButton = new Image(
                Resources.Instance.ButtonOpenUrl
            )
            {
                Size = ControlExtensions.ImageButtonSmallSize,
                Parent = buildUrlPanel
            };
            ControlExtensions.ConvertToButton(_openBuildUrlBuildTemplateButton);
            _openBuildUrlBuildTemplateButton.Click += delegate
            {
                if (_song != null && _song.BuildUrl != null)
                {
                    UrlHelper.OpenUrl(_song.BuildUrl);
                }
                else
                {
                    ScreenNotification.ShowNotification("Failed to open link.");
                }
            };

            // MARK: Build Template

            new Label()
            {
                Text = "Build Template",
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont12,
                TextColor = Color.LightGray,
                Parent = infoPanel
            };
            FlowPanel buildLink = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(8, 0),
                CanScroll = false,
                Parent = infoPanel
            };
            _buildTemplateTextBox = new TextBox()
            {
                Text = "[&DQYfFRomOBV0AAAAcwAAAMsAAAA1FwAAEhcAAAAAAAAAAAAAAAAAAAAAAAA=]",
                Enabled = false,
                Font = GameService.Content.DefaultFont12,
                Parent = buildLink
            };
            _buildTemplateTextBox.TextChanged += delegate
            {
                // Prevent typing
                _buildTemplateTextBox.Text = "[&DQYfFRomOBV0AAAAcwAAAMsAAAA1FwAAEhcAAAAAAAAAAAAAAAAAAAAAAAA=]";
            };
            _copyBuildTemplateButton = new Image(
                Resources.Instance.ButtonCopy
            )
            {
                Size = ControlExtensions.ImageButtonSmallSize,
                Parent = buildLink
            };
            ControlExtensions.ConvertToButton(_copyBuildTemplateButton);
            _copyBuildTemplateButton.Click += delegate
            {
                if (_song != null && _song.BuildTemplateCode != null)
                {
                    ClipboardUtil.WindowsClipboardService.SetTextAsync(
                        _song.BuildTemplateCode
                    );
                    ScreenNotification.ShowNotification("Copied Build to Clipboard");
                }
                else
                {
                    ScreenNotification.ShowNotification("No Build Template to Copy");
                }

            };

            // MARK: Utility Skills Remap

            FlowPanel utilityRemap = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                // OuterControlPadding+AutoSizePadding: effectively form the full 4 point padding of the parent view
                OuterControlPadding = new Vector2(10, 10),
                AutoSizePadding = new Point(10, 10),
                ControlPadding = new Vector2(0, 10),
                Title = "Remap Utility Skills",
                Parent = rootPanel
            };
            new Label()
            {
                Text = "Set the utility icon positions you use if you prefer utility icons in different positions than the song's build template",
                Width = 280,
                AutoSizeHeight = true,
                WrapText = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Parent = utilityRemap
            };

            FlowPanel remapIcons = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                CanScroll = false,
                Parent = utilityRemap
            };
            _remapUtilityImage1 = new Image(
                Resources.Instance.UnknownAbilityIcon
            )
            {
                Size = new Point(72, 72),
                BasicTooltipText = "This should match in-game utility slot 1",
                Parent = remapIcons
            };
            _remapUtilityImage2 = new Image(
                Resources.Instance.UnknownAbilityIcon
            )
            {
                Size = UtilityIconSize,
                BasicTooltipText = "This should match in-game utility slot 2",
                Parent = remapIcons
            };
            _remapUtilityImage3 = new Image(
                Resources.Instance.UnknownAbilityIcon
            )
            {
                Size = UtilityIconSize,
                BasicTooltipText = "This should match in-game utility slot 3",
                Parent = remapIcons
            };
            var rotateRemapButtonPanel = new Panel()
            {
                Size = UtilityIconSize,
                Parent = remapIcons
            };
            var rotateRemapButtons = new Image(
                Resources.Instance.ButtonReload
            )
            {
                Size = UtilityRemapIconSize,
                Location = new Point(
                    (UtilityIconSize.X - UtilityRemapIconSize.X) / 2,
                    (UtilityIconSize.Y - UtilityRemapIconSize.Y) / 2
                ),
                BasicTooltipText = "Change the remapping ordering.",
                Parent = rotateRemapButtonPanel
            };
            rotateRemapButtons.Click += delegate
            {
                if (_song == null)
                    return;

                DanceDanceRotationModule.Instance.SongRepo
                    .UpdateData(
                        _song.Id,
                        songData =>
                        {
                            switch (songData.Utility1Mapping)
                            {
                                case SongData.UtilitySkillMapping.One:
                                    switch (songData.Utility2Mapping)
                                    {
                                        case SongData.UtilitySkillMapping.Two:
                                            // 1,2,3 => 1,3,2
                                            songData.Utility1Mapping = SongData.UtilitySkillMapping.One;
                                            songData.Utility2Mapping = SongData.UtilitySkillMapping.Three;
                                            songData.Utility3Mapping = SongData.UtilitySkillMapping.Two;
                                            break;
                                        case SongData.UtilitySkillMapping.Three:
                                            // 1,3,2 => 2,1,3
                                            songData.Utility1Mapping = SongData.UtilitySkillMapping.Two;
                                            songData.Utility2Mapping = SongData.UtilitySkillMapping.One;
                                            songData.Utility3Mapping = SongData.UtilitySkillMapping.Three;
                                            break;
                                    }
                                    break;
                                case SongData.UtilitySkillMapping.Two:
                                    switch (songData.Utility2Mapping)
                                    {
                                        case SongData.UtilitySkillMapping.One:
                                            // 2,1,3 => 2,3,1
                                            songData.Utility1Mapping = SongData.UtilitySkillMapping.Two;
                                            songData.Utility2Mapping = SongData.UtilitySkillMapping.Three;
                                            songData.Utility3Mapping = SongData.UtilitySkillMapping.One;
                                            break;
                                        case SongData.UtilitySkillMapping.Three:
                                            // 2,3,1 => 3,1,2
                                            songData.Utility1Mapping = SongData.UtilitySkillMapping.Three;
                                            songData.Utility2Mapping = SongData.UtilitySkillMapping.One;
                                            songData.Utility3Mapping = SongData.UtilitySkillMapping.Two;
                                            break;
                                    }
                                    break;
                                case SongData.UtilitySkillMapping.Three:
                                    switch (songData.Utility2Mapping)
                                    {
                                        case SongData.UtilitySkillMapping.One:
                                            // 3,1,2 => 3,2,1
                                            songData.Utility1Mapping = SongData.UtilitySkillMapping.Three;
                                            songData.Utility2Mapping = SongData.UtilitySkillMapping.Two;
                                            songData.Utility3Mapping = SongData.UtilitySkillMapping.One;
                                            break;
                                        case SongData.UtilitySkillMapping.Two:
                                            // 3,2,1 => 1,2,3
                                            songData.Utility1Mapping = SongData.UtilitySkillMapping.One;
                                            songData.Utility2Mapping = SongData.UtilitySkillMapping.Two;
                                            songData.Utility3Mapping = SongData.UtilitySkillMapping.Three;
                                            break;
                                    }
                                    break;
                            }
                            return songData;
                        }
                    );
            };

            // Spacer

            new Label()
            {
                Text = "",
                Height = 4,
                Parent = infoPanel
            };

            // MARK: Practice Settings

            FlowPanel practiceSettingsSection = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                // OuterControlPadding+AutoSizePadding: effectively form the full 4 point padding of the parent view
                OuterControlPadding = new Vector2(10, 10),
                AutoSizePadding = new Point(10, 10),
                ControlPadding = new Vector2(0, 10),
                Title = "Practice Settings",
                Parent = rootPanel
            };

            // Playback Rate

            FlowPanel playbackRateSection = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = practiceSettingsSection
            };
            new Label()
            {
                Text = "Playback Rate",
                BasicTooltipText = "Slows down the note speed.",
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Parent = playbackRateSection
            };
            _playbackRateLabel = new Label()
            {
                Text = "100%",
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont18,
                TextColor = Color.White,
                Parent = playbackRateSection
            };
            _playbackRateTrackbar = new TrackBar()
            {
                Enabled = true,
                MinValue = 10,
                MaxValue = 100,
                Value = 100,
                SmallStep = false,
                Parent = practiceSettingsSection
            };
            _playbackRateTrackbar.ValueChanged +=
                delegate(object sender, ValueEventArgs<float> args)
                {
                    if (_song != null)
                    {
                        DanceDanceRotationModule.Instance.SongRepo
                            .UpdateData(
                                _song.Id,
                                songData =>
                                {
                                    songData.PlaybackRate = args.Value / 100.0f;
                                    return songData;
                                }
                            );
                    }
                    _playbackRateLabel.Text = $"{args.Value}%";
                };

            // Start At

            FlowPanel startAtSection = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = practiceSettingsSection
            };
            new Label()
            {
                Text = "Start At",
                BasicTooltipText = "Sets what time the notes start",
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Parent = startAtSection
            };
            _startAtLabel = new Label()
            {
                Text = "0 : 00",
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont18,
                TextColor = Color.White,
                Parent = startAtSection
            };
            _startAtTrackbar = new TrackBar()
            {
                Enabled = true,
                MinValue = 0,
                MaxValue = 100,
                SmallStep = false,
                Value = 0,
                Parent = practiceSettingsSection
            };
            _startAtTrackbar.ValueChanged +=
                delegate(object sender, ValueEventArgs<float> args)
                {
                    if (_song != null)
                    {
                        DanceDanceRotationModule.Instance.SongRepo
                            .UpdateData(
                                _song.Id,
                                songData =>
                                {
                                    songData.StartAtSecond = (int)args.Value;
                                    return songData;
                                }
                            );
                    }

                    int minutes = (int)args.Value / 60;
                    int seconds = (int)args.Value % 60;
                    _startAtLabel.Text = $"{minutes} : {seconds:00}";
                };

            // Note Speed

            FlowPanel noteSpeedSection = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = practiceSettingsSection
            };
            new Label()
            {
                Text = "Note Speed",
                BasicTooltipText = "Sets how fast notes move.",
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Parent = noteSpeedSection
            };
            _noteSpeedLabel = new Label()
            {
                Text = "100%",
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont18,
                TextColor = Color.White,
                Parent = noteSpeedSection
            };
            _noteSpeedTrackBar = new TrackBar()
            {
                Enabled = true,
                MinValue = SongData.MinimumNotePositionChangePerSecond,
                MaxValue = SongData.MaximumNotePositionChangePerSecond,
                SmallStep = false,
                Value = SongData.DefaultNotePositionChangePerSecond,
                Parent = practiceSettingsSection
            };
            _noteSpeedTrackBar.ValueChanged +=
                delegate(object sender, ValueEventArgs<float> args)
                {
                    if (_song != null)
                    {
                        DanceDanceRotationModule.Instance.SongRepo
                            .UpdateData(
                                _song.Id,
                                songData =>
                                {
                                    songData.NotePositionChangePerSecond = (int)args.Value;
                                    return songData;
                                }
                            );
                    }

                    // While the raw value is in raw X position change per second, that isn't entirely useful to the user
                    // So, convert it to a percentage of the default note speed
                    int percentage = (int)(args.Value * 100.0f / SongData.DefaultNotePositionChangePerSecond);
                    _noteSpeedLabel.Text = percentage + "%";
                };

            // MARK: View Created. Set up subscriptions

            DanceDanceRotationModule.Instance.SongRepo.OnSelectedSongChanged +=
                delegate(object sender, SelectedSongInfo songInfo)
                {
                    OnSelectedSongChanged(songInfo);
                };
        }

        private void OnSelectedSongChanged(SelectedSongInfo songInfo)
        {
            _song = songInfo.Song;
            _songData = songInfo.Data;

            if (_song == null)
            {
                _nameLabel.Text = "--";
                _descriptionLabel.Text = "--";
                _buildUrlTextBox.Text = "--";
                _buildTemplateTextBox.Text = "--";
                _copyBuildTemplateButton.Visible = false;
                _openBuildUrlBuildTemplateButton.Visible = false;
                _remapUtilityImage1.Texture = Resources.Instance.UnknownAbilityIcon;
                _remapUtilityImage2.Texture = Resources.Instance.UnknownAbilityIcon;
                _remapUtilityImage3.Texture = Resources.Instance.UnknownAbilityIcon;
                _playbackRateTrackbar.Value = 100;
                _startAtTrackbar.Value = 0;
                _noteSpeedTrackBar.Value = SongData.DefaultNotePositionChangePerSecond;
                return;
            }

            _nameLabel.Text = _song.Name ?? "<no name>";
            _descriptionLabel.Text = _song.Description ?? "";
            _buildUrlTextBox.Text = _song.BuildUrl ?? "";
            _buildTemplateTextBox.Text = _song.BuildTemplateCode ?? "";
            _openBuildUrlBuildTemplateButton.Visible = !string.IsNullOrEmpty(_song.BuildUrl);
            _copyBuildTemplateButton.Visible = !string.IsNullOrEmpty(_song.BuildTemplateCode);

            GetRemappedAbilityTexture(_song.Utility1, _songData.Utility1Mapping);
            GetRemappedAbilityTexture(_song.Utility2, _songData.Utility2Mapping);
            GetRemappedAbilityTexture(_song.Utility3, _songData.Utility3Mapping);

            _playbackRateTrackbar.Value = (int)Math.Round(_songData.PlaybackRate * 100);
            _startAtTrackbar.Value = _songData.StartAtSecond;
            _startAtTrackbar.MaxValue =
                (int)_song.Notes.LastOrDefault().TimeInRotation.TotalSeconds;
            _noteSpeedTrackBar.Value = _songData.NotePositionChangePerSecond;
        }

        private void GetRemappedAbilityTexture(
            PaletteId paletteId,
            SongData.UtilitySkillMapping skillMapping
        )
        {
            Image image;
            switch (skillMapping)
            {
                case SongData.UtilitySkillMapping.One:
                    image = _remapUtilityImage1;
                    break;
                case SongData.UtilitySkillMapping.Two:
                    image = _remapUtilityImage2;
                    break;
                case SongData.UtilitySkillMapping.Three:
                    image = _remapUtilityImage3;
                    break;
                default:
                    return;
            }
            image.Texture = Resources.Instance.GetAbilityIcon(paletteId);
        }
    }
}