using System;
using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace MusicIndexer
{
    class Program
    {
        static Options options;

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

        static void Verbose(string message, bool lineAfter = true)
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
        }

        private static bool CheckPath(string path)
        {
            return Directory.Exists(path);
        }
    }
}
