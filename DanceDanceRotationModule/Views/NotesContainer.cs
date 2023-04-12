using System;
using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.Storage;
using DanceDanceRotationModule.Util;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using BlishContainer = Blish_HUD.Controls.Container;


namespace DanceDanceRotationModule.Views
{
    public class NotesContainer : Container
    {
        private static readonly Logger Logger = Logger.GetLogger<NotesContainer>();

        /** Position of Perfect location as a percentage of the total width/height (based on orientation) */
        private const float PerfectPosition = 0.14f;
        /** Width/Height of lane lines (depends on orientation) */
        private const int LaneLineThickness = 2;

        // MARK: Inner Types

        internal class CurrentSequenceInfo
        {
            internal bool IsStarted { get; set; }
            internal bool IsPaused { get; set; }
            internal TimeSpan StartTime { get; set; }
            internal TimeSpan PausedTime { get; set; }
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
            internal NotesOrientation Orientation { get; private set; }

            internal int NoteWidth { get; private set; }
            internal int NoteHeight { get; private set; }
            internal int LaneSpacing { get; private set; }
            /** Spacing at the very top, mostly for the note hit text to have somewhere to go */
            internal int VerticalPadding { get; private set; }
            internal int HorizontalPadding { get; private set; }

            /** Where to spawn new notes */
            internal Point NewNotePosition { get; private set; }
            /** Where a note should be at when it is "perfect" */

            internal int NextAbilityIconsHeight { get; private set; }
            internal Point NextAbilityIconsLocation { get; private set; }
            internal Point TargetLocation { get; private set; }

            internal int HitPerfect { get; private set; }
            internal Range<double> HitRangePerfect { get; private set; }
            internal Range<double> HitRangeGreat { get; private set; }
            internal Range<double> HitRangeGood { get; private set; }
            internal Range<double> HitRangeBoo { get; private set; }
            internal int DestroyNotePosition { get; private set; }
            /** How fast the note should move per note change. Determined by SongData. */
            internal double NotePositionChangePerSecond { get; private set; }
            internal double TimeToReachEnd { get; private set; }

            public void Recalculate(
                int width, int height,
                SongData songData,
                NotesOrientation orientation
            )
            {
                Orientation = orientation;


                if (IsVerticalOrientation())
                {
                    // Still need more vertical padding in this orientation, mostly for the Hit text
                    VerticalPadding = (int)(HitText.MovePerSecond * (HitText.TotalLifeTimeMs / 1000.0));
                    HorizontalPadding = 10;
                    LaneSpacing = (height - (HorizontalPadding * 2)) / 100; // 5% of available space should be spacing
                }
                else
                {
                    VerticalPadding = (int)(HitText.MovePerSecond * (HitText.TotalLifeTimeMs / 1000.0));
                    HorizontalPadding = 0;
                    LaneSpacing = (height - (VerticalPadding * 2)) / 100; // 5% of available space should be spacing
                }

                var nextAbilitiesCount = DanceDanceRotationModule.Settings.ShowNextAbilitiesCount.Value;
                if (nextAbilitiesCount > 0)
                {
                    // Show ability icons section as an extra "lane"
                    NoteHeight = (height - (2*VerticalPadding) - LaneSpacing * 5) / 7;
                    NextAbilityIconsHeight = NoteHeight;

                    switch (Orientation)
                    {
                        case NotesOrientation.RightToLeft:
                            NextAbilityIconsLocation = new Point(
                                (int)(width * PerfectPosition) - (NoteWidth / 2),
                                0
                            );
                            break;
                        case NotesOrientation.LeftToRight:
                            NextAbilityIconsLocation = new Point(
                                (int)(width * (1-PerfectPosition)) - (NoteWidth / 2) - ((nextAbilitiesCount-1) * NoteWidth),
                                0
                            );
                            break;
                        case NotesOrientation.TopToBottom:
                            NextAbilityIconsLocation = new Point(
                                HorizontalPadding,
                                height - NoteHeight
                            );
                            break;
                        case NotesOrientation.BottomToTop:
                            NextAbilityIconsLocation = new Point(
                                HorizontalPadding,
                                0
                            );
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    // Hide ability icons section
                    NoteHeight = (height - (2*VerticalPadding) - LaneSpacing * 5) / 6;
                    NextAbilityIconsLocation = new Point(0, 0);
                    NextAbilityIconsHeight = 0;
                }

                if (IsVerticalOrientation())
                {
                    NoteWidth = (width - (2*HorizontalPadding) - LaneSpacing * 5) / 6;
                    NoteHeight = NoteWidth;
                }
                else
                {
                    // If next abilities are shown, count that as an extra "lane"
                    NoteHeight = (height - (2*VerticalPadding) - LaneSpacing * 5)
                         / (NextAbilityIconsHeight > 0 ? 7 : 6);
                    NoteWidth = NoteHeight;
                }

                // How long a note moves before hitting the "perfect" position
                int perfectPosition;

                NotePositionChangePerSecond = Math.Max(
                    songData.NotePositionChangePerSecond,
                    SongData.MinimumNotePositionChangePerSecond
                );

                switch (Orientation)
                {
                    case NotesOrientation.RightToLeft:
                        // New notes spawn at the edge of the window on the Right
                        NewNotePosition = new Point(
                            width,
                            0 // Calculated later based on lane
                        );

                        perfectPosition = (int)Math.Max(
                            PerfectPosition * width,
                            NoteWidth * 1.5
                        );

                        TimeToReachEnd = (NewNotePosition.X - perfectPosition) / NotePositionChangePerSecond;

                        TargetLocation = new Point(
                            (int)(perfectPosition - (NoteWidth / 2.0)),
                            VerticalPadding + NextAbilityIconsHeight
                        );

                        DestroyNotePosition = 0 - NoteWidth;

                        break;
                    case NotesOrientation.LeftToRight:
                        // New notes spawn at the edge of the window on the Left
                        NewNotePosition = new Point(
                            0 - NoteWidth,
                            0 // Calculated later based on lane
                        );

                        perfectPosition = (int)Math.Min(
                            (1-PerfectPosition) * width,
                            width - (NoteWidth * 1.5)
                        );
                        TimeToReachEnd = (NewNotePosition.X - perfectPosition) / NotePositionChangePerSecond;

                        TargetLocation = new Point(
                            (int)(perfectPosition - (NoteWidth / 2.0)),
                            VerticalPadding + NextAbilityIconsHeight
                        );

                        DestroyNotePosition = width;

                        break;
                    case NotesOrientation.TopToBottom:
                        // New notes spawn at the edge of the window on the Top
                        // NextAbility are at the bottom
                        NewNotePosition = new Point(
                            0, // Calculated later based on lane
                            0 - NoteHeight
                        );

                        perfectPosition = (int)Math.Min(
                            (1-PerfectPosition) * (height - NextAbilityIconsHeight),
                            height - (NoteHeight * 1.5) - NextAbilityIconsHeight
                        );
                        TimeToReachEnd = (NewNotePosition.Y - perfectPosition) / NotePositionChangePerSecond;

                        TargetLocation = new Point(
                            HorizontalPadding,
                            (int)(perfectPosition - (NoteHeight / 2.0))
                        );

                        DestroyNotePosition = height - NextAbilityIconsHeight;

                        break;
                    case NotesOrientation.BottomToTop:
                        // New notes spawn at the edge of the window on the Bottom
                        // NextAbility are at the top
                        NewNotePosition = new Point(
                            0, // Calculated later based on lane
                            height
                        );

                        perfectPosition = (int)Math.Max(
                            NextAbilityIconsHeight + (PerfectPosition * (height - NextAbilityIconsHeight)),
                            NextAbilityIconsHeight + (NoteHeight * 1.5)
                        );
                        TimeToReachEnd = (NewNotePosition.Y - perfectPosition) / NotePositionChangePerSecond;

                        TargetLocation = new Point(
                            HorizontalPadding,
                            (int)(perfectPosition - (NoteHeight / 2.0))
                        );

                        DestroyNotePosition = NextAbilityIconsHeight;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

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

            // Standard Note Size
            public Point GetNewNoteSize()
            {
                return new Point(NoteWidth, NoteHeight);
            }

            // Note Size for de-emphasized things, like Weapon1 when it's auto hit
            public Point GetNewNoteSizeSmall()
            {
                return new Point(NoteWidth * 3 / 4, NoteHeight * 3 / 4);
            }
            public Point GetNewNoteLocation(int lane)
            {
                switch (Orientation)
                {
                    case NotesOrientation.RightToLeft:
                        // Spawns on Right side
                        return new Point(
                            NewNotePosition.X,
                            VerticalPadding + NextAbilityIconsHeight + (lane * (NoteHeight + LaneSpacing))
                        );
                    case NotesOrientation.LeftToRight:
                        // Spawns on Left side
                        return new Point(
                            NewNotePosition.X,
                            VerticalPadding + NextAbilityIconsHeight + (lane * (NoteHeight + LaneSpacing))
                        );
                    case NotesOrientation.TopToBottom:
                        // Spawns on Top side
                        return new Point(
                            HorizontalPadding + (lane * (NoteWidth + LaneSpacing)),
                            NewNotePosition.Y
                        );
                    case NotesOrientation.BottomToTop:
                        // Spawns on Top side
                        return new Point(
                            HorizontalPadding + (lane * (NoteWidth + LaneSpacing)),
                            NewNotePosition.Y
                        );
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            /** Returns the amount a note should move given an update */
            public Vector2 GetNoteChangeLocation(
                GameTime gameTime
            )
            {
                float moveAmount = (float)(NotePositionChangePerSecond * (gameTime.ElapsedGameTime.Milliseconds) / 1000.0);

                switch (Orientation)
                {
                    case NotesOrientation.RightToLeft:
                        return new Vector2(
                            -1 * moveAmount,
                            0
                        );
                    case NotesOrientation.LeftToRight:
                        return new Vector2(
                            moveAmount,
                            0
                        );
                    case NotesOrientation.TopToBottom:
                        return new Vector2(
                            0,
                            moveAmount
                        );
                    case NotesOrientation.BottomToTop:
                        // Spawns on Top side
                        return new Vector2(
                            0,
                            -1 * moveAmount
                        );
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public bool IsVerticalOrientation()
            {
                return OrientationExtensions.IsVertical(Orientation);
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
            private const float FadeInTime = 0.7f;
            private const float HitAnimationTime = 0.2f;
            private const int HitAnimationScaleDivisor = 4;

            private WindowInfo _windowInfo;
            internal Note Note { get; set; }
            internal Image Image { get; set; }
            internal Label Label { get; set; }
            // XPosition/YPosition is stored here as a double instead of only using the Image,
            // because Image can only be in int positions, and the GameTime may need to
            // be more granular than that
            internal double XPosition { get; set; }
            internal double YPosition { get; set; }

            private bool _isHit;
            private bool _allowMovement = true;

            internal bool ShouldRemove { get; private set; }

            public event EventHandler<HitType> OnHit;

            public ActiveNote(WindowInfo windowInfo, Note note, BlishContainer parent)
            {
                _windowInfo = windowInfo;
                this.Note = note;

                var keyBinding = DanceDanceRotationModule.Settings
                    .GetKeyBindingForNoteType(
                        note.NoteType
                    );
                string hotkeyText =
                    (keyBinding != null)
                        ? KeysExtensions.NotesString(keyBinding.Value)
                        : "?";

                // string text = keyBinding.Value.GetBindingDisplayText();
                int lane = NoteTypeExtensions.NoteLane(note.NoteType);

                // Respect "ShowAbilityIconsForNotes" preference
                AsyncTexture2D noteBackground =
                    DanceDanceRotationModule.Settings.ShowAbilityIconsForNotes.Value
                        ? Resources.Instance.GetAbilityIcon(note.AbilityId)
                        : (AsyncTexture2D)NoteTypeExtensions.NoteImage(note.NoteType);

                Image = new Image(
                    noteBackground
                )
                {
                    Size = windowInfo.GetNewNoteSize(),
                    Location = _windowInfo.GetNewNoteLocation(lane),
                    ZIndex = 2,
                    Opacity = 0.7f,
                    Parent = parent
                };
                // Adjust the size and position of the icon if AutoHit is on and this is a Weapon1
                bool isMiniIcon =
                    DanceDanceRotationModule.Settings.AutoHitWeapon1.Value &&
                    note.NoteType == NoteType.Weapon1 &&
                    note.OverrideAuto == false;
                if (isMiniIcon)
                {
                    var oldSize = Image.Size;
                    Image.ZIndex = 1;
                    Image.Size = _windowInfo.GetNewNoteSizeSmall();
                    Image.Location = new Point(
                        Image.Location.X + (oldSize.X - Image.Size.X) / 2,
                        Image.Location.Y + (oldSize.Y - Image.Size.Y) / 2
                    );
                }

                BitmapFont font;
                if (hotkeyText.Length < 3)
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
                    Text = hotkeyText,
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

                // The center of the note should be at (XPosition,YPosition), but the Image point is the top left corner
                this.XPosition = Image.Location.X + Image.Width / 2;
                this.YPosition = Image.Location.Y + Image.Height / 2;

                _isHit = false;
                this.ShouldRemove = false;

                // Fade Note in:
                Image.Opacity = 0.0f;
                Label.Opacity = 0.0f;
                Animation.Tweener.Tween(Image, new
                {
                    Opacity = 1.0f
                }, FadeInTime);

                // "ShowHotkeys" preference
                // Just setting opacity to 0 so all the calculations that need the label position work. Lazy.
                bool hideHotkey =
                    isMiniIcon ||
                    DanceDanceRotationModule.Settings.ShowHotkeys.Value == false;

                if (hideHotkey == false)
                {
                    Animation.Tweener.Tween(Label, new
                    {
                        Opacity = 1.0f
                    }, FadeInTime);
                }
            }

            public void Update(
                Vector2 positionChange
            )
            {
                XPosition += positionChange.X;
                YPosition += positionChange.Y;
                if (
                    _isHit == false &&
                    DanceDanceRotationModule.Settings.AutoHitWeapon1.Value &&
                    Note.NoteType == NoteType.Weapon1 &&
                    Note.OverrideAuto == false &&
                    IsPastPerfect()
                )
                {
                    // Special Case: If AutoHitWeapon1 is enabled, remove the note in perfect
                    //               No hit text needs to be made
                    _isHit = true;
                    PlayHitAnimation();
                }
                else if (IsPastDestroy())
                {
                    ShouldRemove = true;
                }
                else
                {
                    if (_isHit == false && IsPastMiss())
                    {
                        SetHit(HitType.Miss);
                        PlayMissAnimation();
                    }

                    if (_allowMovement)
                    {
                        // Move it
                        Image.Location = new Point(
                            // Center Image over X position
                            (int)(XPosition) - Image.Width / 2,
                            (int)(YPosition) - Image.Height / 2
                        );
                        Label.Location = new Point(
                            Image.Location.X + ((Image.Width - Label.Width) / 2),
                            Image.Location.Y + ((Image.Height - Label.Height) / 2)
                        );
                    }
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
                    DanceDanceRotationModule.Settings.AutoHitWeapon1.Value &&
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
                var axisToCheck =
                    _windowInfo.IsVerticalOrientation()
                        ? YPosition
                        : XPosition;
                if (_windowInfo.HitRangePerfect.IsInBetween(axisToCheck))
                {
                    hitType = HitType.Perfect;
                    ScreenNotification.ShowNotification("Perfect");
                }
                else if (_windowInfo.HitRangeGreat.IsInBetween(axisToCheck))
                {
                    hitType = HitType.Great;
                    ScreenNotification.ShowNotification("Great");
                }
                else if (_windowInfo.HitRangeGood.IsInBetween(axisToCheck))
                {
                    hitType = HitType.Good;
                    ScreenNotification.ShowNotification("Good");
                }
                else if (_windowInfo.HitRangeBoo.IsInBetween(axisToCheck))
                {
                    hitType = HitType.Boo;
                    ScreenNotification.ShowNotification("Boo");
                }
                else
                {
                    hitType = HitType.Miss;
                    ScreenNotification.ShowNotification("Miss");
                }

                SetHit(hitType);
                PlayHitAnimation();
                return true;
            }

            public void Dispose()
            {
                Image.Dispose();
                Label.Dispose();
            }

            private void SetHit(HitType hitType)
            {
                if (_isHit)
                    return;

                _isHit = true;

                OnHit?.Invoke(this, hitType);
            }

            private void PlayHitAnimation()
            {
                _allowMovement = false;

                var startSize = Image.Size;
                var startPosition = Image.Location;
                Animation.Tweener
                    .Tween(
                        Image,
                        new {
                            Size = new Point(
                                startSize.X / HitAnimationScaleDivisor,
                                startSize.Y / HitAnimationScaleDivisor
                            ),
                            Location = new Point(
                                startPosition.X + (Image.Width / HitAnimationScaleDivisor),
                                startPosition.Y + (Image.Height / HitAnimationScaleDivisor)
                            ),
                            Opacity = 0.0f,
                        },
                        HitAnimationTime
                    )
                    .OnComplete(() =>
                    {
                        ShouldRemove = true;
                    });
                Animation.Tweener.Tween(Label, new
                {
                    Opacity = 0.0f
                }, 0.1f);
            }

            private void PlayMissAnimation()
            {
                Image.BackgroundColor = Color.Red;
                // Fade out note. Note, it will keep moving
                Animation.Tweener.Tween(Image, new
                {
                    Opacity = 0.0f
                }, 0.4f);
                Animation.Tweener.Tween(Label, new
                {
                    Opacity = 0.0f
                }, 0.1f);
            }

            internal void ShowTooltip()
            {
                string abilityName = Resources.Instance.GetAbilityName(
                    Note.AbilityId
                );
                if (abilityName.Length != 0)
                {
                    abilityName += ", ";
                }

                string tooltip = String.Format(
                    "{0}Time: {1:0.000}s, Dur: {2:n0}ms",
                    abilityName,
                    Note.TimeInRotation.TotalMilliseconds / 1000.0,
                    Note.Duration.TotalMilliseconds
                );
                // Set the BasicTooltipText of both because I can't figure out
                // how to disable the Label from overriding the show tooltip of the Image
                // when it is hovered over.
                Image.BasicTooltipText = tooltip;
                Label.BasicTooltipText = tooltip;
            }

            internal void HideTooltip()
            {
                Image.BasicTooltipText = "";
                Label.BasicTooltipText = "";
            }

            /** Returns true if this active note is at or past the "Perfect" position, depending on orientation */
            private bool IsPastPerfect()
            {
                switch (_windowInfo.Orientation)
                {
                    case NotesOrientation.RightToLeft:
                        return XPosition <= _windowInfo.HitPerfect;
                    case NotesOrientation.LeftToRight:
                        return XPosition >= _windowInfo.HitPerfect;
                    case NotesOrientation.TopToBottom:
                        return YPosition >= _windowInfo.HitPerfect;
                    case NotesOrientation.BottomToTop:
                        return YPosition <= _windowInfo.HitPerfect;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            /** Returns true if this active note is at or past the "Boo" position, and therefor a Miss, depending on orientation */
            private bool IsPastMiss()
            {
                switch (_windowInfo.Orientation)
                {
                    case NotesOrientation.RightToLeft:
                        return XPosition <= _windowInfo.HitRangeBoo.Min;
                    case NotesOrientation.LeftToRight:
                        return XPosition >= _windowInfo.HitRangeBoo.Max;
                    case NotesOrientation.TopToBottom:
                        return YPosition >= _windowInfo.HitRangeBoo.Max;
                    case NotesOrientation.BottomToTop:
                        return YPosition <= _windowInfo.HitRangeBoo.Min;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            /** Returns true if this active note is at or past the "destroy" position, and therefor should be removed */
            private bool IsPastDestroy()
            {
                switch (_windowInfo.Orientation)
                {
                    case NotesOrientation.RightToLeft:
                        return XPosition <= _windowInfo.DestroyNotePosition;
                    case NotesOrientation.LeftToRight:
                        return XPosition >= _windowInfo.DestroyNotePosition;
                    case NotesOrientation.TopToBottom:
                        return YPosition >= _windowInfo.DestroyNotePosition;
                    case NotesOrientation.BottomToTop:
                        return YPosition <= _windowInfo.DestroyNotePosition;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
                    // Without ClipsBounds allows this text to potentially float into the parent container
                    // which looks a bit better than it just cutting cut off.
                    ClipsBounds = false,
                    Location = location,
                    Parent = parent
                };
                // Center text on passed in location
                _label.Location = new Point(
                    location.X - (_label.Width / 2),
                    location.Y - (_label.Height / 2)
                );
                _remainLifeMs = TotalLifeTimeMs;
                _yPos = _label.Location.Y;
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
            internal Note Note { get; }
            internal Image Image { get; }
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
                Image = new Image(
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

            public void Update(GameTime gameTime, TimeSpan timeInRotation)
            {
                // TODO: Animation here
                if (timeInRotation.TotalMilliseconds > Note.TimeInRotation.TotalMilliseconds + (_windowInfo.TimeToReachEnd*1000))
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
        private Label _timeLabel;

        // MARK: Events

        /** Emits the currently started state */
        public event EventHandler<bool> OnStartStop;

        // MARK: Constructor

        public NotesContainer()
        {
            _windowInfo.Recalculate(
                Width, Height,
                _songData,
                DanceDanceRotationModule.Settings.Orientation.Value
            );

            CreateTarget();
            UpdateTarget();
            CreateBackgroundLines();
            UpdateBackgroundLines();

            _timeLabel = new Label()
            {
                Text = "",
                Visible = false,
                Width = 100,
                AutoSizeHeight = true,
                Font = GameService.Content.DefaultFont14,
                TextColor = Color.LightGray,
                Location = new Point(10, 10),
                Parent = this
            };

            // Listen for selected song changes to update the notes
            DanceDanceRotationModule.SongRepo.OnSelectedSongChanged +=
                delegate(object sender, SelectedSongInfo songInfo)
                {
                    if (songInfo.Song != null)
                    {
                        SetNoteSequence(
                            songInfo.Song.Notes,
                            songInfo.Data
                        );
                    }
                };
            DanceDanceRotationModule.Settings.ShowNextAbilitiesCount.SettingChanged +=
                delegate
                {
                    Reset();
                    RecalculateLayout();
                };
            DanceDanceRotationModule.Settings.Orientation.SettingChanged +=
                delegate(object sender, ValueChangedEventArgs<NotesOrientation> args)
                {
                    Logger.Trace("Orientation Updated. Resetting and recalculating layout and creating new target and lines");
                    Reset();
                    RecalculateLayout();
                    CreateTarget();
                    UpdateTarget();
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

            _windowInfo.Recalculate(
                Width, Height,
                _songData,
                DanceDanceRotationModule.Settings.Orientation.Value
            );
            UpdateTarget();
            UpdateBackgroundLines();
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
            Logger.Trace($"Setting Notes Sequence");
            Reset();
            _currentSequence.Clear();
            _currentSequence.AddRange(notes);
            _songData = songData;
            _windowInfo.Recalculate(
                Width, Height,
                _songData,
                DanceDanceRotationModule.Settings.Orientation.Value
            );
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
                Play();
            }
        }

        public void Play()
        {
            if (_currentSequence.Count == 0)
            {
                Logger.Warn("Play pressed, but no song loaded.");
            }
            _timeLabel.Visible = false;

            if (_info.IsStarted == false)
            {
                // Start from stopped
                Logger.Trace("Starting Notes");

                _info.IsStarted = true;
                _info.StartTime = _lastGameTime;

                if (_songData.StartAtSecond > 0)
                {
                    TimeSpan startTime = TimeSpan.FromSeconds(_songData.StartAtSecond);

                    // Adjust the current index up so the first notes don't spawn in a clump
                    foreach (Note note in _currentSequence)
                    {
                        if (note.TimeInRotation < startTime)
                        {
                            _info.SequenceIndex += 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Set StartTime back in time to begin later in the song. This must be adjusted by the rate to be accurate
                    startTime = startTime.Divide(
                        new decimal(_songData.PlaybackRate)
                    );
                    _info.StartTime -= startTime;
                }

                OnStartStop?.Invoke(this, _info.IsStarted);
                AddInitialAbilityIcons();
            }
            else
            {
                // Resume from paused
                Logger.Trace("Resuming Notes");

                _info.IsPaused = false;
                _info.StartTime += _lastGameTime - _info.PausedTime;
                HideAllTooltips();
            }
        }

        public void Pause()
        {
            if (_info.IsStarted && _info.IsPaused == false)
            {
                Logger.Trace("Pausing Notes");
                _info.IsPaused = true;
                _info.PausedTime = _lastGameTime;

                TimeSpan totalInGameTime = (_info.PausedTime - _info.StartTime);
                _timeLabel.Text = $"{totalInGameTime.Minutes} : {totalInGameTime.Seconds:00}";
                _timeLabel.Visible = true;

                ShowAllTooltips();
            }
            else
            {
                Logger.Trace("Pause called, but notes are not started");
            }
        }

        public void Reset()
        {
            if (_info.IsStarted)
            {
                Logger.Trace("Reset");
                _info.IsStarted = false;
                _info.IsPaused = false;
                _info.Reset();
                _timeLabel.Visible = false;
                OnStartStop?.Invoke(this, _info.IsStarted);
            }
        }

        public bool IsStarted()
        {
            return _info.IsStarted;
        }

        public bool IsPaused()
        {
            return _info.IsStarted && _info.IsPaused;
        }

        public void UpdateNotes(GameTime gameTime)
        {
            _lastGameTime = gameTime.TotalGameTime;

            if (_info.IsStarted == false || _info.IsPaused)
            {
                return;
            }

            TimeSpan timeInRotation = _lastGameTime - _info.StartTime;
            double playbackRate = _songData.PlaybackRate;
            timeInRotation = timeInRotation.Multiply(new decimal(playbackRate));

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
            else
            {
                // Check if song has ended and all notes are gone
                if (_info.ActiveNotes.Count == 0)
                {
                    Logger.Trace("No more active notes. Resetting.");
                    Reset();
                }
            }

            // Update Active Notes and remove any that want to be destroyed
            Vector2 moveAmount = _windowInfo.GetNoteChangeLocation(gameTime);
            for (int index = _info.ActiveNotes.Count - 1; index >= 0; index--)
            {
                ActiveNote activeNote = _info.ActiveNotes[index];
                activeNote.Update(moveAmount);
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
            if (_info.IsStarted == false)
            {
                return;
            }

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
                    note.Image.Location.X + (note.Image.Width / 2),
                    note.Image.Location.Y + (note.Image.Height / 2)
                )
            );
            _info.HitTexts.Add(hitText);
        }

        // MARK: Tooltips

        /**
         * Shows all tooltips. Enabled when the notes are paused
         */
        private void ShowAllTooltips()
        {
            foreach (ActiveNote activeNote in _info.ActiveNotes)
            {
                activeNote.ShowTooltip();
            }
        }

        /**
         * Hides tooltips on all skills. Should be down when the notes are running
         */
        private void HideAllTooltips()
        {
            foreach (ActiveNote activeNote in _info.ActiveNotes)
            {
                activeNote.HideTooltip();
            }
        }

        // MARK: Background Lines

        private void CreateBackgroundLines()
        {
            if (_laneLines == null || _laneLines.Count < 6)
            {
                _laneLines = new List<Control>();
                for (int lane = 0; lane < 6; lane++)
                {
                    _laneLines.Add(
                        new Image()
                        {
                            BackgroundColor = Color.White,
                            Height = LaneLineThickness,
                            Width = Width,
                            Opacity = 0.1f,
                            Parent = this,
                        }
                    );
                }
            }
        }

        private void UpdateBackgroundLines()
        {
            if (_laneLines == null || _laneLines.Count < 6)
            {
                return;
            }

            int staticPosition;
            switch (_windowInfo.Orientation)
            {
                case NotesOrientation.RightToLeft:
                    staticPosition = _windowInfo.TargetLocation.X + _windowInfo.NoteWidth;
                    break;
                case NotesOrientation.LeftToRight:
                    staticPosition = _windowInfo.TargetLocation.X - Width;
                    break;
                case NotesOrientation.TopToBottom:
                    staticPosition = _windowInfo.TargetLocation.Y - Height;
                    break;
                case NotesOrientation.BottomToTop:
                    staticPosition = _windowInfo.TargetLocation.Y + _windowInfo.NoteHeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (int lane = 0; lane < 6; lane++)
            {
                // Find the location of middle of the lane
                var location = _windowInfo.GetNewNoteLocation(lane);
                if (_windowInfo.IsVerticalOrientation())
                {
                    location.X += (_windowInfo.GetNewNoteSize().X / 2) - 1;
                    location.Y = staticPosition;
                    _laneLines[lane].Height = Height;
                    _laneLines[lane].Width = LaneLineThickness;
                }
                else
                {
                    location.X = staticPosition;
                    location.Y += (_windowInfo.GetNewNoteSize().Y / 2) - 1;
                    _laneLines[lane].Height = LaneLineThickness;
                    _laneLines[lane].Width = Width;
                }

                _laneLines[lane].Location = location;
            }
        }

        // MARK: Target

        private void CreateTarget()
        {
            const float targetOpacity = 0.5f;

            _targetTop?.Dispose();
            _targetBottom?.Dispose();
            foreach (var target in _targetCircles)
            {
                target.Dispose();
            }
            _targetCircles.Clear();
            foreach (var target in _targetSpacers)
            {
                target.Dispose();
            }
            _targetSpacers.Clear();

            if (_windowInfo.IsVerticalOrientation())
            {
                // Target is horizontal (using left/right end pieces)
                _targetTop = new Image(Resources.Instance.DdrTargetLeft)
                {
                    Width = 24,
                    Height = 64,
                    Location = new Point(0, 0),
                    Opacity = targetOpacity,
                    Parent = this
                };
                _targetBottom = new Image(Resources.Instance.DdrTargetRight)
                {
                    Width = 24,
                    Height = 64,
                    Location = new Point(0, 0),
                    Opacity = targetOpacity,
                    Parent = this
                };
                for (int i = 0; i < 6; i++)
                {
                    _targetCircles.Add(
                    new Image(Resources.Instance.DdrTargetCircle)
                        {
                            Width = 64,
                            Height = 64,
                            Location = new Point(0, 0),
                            Opacity = targetOpacity,
                            Parent = this
                        }
                    );
                }
                for (int i = 0; i < 5; i++)
                {
                    _targetSpacers.Add(
                    new Image(Resources.Instance.DdrTargetSpacer)
                        {
                            Width = 24,
                            Height = 64,
                            Location = new Point(0, 0),
                            Opacity = targetOpacity,
                            Parent = this
                        }
                    );
                }
            }
            else
            {
                _targetTop = new Image(Resources.Instance.DdrTargetTop)
                {
                    Width = 64,
                    Height = 24,
                    Location = new Point(0, 0),
                    Opacity = targetOpacity,
                    Parent = this
                };
                _targetBottom = new Image(Resources.Instance.DdrTargetBottom)
                {
                    Width = 64,
                    Height = 24,
                    Location = new Point(0, 0),
                    Opacity = targetOpacity,
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
                            Opacity = targetOpacity,
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
                            Opacity = targetOpacity,
                            Parent = this
                        }
                    );
                }
            }

            _targetCreated = true;
        }

        private void UpdateTarget()
        {

            if (_windowInfo.IsVerticalOrientation())
            {
                int targetHeight = _windowInfo.NoteHeight;
                int xPos = _windowInfo.TargetLocation.X - _targetTop.Width;
                int yPos = _windowInfo.TargetLocation.Y;

                int roundEdgesWidth = _windowInfo.HorizontalPadding;

                _targetTop.Width =  roundEdgesWidth;
                _targetTop.Height = targetHeight;
                _targetTop.Location = new Point(xPos, yPos);
                xPos += _targetTop.Width;

                for (int index = 0; index < _targetCircles.Count; index++)
                {
                    _targetCircles[index].Width = _windowInfo.NoteWidth;
                    _targetCircles[index].Height =  targetHeight;
                    _targetCircles[index].Location = new Point(xPos, yPos);
                    xPos += _targetCircles[index].Width;

                    if (index < _targetSpacers.Count)
                    {
                        // Add a little bit of overlap to prevent gaps
                        _targetSpacers[index].Width = _windowInfo.LaneSpacing;
                        _targetSpacers[index].Height =  targetHeight;
                        _targetSpacers[index].Location = new Point(xPos, yPos);
                        xPos += _targetSpacers[index].Width;
                    }
                }

                _targetBottom.Width = roundEdgesWidth;
                _targetBottom.Height =  targetHeight;
                _targetBottom.Location = new Point(xPos, yPos);
            }
            else
            {
                int targetWidth = _windowInfo.NoteWidth;
                int xPos = _windowInfo.TargetLocation.X;
                int yPos = _windowInfo.TargetLocation.Y - _targetTop.Height;

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
                        yPos += _targetSpacers[index].Height;
                    }
                }

                _targetBottom.Height = roundEdgesHeight;
                _targetBottom.Width = targetWidth;
                _targetBottom.Location = new Point(xPos, yPos);
            }
        }

        // MARK: Ability Icon Stuff

        private void AddInitialAbilityIcons()
        {
            var totalAbilityIcons = DanceDanceRotationModule.Settings.ShowNextAbilitiesCount.Value;
            if (totalAbilityIcons > 0)
            {
                for (int index = 0, size = Math.Min(totalAbilityIcons, _currentSequence.Count); index < size; index++)
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
            var xPos = _windowInfo.NextAbilityIconsLocation.X;
            var yPos = _windowInfo.NextAbilityIconsLocation.Y;
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

        protected override void DisposeControl()
        {
            base.DisposeControl();

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
        }

        // MARK: Properties

        private bool _targetCreated;
        private Image _targetTop;
        private Image _targetBottom;
        private List<Image> _targetCircles = new List<Image>(6);
        private List<Image> _targetSpacers = new List<Image>(5);
        private List<Control> _laneLines;
    }


}