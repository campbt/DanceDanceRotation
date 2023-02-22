using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using DanceDanceRotationModule.Model;
using SharpDX.X3DAudio;

namespace DanceDanceRotationModule.Storage
{
    /**
     * Stores the current selection of Songs
     */
    public class SongRepo
    {
        private Dictionary<Song.ID, Song> _songs;

        public event EventHandler OnSongsChanged;
        public event EventHandler<Song> OnSelectedSongChanged;

        public SongRepo()
        {
            _songs = new Dictionary<Song.ID, Song>();
        }

        public void SetSelectedSong(Song.ID songId)
        {
            Song song = _songs[songId];
            if (song != null)
            {
                OnSelectedSongChanged?.Invoke(sender: this, song);
            }
        }

        public SongRepo AddSong(Song song)
        {
            _songs.Remove(song.Id);
            _songs.Add(song.Id, song);
            OnSongsChanged?.Invoke(sender: this, null);
            return this;
        }

        public SongRepo LoadSongs(List<Song> songs)
        {
            foreach (var song in songs)
            {
                _songs.Add(song.Id, song);
            }
            EventHandler onSongsChanged = this.OnSongsChanged;
            if (onSongsChanged != null)
            {
                onSongsChanged.Invoke(sender: this, null);
            }
            return this;
        }

        public SongRepo Reset()
        {
            _songs.Clear();
            foreach (var song in GetInitialSongList())
            {
                AddSong(song);
            }
            return this;
        }

        public List<Song> GetAllSongs()
        {
            return _songs.Values.ToList();
        }

        public Song GetSong(Song.ID songId)
        {
            return _songs[songId];
        }

        private static List<Song> GetInitialSongList()
        {
            List<Song> songs = new List<Song>();
            songs.Add(CreateWeaverSong());

            return songs;
        }

        private static Song CreateWeaverSong()
        {
            List<Note> notes = new List<Note>();
            int time = 0;

            void Add(NoteType noteType, int duration)
            {
                notes.Add(new Note(noteType, TimeSpan.FromMilliseconds(time += duration)));
            }

            Add(NoteType.UtilitySkill1, 1120);
            Add(NoteType.Weapon3, 685);
            for (int i = 0; i < 3; i++)
            {
                Add(NoteType.ProfessionSkill1, 450);
                Add(NoteType.Weapon2, 722);
                Add(NoteType.Weapon3, 800);
                Add(NoteType.UtilitySkill3, 300);
                Add(NoteType.UtilitySkill2, 1040);
                Add(NoteType.ProfessionSkill1, 250);
                Add(NoteType.Weapon1, 1200);
                Add(NoteType.Weapon3, 435);
                Add(NoteType.Weapon4, 480);
                Add(NoteType.Weapon5, 561);
                Add(NoteType.Weapon2, 561);
                Add(NoteType.ProfessionSkill3, 180);
                Add(NoteType.Weapon1, 441);
                Add(NoteType.Weapon1, 436);
                Add(NoteType.Weapon1, 763);
                Add(NoteType.Weapon1, 441);
                Add(NoteType.Weapon1, 436);
                Add(NoteType.Weapon1, 763);
                Add(NoteType.ProfessionSkill3, 180);
                Add(NoteType.Weapon1, 441);
                Add(NoteType.Weapon1, 436);
                Add(NoteType.Weapon1, 763);
                Add(NoteType.Weapon1, 441);
                Add(NoteType.Weapon1, 436);
                Add(NoteType.Weapon1, 763);
                Add(NoteType.Weapon3, 433);
            }

            return new Song()
            {
                Id = new Song.ID("powerweaver"),
                Description = "Very basic Power Weaver rotation.",
                Notes = notes
            };
        }
    }
}