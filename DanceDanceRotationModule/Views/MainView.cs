using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings.UI.Views;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.NoteDisplay;
using DanceDanceRotationModule.Storage;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Color = Microsoft.Xna.Framework.Color;
using Container = Blish_HUD.Controls.Container;
using Image = Blish_HUD.Controls.Image;
using Point = Microsoft.Xna.Framework.Point;

namespace DanceDanceRotationModule
{
    public class MainView : View
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
            DanceDanceRotationModule.DanceDanceRotationModuleInstance.BackgroundOpacity.SettingChanged += delegate(object sender, ValueChangedEventArgs<float> args)
            {
                backgroundPanel.Opacity = args.NewValue;
                topPanelBackground.Opacity = 1 - args.NewValue;
            };
            backgroundPanel.Opacity = DanceDanceRotationModule.DanceDanceRotationModuleInstance.BackgroundOpacity.Value;
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

            // MARK: Start/Reset Button

            _startButton = new StandardButton() // this label is used as heading
            {
                Text = "Start",
                Left = 30,
                Parent = _topPanel
            };
            _startButton.Click += delegate
            {
                _notesContainer.ToggleStart();
            };

            // MARK: Song List Button

            var songListButton = new Image(
                Resources.Instance.SongListIcon
            )
            {
                Parent = _topPanel
            };
            ControlExtensions.ConvertToButton(songListButton);
            songListButton.Click += delegate
            {
                DanceDanceRotationModule.DanceDanceRotationModuleInstance.ToggleSongList();
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
                DanceDanceRotationModule.DanceDanceRotationModuleInstance.ToggleSongInfo();
            };
            // Make value equal to currently selected song
            DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongRepo.OnSelectedSongChanged +=
                delegate(object sender, SelectedSongInfo songInfo)
                {
                    activeSongName.Text = songInfo.Song.Name;
                };

            _notesContainer= new NotesContainer()
            {
                // Main Window Settings
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                AutoSizePadding = new Point(12, 12),
                Parent = flowPanel
            };
            _notesContainer.OnStartStop += delegate
            {
                if (_notesContainer.IsStarted())
                {
                    _startButton.Text = "Reset";
                }
                else
                {
                    _startButton.Text = "Start";
                }
            };
        }

        public void Update(GameTime gameTime)
        {
            _notesContainer?.Update(gameTime);
        }

        public void OnHotkeyPressed(NoteType noteType)
        {
            _notesContainer?.OnHotkeyPressed(noteType);
        }

        private NotesContainer _notesContainer;
        private StandardButton _startButton;
        private FlowPanel _topPanel;
    }
}