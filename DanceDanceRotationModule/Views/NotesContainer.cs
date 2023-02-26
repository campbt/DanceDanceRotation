using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.Storage;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using BlishContainer = Blish_HUD.Controls.Container;


namespace DanceDanceRotationModule.NoteDisplay
{
    public class NotesContainer : Blish_HUD.Controls.Container
    {

        // MARK: Inner Types

        internal class CurrentSequenceInfo
        {
            internal bool IsStarted { get; set; }
            internal TimeSpan StartTime { get; set; }
            internal int SequenceIndex { get; set; }
            internal List<ActiveNote> ActiveNotes = new List<ActiveNote>();
            internal List<HitText> HitTexts = new List<HitText>();
            internal int AbilityIconIndex { get; set; }
            internal List<AbilityIcon> AbilityIcons = new List<AbilityIcon>();

            public void Reset()
            {
                IsStarted = false;
                SequenceIndex = 0;
                StartTime = TimeSpan.Zero;
                AbilityIconIndex = 0;

                foreach (ActiveNote activeNote in ActiveNotes)
                {
                    activeNote.Dispose();
                }
                ActiveNotes.Clear();

                foreach (HitText hitText in HitTexts)
                {
                    hitText.Dispose();
                }
                HitTexts.Clear();

                foreach (AbilityIcon abilityIcon in AbilityIcons)
                {
                    abilityIcon.Dispose();
                }
                AbilityIcons.Clear();
            }
        }

        /**
         * Stores information about this container's window and is recalculated any time those bounds change
         * This is used to help position the active notes
         */
        internal class WindowInfo
        {
            internal const double TimeToReachEnd = 3.0;

            internal int NoteWidth { get; private set; }
            internal int NoteHeight { get; private set; }
            internal int LaneSpacing { get; private set; }
            /** Spacing at the very top, mostly for the note hit text to have somewhere to go */
            internal int VerticalPadding { get; private set; }

            /** Where to spawn new notes */
            internal int NewNoteXPosition { get; private set; }
            /** Where a note should be at when it is "perfect" */

            internal int AbilityIconsHeight { get; private set; }
            internal int AbilityIconsLocationY { get; private set; }
            internal int TargetLocationY { get; private set; }

            internal int HitPerfect { get; private set; }
            internal Range<double> HitRangePerfect { get; private set; }
            internal Range<double> HitRangeGreat { get; private set; }
            internal Range<double> HitRangeGood { get; private set; }
            internal Range<double> HitRangeBoo { get; private set; }
            internal int DestroyNotePosition { get; private set; }
            /** How fast the note should move per note change */
            internal double NotePositionChangePerSecond { get; private set; }

            public void Recalculate(int width, int height)
            {
                // How long a note moves before hitting the "perfect" position
                int perfectPosition = (int)(0.14 * width);

                VerticalPadding = (int)(HitText.MovePerSecond * (HitText.TotalLifeTimeMs / 1000.0));
                    LaneSpacing = (height - (VerticalPadding * 2)) / 100; // 5% of available space should be spacing
                if (DanceDanceRotationModule.DanceDanceRotationModuleInstance.ShowAbilityIconQueue.Value)
                {
                    // Show ability icons section as an extra "lane"
                    NoteHeight = (height - (2*VerticalPadding) - LaneSpacing * 5) / 7;
                    AbilityIconsLocationY = 0;
                    AbilityIconsHeight = NoteHeight;
                }
                else
                {
                    // Hide ability icons section
                    NoteHeight = (height - (2*VerticalPadding) - LaneSpacing * 5) / 6;
                    AbilityIconsLocationY = 0;
                    AbilityIconsHeight = 0;
                }
                NoteWidth = NoteHeight;
                TargetLocationY = VerticalPadding + AbilityIconsHeight;

                DestroyNotePosition = 0;

                // New notes spawn right at the edge of the window
                NewNoteXPosition = width;
                NotePositionChangePerSecond = (NewNoteXPosition - perfectPosition) / TimeToReachEnd;

                // Define the ranges for the other's
                HitPerfect = perfectPosition;
                HitRangePerfect = ConstructHitRange(perfectPosition, NotePositionChangePerSecond, 33);
                HitRangeGreat = ConstructHitRange(perfectPosition, NotePositionChangePerSecond, 92);
                HitRangeGood = ConstructHitRange(perfectPosition, NotePositionChangePerSecond, 142);
                HitRangeBoo = ConstructHitRange(perfectPosition, NotePositionChangePerSecond, 225);
            }

            /** Returns a range with min/max for a range. windowMs is the tolerance around the perfect position allowed for the range. */
            private Range<double> ConstructHitRange(int perfectPosition, double notePositionChangePerSecond, int windowMs)
            {
                return new Range<double>(
                    perfectPosition - (notePositionChangePerSecond * (windowMs / 1000.0)),
                    perfectPosition + (notePositionChangePerSecond * (windowMs / 1000.0))
                );
            }

            public Point GetNewNoteSize()
            {
                return new Point(NoteWidth, NoteHeight);
            }
            public Point GetNewNoteLocation(int lane)
            {
                var yPos = VerticalPadding + AbilityIconsHeight + (lane * (NoteHeight + LaneSpacing));
                return new Point(NewNoteXPosition, yPos);
            }
        }

        internal enum HitType
        {
            Perfect,
            Great,
            Good,
            Boo,
            Miss
        }

        /**
         * Represents a Note that is on the container and moving
         */
        internal class ActiveNote
        {
            private WindowInfo _windowInfo;
            internal Note Note { get; set; }
            internal Image Image { get; set; }
            internal Label Label { get; set; }
            // XPosition is stored here as a double instead of only using the Image,
            // because Image can only be in int positions, and the GameTime may need to
            // be more granular than that
            internal double XPosition { get; set; }

            private bool _isHit = false;

            internal bool ShouldRemove { get; private set; }

            public event EventHandler<HitType> OnHit;

            public ActiveNote(WindowInfo windowInfo, Note note, BlishContainer parent)
            {
                _windowInfo = windowInfo;
                this.Note = note;

                var keyBinding = DanceDanceRotationModule.DanceDanceRotationModuleInstance.GetKeyBindingForNoteType(note.NoteType);
                string text =
                    (keyBinding != null)
                        ? KeysExtensions.NotesString(keyBinding.Value)
                        : "?";

                // string text = keyBinding.Value.GetBindingDisplayText();
                int lane = NoteTypeExtensions.NoteLane(note.NoteType);

                // Respect "ShowAbilityIconsForNotes" preference
                var noteBackground =
                    DanceDanceRotationModule.DanceDanceRotationModuleInstance.ShowAbilityIconsForNotes.Value
                        ? Resources.Instance.GetAbilityIcon(note.AbilityId)
                        : NoteTypeExtensions.NoteImage(note.NoteType);

                this.Image = new Image(
                    noteBackground
                )
                {
                    Size = _windowInfo.GetNewNoteSize(),
                    Location = _windowInfo.GetNewNoteLocation(lane),
                    Opacity = 0.7f,
                    Parent = parent
                };

                BitmapFont font;
                if (text.Length < 3)
                {
                    if (Image.Height > 34)
                    {
                        font = GameService.Content.DefaultFont32;
                    }
                    else if (Image.Height > 20)
                    {
                        font = GameService.Content.DefaultFont18;
                    }
                    else if (Image.Height > 16)
                    {
                        font = GameService.Content.DefaultFont14;
                    }
                    else
                    {
                        font = GameService.Content.DefaultFont12;
                    }
                }
                else
                {
                    // Longer strings need to just always use a smaller font
                    if (Image.Height > 48)
                    {
                        font = GameService.Content.DefaultFont32;
                    }
                    else if (Image.Height > 32)
                    {
                        font = GameService.Content.DefaultFont18;
                    }
                    else if (Image.Height > 24)
                    {
                        font = GameService.Content.DefaultFont14;
                    }
                    else
                    {
                        font = GameService.Content.DefaultFont12;
                    }
                }

                Label = new Label() // this label is used as heading
                {
                    Text = text,
                    TextColor = Color.White,
                    Font = font,
                    StrokeText = true,
                    ShowShadow = true,
                    AutoSizeHeight = true,
                    AutoSizeWidth = true,
                    Parent = parent
                };
                // Must set this AFTER creation, so the auto width/height is used
                Label.Location = new Point(
                    (int)(XPosition) + ((Image.Width - Label.Width) / 2),
                    Image.Location.Y + ((Image.Height - Label.Height) / 2)
                );

                this.XPosition = Image.Location.X;

                _isHit = false;
                this.ShouldRemove = false;
            }

            public void Update(GameTime gameTime, double moveAmount)
            {
                XPosition -= moveAmount;
                if (
                    DanceDanceRotationModule.DanceDanceRotationModuleInstance.AutoHitWeapon1.Value &&
                    Note.NoteType == NoteType.Weapon1 &&
                    XPosition <= _windowInfo.HitPerfect
                )
                {
                    // Special Case: If AutoHitWeapon1 is enabled, remove the note in perfect
                    //               No hit text needs to be made
                    ShouldRemove = true;
                }
                else if (XPosition <= _windowInfo.DestroyNotePosition)
                {
                    ShouldRemove = true;
                }
                else
                {
                    if (_isHit == false && XPosition <= _windowInfo.HitRangeBoo.Min)
                    {
                        setHit(HitType.Miss);
                        Image.BackgroundColor = Color.Red;
                    }

                    // Move it
                    Image.Location = new Point(
                        // Center Image over X position
                        (int)(XPosition) - (Image.Width / 2),
                        Image.Location.Y
                    );
                    Label.Location = new Point(
                        Image.Location.X + ((Image.Width - Label.Width) / 2),
                        Label.Location.Y
                    );
                }
            }

            /** User pressed the hotkey for this note */
            public bool OnHotkeyPressed()
            {
                if (_isHit)
                {
                    // Ignore
                    return false;
                }

                if (
                    DanceDanceRotationModule.DanceDanceRotationModuleInstance.AutoHitWeapon1.Value &&
                    Note.NoteType == NoteType.Weapon1
                )
                {
                    // Ignore presses on the Weapon1 skills if auto hit weapon 1 is active
                    return false;
                }

                if (XPosition > _windowInfo.HitRangeBoo.Max)
                {
                    // Ignore presses not even in consideration
                    return false;
                }

                HitType hitType;
                if (_windowInfo.HitRangePerfect.IsInBetween(XPosition))
                {
                    hitType = HitType.Perfect;
                    ScreenNotification.ShowNotification("Perfect");
                }
                else if (_windowInfo.HitRangeGreat.IsInBetween(XPosition))
                {
                    hitType = HitType.Great;
                    ScreenNotification.ShowNotification("Great");
                }
                else if (_windowInfo.HitRangeGood.IsInBetween(XPosition))
                {
                    hitType = HitType.Good;
                    ScreenNotification.ShowNotification("Good");
                }
                else if (_windowInfo.HitRangeBoo.IsInBetween(XPosition))
                {
                    hitType = HitType.Boo;
                    ScreenNotification.ShowNotification("Boo");
                }
                else
                {
                    hitType = HitType.Miss;
                    ScreenNotification.ShowNotification("Miss");
                }

                ShouldRemove = true;
                setHit(hitType);
                return true;
            }

            public void Dispose()
            {
                Image.Dispose();
                Label.Dispose();
            }

            private void setHit(HitType hitType)
            {
                if (_isHit)
                    return;

                _isHit = true;
                Label.Visible = false;

                OnHit?.Invoke(this, hitType);
            }
        }

        // MARK: HitText

        /** Labels like "Perfect" that appear when a note is indicated. */
        internal class HitText
        {
            internal static double MovePerSecond = 10.0;
            internal static double TotalLifeTimeMs = 1500.0;

            private Label _label;
            private double _yPos;
            private double _remainLifeMs;

            public HitText(BlishContainer parent, HitType hitType, Point location)
            {
                String text;
                Color textColor;
                switch (hitType)
                {
                    case HitType.Perfect:
                        text = "Perfect";
                        textColor = Color.Aqua;
                        break;
                    case HitType.Great:
                        text = "Great";
                        textColor = Color.CornflowerBlue;
                        break;
                    case HitType.Good:
                        textColor = Color.SpringGreen;
                        text = "Good";
                        break;
                    case HitType.Boo:
                        textColor = Color.OrangeRed;
                        text = "Boo";
                        break;
                    case HitType.Miss:
                        textColor = Color.Red;
                        text = "Miss";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(hitType), hitType, null);
                }

                _label = new Label()
                {
                    Text = text,
                    TextColor = textColor,
                    Font = GameService.Content.DefaultFont18,
                    ShowShadow = true,
                    AutoSizeHeight = true,
                    AutoSizeWidth = true,
                    Location = location,
                    Parent = parent
                };
                _remainLifeMs = TotalLifeTimeMs;
                _yPos = location.Y;
            }

            public void Update(GameTime gameTime)
            {
                _remainLifeMs -= gameTime.ElapsedGameTime.Milliseconds;
                _yPos -= (gameTime.ElapsedGameTime.Milliseconds / 1000.0) * MovePerSecond;
                _label.Opacity = Math.Max(0.0f, (float)(_remainLifeMs / TotalLifeTimeMs));
                _label.Location = new Point(
                    _label.Location.X,
                    (int)_yPos
                );
            }

            public bool ShouldDispose()
            {
                return _remainLifeMs < 0;
            }

            public void Dispose()
            {
                _label.Dispose();
            }
        }

        // MARK: AbilityIcon

        internal class AbilityIcon
        {
            internal Note Note { get; set; }
            internal Image Image { get; set; }
            internal double XPosition { get; set; }
            internal bool IsDisappearing { get; private set; }
            internal bool ShouldDispose { get; private set; }

            private WindowInfo _windowInfo;

            public AbilityIcon(
                WindowInfo windowInfo,
                Note note,
                BlishContainer parent
            )
            {
                _windowInfo = windowInfo;
                Note = note;
                this.Image = new Image(
                    Resources.Instance.GetAbilityIcon(note.AbilityId)
                )
                {
                    Size = windowInfo.GetNewNoteSize(),
                    // Location doesn't really matter
                    Location = windowInfo.GetNewNoteLocation(6),
                    Opacity = 0.7f,
                    Parent = parent
                };

                ShouldDispose = false;
            }

            void SetTargetPosition(int xPos)
            {
                // TODO: Animate to here
                Image.Location = new Point(xPos, Image.Location.Y);
            }

            public void Update(GameTime gameTime, TimeSpan timeInRotation)
            {
                // TODO: Animation here
                if (timeInRotation.TotalMilliseconds > Note.TimeInRotation.TotalMilliseconds + (WindowInfo.TimeToReachEnd*1000))
                {
                    ShouldDispose = true;
                }
            }

            public void Dispose()
            {
                Image.Dispose();
            }
        }

        // MARK: Properties

        private List<Note> _currentSequence = new List<Note>();
        private SongData _songData;
        private TimeSpan _lastGameTime;
        private CurrentSequenceInfo _info = new CurrentSequenceInfo();
        private WindowInfo _windowInfo = new WindowInfo();

        // MARK: Events

        /** Emits the currently started state */
        public event EventHandler<bool> OnStartStop;

        // MARK: Constructor

        public NotesContainer()
        {
            _windowInfo.Recalculate(Width, Height);

            CreateTarget();
            UpdateTarget();

            // Listen for selected song changes to update the notes
            DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongRepo.OnSelectedSongChanged +=
                delegate(object sender, SelectedSongInfo songInfo)
                {
                    SetNoteSequence(
                        songInfo.Song.Notes,
                        songInfo.Data
                    );
                };
            DanceDanceRotationModule.DanceDanceRotationModuleInstance.ShowAbilityIconQueue.SettingChanged +=
                delegate
                {
                    Reset();
                    RecalculateLayout();
                };
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_targetCreated == false)
            {
                // RecalculateLayout can be evaluated before constructor is hit
                // Just ignore it
                return;
            }

            _windowInfo.Recalculate(Width, Height);
            UpdateTarget();
            RecalculateLayoutAbilityIcons();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
            Reset();
        }

        public void SetNoteSequence(
            List<Note> notes,
            SongData songData
        )
        {
            Reset();
            _currentSequence.Clear();
            _currentSequence.AddRange(notes);
            _songData = songData;
        }

        public void ToggleStart()
        {
            if (_info.IsStarted)
            {
                ScreenNotification.ShowNotification("Stopped");
                Reset();
            }
            else
            {
                ScreenNotification.ShowNotification("Starting");
                Reset();
                Start();
            }
        }

        public void Start()
        {
            if (_info.IsStarted == false)
            {
                _info.IsStarted = true;
                _info.StartTime = _lastGameTime;
                OnStartStop?.Invoke(this, _info.IsStarted);
                AddInitialAbilityIcons();
            }
        }

        public void Reset()
        {
            if (_info.IsStarted)
            {
                _info.IsStarted = false;
                _info.Reset();
                OnStartStop?.Invoke(this, _info.IsStarted);
            }
        }

        public bool IsStarted()
        {
            return _info.IsStarted;
        }

        public void Update(GameTime gameTime)
        {
            _lastGameTime = gameTime.TotalGameTime;

            if (_info.IsStarted == false)
            {
                return;
            }

            TimeSpan timeInRotation = _lastGameTime - _info.StartTime;
            double PlaybackRate = DanceDanceRotationModule.DanceDanceRotationModuleInstance.PlaybackRate.Value / 100.0;
            timeInRotation = timeInRotation.Multiply(new decimal(PlaybackRate));

            // Check to add notes
            if (_info.SequenceIndex < _currentSequence.Count)
            {
                Note nextNote = _currentSequence[_info.SequenceIndex];
                if (nextNote.TimeInRotation < timeInRotation)
                {
                    AddNote(nextNote);
                    _info.SequenceIndex += 1;
                }
            }

            // Update Active Notes and remove any that want to be destroyed
            double moveAmount = (_windowInfo.NotePositionChangePerSecond * (gameTime.ElapsedGameTime.Milliseconds) / 1000.0);
            for (int index = _info.ActiveNotes.Count - 1; index >= 0; index--)
            {
                ActiveNote activeNote = _info.ActiveNotes[index];
                activeNote.Update(gameTime, moveAmount);
                if (activeNote.ShouldRemove)
                {
                    activeNote.Dispose();
                    _info.ActiveNotes.RemoveAt(index);
                }
            }

            // Let all hit texts update and remove any that want to be disposed
            for (int index = _info.HitTexts.Count - 1; index >= 0; index--)
            {
                HitText hitText = _info.HitTexts[index];
                hitText.Update(gameTime);
                if (hitText.ShouldDispose())
                {
                    hitText.Dispose();
                    _info.HitTexts.RemoveAt(index);
                }
            }

            // if (_info.AbilityIcons.Count == 0)
            // {
            //     AddInitialAbilityIcons();
            // }
            for (int index = _info.AbilityIcons.Count - 1; index >= 0; index--)
            {
                AbilityIcon abilityIcon = _info.AbilityIcons[index];
                abilityIcon.Update(gameTime, timeInRotation);
                if (abilityIcon.ShouldDispose)
                {
                    abilityIcon.Dispose();
                    _info.AbilityIcons.RemoveAt(index);
                    if (_info.AbilityIconIndex < _currentSequence.Count)
                    {
                        AddAbilityIcon(_currentSequence[_info.AbilityIconIndex]);
                        _info.AbilityIconIndex += 1;
                        RecalculateLayoutAbilityIcons();
                    }
                }
            }
        }

        /**
         * Called when the user presses one of the hotkeys.
         * This should check for active notes that with that type and
         * "hit" them.
         */
        public void OnHotkeyPressed(NoteType noteType)
        {
            foreach (var activeNote in _info.ActiveNotes)
            {
                if (activeNote.Note.NoteType == noteType)
                {
                    if (activeNote.OnHotkeyPressed())
                    {
                        break;
                    }
                }
            }
        }

        // MARK: Adding things

        private void AddNote(Note note)
        {
            // Special: Remap utility ability notes based on settings
            note.NoteType = _songData.RemapNoteType(note.NoteType);

            var activeNote = new ActiveNote(
                _windowInfo,
                note,
                this
            );
            activeNote.OnHit += delegate(object sender, HitType hitType)
            {
                AddHitText(activeNote, hitType);
            };

            _info.ActiveNotes.Add(
                activeNote
            );
        }

        private void AddHitText(
            ActiveNote note,
            HitType hitType
        )
        {
            HitText hitText = new HitText(
                this,
                hitType,
                new Point(
                    note.Image.Location.X,
                    note.Image.Location.Y
                )
            );
            _info.HitTexts.Add(hitText);
        }

        // MARK: Target

        private void CreateTarget()
        {
            float TargetOpacity = 0.5f;
            _targetTop = new Image(Resources.Instance.DdrTargetTop)
            {
                Width = 64,
                Height = 24,
                Location = new Point(0, 0),
                Opacity = TargetOpacity,
                Parent = this
            };
            _targetBottom = new Image(Resources.Instance.DdrTargetBottom)
            {
                Width = 64,
                Height = 24,
                Location = new Point(0, 0),
                Opacity = TargetOpacity,
                Parent = this
            };
            _targetCircles = new List<Image>(6);
            for (int i = 0; i < 6; i++)
            {
                _targetCircles.Add(
                new Image(Resources.Instance.DdrTargetCircle)
                    {
                        Width = 64,
                        Height = 64,
                        Location = new Point(0, 0),
                        Opacity = TargetOpacity,
                        Parent = this
                    }
                );
            }
            _targetSpacers = new List<Image>(5);
            for (int i = 0; i < 5; i++)
            {
                _targetSpacers.Add(
                new Image(Resources.Instance.DdrTargetSpacer)
                    {
                        Width = 64,
                        Height = 24,
                        Location = new Point(0, 0),
                        Opacity = TargetOpacity,
                        Parent = this
                    }
                );
            }

            _targetCreated = true;
        }

        private void UpdateTarget()
        {
            double perfectCenter = (_windowInfo.HitRangePerfect.Max + _windowInfo.HitRangePerfect.Min) / 2.0;

            int targetWidth = _windowInfo.NoteWidth;

            int xPos = (int)(perfectCenter - (_windowInfo.NoteWidth / 2.0));
            int yPos = _windowInfo.TargetLocationY - _targetTop.Height;

            int roundEdgesHeight = (int)Math.Min(
                _windowInfo.VerticalPadding - 4,
                targetWidth * 0.375
            );

            _targetTop.Height = roundEdgesHeight;
            _targetTop.Width = targetWidth;
            _targetTop.Location = new Point(xPos, yPos);
            yPos += _targetTop.Height;

            for (int index = 0; index < _targetCircles.Count; index++)
            {
                _targetCircles[index].Height = _windowInfo.NoteHeight;
                _targetCircles[index].Width = targetWidth;
                _targetCircles[index].Location = new Point(xPos, yPos);
                yPos += _targetCircles[index].Height;

                if (index < _targetSpacers.Count)
                {
                    // Add a little bit of overlap to prevent gaps
                    _targetSpacers[index].Height = _windowInfo.LaneSpacing;
                    _targetSpacers[index].Width = targetWidth;
                    _targetSpacers[index].Location = new Point(xPos, yPos);
                    yPos += _windowInfo.LaneSpacing;
                }
            }

            _targetBottom.Height = roundEdgesHeight;
            _targetBottom.Width = targetWidth;
            _targetBottom.Location = new Point(xPos, yPos);
        }

        // MARK: Ability Icon Stuff

        private void AddInitialAbilityIcons()
        {
            if (DanceDanceRotationModule.DanceDanceRotationModuleInstance.ShowAbilityIconQueue.Value)
            {
                for (int index = 0, size = Math.Min(3, _currentSequence.Count); index < size; index++)
                {
                    AddAbilityIcon(_currentSequence[index]);
                    _info.AbilityIconIndex += 1;
                }
                RecalculateLayoutAbilityIcons();
            }
        }

        private void AddAbilityIcon(Note note)
        {
            var icon = new AbilityIcon(
                _windowInfo,
                note,
                this
            );
            _info.AbilityIcons.Add(
                icon
            );
        }
        private void RecalculateLayoutAbilityIcons()
        {
            // Update AbilityIcons
            var size = _windowInfo.NoteWidth;
            var xPos = _windowInfo.HitPerfect - (size/2);
            var yPos = _windowInfo.AbilityIconsLocationY;
            for (int index = 0; index < _info.AbilityIcons.Count; index++)
            {
                AbilityIcon abilityIconImage = _info.AbilityIcons[index];
                abilityIconImage.Image.Size = new Point(size, size);
                abilityIconImage.Image.Location = new Point(
                    xPos,
                    yPos
                );
                xPos += size;
            }
        }

        public void Destroy()
        {
            _targetTop.Dispose();
            _targetBottom.Dispose();
            foreach (var targetCircle in _targetCircles)
            {
                targetCircle.Dispose();
            }
            foreach (var targetSpacer in _targetSpacers)
            {
                targetSpacer.Dispose();
            }
            Dispose();
        }

        // MARK: Properties

        private bool _targetCreated;
        private Image _targetTop;
        private Image _targetBottom;
        private List<Image> _targetCircles;
        private List<Image> _targetSpacers;
    }


}