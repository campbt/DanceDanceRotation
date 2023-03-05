using Blish_HUD.Content;
using Blish_HUD.Graphics;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;

namespace DanceDanceRotationModule.Views
{
    /// <summary>
    /// The StandardWindow is a control meant to replicate the standard Guild Wars 2 windows.
    /// </summary>
    public class DdrNotesWindow : DdrWindowBase
    {
        public const int InitialWidth = 800;
        public const int InitialHeight = 400;

        public DdrNotesWindow(
          AsyncTexture2D background,
          Rectangle windowRegion,
          Rectangle contentRegion)
        {
          this.ConstructWindow(background, windowRegion, contentRegion);
        }

        public DdrNotesWindow(Texture2D background, Rectangle windowRegion, Rectangle contentRegion)
          : this((AsyncTexture2D) background, windowRegion, contentRegion)
        {
        }

        public DdrNotesWindow(
          AsyncTexture2D background,
          Rectangle windowRegion,
          Rectangle contentRegion,
          Point windowSize)
        {
          this.ConstructWindow(background, windowRegion, contentRegion, windowSize);
        }

        public DdrNotesWindow(
          Texture2D background,
          Rectangle windowRegion,
          Rectangle contentRegion,
          Point windowSize)
          : this((AsyncTexture2D) background, windowRegion, contentRegion, windowSize)
        {
        }

        /// <summary>Shows the window with the provided view.</summary>
        public void Show(IView view)
        {
          this.ShowView(view);
          this.Show();
        }

        /// <summary>
        /// Shows the window with the provided view if it is hidden.
        /// Hides the window if it is currently showing.
        /// </summary>
        public void ToggleWindow(IView view)
        {
          if (this.Visible)
            this.Hide();
          else
            this.Show(view);
        }

        /**
         * Overrides min/max allowed for the window, mostly allowing more Width than default, and constraining Height better.
         */
        override protected Point HandleWindowResize(Point newSize) =>
          new Point(
            MathHelper.Clamp(newSize.X, 400, 2048),
            MathHelper.Clamp(newSize.Y, 280, 800)
          );

    }

    /**
     * A customized copy of the [DdrWindowBase] developed by the Blish team
     */
    public abstract class DdrWindowBase : Container, IWindow, IViewContainer
    {
      private const int STANDARD_TITLEBAR_HEIGHT = 40;
      private const int STANDARD_TITLEBAR_VERTICAL_OFFSET = 11;
      private const int STANDARD_LEFTTITLEBAR_HORIZONTAL_OFFSET = 2;
      private const int STANDARD_RIGHTTITLEBAR_HORIZONTAL_OFFSET = 16;
      private const int STANDARD_TITLEOFFSET = 80;
      private const int STANDARD_SUBTITLEOFFSET = 20;
      private const int STANDARD_MARGIN = 16;
      private const int SIDEBAR_WIDTH = 46;
      private const int SIDEBAR_OFFSET = 3;
      private const int RESIZEHANDLE_SIZE = 16;
      private const string WINDOW_SETTINGS = "WindowSettings2";
      private static readonly Texture2D _textureTitleBarLeft = Control.Content.GetTexture("titlebar-inactive");
      private static readonly Texture2D _textureTitleBarRight = Control.Content.GetTexture("window-topright");
      private static readonly Texture2D _textureTitleBarLeftActive = Control.Content.GetTexture("titlebar-active");
      private static readonly Texture2D _textureTitleBarRightActive = Control.Content.GetTexture("window-topright-active");
      private static readonly Texture2D _textureExitButton = Control.Content.GetTexture("button-exit");
      private static readonly Texture2D _textureExitButtonActive = Control.Content.GetTexture("button-exit-active");
      private static readonly Texture2D _textureBlackFade = Control.Content.GetTexture("fade-down-46");
      // DDR: Custom settings
      private static readonly SettingCollection _windowSettings =
        DanceDanceRotationModule.Instance.SettingsManager.ModuleSettings.AddSubCollection(
          "DdrWindowBase");
      private readonly AsyncTexture2D _textureWindowCorner = AsyncTexture2D.FromAssetId(156008);
      private readonly AsyncTexture2D _textureWindowResizableCorner = AsyncTexture2D.FromAssetId(156009);
      private readonly AsyncTexture2D _textureWindowResizableCornerActive = AsyncTexture2D.FromAssetId(156010);
      private readonly AsyncTexture2D _textureSplitLine = AsyncTexture2D.FromAssetId(605026);
      private string _title = "No Title";
      private string _subtitle = "";
      private bool _canClose = true;
      private bool _canCloseWithEscape = true;
      private bool _canResize;
      private AsyncTexture2D _emblem;
      private bool _topMost;
      protected bool _savesPosition;
      protected bool _savesSize;
      private string _id;
      private bool _dragging;
      private bool _resizing;
      private readonly Tween _animFade;
      private bool _savedVisibility;
      private bool _showSideBar;
      private int _sideBarHeight = 100;
      private double _lastWindowInteract;
      private Rectangle _leftTitleBarDrawBounds = Rectangle.Empty;
      private Rectangle _rightTitleBarDrawBounds = Rectangle.Empty;
      private Rectangle _subtitleDrawBounds = Rectangle.Empty;
      private Rectangle _emblemDrawBounds = Rectangle.Empty;
      private Rectangle _sidebarInactiveDrawBounds = Rectangle.Empty;
      private Point _dragStart = Point.Zero;
      private Point _resizeStart = Point.Zero;
      private Point _contentMargin;
      private float _windowToTextureWidthRatio;
      private float _windowToTextureHeightRatio;
      private float _windowLeftOffsetRatio;
      private float _windowTopOffsetRatio;

      /// <summary>
      /// Registers the window so that its zindex can be calculated against other windows.
      /// </summary>
      [Obsolete("Windows no longer need to be registered or unregistered.", true)]
      public static void RegisterWindow(IWindow window)
      {
      }

      /// <summary>
      /// Unregisters the window so that its zindex is not longer calculated against other windows.
      /// </summary>
      [Obsolete("Windows no longer need to be registered or unregistered.", true)]
      public static void UnregisterWindow(IWindow window)
      {
      }

      public static IEnumerable<IWindow> GetWindows() => GameService.Graphics.SpriteScreen.GetChildrenOfType<IWindow>();

      /// <summary>
      /// Returns the calculated zindex offset.  This should be added to the base zindex (typically <see cref="F:Blish_HUD.Controls.Screen.WINDOW_BASEZINDEX" />) and returned as the zindex.
      /// </summary>
      public static int GetZIndex(IWindow thisWindow)
      {
        IWindow[] array = DdrWindowBase.GetWindows().ToArray<IWindow>();
        if (!((IEnumerable<IWindow>) array).Contains<IWindow>(thisWindow))
          throw new InvalidOperationException("thisWindow must be a direct child of GameService.Graphics.SpriteScreen before ZIndex can automatically be calculated.");
        return 41 + ((IEnumerable<IWindow>) array).OrderBy<IWindow, bool>((Func<IWindow, bool>) (window => window.TopMost)).ThenBy<IWindow, double>((Func<IWindow, double>) (window => window.LastInteraction)).TakeWhile<IWindow>((Func<IWindow, bool>) (window => window != thisWindow)).Count<IWindow>();
      }

      /// <summary>
      /// Gets or sets the active window. Returns null if no window is visible.
      /// </summary>
      public static IWindow ActiveWindow
      {
        get => DdrWindowBase.GetWindows().Where<IWindow>((Func<IWindow, bool>) (w => w.Visible)).OrderByDescending<IWindow, int>(new Func<IWindow, int>(DdrWindowBase.GetZIndex)).FirstOrDefault<IWindow>();
        set => value.BringWindowToFront();
      }

      public override int ZIndex
      {
        get => this._zIndex + DdrWindowBase.GetZIndex((IWindow) this);
        set => this.SetProperty<int>(ref this._zIndex, value, propertyName: nameof (ZIndex));
      }

      /// <summary>The text shown at the top of the window.</summary>
      public string Title
      {
        get => this._title;
        set => this.SetProperty<string>(ref this._title, value, true, nameof (Title));
      }

      /// <summary>
      /// The text shown to the right of the title in the title bar.
      /// This text is smaller and is normally used to show the current tab name and/or hotkey used to open the window.
      /// </summary>
      public string Subtitle
      {
        get => this._subtitle;
        set => this.SetProperty<string>(ref this._subtitle, value, true, nameof (Subtitle));
      }

      /// <summary>
      /// If <c>true</c>, draws an X icon on the window's titlebar and allows the user to close it by pressing it.
      /// <br /><br />Default: <c>true</c>
      /// </summary>
      public bool CanClose
      {
        get => this._canClose;
        set => this.SetProperty<bool>(ref this._canClose, value, propertyName: nameof (CanClose));
      }

      /// <summary>
      /// If <c>true</c>, the window will close when the user presses the escape key.
      /// <see cref="P:Blish_HUD.Controls.DdrWindowBase.CanClose" /> must also be set to <c>true</c>.
      /// <br /><br />Default: <c>true</c>
      /// </summary>
      public bool CanCloseWithEscape
      {
        get => this._canCloseWithEscape;
        set => this.SetProperty<bool>(ref this._canCloseWithEscape, value, propertyName: nameof (CanCloseWithEscape));
      }

      /// <summary>
      /// If <c>true</c>, allows the window to be resized by dragging the bottom right corner.
      /// <br /><br />Default: <c>false</c>
      /// </summary>
      public bool CanResize
      {
        get => this._canResize;
        set => this.SetProperty<bool>(ref this._canResize, value, propertyName: nameof (CanResize));
      }

      /// <summary>
      /// The emblem/badge displayed in the top left corner of the window.
      /// </summary>
      public Texture2D Emblem
      {
        get => (Texture2D) this._emblem;
        set => this.SetProperty<AsyncTexture2D>(ref this._emblem, (AsyncTexture2D) value, true, nameof (Emblem));
      }

      /// <summary>
      /// If <c>true</c>, this window will show on top of all other windows, regardless of which one had focus last.
      /// <br /><br />Default: <c>false</c>
      /// </summary>
      public bool TopMost
      {
        get => this._topMost;
        set => this.SetProperty<bool>(ref this._topMost, value, propertyName: nameof (TopMost));
      }

      /// <summary>
      /// If <c>true</c>, the window will remember its position between Blish HUD sessions.
      /// Requires that <see cref="P:Blish_HUD.Controls.DdrWindowBase.Id" /> be set.
      /// <br /><br />Default: <c>false</c>
      /// </summary>
      public bool SavesPosition
      {
        get => this._savesPosition;
        set => this.SetProperty<bool>(ref this._savesPosition, value, propertyName: nameof (SavesPosition));
      }

      /// <summary>
      /// If <c>true</c>, the window will remember its size between Blish HUD sessions.
      /// Requires that <see cref="P:Blish_HUD.Controls.DdrWindowBase.Id" /> be set.
      /// <br /><br />Default: <c>false</c>
      /// </summary>
      public bool SavesSize
      {
        get => this._savesSize;
        set => this.SetProperty<bool>(ref this._savesSize, value, propertyName: nameof (SavesSize));
      }

      /// <summary>
      /// A unique id to identify the window.  Used with <see cref="P:Blish_HUD.Controls.DdrWindowBase.SavesPosition" /> and <see cref="P:Blish_HUD.Controls.DdrWindowBase.SavesSize" /> as a unique
      /// identifier to remember where the window is positioned and its size.
      /// </summary>
      public string Id
      {
        get => this._id;
        set => this.SetProperty<string>(ref this._id, value, propertyName: nameof (Id));
      }

      /// <summary>Indicates if the window is actively being dragged.</summary>
      public bool Dragging
      {
        get => this._dragging;
        private set => this.SetProperty<bool>(ref this._dragging, value, propertyName: nameof (Dragging));
      }

      /// <summary>Indicates if the window is actively being resized.</summary>
      public bool Resizing
      {
        get => this._resizing;
        private set => this.SetProperty<bool>(ref this._resizing, value, propertyName: nameof (Resizing));
      }

      protected DdrWindowBase()
      {
        this.Opacity = 0.0f;
        this.Visible = false;
        this._zIndex = 41;
        this.ClipsBounds = false;
        GameService.Input.Mouse.LeftMouseButtonReleased += new EventHandler<MouseEventArgs>(this.OnGlobalMouseRelease);
        GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += (EventHandler<ValueEventArgs<bool>>) ((_param1, _param2) => DdrWindowBase.UpdateWindowBaseDynamicHUDCombatState(this));
        GameService.GameIntegration.Gw2Instance.IsInGameChanged += (EventHandler<ValueEventArgs<bool>>) ((_param1, _param2) => DdrWindowBase.UpdateWindowBaseDynamicHUDLoadingState(this));
        this._animFade = Control.Animation.Tweener.Tween<DdrWindowBase>(this, (object) new
        {
          Opacity = 1f
        }, 0.2f).Repeat().Reflect();
        this._animFade.Pause();
        this._animFade.OnComplete((Action) (() =>
        {
          this._animFade.Pause();
          if ((double) this._opacity > 0.0)
            return;
          this.Visible = false;
        }));
      }

      public static void UpdateWindowBaseDynamicHUDCombatState(DdrWindowBase wb)
      {
        if (GameService.Overlay.DynamicHUDWindows == DynamicHUDMethod.ShowPeaceful && GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
        {
          wb._savedVisibility = wb.Visible;
          if (!wb._savedVisibility)
            return;
          wb.Hide();
        }
        else
        {
          if (!wb._savedVisibility)
            return;
          wb.Show();
        }
      }

      public static void UpdateWindowBaseDynamicHUDLoadingState(DdrWindowBase wb)
      {
        if (GameService.Overlay.DynamicHUDLoading == DynamicHUDMethod.NeverShow && !GameService.GameIntegration.Gw2Instance.IsInGame)
        {
          wb._savedVisibility = wb.Visible;
          if (!wb._savedVisibility)
            return;
          wb.Hide();
        }
        else
        {
          if (!wb._savedVisibility)
            return;
          wb.Show();
        }
      }

      public override void UpdateContainer(GameTime gameTime)
      {
        if (this.Dragging)
        {
          this.Location = this.Location + (Control.Input.Mouse.Position - this._dragStart);
          this._dragStart = Control.Input.Mouse.Position;
        }
        else
        {
          if (!this.Resizing)
            return;
          this.Size = this.HandleWindowResize(this._resizeStart + (Control.Input.Mouse.Position - this._dragStart));
        }
      }

      /// <summary>
      /// Shows the window if it is hidden.
      /// Hides the window if it is currently showing.
      /// </summary>
      public void ToggleWindow()
      {
        if (this.Visible)
          this.Hide();
        else
          this.Show();
      }

      /// <summary>Shows the window.</summary>
      public override void Show()
      {
        this.BringWindowToFront();
        if (this.Visible)
          return;
        if (this.Id != null)
        {
          SettingEntry settingEntry1;
          if (this.SavesPosition && DdrWindowBase._windowSettings.TryGetSetting(this.Id, out settingEntry1))
          {
            if (!(settingEntry1 is SettingEntry<Point> settingEntry2))
              settingEntry2 = new SettingEntry<Point>();
            this.Location = settingEntry2.Value;
          }
          SettingEntry settingEntry3;
          if (this.SavesSize && DdrWindowBase._windowSettings.TryGetSetting(this.Id + "_size", out settingEntry3))
          {
            if (!(settingEntry3 is SettingEntry<Point> settingEntry4))
              settingEntry4 = new SettingEntry<Point>();
            this.Size = settingEntry4.Value;
          }
        }
        this.Location = new Point(MathHelper.Clamp(this._location.X, 0, GameService.Graphics.SpriteScreen.Width - 64), MathHelper.Clamp(this._location.Y, 0, GameService.Graphics.SpriteScreen.Height - 64));
        this.Opacity = 0.0f;
        this.Visible = true;
        this._animFade.Resume();
      }

      /// <summary>Hides the window.</summary>
      public override void Hide()
      {
        if (!this.Visible)
          return;
        this.Dragging = false;
        this._animFade.Resume();
        Control.Content.PlaySoundEffectByName("window-close");
      }

      public ViewState ViewState { get; protected set; }

      public IView CurrentView { get; protected set; }

      protected void ShowView(IView view)
      {
        this.ClearView();
        if (view == null)
          return;
        this.ViewState = ViewState.Loading;
        this.CurrentView = view;
        Progress<string> progress = new Progress<string>((Action<string>) (progressReport => { }));
        view.Loaded += new EventHandler<EventArgs>(this.OnViewBuilt);
        view.DoLoad((IProgress<string>) progress).ContinueWith(new Action<Task<bool>>(this.BuildView));
      }

      protected void ClearView()
      {
        if (this.CurrentView != null)
        {
          this.CurrentView.Loaded -= new EventHandler<EventArgs>(this.OnViewBuilt);
          this.CurrentView.DoUnload();
        }
        this.ClearChildren();
        this.ViewState = ViewState.None;
      }

      private void OnViewBuilt(object sender, EventArgs e)
      {
        this.CurrentView.Loaded -= new EventHandler<EventArgs>(this.OnViewBuilt);
        this.ViewState = ViewState.Loaded;
      }

      private void BuildView(Task<bool> loadResult)
      {
        if (!loadResult.Result)
          return;
        this.CurrentView.DoBuild((Container) this);
      }

      protected bool ShowSideBar
      {
        get => this._showSideBar;
        set => this.SetProperty<bool>(ref this._showSideBar, value, propertyName: nameof (ShowSideBar));
      }

      protected int SideBarHeight
      {
        get => this._sideBarHeight;
        set => this.SetProperty<int>(ref this._sideBarHeight, value, true, nameof (SideBarHeight));
      }

      double IWindow.LastInteraction => this._lastWindowInteract;

      protected Rectangle TitleBarBounds { get; private set; } = Rectangle.Empty;

      protected Rectangle ExitButtonBounds { get; private set; } = Rectangle.Empty;

      protected Rectangle ResizeHandleBounds { get; private set; } = Rectangle.Empty;

      protected Rectangle SidebarActiveBounds { get; private set; } = Rectangle.Empty;

      protected Rectangle BackgroundDestinationBounds { get; private set; } = Rectangle.Empty;

      public override void RecalculateLayout()
      {
        this._rightTitleBarDrawBounds = new Rectangle(this.TitleBarBounds.Width - DdrWindowBase._textureTitleBarRight.Width + 16, this.TitleBarBounds.Y - 11, DdrWindowBase._textureTitleBarRight.Width, DdrWindowBase._textureTitleBarRight.Height);
        Rectangle titleBarBounds = this.TitleBarBounds;
        int x = titleBarBounds.Location.X - 2;
        titleBarBounds = this.TitleBarBounds;
        int y = titleBarBounds.Location.Y - 11;
        int width = Math.Min(DdrWindowBase._textureTitleBarLeft.Width, this._rightTitleBarDrawBounds.Left - 2);
        int height = DdrWindowBase._textureTitleBarLeft.Height;
        this._leftTitleBarDrawBounds = new Rectangle(x, y, width, height);
        if (!string.IsNullOrWhiteSpace(this.Title) && !string.IsNullOrWhiteSpace(this.Subtitle))
          this._subtitleDrawBounds = this._leftTitleBarDrawBounds.OffsetBy(80 + (int) Control.Content.DefaultFont32.MeasureString(this.Title).Width + 20, 0);
        if (this._emblem != null)
          this._emblemDrawBounds = new Rectangle(this._leftTitleBarDrawBounds.X + 40 - this._emblem.Width / 2 - 16, this._leftTitleBarDrawBounds.Bottom - DdrWindowBase._textureTitleBarLeft.Height / 2 - this._emblem.Height / 2, this._emblem.Width, this._emblem.Height);
        this.ExitButtonBounds = new Rectangle(this._rightTitleBarDrawBounds.Right - 32 - DdrWindowBase._textureExitButton.Width, this._rightTitleBarDrawBounds.Y + 16, DdrWindowBase._textureExitButton.Width, DdrWindowBase._textureExitButton.Height);
        int num1 = this._leftTitleBarDrawBounds.Bottom - 11;
        int num2 = this.Size.Y - num1;
        this.SidebarActiveBounds = new Rectangle(this._leftTitleBarDrawBounds.X + 3, num1 - 3, 46, this.SideBarHeight);
        this._sidebarInactiveDrawBounds = new Rectangle(this._leftTitleBarDrawBounds.X + 3, num1 - 3 + this.SideBarHeight, 46, num2 - this.SideBarHeight);
        this.ResizeHandleBounds = new Rectangle(this.Width - this._textureWindowCorner.Width, this.Height - this._textureWindowCorner.Height, this._textureWindowCorner.Width, this._textureWindowCorner.Height);
      }

      protected bool MouseOverTitleBar { get; private set; }

      protected bool MouseOverExitButton { get; private set; }

      protected bool MouseOverResizeHandle { get; private set; }

      protected override void OnMouseMoved(MouseEventArgs e)
      {
        this.ResetMouseRegionStates();
        if (this.RelativeMousePosition.Y < this.TitleBarBounds.Bottom)
        {
          if (this.ExitButtonBounds.Contains(this.RelativeMousePosition))
            this.MouseOverExitButton = true;
          else
            this.MouseOverTitleBar = true;
        }
        else if (this._canResize)
        {
          Rectangle resizeHandleBounds = this.ResizeHandleBounds;
          if (resizeHandleBounds.Contains(this.RelativeMousePosition))
          {
            int x = this.RelativeMousePosition.X;
            resizeHandleBounds = this.ResizeHandleBounds;
            int num1 = resizeHandleBounds.Right - 16;
            if (x > num1)
            {
              int y = this.RelativeMousePosition.Y;
              resizeHandleBounds = this.ResizeHandleBounds;
              int num2 = resizeHandleBounds.Bottom - 16;
              if (y > num2)
                this.MouseOverResizeHandle = true;
            }
          }
        }
        base.OnMouseMoved(e);
      }

      private void OnGlobalMouseRelease(object sender, MouseEventArgs e)
      {
        if (!this.Visible)
          return;
        if (this.Id != null)
        {
          if (this.SavesPosition && this.Dragging)
          {
            if (!(DdrWindowBase._windowSettings[this.Id] is SettingEntry<Point> settingEntry))
              settingEntry = DdrWindowBase._windowSettings.DefineSetting<Point>(this.Id, this.Location);
            settingEntry.Value = this.Location;
          }
          else if (this.SavesSize && this.Resizing)
          {
            if (!(DdrWindowBase._windowSettings[this.Id + "_size"] is SettingEntry<Point> settingEntry))
              settingEntry = DdrWindowBase._windowSettings.DefineSetting<Point>(this.Id + "_size", this.Size);
            settingEntry.Value = this.Size;
          }
        }
        this.Dragging = false;
        this.Resizing = false;
      }

      protected override void OnMouseLeft(MouseEventArgs e)
      {
        this.ResetMouseRegionStates();
        base.OnMouseLeft(e);
      }

      protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
      {
        this.BringWindowToFront();
        if (this.MouseOverTitleBar)
        {
          this.Dragging = true;
          this._dragStart = Control.Input.Mouse.Position;
        }
        else if (this.MouseOverResizeHandle)
        {
          this.Resizing = true;
          this._resizeStart = this.Size;
          this._dragStart = Control.Input.Mouse.Position;
        }
        else if (this.MouseOverExitButton && this.CanClose)
          this.Hide();
        base.OnLeftMouseButtonPressed(e);
      }

      protected override void OnClick(MouseEventArgs e)
      {
        if (this.MouseOverResizeHandle && e.IsDoubleClick)
          this.Size = new Point(this.WindowRegion.Width, this.WindowRegion.Height + 40);
        base.OnClick(e);
      }

      private void ResetMouseRegionStates()
      {
        this.MouseOverTitleBar = false;
        this.MouseOverExitButton = false;
        this.MouseOverResizeHandle = false;
      }

      /// <summary>
      /// Modifies the window size as it's being resized.
      /// Override to lock the window size at specific intervals or implement other resize behaviors.
      /// </summary>
      protected virtual Point HandleWindowResize(Point newSize) => new Point(MathHelper.Clamp(newSize.X, Math.Max(this.ContentRegion.X + this._contentMargin.X + 16, this._subtitleDrawBounds.Left + 16), 1024), MathHelper.Clamp(newSize.Y, this.ShowSideBar ? this._sidebarInactiveDrawBounds.Top + 16 : this.ContentRegion.Y + this._contentMargin.Y + 16, 1024));

      public void BringWindowToFront() => this._lastWindowInteract = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;

      protected AsyncTexture2D WindowBackground { get; set; }

      protected Rectangle WindowRegion { get; set; }

      protected Rectangle WindowRelativeContentRegion { get; set; }

      protected void ConstructWindow(
        AsyncTexture2D background,
        Rectangle windowRegion,
        Rectangle contentRegion)
      {
        this.ConstructWindow(background, windowRegion, contentRegion, new Point(windowRegion.Width, windowRegion.Height + 40));
      }

      protected void ConstructWindow(
        Texture2D background,
        Rectangle windowRegion,
        Rectangle contentRegion)
      {
        this.ConstructWindow((AsyncTexture2D) background, windowRegion, contentRegion);
      }

      protected void ConstructWindow(
        AsyncTexture2D background,
        Rectangle windowRegion,
        Rectangle contentRegion,
        Point windowSize)
      {
        this.WindowBackground = background;
        this.WindowRegion = windowRegion;
        this.WindowRelativeContentRegion = contentRegion;
        // DDR: Adjust padding so the content is flush with the title bar
        this.Padding = new Thickness(
          (float) Math.Max(windowRegion.Top - 40, 11) + 16,
          (float) (background.Width - windowRegion.Right),
          (float) (background.Height - windowRegion.Bottom + 40),
          (float) windowRegion.Left
        );
        // this.Padding = new Thickness((float) Math.Max(windowRegion.Top - 40, 11), (float) (background.Width - windowRegion.Right), (float) (background.Height - windowRegion.Bottom + 40), (float) windowRegion.Left);
        this.ContentRegion = new Rectangle(contentRegion.X - (int) this.Padding.Left, contentRegion.Y + 40 - (int) this.Padding.Top, contentRegion.Width, contentRegion.Height);
        this._contentMargin = new Point(windowRegion.Right - contentRegion.Right, windowRegion.Bottom - contentRegion.Bottom);
        this._windowToTextureWidthRatio = (float) (this.ContentRegion.Width + this._contentMargin.X + this.ContentRegion.X) / (float) background.Width;
        this._windowToTextureHeightRatio = (float) (this.ContentRegion.Height + this._contentMargin.Y + this.ContentRegion.Y - 40) / (float) background.Height;
        this._windowLeftOffsetRatio = (float) -windowRegion.Left / (float) background.Width;
        this._windowTopOffsetRatio = (float) -windowRegion.Top / (float) background.Height;
        this.Size = windowSize;
      }

      protected void ConstructWindow(
        Texture2D background,
        Rectangle windowRegion,
        Rectangle contentRegion,
        Point windowSize)
      {
        this.ConstructWindow((AsyncTexture2D) background, windowRegion, contentRegion, windowSize);
      }

      protected override void OnResized(ResizedEventArgs e)
      {
        this.ContentRegion = new Rectangle(this.ContentRegion.X, this.ContentRegion.Y, this.Width - this.ContentRegion.X - this._contentMargin.X, this.Height - this.ContentRegion.Y - this._contentMargin.Y);
        this.CalculateWindow();
        base.OnResized(e);
      }

      private void CalculateWindow()
      {
        this.TitleBarBounds = new Rectangle(0, 0, this.Size.X, 40);
        int width = (int) ((double) (this.ContentRegion.Width + this._contentMargin.X + this.ContentRegion.X) / (double) this._windowToTextureWidthRatio);
        int height = (int) ((double) (this.ContentRegion.Height + this._contentMargin.Y + this.ContentRegion.Y - 40) / (double) this._windowToTextureHeightRatio);
        this.BackgroundDestinationBounds = new Rectangle((int) Math.Floor((double) this._windowLeftOffsetRatio * (double) width), (int) Math.Floor((double) this._windowTopOffsetRatio * (double) height + 40.0), width, height);
      }

      public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
      {
        this.PaintWindowBackground(spriteBatch);
        this.PaintSideBar(spriteBatch);
        this.PaintTitleBar(spriteBatch);
      }

      public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
      {
        this.PaintEmblem(spriteBatch);
        this.PaintTitleText(spriteBatch);
        this.PaintExitButton(spriteBatch);
        this.PaintCorner(spriteBatch);
      }

      private void PaintCorner(SpriteBatch spriteBatch)
      {
        if (this.CanResize)
          spriteBatch.DrawOnCtrl((Control) this, (Texture2D) (this.MouseOverResizeHandle || this.Resizing ? this._textureWindowResizableCornerActive : this._textureWindowResizableCorner), this.ResizeHandleBounds);
        else
          spriteBatch.DrawOnCtrl((Control) this, (Texture2D) this._textureWindowCorner, this.ResizeHandleBounds);
      }

      private void PaintSideBar(SpriteBatch spriteBatch)
      {
        if (!this.ShowSideBar)
          return;
        spriteBatch.DrawOnCtrl((Control) this, ContentService.Textures.Pixel, this.SidebarActiveBounds, Color.Black);
        spriteBatch.DrawOnCtrl((Control) this, DdrWindowBase._textureBlackFade, this._sidebarInactiveDrawBounds);
        spriteBatch.DrawOnCtrl((Control) this, (Texture2D) this._textureSplitLine, new Rectangle(this.SidebarActiveBounds.Right - this._textureSplitLine.Width / 2, this.SidebarActiveBounds.Top, this._textureSplitLine.Width, this._sidebarInactiveDrawBounds.Bottom - this.SidebarActiveBounds.Top));
      }

      private void PaintWindowBackground(SpriteBatch spriteBatch) => spriteBatch.DrawOnCtrl((Control) this, (Texture2D) this.WindowBackground, this.BackgroundDestinationBounds);

      private void PaintTitleBar(SpriteBatch spriteBatch)
      {
        if (this.MouseOver && this.MouseOverTitleBar)
        {
          spriteBatch.DrawOnCtrl((Control) this, DdrWindowBase._textureTitleBarLeftActive, this._leftTitleBarDrawBounds);
          spriteBatch.DrawOnCtrl((Control) this, DdrWindowBase._textureTitleBarRightActive, this._rightTitleBarDrawBounds);
        }
        else
        {
          spriteBatch.DrawOnCtrl((Control) this, DdrWindowBase._textureTitleBarLeft, this._leftTitleBarDrawBounds);
          spriteBatch.DrawOnCtrl((Control) this, DdrWindowBase._textureTitleBarRight, this._rightTitleBarDrawBounds);
        }
      }

      private void PaintTitleText(SpriteBatch spriteBatch)
      {
        if (string.IsNullOrWhiteSpace(this.Title))
          return;
        spriteBatch.DrawStringOnCtrl((Control) this, this.Title, Control.Content.DefaultFont32, this._leftTitleBarDrawBounds.OffsetBy(80, 0), ContentService.Colors.ColonialWhite);
        if (string.IsNullOrWhiteSpace(this.Subtitle))
          return;
        spriteBatch.DrawStringOnCtrl((Control) this, this.Subtitle, Control.Content.DefaultFont16, this._subtitleDrawBounds, Color.White);
      }

      private void PaintExitButton(SpriteBatch spriteBatch)
      {
        if (!this.CanClose)
          return;
        spriteBatch.DrawOnCtrl((Control) this, this.MouseOverExitButton ? DdrWindowBase._textureExitButtonActive : DdrWindowBase._textureExitButton, this.ExitButtonBounds);
      }

      private void PaintEmblem(SpriteBatch spriteBatch)
      {
        if (this._emblem == null)
          return;
        spriteBatch.DrawOnCtrl((Control) this, this.Emblem, this._emblemDrawBounds);
      }

      protected override void DisposeControl()
      {
        if (this.CurrentView != null)
        {
          this.CurrentView.Loaded -= new EventHandler<EventArgs>(this.OnViewBuilt);
          this.CurrentView.DoUnload();
        }
        GameService.Input.Mouse.LeftMouseButtonReleased -= new EventHandler<MouseEventArgs>(this.OnGlobalMouseRelease);
        base.DisposeControl();
      }
    }
}