using System;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<string> Artists
        {
            get
            {
                var artists = AlbumArtists;
                artists.AddRange(Performers);
                return artists.Distinct();
            }
        }

        public IEnumerable<string> ArtistsWithAlbum
        {
            get
            {
                return AlbumArtists.Select(a => $"{a} - {Album}").Distinct();
            }
        }

        public List<string> Performers { get; set; } = new List<string>();

        public List<string> AlbumArtists 
        { 
            get
            {
                // even though there is one Tag.AlbumArtist in some tracks, multiple ones were being passed to this property
                // dirty hack time
                return _albumArtists.Distinct().ToList();
            }
            set
            {
                _albumArtists = value;
            }
        }
        private List<string> _albumArtists = new List<string>();

        public List<string> Genres { get; set; } = new List<string>();

        public string Path { get; set; }

        public int AudioBitrate { get; set; }

        public int AudioChannels { get; set; }

        public int AudioSampleRate { get; set; }

        public int BitsPerSample { get; set; }

        public List<string> Codecs { get; set; } = new List<string>();

        public string Description { get; set; }

        public int BeatsPerMinute { get; set; }

        public TimeSpan Duration { get; set; }

        public List<string> Composers { get; set; } = new List<string>();

        public string Comment { get; set; }
    }
}
