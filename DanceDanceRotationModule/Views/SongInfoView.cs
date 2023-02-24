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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace DanceDanceRotationModule.Views
{
    public class SongInfoView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<SongInfoView>();

        // Data
        private Song _song;
        private SongData _songData;

        // Views
        private Label _nameLabel;
        private Label _descriptionLabel;
        private StandardButton _copyButton;
        private Label _remapLabel;

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

            _nameLabel = new Label()
            {
                Text = "--",
                Font = GameService.Content.DefaultFont18,
                Parent = rootPanel
            };
            _descriptionLabel = new Label()
            {
                Text = "--",
                AutoSizeWidth = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Parent = rootPanel
            };
            _copyButton = new StandardButton()
            {
                Text = "Copy",
                Parent = rootPanel
            };
            _copyButton.Click += delegate
            {
                if (_song != null)
                {
                    ScreenNotification.ShowNotification("Copied Song JSON to Clipboard");
                    ClipboardUtil.WindowsClipboardService.SetTextAsync(
                        SongTranslator.ToJson(_song)
                    );
                }
            };

            FlowPanel utilityRemap = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                Title = "Remap Utility Skills",
                Parent = rootPanel
            };
            StandardButton RotateUtilitiesButton = new StandardButton()
            {
                Text = "Rotate",
                Parent = rootPanel
            };
            RotateUtilitiesButton.Click += delegate
            {
                if (_song == null)
                    return;

                DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongRepo
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
            _remapLabel = new Label()
            {
                Text = "",
                AutoSizeWidth = true,
                Font = GameService.Content.DefaultFont14,
                Parent = utilityRemap
            };

            DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongRepo.OnSelectedSongChanged +=
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
                _remapLabel.Text = "--";
                return;
            }

            _nameLabel.Text = _song.Name;
            _descriptionLabel.Text = _song.Description;

            _remapLabel.Text = String.Format(
                "1-2-3 => {0}-{1}-{2}",
                UtilitySkillDescription(_songData.Utility1Mapping),
                UtilitySkillDescription(_songData.Utility2Mapping),
                UtilitySkillDescription(_songData.Utility3Mapping)
            );
        }

        private String UtilitySkillDescription(SongData.UtilitySkillMapping mapping)
        {
            switch (mapping)
            {
                case SongData.UtilitySkillMapping.One:
                    return "1";
                case SongData.UtilitySkillMapping.Two:
                    return "2";
                case SongData.UtilitySkillMapping.Three:
                    return "3";
            }

            return "";
        }
    }
}