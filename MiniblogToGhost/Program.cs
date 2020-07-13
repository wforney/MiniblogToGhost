﻿namespace MiniblogToGhost
{
    using MiniblogToGhost.Ghost;
    using MiniblogToGhost.Miniblog;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;

    public static class Program
    {
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it’s new directory.
            foreach (var fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        /// <summary>
        /// Returns file names from given folder that comply to given filters
        /// </summary>
        /// <param name="SourceFolder">Folder with files to retrieve</param>
        /// <param name="Filter">Multiple file filters separated by | character</param>
        /// <param name="searchOption">File.IO.SearchOption, could be AllDirectories or TopDirectoryOnly</param>
        /// <returns>
        /// Array of FileInfo objects that presents collection of file names that meet given filter
        /// </returns>
        public static string[] GetFiles(string SourceFolder, string Filter, SearchOption searchOption)
        {
            // ArrayList will hold all file names
            var alFiles = new ArrayList();

            // Create an array of filter string
            var MultipleFilters = Filter.Split('|');

            // for each filter find mathing file names
            foreach (var FileFilter in MultipleFilters)
            {
                // add found file names to array list
                alFiles.AddRange(Directory.GetFiles(SourceFolder, FileFilter, searchOption));
            }

            // returns string array of relevant file names
            return (string[])alFiles.ToArray(typeof(string));
        }

        public static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // Values are available here
                if (options.Verbose)
                {
                    Logger.Log("Input Directory: {0}", options.InputDirectory);
                    Logger.Log("Output Path: {0}", options.OutputPath);
                }
            }

            var valid = PerformValidation(options);
            if (!valid)
            {
                return;
            }

            var tempFolder = Path.Combine(options.OutputPath, "temp");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            var postAnalysis = AnalysePosts(options);

            var continueToImport = true;
            if (postAnalysis.Failures > 0)
            {
                Console.WriteLine("There were {0} failures.  Do you want to import {1} entries. Y/N", postAnalysis.Failures, postAnalysis.Successes);
                var proceed = Console.ReadKey();
                continueToImport = proceed.KeyChar == 'y' || proceed.KeyChar == 'Y';

                if (continueToImport)
                {
                    Logger.Log("Continuing with import.");
                }
            }

            if (postAnalysis.Posts.Count == 0)
            {
                Console.WriteLine("There are no posts to import.");
                continueToImport = false;
            }

            if (!continueToImport)
            {
                Logger.Log("Exiting");
                return;
            }

            var buildGhostObject = Ghost.Utils.BuildGhostObject(postAnalysis.Posts, options);

            SaveJsonFile(buildGhostObject, options.OutputPath);

            //UploadImagesToCloudinary();

            Directory.Delete(tempFolder, true);

            Logger.Log("Finished");
            Console.ReadKey();
        }

        //private static void CreateZipFileWithImages(string inputDirectory, string outputPath, string tempFolder)
        //{
        //    Log("Copying Images Zip file...");

        // var imagePath = Path.Combine(tempFolder, "images"); Directory.CreateDirectory(imagePath);

        // CopyAll(new DirectoryInfo(Path.Combine(inputDirectory, "files")), new DirectoryInfo(imagePath));

        //    var outputFilename = Path.Combine(outputPath, "output.zip");
        //    if (File.Exists(outputFilename))
        //    {
        //        File.Delete(outputFilename);
        //    }
        //    ZipFile.CreateFromDirectory(tempFolder, outputFilename);
        //    //foreach (var filename in GetFiles(Path.Combine(inputDirectory, "files"),  "*.png|*.gif|*.jpg", SearchOption.AllDirectories))
        //    //{
        //    //    // Copy file to output path
        //    //    File.Copy(filename, imagePath);
        //    //    File
        //    //}
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private static PostAnalysis AnalysePosts(Options options)
        {
            var failures = 0;
            var successes = 0;
            var postList = new List<post>();

            foreach (var filename in Directory.GetFiles(options.InputDirectory, "*.xml"))
            {
                Logger.Log("Filename: {0}", filename);

                try
                {
                    postList.Add(Miniblog.Utils.ExtractPostFromFile(filename));
                    successes++;
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to parse post: {0}", ex.Message);
                    failures++;
                }
            }

            return new PostAnalysis()
            {
                Successes = successes,
                Failures = failures,
                Posts = postList
            };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private static bool PerformValidation(Options options)
        {
            Logger.Log("Validating...");

            var valid = true;
            // Check input path exists
            if (!Directory.Exists(options.InputDirectory))
            {
                Logger.Log("Input directory does not exist.");
                valid = false;
            }

            if (!Directory.Exists(options.OutputPath))
            {
                try
                {
                    Directory.CreateDirectory(options.OutputPath);
                }
                catch
                {
                    valid = false;
                }
            }

            Logger.Log(valid ? "OK" : "Failed");

            return valid;
        }

        private static void SaveJsonFile(GhostFormat buildGhostObject, string outputFolder)
        {
            using (var stream1 = new MemoryStream())
            {
                var serialiser = new DataContractJsonSerializer(typeof(GhostFormat));
                serialiser.WriteObject(stream1, buildGhostObject);
                stream1.Position = 0;
                using (var filestream = File.Create(Path.Combine(outputFolder, "output.json")))
                {
                    stream1.CopyTo(filestream);
                }
            }
        }
    }
}
