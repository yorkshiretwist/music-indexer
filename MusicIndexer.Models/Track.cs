using System;
using System.Collections.Generic;

namespace MusicIndexer.Models
{
    /// <summary>
    /// Represents a track
    /// </summary>
    public class Track
    {
        public string Title { get; set; }

        public int TrackNumber { get; set; }

        public int Year { get; set; }

        public string Album { get; set; }

        public ICollection<string> Performers { get; set; } = new List<string>();

        public ICollection<string> AlbumArtists { get; set; } = new List<string>();

        public ICollection<string> Genres { get; set; } = new List<string>();

        public string Path { get; set; }

        public int AudioBitrate { get; set; }

        public int AudioChannels { get; set; }

        public int AudioSampleRate { get; set; }

        public int BitsPerSample { get; set; }

        public ICollection<string> Codecs { get; set; } = new List<string>();

        public string Description { get; set; }

        public int BeatsPerMinute { get; set; }

        public TimeSpan Duration { get; set; }

        public ICollection<string> Composers { get; set; } = new List<string>();

        public string Comment { get; set; }
    }
}
