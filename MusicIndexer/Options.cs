using System;
using CommandLine;

namespace MusicIndexer
{
    /// <summary>
    /// Represents options that can be passed via command line to the app
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Gets or sets a value indicating if verbose output should be enabled
        /// </summary>
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the path at which to start scanning files
        /// </summary>
        [Option('p', "path", Required = true, HelpText = "Set the path at which to start scanning files.")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the path at which to start scanning files
        /// </summary>
        [Option('o', "outputPath", Required = false, HelpText = "Set the path in which to write the output files.")]
        public string OutputPath { get; set; }
    }
}
