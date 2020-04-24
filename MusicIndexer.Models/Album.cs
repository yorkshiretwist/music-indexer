using System.Collections.Generic;

namespace MusicIndexer.Models
{
    /// <summary>
    /// Represents an album
    /// </summary>
    public class Album
    {
        public string Title { get; set; }

        public int Year { get; set; }

        public List<Track> Tracks { get; set; } = new List<Track>();
    }
}
