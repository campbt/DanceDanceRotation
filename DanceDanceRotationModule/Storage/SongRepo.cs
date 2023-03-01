using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using DanceDanceRotationModule.Model;
using MonoGame.Extended.Collections;
using Newtonsoft.Json;
using SharpDX.Direct2D1;
using SharpDX.X3DAudio;

namespace DanceDanceRotationModule.Storage
{
    public struct SelectedSongInfo
    {
        /** @Nullable */
        public Song Song { get; set; }
        public SongData Data { get; set; }
    }

    /**
     * Stores the current selection of Songs
     */
    public class SongRepo
    {
        private static readonly Logger Logger = Logger.GetLogger<SongRepo>();

        private const string SONGS_FOLDER_NAME = "songs";
        public static string SongsDir => DanceDanceRotationModule.DanceDanceRotationModuleInstance.DirectoriesManager.GetFullDirectoryPath(SONGS_FOLDER_NAME);

        private Song.ID _selectedSongId;
        private Dictionary<Song.ID, Song> _songs;
        private Dictionary<Song.ID, SongData> _songDatas;

        public event EventHandler OnSongsChanged;

        // OnSelectedSongChange
        private EventHandler<SelectedSongInfo> SelectedSongChangedHandler;
        public event EventHandler<SelectedSongInfo> OnSelectedSongChanged
        {
            // This code is to immediately emit the selected song when something subscribes
            add
            {
                SelectedSongChangedHandler =
                    (EventHandler<SelectedSongInfo>)Delegate.Combine(SelectedSongChangedHandler, value);
                InvokeSelectedSongInfo();
            }
            remove
            {
                SelectedSongChangedHandler =
                    (EventHandler<SelectedSongInfo>)Delegate.Remove(SelectedSongChangedHandler, value);
            }
        }

        public SongRepo()
        {
            _songs = new Dictionary<Song.ID, Song>();
            _songDatas = new Dictionary<Song.ID, SongData>();
        }

        public void SetSelectedSong(Song.ID songId)
        {
            if (_selectedSongId.Equals(songId))
                return;

            _selectedSongId = songId;
            InvokeSelectedSongInfo();
        }

        // MARK: Mutation

        public void AddSong(string json)
        {
            Logger.Info("Attempting to decode into a song:\n" + json);
            try
            {
                Song song = SongTranslator.FromJson(json);

                Logger.Info(
                    _songs.ContainsKey(song.Id)
                    ? "Successfully replaced song: " + song.Name
                    : "Successfully added song: " + song.Name
                );

                _songs[song.Id] = song;
                OnSongsChanged?.Invoke(sender: this, null);
                // If the added song has the same ID of the selected song, it overwrites it, and all screens should update
                if (_selectedSongId.Equals(song.Id))
                {
                    InvokeSelectedSongInfo();
                }

                var fullFilePath = Path.Combine(SongsDir, $"{song.Name}.json");
                Logger.Info("Attempting to save song file " + fullFilePath);
                File.WriteAllText(fullFilePath, json);
                Logger.Info("Successfull saved song file " + fullFilePath);
                ScreenNotification.ShowNotification("Added Song Successfully");
            }
            catch (Exception exception)
            {
                Logger.Warn(
                    "Failed to decode clipboard contents into a song:\n" +
                    exception.Message + "\n" +
                    exception
                );
                ScreenNotification.ShowNotification("Failed to decode song.");
            }

        }

        /**
         * Returns [SongData] for the songId, or the Default one if one doesn't exist
         */
        public SongData GetSongData(Song.ID songId)
        {
            if (_songDatas.ContainsKey(songId))
            {
                return _songDatas[songId];
            }
            else
            {
                return SongData.DefaultSettings(songId);
            }
        }

        public void UpdateData(
            Song.ID songId,
            Func<SongData, SongData> work
        )
        {
            SongData updatedSongData = work(
                GetSongData(songId)
            );
            updatedSongData.Id = songId;
            _songDatas[songId] = updatedSongData;
            Save();
            if (
                songId.Equals(_selectedSongId) &&
                _songs.ContainsKey(songId)
            ) {
                InvokeSelectedSongInfo();
            }
        }

        public void DeleteSong(Song.ID songId)
        {
            if (_songs.ContainsKey(songId))
            {
                _songs.Remove(songId);
                _songDatas.Remove(songId);
                Save();
                OnSongsChanged?.Invoke(sender: this, null);
                if (_selectedSongId.Equals(songId))
                {
                    InvokeSelectedSongInfo();
                }
            }
        }

        // MARK: Save/Load

        public void Load()
        {
            // Load all .json files in songs directory
            List<Song> loadedSongs = new List<Song>();
            Logger.Info("Loading song .json files in " + SongsDir);
            foreach (string fileName in Directory.GetFiles(SongsDir, "*.json"))
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    try
                    {
                        string json = r.ReadToEnd();
                        Song song = SongTranslator.FromJson(json);
                        loadedSongs.Add(song);
                        Logger.Trace("Successfully loaded song file: " + fileName);
                    }
                    catch (Exception exception)
                    {
                        Logger.Warn(exception, "Failed to load song file: " + fileName);
                    }
                }
                // Do something with the file content
            }
            Logger.Info($"Successfully loaded {loadedSongs.Count} songs.");

            // TODO: Move these settings into this repo?
            _selectedSongId = DanceDanceRotationModule.DanceDanceRotationModuleInstance.SelectedSong.Value;
            var songDatas = DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongDatas.Value;

            foreach (var song in loadedSongs)
            {
                _songs[song.Id] = song;
            }
            foreach (var songData in songDatas)
            {
                _songDatas[songData.Id] = songData;
            }
            EventHandler onSongsChanged = this.OnSongsChanged;
            if (onSongsChanged != null)
            {
                onSongsChanged.Invoke(sender: this, null);
            }
            InvokeSelectedSongInfo();
        }

        /**
         * Saves the repo to disk
         */
        private void Save()
        {
            DanceDanceRotationModule.DanceDanceRotationModuleInstance.SelectedSong.Value = _selectedSongId;
            DanceDanceRotationModule.DanceDanceRotationModuleInstance.SongDatas.Value = _songDatas.Values.ToList();
        }

        public List<Song> GetAllSongs()
        {
            return _songs.Values.ToList();
        }

        public Song GetSong(Song.ID songId)
        {
            return _songs[songId];
        }

        // MARK: Utility

        private void InvokeSelectedSongInfo()
        {
            Song song =
                (_songs.ContainsKey(_selectedSongId))
                    ? _songs[_selectedSongId]
                    : null;
            SongData songData =
                (_songDatas.ContainsKey(_selectedSongId))
                    ? _songDatas[_selectedSongId]
                    : SongData.DefaultSettings(_selectedSongId);

            SelectedSongChangedHandler?.Invoke(
                sender: this,
                new SelectedSongInfo()
                {
                    Song = song,
                    Data = songData
                }
            );
        }
    }
}