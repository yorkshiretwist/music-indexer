# Music Indexer

This simple .net core console app indexes music files found in a directory and produces structured data files (mainly JSON) containing a bunch of information about the files. The data for each track includes, where available:

- Title
- Track number
- Year
- Album name
- Performer(s)
- Album artist(s)
- Genre(s)
- File path
- Audio bit rate, channels, sample rate
- Beats per minute
- Duration
- Composer(s)
- Comments from the file tags

This information is extracted using the [excellent TagLib library](https://github.com/mono/taglib-sharp) and handles many different audio formats including MP3, FLAC, OGG, WAV, WMA, AAC and many more.

By default the output files are written to a folder called "MusicIndexerOutput" on your desktop, but this can be changed.

There are currently three command line parameters you can pass:

- `path` (`p`): the path at which to start the recursive scan for files
- `outputPath` (`o`): the path at which to create the "MusicIndexerOutput" output directory (defaults to your desktop)
- `verbose` (`v`): whether to show verbose output (defaults to false)