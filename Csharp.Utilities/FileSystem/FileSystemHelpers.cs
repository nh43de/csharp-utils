﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Csharp.Utilities.FileSystem
{
    public static class FileSystemHelpers
    {
        public static void DirectoryCopy(string sourceDirName, string destDirName, IProgress<string> progress, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();

            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);

                progress.Report("Copying " + file.Name);

                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (!copySubDirs)
                return;

            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, progress, copySubDirs);
            }
        }
    }
}
