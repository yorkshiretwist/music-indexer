using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text.Json;
using CommandLine;
using MongoDB.Driver;
using MusicLibrary.Models;

namespace MusicLibrary.Indexer
{
    class Program
    {
        static Options options;
        static ICollection<string> ScannedPaths { get; set; } = new List<string>();
        static ICollection<Track> Tracks { get; set; } = new List<Track>();

        static void Main(string[] args)
        {
            Console.WriteLine($"Starting ... attempting to parse arguments");

            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(Start)
                   .WithNotParsed(HandleParseError);

            Console.WriteLine($"Press 'enter' key to exit.");
            Console.ReadLine();
        }

        static void HandleParseError(IEnumerable<Error> errors)
        {
            Error("Failed to parse command line arguments");
            foreach(var error in errors)
            {
                Error($"- {error}", false);
            }
        }

        static void Error(string message, bool lineAfter = true)
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
            Console.ForegroundColor = fg;
            if (lineAfter)
            {
                Console.WriteLine();
            }
        }

        static void Verbose(string message, bool lineAfter = false)
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(message);
            Console.ForegroundColor = fg;
            if (lineAfter)
            {
                Console.WriteLine();
            }
        }

        static void Write(string message, bool lineAfter = true)
        {
            var fg = Console.ForegroundColor;
            Console.WriteLine(message);
            Console.ForegroundColor = fg;
            if (lineAfter)
            {
                Console.WriteLine();
            }
        }

        static void Start(Options opts)
        {
            if (string.IsNullOrWhiteSpace(opts.OutputPath))
            {
                opts.OutputPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }

            Write("Do you want to push the data to a MongoDB-compatible database? If so enter the connnection string here, then press enter to continue:");
            opts.MongoConnectionString = Console.ReadLine();

            options = opts;

            if (options.Verbose)
            {
                Write($"Verbose output enabled.");
            }

            if (!CheckPath(options.Path))
            {
                Error($"The path '{options.Path}' was not found or is not accessible");
                return;
            } else if (options.Verbose)
            {
                Verbose($"The path '{options.Path}' was found and is accessible");
            }

            var directoryInfo = new DirectoryInfo(options.Path);
            ScanDirectory(directoryInfo);

            SaveFiles();
            PushData();
        }

        private static void PushData()
        {
            if (string.IsNullOrWhiteSpace(options.MongoConnectionString))
            {
                Verbose("No MongoDB connection string was entered, so data is not being pushed to a database");
                return;
            }

            var settings = MongoClientSettings.FromUrl(
              new MongoUrl(options.MongoConnectionString)
            );
            settings.SslSettings = new SslSettings() 
            { 
                EnabledSslProtocols = SslProtocols.Tls12 
            };
            var mongoClient = new MongoClient(settings);
            var database = mongoClient.GetDatabase("yorkshiretwist-music-library");

            database.DropCollection("tracks");
            database.CreateCollection("tracks");

            Write("Pushing tracks to MongoDB");
            var batchNo = 1;
            var tracksCollection = database.GetCollection<Track>("tracks");
            foreach (var set in Tracks.InSetsOf(100))
            {
                Verbose($"Batch {batchNo}");
                tracksCollection.InsertMany(set);
                batchNo++;
            }
        }

        private static void SaveFiles()
        {
            var outputPath = Path.Combine(options.OutputPath, "MusicIndexerOutput");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var tracksString = JsonSerializer.Serialize(Tracks);
            File.WriteAllText(Path.Combine(outputPath, "tracks.json"), tracksString);

            var artists = Tracks
                .SelectMany(t => t.Artists, (track, artists) => new { track, artists })
                .GroupBy(t => t.artists, t => t.track)
                .Select(g => new { Artist = g.Key, TrackCount = g.Count() });
            var artistsString = JsonSerializer.Serialize(artists.Distinct().OrderBy(a => a.Artist));
            File.WriteAllText(Path.Combine(outputPath, "artists.json"), artistsString);

            var albumGroupings = Tracks
                .SelectMany(t => t.ArtistsWithAlbum, (track, albumWithArtists) => new { track, albumWithArtists })
                .GroupBy(t => t.albumWithArtists, t => t.track)
                .Select(g => new { Album = g.Key, TrackCount = g.Count() });
            var albumsString = JsonSerializer.Serialize(albumGroupings.OrderBy(a => a.Album));
            File.WriteAllText(Path.Combine(outputPath, "albums.json"), albumsString);

            var genreGroupings = Tracks
                .SelectMany(t => t.Genres, (track, genre) => new { track, genre })
                .GroupBy(t => t.genre, t => t.track)
                .Select(g => new { Genre = g.Key, TrackCount = g.Count() });
            var genresString = JsonSerializer.Serialize(genreGroupings.OrderBy(a => a.Genre));
            File.WriteAllText(Path.Combine(outputPath, "genres.json"), genresString);

            var longestTracks = Tracks
                .OrderByDescending(x => x.Duration)
                .Take(25);
            var longestTracksString = JsonSerializer.Serialize(longestTracks);
            File.WriteAllText(Path.Combine(outputPath, "longest-tracks.json"), longestTracksString);

            var shortestTracks = Tracks
                .Where(x => x.Duration > new TimeSpan())
                .OrderBy(x => x.Duration)
                .Take(25);
            var shortestTracksString = JsonSerializer.Serialize(shortestTracks);
            File.WriteAllText(Path.Combine(outputPath, "shortest-tracks.json"), shortestTracksString);
        }

        private static void ScanDirectory(DirectoryInfo directory)
        {
            if (options.Verbose)
            {
                Verbose($"Scanning {directory.FullName}");
            }

            ScannedPaths.Add(directory.FullName);

            try
            {
                var files = directory.GetFiles();
                foreach (var file in files)
                {
                    ReadFile(file);
                }
            } catch (UnauthorizedAccessException ex)
            {
                Error($"Exception getting files in '{directory.FullName}'", false);
                Error(ex.Message);
                return;
            }
            catch (DirectoryNotFoundException)
            {
                Error($"Directory '{directory.FullName}' was not found");
                return;
            }

            var subDirectories = directory.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                ScanDirectory(subDirectory);
            }

            Write($"Scanned {directory.FullName}", false);
            Write($"{Tracks.Count} tracks indexed so far");
        }

        private static void ReadFile(FileInfo file)
        {
            if (options.Verbose)
            {
                Verbose($"Reading {file.FullName}", true);
            }

            TagLib.File tagFile = null;
            try
            {
                tagFile = TagLib.File.Create(file.FullName);
            } catch (Exception ex)
            {
                Error($"Exception reading file '{file.FullName}'", false);
                Error(ex.Message);
                return;
            }

            if (tagFile == null)
            {
                Error($"Could not create tag file for '{file.FullName}'");
                return;
            }

            if (!tagFile.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Audio))
            {
                if (options.Verbose)
                {
                    Verbose($"File '{file.FullName}; is not an audio file", true);
                }
                return;
            }

            var track = ConstructTrack(file, tagFile);

            if (options.Verbose)
            {
                Verbose($"- Title: {track.Title}");
                Verbose($"- Performers: {string.Join(", ", track.Performers)}");
                Verbose($"- AlbumArtists: {string.Join(", ", track.AlbumArtists)}");
                Verbose($"- Album: {track.Album}");
                Verbose($"- Duration: {track.Duration}");
                Verbose($"- Genres: {string.Join(", ", tagFile.Tag.Genres)}");
                Verbose($"- TrackNumber: {track.TrackNumber}");
                Verbose($"- Year: {track.Year}");
                Verbose($"- BeatsPerMinute: {track.BeatsPerMinute}");
                Verbose($"- Composers: {string.Join(", ", track.Composers)}");
                Verbose($"- AudioBitrate: {track.AudioBitrate}");
                Verbose($"- AudioChannels: {track.AudioChannels}");
                Verbose($"- AudioSampleRate: {track.AudioSampleRate}");
                Verbose($"- BitsPerSample: {track.BitsPerSample}");
                Verbose($"- Codecs: {string.Join(", ", track.Codecs)}");
                Verbose($"- Description: {track.Description}");
                Verbose($"- Comment: {track.Comment}", true);
            }

            IndexTrack(track);
        }

        private static void IndexTrack(Track track)
        {
            Tracks.Add(track);
        }

        private static Track ConstructTrack(FileInfo file, TagLib.File tagFile)
        {
            return new Track
            {
                Title = tagFile.Tag.Title?.Trim(),
                TrackNumber = (int)tagFile.Tag.Track,
                Year = (int)tagFile.Tag.Year,
                Album = tagFile.Tag.Album?.Trim(),
                Performers = tagFile.Tag.Performers?.Select(x => x.Trim()).ToList(),
                AlbumArtists = tagFile.Tag.AlbumArtists?.Select(x => x.Trim()).Distinct().ToList(),
                Genres = tagFile.Tag.Genres?.Select(x => x.Trim()).ToList(),
                Path = file.FullName,
                AudioBitrate = tagFile.Properties.AudioBitrate,
                AudioChannels = tagFile.Properties.AudioChannels,
                AudioSampleRate = tagFile.Properties.AudioSampleRate,
                BitsPerSample = tagFile.Properties.BitsPerSample,
                Codecs = tagFile.Properties.Codecs?.Select(c => c?.ToString()).ToList(),
                Description = tagFile.Properties.Description,
                BeatsPerMinute = (int)tagFile.Tag.BeatsPerMinute,
                Composers = tagFile.Tag.Composers?.Select(x => x.Trim()).ToList(),
                Comment = tagFile.Tag.Comment,
                Duration = tagFile.Properties.Duration
            };
        }

        private static bool CheckPath(string path)
        {
            return Directory.Exists(path);
        }
    }
}
