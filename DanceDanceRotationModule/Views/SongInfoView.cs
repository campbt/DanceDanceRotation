using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
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
    public class SongInfoWindow : StandardWindow
    {
        public SongInfoWindow() :
            this(
                Resources.Instance.SongInfoBackground,
                new Rectangle(40, 26, 333, 676),
                // The content region is slightly larger because of the scroll bar
                new Rectangle(40, 26, 344, 676)
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
        private Label _professionLabel;
        private Label _descriptionLabel;
        private TextBox _buildUrlTextBox;
        private Image _openBuildUrlBuildTemplateButton;
        private Label _buildTemplateLabel;
        private TextBox _buildTemplateTextBox;
        private Image _copyBuildTemplateButton;
        private Label _remapInstructionsText;
        private Image _remapUtilityImage1;
        private Image _remapUtilityImage2;
        private Image _remapUtilityImage3;
        private Image _rotateRemapButton;
        private Checkbox _noMissModeCheckbox;
        private Label _playbackRateLabel;
        private TrackBar _playbackRateTrackbar;
        private Label _startAtLabel;
        private TrackBar _startAtTrackbar;
        private Label _notePaceLabel;
        private TrackBar _notePaceTrackBar;

        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        protected override void Build(Container buildPanel)
        {
            FlowPanel rootPanel = new FlowPanel()
            {
                // TODO: This does get rid of scroll bars, interestingly enough
                // Width = buildPanel.Width + 8,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                OuterControlPadding = new Vector2(0, 0),
                AutoSizePadding = new Point(0, 0),
                Parent = buildPanel
            };

            // Minor offset because of the potential scrollbar
            // var childSectionWidth = buildPanel.Width - 18;
            var childSectionWidth = buildPanel.Width;

            FlowPanel namePanel = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                // OuterControlPadding+AutoSizePadding: effectively form the full 4 point padding of the parent view
                OuterControlPadding = new Vector2(10, 10),
                AutoSizePadding = new Point(10, 10),
                // ControlPadding is padding in between the elements
                ControlPadding = new Vector2(0, 2),
                Parent = rootPanel
            };
            _nameLabel = new Label()
            {
                Text = "--",
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont18,
                Parent = namePanel
            };
            _professionLabel = new Label()
            {
                Text = "",
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont14,
                Parent = namePanel
            };

            FlowPanel descriptionPanel = new FlowPanel()
            {
                Width = childSectionWidth,
                // WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                // OuterControlPadding+AutoSizePadding: effectively form the full 4 point padding of the parent view
                OuterControlPadding = new Vector2(10, 10),
                AutoSizePadding = new Point(10, 10),
                // ControlPadding is padding in between the elements
                ControlPadding = new Vector2(0, 8),
                Title = "Description",
                CanCollapse = true,
                Parent = rootPanel
            };

            _descriptionLabel = new Label()
            {
                Text = "--",
                Width = childSectionWidth,
                // AutoSizeWidth = true, <-- Don't use this here. It messes up height.
                AutoSizeHeight = true,
                WrapText = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Parent = descriptionPanel
            };

            // Spacer

            new Label()
            {
                Text = "",
                Height = 4,
                Parent = descriptionPanel
            };

            // MARK: Build Template

            new Label()
            {
                Text = "Build URL",
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont12,
                TextColor = Color.LightGray,
                Parent = descriptionPanel
            };
            FlowPanel buildUrlPanel = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(8, 0),
                CanScroll = false,
                Parent = descriptionPanel
            };
            _buildUrlTextBox = new TextBox()
            {
                Text = "--",
                Enabled = false,
                Font = GameService.Content.DefaultFont12,
                Parent = buildUrlPanel
            };
            _buildUrlTextBox.TextChanged += delegate
            {
                // Prevent typing
                _buildUrlTextBox.Text = _song?.BuildUrl ?? "";
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
                Logger.Info("OpenBuildUrl Clicked");
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

            _buildTemplateLabel = new Label()
            {
                Text = "Build Template",
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont12,
                TextColor = Color.LightGray,
                Parent = descriptionPanel
            };
            FlowPanel buildLink = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(8, 0),
                CanScroll = false,
                Parent = descriptionPanel
            };
            _buildTemplateTextBox = new TextBox()
            {
                Text = "--",
                Enabled = false,
                Font = GameService.Content.DefaultFont12,
                Parent = buildLink
            };
            _buildTemplateTextBox.TextChanged += delegate
            {
                // Prevent typing
                _buildTemplateTextBox.Text = _song?.BuildTemplateCode ?? "";
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
                Logger.Info("CopyBuild Clicked");
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

            // MARK: Practice Settings

            FlowPanel practiceSettingsSection = new FlowPanel()
            {
                Width = childSectionWidth,
                HeightSizingMode = SizingMode.AutoSize,
                // OuterControlPadding+AutoSizePadding: effectively form the full 4 point padding of the parent view
                OuterControlPadding = new Vector2(10, 10),
                AutoSizePadding = new Point(10, 10),
                ControlPadding = new Vector2(0, 10),
                Title = "Practice Settings",
                CanCollapse = true,
                Parent = rootPanel
            };

            // No Miss Mode

            FlowPanel noMissModeSection = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = practiceSettingsSection
            };
            _noMissModeCheckbox = new Checkbox()
            {
                Padding = new Thickness(26, 26),
                Parent = noMissModeSection
            };
            _noMissModeCheckbox.CheckedChanged +=
                delegate(object sender, CheckChangedEvent checkChangedEvent)
                {
                    if (_song != null)
                    {
                        DanceDanceRotationModule.SongRepo
                            .UpdateData(
                                _song.Id,
                                songData =>
                                {
                                    songData.NoMissMode = checkChangedEvent.Checked;
                                    return songData;
                                }
                            );
                    }
                };
            FlowPanel noMissModeTextSection = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Padding = new Thickness(0, 20),
                Parent = noMissModeSection
            };
            new Label()
            {
                Text = "No Miss Mode",
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Parent = noMissModeTextSection
            };
            new Label()
            {
                Text = "Notes will automatically pause in the perfect position and resume when hit. Great for learning the rotation keys without worrying about timing.",
                WrapText = true,
                Width = 260,
                // Height = 50,
                // AutoSizeWidth = true,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont12,
                TextColor = Color.LightGray,
                Parent = noMissModeTextSection
            };

            // Playback Rate - (Note Speed)

            FlowPanel playbackRateSection = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = practiceSettingsSection
            };
            new Label()
            {
                Text = "Note Speed",
                BasicTooltipText = "Setting under 100% slows down the song, adding more time in between notes.",
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
                        DanceDanceRotationModule.SongRepo
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

            // Note Pace

            FlowPanel notePaceSection = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Parent = practiceSettingsSection
            };
            new Label()
            {
                Text = "Note Pace",
                BasicTooltipText = "Sets how fast notes move. This does not affect how fast you have to press buttons.",
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Parent = notePaceSection
            };
            _notePaceLabel = new Label()
            {
                Text = "100%",
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont18,
                TextColor = Color.White,
                Parent = notePaceSection
            };
            _notePaceTrackBar = new TrackBar()
            {
                Enabled = true,
                MinValue = SongData.MinimumNotePositionChangePerSecond,
                MaxValue = SongData.MaximumNotePositionChangePerSecond,
                SmallStep = false,
                Value = SongData.DefaultNotePositionChangePerSecond,
                Parent = practiceSettingsSection
            };
            _notePaceTrackBar.ValueChanged +=
                delegate(object sender, ValueEventArgs<float> args)
                {
                    if (_song != null)
                    {
                        DanceDanceRotationModule.SongRepo
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
                    _notePaceLabel.Text = percentage + "%";
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
                        DanceDanceRotationModule.SongRepo
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

            // Spacer

            new Label()
            {
                Text = "",
                Height = 4,
                Parent = descriptionPanel
            };

            // MARK: Utility Skills Remap

            FlowPanel utilityRemap = new FlowPanel()
            {
                Width = childSectionWidth,
                HeightSizingMode = SizingMode.AutoSize,
                // OuterControlPadding+AutoSizePadding: effectively form the full 4 point padding of the parent view
                OuterControlPadding = new Vector2(10, 10),
                AutoSizePadding = new Point(10, 10),
                ControlPadding = new Vector2(0, 10),
                Title = "Remap Utility Skills",
                CanCollapse = true,
                Parent = rootPanel
            };
            _remapInstructionsText = new Label()
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
            _rotateRemapButton = new Image(
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
            _rotateRemapButton.Click += delegate
            {
                Logger.Info("RotateRemapping Clicked");
                if (_song == null)
                    return;

                DanceDanceRotationModule.SongRepo
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

            // MARK: View Created. Set up subscriptions

            DanceDanceRotationModule.SongRepo.OnSelectedSongChanged +=
                delegate(object sender, SelectedSongInfo songInfo)
                {
                    OnSelectedSongChanged(songInfo);
                };
        }

        private void OnSelectedSongChanged(SelectedSongInfo songInfo)
        {
            Logger.Trace($"OnSelectedSongChanged: {songInfo.Song?.Id.Name}");
            _song = songInfo.Song;
            _songData = songInfo.Data;

            if (_song == null)
            {
                _nameLabel.Text = "--";
                _descriptionLabel.Text = "--";
                _professionLabel.Text = "";
                _buildUrlTextBox.Text = "--";
                _buildTemplateTextBox.Text = "--";
                _copyBuildTemplateButton.Visible = false;
                _openBuildUrlBuildTemplateButton.Visible = false;
                _remapUtilityImage1.Texture = Resources.Instance.UnknownAbilityIcon;
                _remapUtilityImage2.Texture = Resources.Instance.UnknownAbilityIcon;
                _remapUtilityImage3.Texture = Resources.Instance.UnknownAbilityIcon;
                SetRemappingEnabled();
                _noMissModeCheckbox.Checked = false;
                _playbackRateTrackbar.Value = 100;
                _startAtTrackbar.Value = 0;
                _notePaceTrackBar.Value = SongData.DefaultNotePositionChangePerSecond;
                return;
            }

            _nameLabel.Text = _song.Name ?? "<no name>";
            _descriptionLabel.Text = _song.Description ?? "";
            _professionLabel.Text = _song.EliteName;
            _professionLabel.TextColor = ProfessionExtensions.GetProfessionColor(_song.Profession);
            _buildUrlTextBox.Text = _song.BuildUrl ?? "--";
            _openBuildUrlBuildTemplateButton.Visible = !string.IsNullOrEmpty(_song.BuildUrl);

            _buildTemplateLabel.Visible = !string.IsNullOrEmpty(_song.BuildTemplateCode);
            _buildTemplateTextBox.Visible = !string.IsNullOrEmpty(_song.BuildTemplateCode);
            _buildTemplateTextBox.Text = _song.BuildTemplateCode ?? "--";
            _copyBuildTemplateButton.Visible = !string.IsNullOrEmpty(_song.BuildTemplateCode);

            switch (_song.Profession)
            {
                case Profession.Common:
                    DisableRemapping(
                        "Utility remapping is disabled.",
                        textColor: Color.LightGray
                    );
                    break;
                case Profession.Revenant:
                    DisableRemapping(
                        "Remapping utilities is currently disabled for Revenant. Utility skills on both Legends must match the song's build.",
                        textColor: Color.Tomato
                    );
                    break;
                default:
                    SetRemappingEnabled();
                    break;
            }

            GetRemappedAbilityTexture(_song.Utility1, _songData.Utility1Mapping);
            GetRemappedAbilityTexture(_song.Utility2, _songData.Utility2Mapping);
            GetRemappedAbilityTexture(_song.Utility3, _songData.Utility3Mapping);

            _noMissModeCheckbox.Checked = _songData.NoMissMode;
            _playbackRateTrackbar.Value = (int)Math.Round(_songData.PlaybackRate * 100);
            _startAtTrackbar.Value = _songData.StartAtSecond;
            _startAtTrackbar.MaxValue =
                (int)_song.Notes.LastOrDefault().TimeInRotation.TotalSeconds;
            _notePaceTrackBar.Value = _songData.NotePositionChangePerSecond;
        }

        private void SetRemappingEnabled()
        {
            _rotateRemapButton.Visible = true;
            _remapInstructionsText.Text = "Set the utility icon positions you use if you prefer utility icons in different positions than the song's build template.";
            _remapInstructionsText.TextColor = Color.LightGray;
            _remapUtilityImage1.Visible = true;
            _remapUtilityImage2.Visible = true;
            _remapUtilityImage3.Visible = true;
        }

        private void DisableRemapping(
            string reason,
            Color textColor
        )
        {
            _rotateRemapButton.Visible = false;
            _remapInstructionsText.Text = reason;
            _remapInstructionsText.TextColor = textColor;
            _remapUtilityImage1.Visible = false;
            _remapUtilityImage2.Visible = false;
            _remapUtilityImage3.Visible = false;
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