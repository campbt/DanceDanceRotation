using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using DanceDanceRotationModule.Model;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1;

namespace DanceDanceRotationModule.Storage
{
    public class ModuleSettings
    {
        private static readonly Logger Logger = Logger.GetLogger<ModuleSettings>();

        public ModuleSettings(SettingCollection settings)
        {
            Logger.Info("Initializing Settings");

            InitGeneral(settings);
            InitAbilityHotkeys(settings);
            InitControlHotkeys(settings);
            InitWindowVisibilityHotkeys(settings);
            InitSongRepoSettings(settings);
            InitHelpSettings(settings);
            InitHiddenSettings(settings);
        }

        #region General

        // MARK: Settings - General

        internal SettingEntry<NotesOrientation> Orientation { get; private set; }
        internal SettingEntry<float> BackgroundOpacity { get; private set; }
        internal SettingEntry<bool> AutoHitWeapon1 { get; private set; }
        internal SettingEntry<bool> ShowAbilityIconsForNotes { get; private set; }
        internal SettingEntry<bool> ShowHotkeys { get; private set; }
        internal SettingEntry<bool> ShowOnlyCharacterClassSongs { get; private set; }
        internal SettingEntry<bool> CompactMode { get; private set; }
        internal SettingEntry<bool> StartSongsWithFirstSkill { get; private set; }
        internal SettingEntry<int> ShowNextAbilitiesCount { get; private set; }

        private void InitGeneral(SettingCollection settings)
        {
            var generalSettings = settings.AddSubCollection(
                collectionKey: "general_settings",
                renderInUi: true,
                displayNameFunc: () => "General",
                lazyLoaded: false
            );

            Orientation = generalSettings.DefineSetting("NotesOrientation",
                NotesOrientation.RightToLeft,
                () => "Notes Orientation".PadRight(34),
                () => "Sets the direction notes will travel while playing.");

            BackgroundOpacity = generalSettings.DefineSetting("BackgroundOpacity",
                0.9f,
                () => "Background Transparency",
                () => "Sets the transparency of the notes background. Min=0% Max=100%");
            BackgroundOpacity.SetRange(0.0f, 1.0f);

            AutoHitWeapon1 = generalSettings.DefineSetting("AutoHitWeapon1",
                true,
                () => "Auto Hit Weapon 1",
                () => "If enabled, Weapon1 skills will automatically clear, instead of requiring hotkey presses, since they are probably on auto-cast.");

            ShowAbilityIconsForNotes = generalSettings.DefineSetting("ShowAbilityIconsForNotes",
                true,
                () => "Show ability icon as note",
                () => "If enabled, notes will use the actual ability icon instead of the generic.");

            ShowHotkeys = generalSettings.DefineSetting("ShowHotkeys",
                true,
                () => "Show ability hotkeys",
                () => "If enabled, notes will have the hotkeys displayed on top of them.");

            ShowOnlyCharacterClassSongs = generalSettings.DefineSetting("ShowOnlyCharacterClassSongs",
                true,
                () => "Only show current profession songs",
                () => "If enabled, the song list will only show songs for the current profession");

            CompactMode = generalSettings.DefineSetting("CompactMode",
                false,
                () => "Compact Mode",
                () => "If enabled, notes will try to be in a single lane and only shifted to other lanes to avoid collisions. Does NOT work with Ability Bar orientation.");

            StartSongsWithFirstSkill = generalSettings.DefineSetting("StartSongsWithFirstSkill",
                true,
                () => "Start with first skill",
                () => "If enabled, the song can be started by pressing the hotkey for the first ability.\nNotes will be shifted so the first note is already in the 'Perfect' location.\nThis has no effect if the song is set to start later than the beginning.");

            ShowNextAbilitiesCount = generalSettings.DefineSetting("ShowNextAbilitiesCount",
                0,
                () => "Show next abilities",
                () => "If enabled, the next X ability icons will be shown above the notes section, with no animations.");
            ShowNextAbilitiesCount.SetRange(0, 10);
        }

        #endregion

        #region Ability Hotkeys

        // MARK: Settings - Ability Hotkeys

        internal SettingCollection AbilityHotkeysSettings { get; private set; }
        internal SettingEntry<KeyBinding> SwapWeapons { get; private set; }
        internal SettingEntry<KeyBinding> Dodge { get; private set; }
        internal SettingEntry<KeyBinding> Weapon1 { get; private set; }
        internal SettingEntry<KeyBinding> Weapon2 { get; private set; }
        internal SettingEntry<KeyBinding> Weapon3 { get; private set; }
        internal SettingEntry<KeyBinding> Weapon4 { get; private set; }
        internal SettingEntry<KeyBinding> Weapon5 { get; private set; }
        internal SettingEntry<KeyBinding> HealingSkill { get; private set; }
        internal SettingEntry<KeyBinding> UtilitySkill1 { get; private set; }
        internal SettingEntry<KeyBinding> UtilitySkill2 { get; private set; }
        internal SettingEntry<KeyBinding> UtilitySkill3 { get; private set; }
        internal SettingEntry<KeyBinding> EliteSkill { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill1 { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill2 { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill3 { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill4 { get; private set; }
        internal SettingEntry<KeyBinding> ProfessionSkill5 { get; private set; }
        internal SettingEntry<KeyBinding> WeaponStow { get; private set; }

        private void InitAbilityHotkeys(SettingCollection settings)
        {
            AbilityHotkeysSettings = settings.AddSubCollection(
                collectionKey: "hotkey_settings",
                renderInUi: true,
                displayNameFunc: () => "Ability Hotkeys",
                lazyLoaded: false
            );
            SwapWeapons      = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.WeaponSwap);
            Dodge            = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Dodge);
            Weapon1          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon1);
            Weapon2          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon2);
            Weapon3          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon3);
            Weapon4          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon4);
            Weapon5          = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Weapon5);
            HealingSkill     = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Heal);
            UtilitySkill1    = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Utility1);
            UtilitySkill2    = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Utility2 );
            UtilitySkill3    = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Utility3);
            EliteSkill       = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Elite );
            ProfessionSkill1 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Profession1);
            ProfessionSkill2 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Profession2);
            ProfessionSkill3 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Profession3);
            ProfessionSkill4 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Profession4);
            ProfessionSkill5 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.Profession5);
            WeaponStow       = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.WeaponStow);
        }

        private SettingEntry<KeyBinding> DefineHotkeySetting(SettingCollection settings, NoteType noteType)
        {
            SettingEntry<KeyBinding> retval = settings.DefineSetting(noteType.ToString(),
                new KeyBinding(NoteTypeExtensions.DefaultHotkey(noteType)),
                () => NoteTypeExtensions.HotkeyDescription(noteType),
                () => "Hotkey used for " + NoteTypeExtensions.HotkeyDescription(noteType) + "\nThis must match your in-game hotkeys to work!");
            retval.Value.Enabled = true;
            retval.Value.Activated += delegate
            {
                if (DanceDanceRotationModule.Instance == null)
                {
                    // It's possible a hotkey could be triggered after the module is disabled.
                    Logger.GetLogger<ModuleSettings>().Debug("Settings Hotkey was triggered byt the DDR Module Instance is null. This note press will be ignored : " + noteType);
                }
                else
                {
                    if (DanceDanceRotationModule.Instance.IsNotesWindowVisible())
                    {
                        DanceDanceRotationModule.Instance
                            .GetNotesContainer()
                            ?.OnHotkeyPressed(
                                noteType
                            );
                    }
                }
            };
            return retval;
        }

        public SettingEntry<KeyBinding> GetKeyBindingForNoteType(NoteType noteType)
        {
            SettingEntry<KeyBinding> retval = new SettingEntry<KeyBinding>();
            AbilityHotkeysSettings.TryGetSetting<KeyBinding>(noteType.ToString(), out retval);
            return retval;
        }

        #endregion


        #region Control Hotkeys

        // MARK: Settings - Control Hotkeys

        internal SettingEntry<KeyBinding> ToggleHotkey { get; private set; }
        internal SettingEntry<KeyBinding> PlayHotkey { get; private set; }
        internal SettingEntry<KeyBinding> PauseHotkey { get; private set; }
        internal SettingEntry<KeyBinding> StopHotkey { get; private set; }

        private void InitControlHotkeys(SettingCollection settings)
        {
            var controlHotkeySettings = settings.AddSubCollection(
                collectionKey: "control_hotkey_settings",
                renderInUi: true,
                displayNameFunc: () => "Control Hotkeys",
                lazyLoaded: false
            );
            ToggleHotkey = controlHotkeySettings.DefineSetting("ToggleHotkey",
                new KeyBinding(Keys.None),
                () => "Toggle Play/Stop",
                () => "Will start or stop the DDR rotation when pressed based on if it is currently playing.");
            ToggleHotkey.Value.Enabled = true;
            ToggleHotkey.Value.Activated += delegate
            {
                DanceDanceRotationModule.Instance
                    .GetNotesContainer()
                    ?.ToggleStart();
            };
            PlayHotkey = controlHotkeySettings.DefineSetting("PlayHotkey",
                new KeyBinding(Keys.None),
                () => "Play",
                () => "Will start the DDR rotation when pressed, if not already playing.");
            PlayHotkey.Value.Enabled = true;
            PlayHotkey.Value.Activated += delegate
            {
                DanceDanceRotationModule.Instance
                    .GetNotesContainer()
                    ?.Play();
            };
            PauseHotkey = controlHotkeySettings.DefineSetting("PauseHotkey",
                new KeyBinding(Keys.None),
                () => "Pause",
                () => "Will pause the DDR rotation when pressed, if playing.");
            PauseHotkey.Value.Enabled = true;
            PauseHotkey.Value.Activated += delegate
            {
                DanceDanceRotationModule.Instance
                    .GetNotesContainer()
                    ?.Pause();
            };
            StopHotkey = controlHotkeySettings.DefineSetting("StopHotkey",
                new KeyBinding(Keys.None),
                () => "Stop",
                () => "Will stop the DDR rotation when pressed, if playing.");
            StopHotkey.Value.Enabled = true;
            StopHotkey.Value.Activated += delegate
            {
                DanceDanceRotationModule.Instance
                    .GetNotesContainer()
                    ?.Stop();
            };
        }

        #endregion

        #region Window Visibility Hotkeys

        // MARK: Settings - Window Visibility Hotkeys

        internal SettingEntry<KeyBinding> ToggleNotesWindowHotkey { get; private set; }
        internal SettingEntry<KeyBinding> ToggleSongListWindowHotkey { get; private set; }
        internal SettingEntry<KeyBinding> ToggleSongInfoWindowHotkey { get; private set; }

        private void InitWindowVisibilityHotkeys(SettingCollection settings)
        {
            var windowHotkeySettings = settings.AddSubCollection(
                collectionKey: "window_hotkey_settings",
                renderInUi: true,
                displayNameFunc: () => "Window Hotkeys",
                lazyLoaded: false
            );
            ToggleNotesWindowHotkey = windowHotkeySettings.DefineSetting("ToggleNotesWindowHotkey",
                new KeyBinding(Keys.None),
                () => "Toggle Notes Window",
                () => "Will Show/Hide the main Notes window");
            ToggleNotesWindowHotkey.Value.Enabled = true;
            ToggleNotesWindowHotkey.Value.Activated += delegate
            {
                DanceDanceRotationModule.Instance.ToggleNotesWindow();
            };
            ToggleSongListWindowHotkey = windowHotkeySettings.DefineSetting("ToggleSongListWindowHotkey",
                new KeyBinding(Keys.None),
                () => "Toggle Song List Window",
                () => "Will Show/Hide the Song List window");
            ToggleSongListWindowHotkey.Value.Enabled = true;
            ToggleSongListWindowHotkey.Value.Activated += delegate
            {
                DanceDanceRotationModule.Instance.ToggleSongList();
            };
            ToggleSongInfoWindowHotkey = windowHotkeySettings.DefineSetting("ToggleSongInfoWindowHotkey",
                new KeyBinding(Keys.None),
                () => "Toggle Song Info Window",
                () => "Will Show/Hide the Song Info window");
            ToggleSongInfoWindowHotkey.Value.Enabled = true;
            ToggleSongInfoWindowHotkey.Value.Activated += delegate
            {
                DanceDanceRotationModule.Instance.ToggleSongInfo();
            };
        }

        #endregion

        #region Song Repo Settings

        // MARK: Settings - Song Repo

        internal SettingEntry<List<SongData>> SongDatas { get; private set; }
        internal SettingEntry<Song.ID> SelectedSong { get; private set; }

        private void InitSongRepoSettings(SettingCollection settings)
        {
            var songRepoSettings = settings.AddSubCollection("song_repo_settings");
            SongDatas = songRepoSettings.DefineSetting("SavedSongSettings", new List<SongData>());
            SelectedSong = songRepoSettings.DefineSetting("SelectedSong", new Song.ID());
        }

        #endregion

        #region Help Settings

        // MARK: Settings - Help Settings (for initial runs)

        internal SettingEntry<bool> HasShownHelpWindow { get; private set; }
        internal SettingEntry<bool> HasShownInitialSongInfo { get; private set; }

        private void InitHelpSettings(SettingCollection settings)
        {
            var helpSettings = settings.AddSubCollection("help_settings");
            HasShownHelpWindow = helpSettings.DefineSetting("HasShownHelpWindow", false);
            HasShownInitialSongInfo = helpSettings.DefineSetting("HasShownInitialSongInfo", false);
        }

        #endregion

        #region Hidden Settings

        // MARK: Settings - Hidden Settings (for initial runs)

        internal SettingEntry<string> LastDefaultSongsLoadedVersion { get; private set; }

        private void InitHiddenSettings(SettingCollection settings)
        {
            var hiddenSettings = settings.AddSubCollection("hidden_settings");
            LastDefaultSongsLoadedVersion = hiddenSettings.DefineSetting("LastDefaultSongsLoadedVersion", "0.0.0");
        }

        #endregion

        // MARK: Settings - Dispose Settings

        public void Dispose()
        {
            ClearSettingChanged(Orientation);
            ClearSettingChanged(BackgroundOpacity);
            ClearSettingChanged(AutoHitWeapon1);
            ClearSettingChanged(ShowAbilityIconsForNotes);
            ClearSettingChanged(ShowHotkeys);
            ClearSettingChanged(ShowOnlyCharacterClassSongs);
            ClearSettingChanged(ShowNextAbilitiesCount);
            ClearSettingChanged(StartSongsWithFirstSkill);
            ClearSettingChanged(SwapWeapons);
            ClearSettingChanged(Dodge);
            ClearSettingChanged(Weapon1);
            ClearSettingChanged(Weapon2);
            ClearSettingChanged(Weapon3);
            ClearSettingChanged(Weapon4);
            ClearSettingChanged(Weapon5);
            ClearSettingChanged(HealingSkill);
            ClearSettingChanged(UtilitySkill1);
            ClearSettingChanged(UtilitySkill2);
            ClearSettingChanged(UtilitySkill3);
            ClearSettingChanged(EliteSkill);
            ClearSettingChanged(ProfessionSkill1);
            ClearSettingChanged(ProfessionSkill2);
            ClearSettingChanged(ProfessionSkill3);
            ClearSettingChanged(ProfessionSkill4);
            ClearSettingChanged(ProfessionSkill5);
            ClearSettingChanged(WeaponStow);
            ClearSettingChanged(SongDatas);
            ClearSettingChanged(SelectedSong);
            ClearSettingChanged(HasShownHelpWindow);
            ClearSettingChanged(HasShownInitialSongInfo);
            ClearSettingChanged(LastDefaultSongsLoadedVersion);
        }

        /**
         * A bit hacky, but this will go find any delegates set up on SettingsChanged and remove them.
         *
         * The settings are stored in the main Blish module, not in the DDR module, which means when DDR
         * gets disabled and tries to clean itself up, there may still be references held by the Blish settings
         * system. This reaches into that area of blish and removes them. As this only touches settings defined
         * by DDR, this shouldn't have any effect on other modules the user is running.
         */
        private static void ClearSettingChanged<T>(SettingEntry<T> entry)
        {
            FieldInfo f1 = GetEventField(entry.GetType(), "SettingChanged");
            if (f1 == null) return;

            object obj = f1.GetValue(entry);
            if (obj == null) return;

            EventHandler<ValueChangedEventArgs<T>> handler = (EventHandler<ValueChangedEventArgs<T>>)obj;
            Delegate[] dary = handler.GetInvocationList();
            foreach (Delegate del in dary)
            {
                entry.SettingChanged -= (EventHandler<ValueChangedEventArgs<T>>)del;
            }
        }

        private static FieldInfo GetEventField(Type type, string eventName)
        {
            FieldInfo field = null;
            while (type != null)
            {
                /* Find events defined as field */
                field = type.GetField(eventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) ||
                                      field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                    break;

                /* Find events defined as property { add; remove; } */
                field = type.GetField("EVENT_" + eventName.ToUpper(),
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    break;
                type = type.BaseType;
            }

            return field;
        }
    }
}