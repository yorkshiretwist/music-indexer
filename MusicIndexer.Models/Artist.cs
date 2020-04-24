using System;
using System.Collections.Generic;

namespace MusicIndexer.Models
{
    /// <summary>
    /// Represents an artist
    /// </summary>
    public class Artist
    {
        public string Name { get; set; }

        public ICollection<Album> Albums { get; set; } = new List<Album>();
    }
}
