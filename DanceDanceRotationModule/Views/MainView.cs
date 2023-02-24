using System.Collections.ObjectModel;
using System.ComponentModel;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.NoteDisplay;
using DanceDanceRotationModule.Storage;
using Microsoft.Xna.Framework;
using Container = Blish_HUD.Controls.Container;

namespace DanceDanceRotationModule
{
    public class MainView : View
    {
        protected override void Build(Container buildPanel)
        {
            FlowPanel rootPanel = new FlowPanel();
            rootPanel.WidthSizingMode = SizingMode.Fill;
            rootPanel.HeightSizingMode = SizingMode.Fill;
            rootPanel.FlowDirection = ControlFlowDirection.SingleTopToBottom;
            rootPanel.CanScroll = false;
            rootPanel.Parent = buildPanel;

            _topPanel = new FlowPanel()
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                CanScroll = false,
                Parent = rootPanel
            };

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

            _activeSongName = new Label()
            {
                Text = "",
                Width = 1000,
                Font = GameService.Content.DefaultFont18,
                Parent = _topPanel
            };
            // Make value equal to currently selected song
            DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongRepo.OnSelectedSongChanged +=
                delegate(object sender, SelectedSongInfo songInfo)
                {
                    _activeSongName.Text = "  " + songInfo.Song.Name;
                };

            _notesContainer= new NotesContainer()
            {
                // Main Window Settings
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                AutoSizePadding = new Point(12, 12),
                Parent = rootPanel
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
        private Label _activeSongName;
    }
}