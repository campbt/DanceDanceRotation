﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.NoteDisplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace DanceDanceRotationModule.Storage
{
    public class SongListView : View
    {
        private static readonly Logger Logger = Logger.GetLogger<SongListView>();
        public SongListView()
        {
            DanceDanceRotationModule.DanceDanceRotationModuleInstance.SelectedSong.SettingChanged += delegate
            {
                BuildSongList();
            };
            DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongRepo.OnSongsChanged += delegate
            {
                BuildSongList();
            };
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

            // Panel topPanel = new Panel()
            // {
            //     Height = 30,
            //     WidthSizingMode = SizingMode.Fill,
            //     Parent = rootPanel
            // };
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
                try
                {
                    Logger.Info("Attempting to read in clipboard contents");
                    string clipboardContents = ClipboardUtil.WindowsClipboardService.GetTextAsync().Result;
                    Logger.Info("Attempting to decode into a song:\n" + clipboardContents);
                    Song song = SongTranslator.FromJson(clipboardContents);
                    Logger.Info("Decode Was Successful:\n" + song);
                    DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongRepo.AddSong(song);
                    ScreenNotification.ShowNotification("Added Song Successfully");
                }
                catch (Exception e)
                {
                    Logger.Warn(
                        "Failed to decode clipboard contents into a song:\n" +
                        e.Message + "\n" +
                        e
                    );
                    ScreenNotification.ShowNotification("Failed to decode song.");
                }
            };

            StandardButton resetButton = new StandardButton()
            {
                Text = "Reset",
                Parent = topPanel
            };
            resetButton.Click += delegate
            {
                ScreenNotification.ShowNotification("Resetting Songs");
                DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongRepo.Reset();
            };
            // resetButton.Location = new Point(
            //     topPanel.Width - resetButton.Width - 8,
            //     resetButton.Top
            // );

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

            BuildSongList();
        }

        private void BuildSongList()
        {
            _songsListPanel.ClearChildren();
            _rows.Clear();

            var selectedSong = DanceDanceRotationModule.DanceDanceRotationModuleInstance.SelectedSong.Value;
            var songList = DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongRepo.GetAllSongs();

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

        /**
         * Just used for development.
         * TODO: Remove
         */
        private void BuildDummySongList()
        {
            _songsListPanel.ClearChildren();
            var selectedSong = DanceDanceRotationModule.DanceDanceRotationModuleInstance.SelectedSong.Value;

            List<Song> songList = new List<Song>();
            for (int i = 0; i < 60; i++)
            {
                songList.Add(
                    new Song()
                    {
                        Id = new Song.ID() { Name = "song_" + i },
                        Description = "Lorum Ipsum something asdlkjb asoiasdlkj asdjlkasjlk",
                        Notes = new List<Note>()
                    }
                );
            }

            foreach (var song in songList)
            {
                var blah = new SongListRow(song, selectedSong)
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
        private Label DescriptionLabel { get; }
        private StandardButton CopyButton { get; }

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
                DanceDanceRotationModule.DanceDanceRotationModuleInstance.SelectedSong.Value = song.Id;
            };

            NameLabel = new Label()
            {
                Text = song.Name,
                Font = GameService.Content.DefaultFont18,
                Location = new Point(Checkbox.Width + 20, 8),
                Parent = this
            };
            DescriptionLabel = new Label()
            {
                Text = song.Description,
                AutoSizeWidth = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Location = new Point(
                    NameLabel.Left,
                    NameLabel.Bottom + 2
                ),
                Parent = this
            };
            CopyButton = new StandardButton()
            {
                Text = "Copy",
                Parent = this,
                Location = new Point(200, 0)
            };
            CopyButton.Click += delegate
            {
                ScreenNotification.ShowNotification("Copied Song to Clipboard");
                ClipboardUtil.WindowsClipboardService.SetTextAsync(
                    SongTranslator.ToJson(song)
                );
            };

            Height = 16 + NameLabel.Height + DescriptionLabel.Height;
        }

        protected override void OnContentResized(RegionChangedEventArgs e)
        {
            base.OnContentResized(e);

            if (NameLabel == null)
                return;

            var actualHeight = 16 + NameLabel.Height + DescriptionLabel.Height;
            // var centerY = actualHeight - (Checkbox.Height / 2);
            var centerY = actualHeight / 2;

            CopyButton.Location = new Point(
                // Width - copyButton.Width,
                Width - CopyButton.Width,
                centerY - (CopyButton.Height / 2)
                // NameLabel.Top + ((songDescription.Bottom - songName.Top) / 2) - (Checkbox.Height / 2)
            );

            // Center Checkbox
            Checkbox.Location = new Point(
                Checkbox.Location.X,
                centerY - (Checkbox.Height / 2)
                // Height - (Checkbox.Height / 2)
                // songName.Top + ((songDescription.Bottom - songName.Top) / 2) - (Checkbox.Height / 2)
            );

            NameLabel.Location = new Point(
                Checkbox.Width + 20,
                8
            );
            NameLabel.Width = Width - CopyButton.Width - 8 - NameLabel.Left;
            DescriptionLabel.Location = new Point(
                Checkbox.Width + 20,
                8 + NameLabel.Height
            );
            DescriptionLabel.Width = NameLabel.Width;
        }
    }
}