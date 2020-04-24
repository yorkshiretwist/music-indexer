using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using MusicIndexer.Models;

namespace MusicIndexer
{
    class Program
    {
        static Options options;
        static ICollection<string> scannedPaths { get; set; } = new List<string>();
        static ICollection<Artist> artists { get; set; } = new List<Artist>();
        static ICollection<Album> albums { get; set; } = new List<Album>();

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
        }

        private static void ScanDirectory(DirectoryInfo directory)
        {
            if (options.Verbose)
            {
                Verbose($"Scanning {directory.FullName}");
            }

            scannedPaths.Add(directory.FullName);

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

            if (options.Verbose)
            {
                Verbose($"Scanned {directory.FullName}");
            }
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
        }

        private static Track ConstructTrack(FileInfo file, TagLib.File tagFile)
        {
            return new Track
            {
                Title = tagFile.Tag.Title,
                TrackNumber = (int)tagFile.Tag.Track,
                Year = (int)tagFile.Tag.Year,
                Album = tagFile.Tag.Album,
                Performers = tagFile.Tag.Performers,
                AlbumArtists = tagFile.Tag.AlbumArtists,
                Genres = tagFile.Tag.Genres,
                Path = file.FullName,
                AudioBitrate = tagFile.Properties.AudioBitrate,
                AudioChannels = tagFile.Properties.AudioChannels,
                AudioSampleRate = tagFile.Properties.AudioSampleRate,
                BitsPerSample = tagFile.Properties.BitsPerSample,
                Codecs = tagFile.Properties.Codecs.Select(c => c.ToString()).ToList(),
                Description = tagFile.Properties.Description,
                BeatsPerMinute = (int)tagFile.Tag.BeatsPerMinute,
                Composers = tagFile.Tag.Composers,
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
