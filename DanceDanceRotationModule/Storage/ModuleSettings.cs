﻿using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using DanceDanceRotationModule.Model;
using Microsoft.Xna.Framework.Input;

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
        }

        #region General

        // MARK: Settings - General

        internal SettingEntry<float> BackgroundOpacity { get; private set; }
        internal SettingEntry<bool> AutoHitWeapon1 { get; private set; }
        internal SettingEntry<bool> ShowAbilityIconsForNotes { get; private set; }
        internal SettingEntry<bool> ShowHotkeys { get; private set; }
        internal SettingEntry<bool> ShowNextAbilities { get; private set; }

        private void InitGeneral(SettingCollection settings)
        {
            var generalSettings = settings.AddSubCollection(
                collectionKey: "general_settings",
                renderInUi: true,
                displayNameFunc: () => "General",
                lazyLoaded: false
            );

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

            ShowNextAbilities = generalSettings.DefineSetting("ShowNextAbilities",
                false,
                () => "Show next abilities",
                () => "If enabled, the next few ability icons will be shown.");
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
            HealingSkill     = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.HealingSkill);
            UtilitySkill1    = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.UtilitySkill1);
            UtilitySkill2    = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.UtilitySkill2 );
            UtilitySkill3    = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.UtilitySkill3);
            EliteSkill       = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.EliteSkill );
            ProfessionSkill1 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill1);
            ProfessionSkill2 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill2);
            ProfessionSkill3 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill3);
            ProfessionSkill4 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill4);
            ProfessionSkill5 = DefineHotkeySetting(AbilityHotkeysSettings, NoteType.ProfessionSkill5);
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
                DanceDanceRotationModule.Instance
                    .GetNotesContainer()
                    ?.OnHotkeyPressed(
                        noteType
                    );
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
                    ?.Reset();
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

        #region Hidden Settings

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
    }
}