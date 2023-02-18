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
                _isHit = true;
                Image.BackgroundColor = Color.Red;
            }

            /** User pressed the hotkey for this note */
            public bool OnHotkeyPressed()
            {
                if (_isHit)
                {
                    // Ignore
                    return false;
                }

                _isHit = true;

                if (_windowInfo.HitRangePerfect.IsInBetween(XPosition))
                {
                    ScreenNotification.ShowNotification("Perfect");
                }
                else if (_windowInfo.HitRangeGreat.IsInBetween(XPosition))
                {
                    ScreenNotification.ShowNotification("Great");
                }
                else if (_windowInfo.HitRangeGood.IsInBetween(XPosition))
                {
                    ScreenNotification.ShowNotification("Good");
                }
                else if (_windowInfo.HitRangeBoo.IsInBetween(XPosition))
                {
                    ScreenNotification.ShowNotification("Boo");
                }
                else
                {
                    ScreenNotification.ShowNotification("Miss");
                }

                ShouldRemove = true;

                return true;
            }

            public void Dispose()
            {
                Image.Dispose();
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
            List<Note> notes = new List<Note>();
            notes.Add(new Note(NoteType.Weapon1, TimeSpan.Zero));
            notes.Add(new Note(NoteType.Weapon2, TimeSpan.FromMilliseconds(1000)));
            notes.Add(new Note(NoteType.Weapon3, TimeSpan.FromMilliseconds(2000)));
            notes.Add(new Note(NoteType.Weapon4, TimeSpan.FromMilliseconds(3000)));
            notes.Add(new Note(NoteType.Weapon5, TimeSpan.FromMilliseconds(4000)));
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

            // Move Active Notes
            double moveAmount = (_windowInfo.NotePositionChangePerSecond * (gameTime.ElapsedGameTime.Milliseconds) / 1000.0);
            for (int index = _info.ActiveNotes.Count - 1; index >= 0; index--)
            {
                ActiveNote activeNote = _info.ActiveNotes[index];

                activeNote.XPosition -= moveAmount;
                if (activeNote.XPosition <= _windowInfo.DestroyNotePosition)
                {
                    activeNote.Dispose();
                    _info.ActiveNotes.RemoveAt(index);
                }
                else
                {
                    if (activeNote.XPosition <= _windowInfo.HitRangeBoo.Min)
                    {
                        activeNote.MarkAsMissed();
                    }

                    // Move it
                    activeNote.Image.Location = new Point(
                        (int)(activeNote.XPosition),
                        activeNote.Image.Location.Y
                    );
                }

                if (activeNote.ShouldRemove)
                {
                    _info.ActiveNotes.RemoveAt(index);
                    activeNote.Dispose();
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
            var Lane = 0;
            switch (note.NoteType)
            {
                case NoteType.Weapon1:
                    Lane = 0;
                    break;
                case NoteType.Weapon2:
                    Lane = 1;
                    break;
                case NoteType.Weapon3:
                    Lane = 2;
                    break;
                case NoteType.Weapon4:
                    Lane = 3;
                    break;
                case NoteType.Weapon5:
                    Lane = 4;
                    break;
                case NoteType.WeaponSwap:
                    Lane = 5;
                    break;
                default:
                    Lane = 0;
                    break;
            }

            var noteImage = new Image(
                Resources.Instance.MugTexture
            )
            {
                Size = _windowInfo.GetNewNoteSize(),
                Location = _windowInfo.GetNewNoteLocation(Lane),
                Parent = this
            };
            var activeNote = new ActiveNote(
                _windowInfo,
                note,
                noteImage
            );
            activeNote.XPosition = noteImage.Location.X;

            _info.ActiveNotes.Add(
                activeNote
            );
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