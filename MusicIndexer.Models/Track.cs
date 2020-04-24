using System;
using System.Collections.Generic;
using System.Text;

namespace MusicIndexer.Models
{
    /// <summary>
    /// Represents a track
    /// </summary>
    public class Track
    {
        public string Name { get; set; }

        public int Number { get; set; }

        public Artist Artist { get; set; }

        public string Gentre { get; set; }
    }
}
