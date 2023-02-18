using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Color = Microsoft.Xna.Framework.Color;
using BlishContainer = Blish_HUD.Controls.Container;


namespace DanceDanceRotationModule.NoteDisplay
{
    public class NotesContainer : Blish_HUD.Controls.Container
    {
        internal class CurrentSequenceInfo
        {
            internal bool IsStarted { get; set; }
            internal TimeSpan StartTime { get; set; }
            internal int SequenceIndex { get; set; }
            internal List<ActiveNote> ActiveNotes = new List<ActiveNote>();
            internal List<HitText> HitTexts = new List<HitText>();

            public void Reset()
            {
                IsStarted = false;
                SequenceIndex = 0;
                StartTime = TimeSpan.Zero;

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
            }
        }

        /**
         * Stores information about this container's window and is recalculated any time those bounds change
         * This is used to help position the active notes
         */
        internal class WindowInfo
        {
            internal int NoteWidth { get; private set; }
            internal int NoteHeight { get; private set; }
            internal int LaneSpacing { get; private set; }

            /** Where to spawn new notes */
            internal int NewNoteXPosition { get; private set; }
            /** Where a note should be at when it is "perfect" */

            internal Range<double> HitRangePerfect { get; private set; }
            internal Range<double> HitRangeGreat { get; private set; }
            internal Range<double> HitRangeGood { get; private set; }
            internal Range<double> HitRangeBoo { get; private set; }
            internal int DestroyNotePosition { get; private set; }
            /** How fast the note should move per note change */
            internal double NotePositionChangePerSecond { get; private set; }

            public void Recalculate(int width, int height)
            {
                NoteHeight = (height - 60) / 7;
                LaneSpacing = NoteHeight / 3;
                NoteWidth = NoteHeight;

                DestroyNotePosition = 0;

                // New notes spawn right at the edge of the window
                NewNoteXPosition = width;
                // How long a note moves before hitting the "perfect" position
                double timeToReachEnd = 3.0;
                int perfectPosition = (int)(0.2 * width);
                NotePositionChangePerSecond = (NewNoteXPosition - perfectPosition) / timeToReachEnd;

                // Define the ranges for the other's
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
                var yPos = 60 + NoteHeight / 7 + (lane * NoteHeight) + ((lane - 1) * LaneSpacing);
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
            // XPosition is stored here as a double instead of only using the Image,
            // because Image can only be in int positions, and the GameTime may need to
            // be more granular than that
            internal double XPosition { get; set; }

            private bool _isHit = false;

            internal bool ShouldRemove { get; private set; }

            public event EventHandler<HitType> OnHit;

            public ActiveNote(WindowInfo windowInfo, Note note, Image image)
            {
                _windowInfo = windowInfo;
                this.Note = note;
                this.Image = image;
                this.XPosition = image.Location.X;
                _isHit = false;
                this.ShouldRemove = false;
            }

            public void MarkAsMissed()
            {
                if (_isHit)
                {
                    // Ignore
                    return;
                }

                _isHit = true;
                Image.BackgroundColor = Color.Red;
            }

            public void Update(GameTime gameTime, double moveAmount)
            {
                XPosition -= moveAmount;
                if (XPosition <= _windowInfo.DestroyNotePosition)
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
                        (int)(XPosition),
                        Image.Location.Y
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
            }

            private void setHit(HitType hitType)
            {
                if (_isHit)
                    return;

                EventHandler<HitType> onHit = this.OnHit;
                if (onHit != null)
                {
                    onHit.Invoke(this, hitType);
                }

                _isHit = true;
            }
        }

        /** Labels like "Perfect" that appear when a note is indicated. */
        internal class HitText
        {
            private static double MovePerSecond = 80.0;
            private static double TotalLifeTimeMs = 1500.0;

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

        private List<Note> _currentSequence = new List<Note>();
        private TimeSpan _lastGameTime;
        private CurrentSequenceInfo _info = new CurrentSequenceInfo();
        private WindowInfo _windowInfo = new WindowInfo();

        public NotesContainer()
        {
            // Main Window Settings
            BackgroundColor = Color.Black;
            Width = 800;
            Height = 200;
            // HeightSizingMode = SizingMode.AutoSize;
            // WidthSizingMode = SizingMode.AutoSize;
            Location = new Point(400, 400);
            Parent = GameService.Graphics.SpriteScreen;

            _startButton = new Label() // this label is used as heading
            {
                Text = "Start",
                TextColor = Color.Red,
                Font = GameService.Content.DefaultFont32,
                ShowShadow = true,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Location = new Point(10, 2),
                Parent = this
            };
            _startButton.Click += delegate
            {
                Start();
            };
            LoadDebugSequence();

            _windowInfo.Recalculate(Width, Height);
        }

        public void SetNoteSequence(List<Note> notes)
        {
            _currentSequence.Clear();
            _currentSequence.AddRange(notes);
        }

        private void LoadDebugSequence()
        {
            List<NoteType> noteTypes = new List<NoteType>();
            for (int i = 0; i < 3; i++)
            {
                noteTypes.Add(NoteType.Weapon1);
                noteTypes.Add(NoteType.Weapon2);
                noteTypes.Add(NoteType.Weapon3);
                noteTypes.Add(NoteType.Weapon4);
                noteTypes.Add(NoteType.Weapon5);
            }

            List<Note> notes = new List<Note>();
            int time = 0;
            foreach (NoteType noteType in noteTypes)
            {
                notes.Add(new Note(noteType, TimeSpan.FromMilliseconds(time += 300)));
            }
            SetNoteSequence(notes);
        }

        public void Start()
        {
            if (_info.IsStarted)
            {
                ScreenNotification.ShowNotification("Stopped");
                _info.Reset();
            }
            else
            {
                ScreenNotification.ShowNotification("Starting");
                _info.Reset();
                _info.IsStarted = true;
                _info.StartTime = _lastGameTime;
            }

            double perfectCenter = (_windowInfo.HitRangePerfect.Max + _windowInfo.HitRangePerfect.Min) / 2.0;

            // TODO: Better images
            var LineHeight = this.Height;
            _perfectStartLine = new Image(Resources.Instance.MugTexture)
            {
                Width = 2,
                Height = LineHeight,
                BackgroundColor = Color.White,
                Location = new Point((int)(perfectCenter - (_windowInfo.NoteWidth / 2)), 0),
                Parent = this
            };
            _perfectEndLine = new Image(Resources.Instance.MugTexture)
            {
                Width = 2,
                Height = LineHeight,
                BackgroundColor = Color.White,
                Location = new Point((int)(perfectCenter + (_windowInfo.NoteWidth / 2)), 0),
                Parent = this
            };
        }

        public void Update(GameTime gameTime)
        {
            _lastGameTime = gameTime.TotalGameTime;

            if (_info.IsStarted == false)
            {
                return;
            }

            TimeSpan timeInRotation = _lastGameTime - _info.StartTime;

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

        private void AddNote(Note note)
        {
            int lane;
            switch (note.NoteType)
            {
                case NoteType.Weapon1:
                    lane = 0;
                    break;
                case NoteType.Weapon2:
                    lane = 1;
                    break;
                case NoteType.Weapon3:
                    lane = 2;
                    break;
                case NoteType.Weapon4:
                    lane = 3;
                    break;
                case NoteType.Weapon5:
                    lane = 4;
                    break;
                case NoteType.WeaponSwap:
                    lane = 5;
                    break;
                default:
                    lane = 0;
                    break;
            }

            var noteImage = new Image(
                Resources.Instance.MugTexture
            )
            {
                Size = _windowInfo.GetNewNoteSize(),
                Location = _windowInfo.GetNewNoteLocation(lane),
                Parent = this
            };
            var activeNote = new ActiveNote(
                _windowInfo,
                note,
                noteImage
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
                    note.Image.Location.Y - 20
                )
            );
            _info.HitTexts.Add(hitText);
        }

        public void Destroy()
        {
            _startButton.Dispose();
            _perfectStartLine.Dispose();
            _perfectEndLine.Dispose();
            Dispose();
        }

        // MARK: Properties

        private Label _startButton;
        private Image _perfectStartLine;
        private Image _perfectEndLine;
    }


}