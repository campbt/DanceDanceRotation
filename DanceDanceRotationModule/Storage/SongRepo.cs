using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using DanceDanceRotationModule.Model;
using DanceDanceRotationModule.Util;
using Newtonsoft.Json;

namespace DanceDanceRotationModule.Storage
{
    public struct SelectedSongInfo
    {
        /** @Nullable */
        public Song Song { get; set; }
        public SongData Data { get; set; }
    }

    public struct DefaultSongsInfo
    {
        public string version { get; set; }
    }

    /**
     * Stores the current selection of Songs
     */
    public class SongRepo
    {
        private static readonly Logger Logger = Logger.GetLogger<SongRepo>();

        private const string SongsFolderName = "danceDanceRotation/songs";
        private static string SongsDir => DanceDanceRotationModule.Instance.DirectoriesManager.GetFullDirectoryPath(SongsFolderName);

        private Song.ID _selectedSongId;
        private Dictionary<Song.ID, Song> _songs;
        private Dictionary<Song.ID, SongData> _songDatas;

        private FileSystemWatcher _fileSystemWatcher;

        public event EventHandler OnSongsChanged;

        // OnSelectedSongChange
        private EventHandler<SelectedSongInfo> _selectedSongChangedHandler;
        public event EventHandler<SelectedSongInfo> OnSelectedSongChanged
        {
            // This code is to immediately emit the selected song when something subscribes
            add
            {
                _selectedSongChangedHandler =
                    (EventHandler<SelectedSongInfo>)Delegate.Combine(_selectedSongChangedHandler, value);
                InvokeSelectedSongInfo();
            }
            remove
            {
                _selectedSongChangedHandler =
                    (EventHandler<SelectedSongInfo>)Delegate.Remove(_selectedSongChangedHandler, value);
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

            Logger.Info($"Setting selected song to: {songId.Name}");
            _selectedSongId = songId;
            DanceDanceRotationModule.Settings.SelectedSong.Value = songId;
            InvokeSelectedSongInfo();
        }

        // MARK: Mutation

        public Song AddSong(
            string json,
            bool showNotification,
            bool alertListeners = true
        )
        {
            Song song = null;
            Logger.Info($"Attempting to decode into a song:\n{json}");

            try
            {
                song = SongTranslator.FromJson(json);

                Logger.Info(
                    _songs.ContainsKey(song.Id)
                    ? $"Successfully replaced song: {song.Name}"
                    : $"Successfully added song: {song.Name}"
                );

                _songs[song.Id] = song;
                if (alertListeners)
                {
                    OnSongsChanged?.Invoke(sender: this, null);
                }

                if (_selectedSongId.Equals(song.Id))
                {
                    // If the added song has the same ID of the selected song, it overwrites it,
                    // and all screens should update, even though the ID didn't change
                    InvokeSelectedSongInfo();
                }

                var fullFilePath = GetSongPath(song);
                Logger.Info($"Attempting to save song file {fullFilePath}");

                // Disable watcher, or it can infinite loop
                _fileSystemWatcher.EnableRaisingEvents = false;

                string prettyJson = JsonHelper.FormatJson(json);
                File.WriteAllText(fullFilePath, prettyJson);
                Logger.Info($"Successfully saved pretty song file {fullFilePath}");

                if (showNotification)
                {
                    ScreenNotification.ShowNotification("Added Song Successfully");
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(
                    $"Failed to decode clipboard contents into a song:\n{exception.Message}\n{exception}"
                );
                if (showNotification)
                {
                    ScreenNotification.ShowNotification("Failed to decode song.");
                }
            }

            // Re-enable watching
            _fileSystemWatcher.EnableRaisingEvents = true;
            return song;
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

            SongData originalData = GetSongData(songId);
            SongData updatedSongData = work(
                originalData
            );
            if (updatedSongData.Equals(originalData))
            {
                // Ignore. This may be something like a SongInfo callback that didn't actually cause a change
                return;
            }
            Logger.Info($"SongData Updated: {songId.Name}\n{updatedSongData}");
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
                Logger.Info($"Removing song: {songId.Name}");
                Song song = _songs[songId];
                _songs.Remove(songId);
                _songDatas.Remove(songId);

                // Attempt to delete file with this song's name
                var fullFilePath = GetSongPath(song);
                Logger.Info($"Attempting to delete song file {fullFilePath}");
                File.Delete(fullFilePath);

                OnSongsChanged?.Invoke(sender: this, null);
                if (_selectedSongId.Equals(songId))
                {
                    InvokeSelectedSongInfo();
                }
            }
        }

        // MARK: Save/Load

        public Task Load()
        {
            // Load all .json files in songs directory
            LoadSongFiles();
            LoadDefaultSongFiles();

            // Load song specific settings for every song
            var songDatas = DanceDanceRotationModule.Settings.SongDatas.Value;
            foreach (var songData in songDatas)
            {
                _songDatas[songData.Id] = songData;
            }

            Logger.Info($"Setting initial default song {DanceDanceRotationModule.Settings.SelectedSong.Value.Name}");

            // Load the last selected song
            SetSelectedSong(
                DanceDanceRotationModule.Settings.SelectedSong.Value
            );

            return Task.CompletedTask;
        }

        private void LoadSongFiles()
        {
            // Load all .json files in songs directory
            List<Song> loadedSongs = new List<Song>();
            Logger.Info($"Loading song .json files in {SongsDir}");
            foreach (string fileName in Directory.GetFiles(SongsDir, "*.json"))
            {
                try
                {
                    using (StreamReader r = new StreamReader(fileName))
                    {
                        string json = r.ReadToEnd();
                        Song song = SongTranslator.FromJson(json);
                        loadedSongs.Add(song);
                        Logger.Trace($"Successfully loaded song file: {fileName}");
                    }
                }
                catch (Exception exception)
                {
                    Logger.Warn(exception, $"Failed to load song file: {fileName}");
                }
            }
            Logger.Info($"Successfully loaded {loadedSongs.Count} songs.");

            _songs.Clear();
            foreach (var song in loadedSongs)
            {
                _songs[song.Id] = song;
            }
            OnSongsChanged?.Invoke(sender: this, null);
        }

        /**
         * Loads the songs in the defaultSongs/ directory
         */
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private void LoadDefaultSongFiles()
        {
            // Not sure if there is a way to just search for resources in the directory,
            // so all default songs have to be listed here.

            const string defaultSongsInfoFileName = "defaultSongsInfo.json";
            DefaultSongsInfo? defaultSongsInfo = null;
            try
            {
                var fileStream = DanceDanceRotationModule.Instance.ContentsManager.GetFileStream(defaultSongsInfoFileName);
                using (StreamReader r = new StreamReader(fileStream))
                {
                    var json = r.ReadToEnd();
                    defaultSongsInfo = JsonConvert.DeserializeObject<DefaultSongsInfo>(json);
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Failed to load default songs info file: " + defaultSongsInfoFileName);
            }

            if (defaultSongsInfo == null)
            {
                Logger.Warn("Failed to load default songs info file: " + defaultSongsInfoFileName);
                return;
            }

            const string defaultSongsFileName = "defaultSongs.json";
            Logger.Info($"Attempting to load default songs from ref/${defaultSongsFileName}.");
            try
            {
                var fileStream = DanceDanceRotationModule.Instance.ContentsManager.GetFileStream(defaultSongsFileName);
                using (StreamReader r = new StreamReader(fileStream))
                {
                    string lastDefaultSongsLoadedVersion =
                        DanceDanceRotationModule.Settings.LastDefaultSongsLoadedVersion.Value;
                    string version = defaultSongsInfo?.version ?? "(unknown)";

                    bool shouldLoadDefaultSongs = false;
                    if (lastDefaultSongsLoadedVersion.Equals(version) == false)
                    {
                        Logger.Info(
                            $"Module version has changed ({lastDefaultSongsLoadedVersion} -> {version})! Loading default songs");
                        shouldLoadDefaultSongs = true;
                    }
                    else if (_songs.Count == 0)
                    {
                        Logger.Info($"No songs were found in {SongsDir}! Loading default songs");
                        shouldLoadDefaultSongs = true;
                    }

                    if (shouldLoadDefaultSongs)
                    {
                        DanceDanceRotationModule.Settings.LastDefaultSongsLoadedVersion.Value = version;
                        string json = r.ReadToEnd();
                        List<Object> songsArray = JsonConvert.DeserializeObject<List<Object>>(json);
                        foreach (var songJson in songsArray)
                        {
                            AddSong(
                                songJson.ToString(),
                                showNotification: false,
                                alertListeners: false
                            );
                        }
                        Logger.Info($"Successfully loaded {_songs.Count} songs.");
                        OnSongsChanged?.Invoke(sender: this, null);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Failed to load song file: " + defaultSongsFileName);
            }
        }

        /**
         * Saves the repo to disk
         */
        private void Save()
        {
            DanceDanceRotationModule.Settings.SelectedSong.Value = _selectedSongId;
            DanceDanceRotationModule.Settings.SongDatas.Value = _songDatas.Values.ToList();
            Logger.Info("Saved SongRepo");
        }

        public void StartDirectoryWatcher()
        {
            _fileSystemWatcher = new FileSystemWatcher();
            _fileSystemWatcher.Path = SongsDir;

            // Only care about Write changes for Changed. FileName is used to observe Created/Deleted
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            // Only care about song json files
            _fileSystemWatcher.Filter = "*.json";

            _fileSystemWatcher.Changed += OnSongChanged;
            _fileSystemWatcher.Created += OnSongChanged;
            _fileSystemWatcher.Deleted += OnSongChanged;

            // Begin watching.
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        // Event handler
        private static void OnSongChanged(object source, FileSystemEventArgs @event)
        {
            Logger.Info("Song File Event: " + @event.FullPath + " " + @event.ChangeType);
            switch (@event.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    string json = "";
                    try
                    {
                        using (StreamReader r = new StreamReader(@event.FullPath))
                        {
                            json = r.ReadToEnd();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Failed to load newly created or changed file!");
                    }

                    if (json.Length > 0)
                    {
                        DanceDanceRotationModule.SongRepo.AddSong(
                            json,
                            showNotification: true
                        );
                    }
                    break;
                case WatcherChangeTypes.Renamed:
                    // This doesn't matter
                    break;
                case WatcherChangeTypes.All:
                case WatcherChangeTypes.Deleted:
                default:
                    // No easy way to know which song was
                    DanceDanceRotationModule.SongRepo.LoadSongFiles();
                    break;
            }
        }

        // MARK: Get Data

        public List<Song> GetAllSongs()
        {
            return _songs.Values.ToList();
        }

        public Song GetSong(Song.ID songId)
        {
            return _songs[songId];
        }

        public Song.ID GetSelectedSongId()
        {
            return _selectedSongId;
        }

        // MARK: Dispose

        public void Dispose()
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Dispose();

            _songs.Clear();
            _songDatas.Clear();
            Logger.Info("Disposed SongRepo");
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

            _selectedSongChangedHandler?.Invoke(
                sender: this,
                new SelectedSongInfo()
                {
                    Song = song,
                    Data = songData
                }
            );
        }

        private string GetSongPath(Song song)
        {
            return Path.Combine(SongsDir, $"{song.Name}.json");
        }
    }
}