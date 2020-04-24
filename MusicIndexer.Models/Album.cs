using System;
using System.Collections.Generic;
using System.Text;

namespace MusicIndexer.Models
{
    /// <summary>
    /// Represents an album
    /// </summary>
    public class Album
    {
        public string Name { get; set; }

        public int YearOfRelease { get; set; }

        public int YearOfRecording { get; set; }

        public ICollection<Track> Tracks { get; set; } = new List<Track>();
    }
}
