using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.NoteDisplay;
using Microsoft.Xna.Framework;

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

            _startButton = new StandardButton() // this label is used as heading
            {
                Text = "Start",
                Left = 30,
                Parent = rootPanel
            };
            _startButton.Click += delegate
            {
                _notesContainer.ToggleStart();
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


        // private ViewContainer GetStandardPanel(Panel rootPanel, string title)
        // {
        //   ViewContainer standardPanel = new ViewContainer();
        //   standardPanel.WidthSizingMode = SizingMode.Fill;
        //   standardPanel.HeightSizingMode = SizingMode.AutoSize;
        //   standardPanel.Title = title;
        //   standardPanel.ShowBorder = true;
        //   standardPanel.Parent = (Container) rootPanel;
        //   return standardPanel;
        // }
        //
        // private void BuildOverlaySettings(Panel rootPanel)
        // {
        //   this.GetStandardPanel(rootPanel, Blish_HUD.Strings.GameServices.OverlayService.OverlaySettingsSection).Show((IView) new SettingsView(GameService.Overlay.OverlaySettings));
        //   this.GetStandardPanel(rootPanel, Blish_HUD.Strings.GameServices.OverlayService.OverlayDynamicHUDSection).Show((IView) new SettingsView(GameService.Overlay.DynamicHUDSettings));
        //   this.GetStandardPanel(rootPanel, Blish_HUD.Strings.GameServices.GraphicsService.GraphicsSettingsSection).Show((IView) new SettingsView(GameService.Graphics.GraphicsSettings));
        //   this.GetStandardPanel(rootPanel, Blish_HUD.Strings.GameServices.DebugService.DebugSettingsSection).Show((IView) new SettingsView(GameService.Debug.DebugSettings));
        // }

        private NotesContainer _notesContainer;
        private StandardButton _startButton;
    }
}