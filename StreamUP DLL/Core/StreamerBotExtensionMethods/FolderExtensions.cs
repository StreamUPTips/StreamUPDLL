using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace StreamUP
{
    partial class StreamUpLib
    {
        public string GetRandomAudioFromFolder(string folderPath)
        {
            // Get all audio files from the folder
            string[] audioFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".aac", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // Check if there are any audio in the folder
            if (audioFiles.Length == 0)
            {
                LogError("No Audio Files in Selected Folder");
                return null;
            }

            // Select a random Audio

            int randomIndex = random.Next(0, audioFiles.Length);

            // Return the full path of the randomly selected Audio
            return audioFiles[randomIndex];
        }

        public string GetRandomImageFromFolder(string folderPath)
        {
            // Get all image files from the folder
            string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // Check if there are any images in the folder
            if (imageFiles.Length == 0)
            {
                LogError("No Image Files in Selected Folder");
                return null;
            }

            // Select a random image
            int randomIndex = random.Next(0, imageFiles.Length);

            // Return the full path of the randomly selected image
            return imageFiles[randomIndex];
        }

        public string GetRandomVideoFromFolder(string folderPath)
        {
            // Get all Video files from the folder
            string[] videoFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".flv", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".webm", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // Check if there are any Videos in the folder
            if (videoFiles.Length == 0)
            {
                LogError("No Video Files in Selected Folder");
                return null;
            }


            int randomIndex = random.Next(0, videoFiles.Length);

            // Return the full path of the randomly selected Video
            return videoFiles[randomIndex];
        }

        public string GetRandomFileFromFolder(string folderPath)
        {
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly).ToArray();

            if (files.Length == 0)
            {
                LogError("No Files in Selected Folder");
                return null;
            }

            int randomIndex = random.Next(0, files.Length);

            // Return the full path of the randomly selected Video
            return files[randomIndex];
        }

        public string GetRandomTextFileFromFolder(string folderPath)
        {
            // Get all text files from the folder
            string[] textFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)).ToArray();

            // Check if there are any txt files in the folder
            if (textFiles.Length == 0)
            {
                LogError("No Text Files in Selected Folder");
                return null;
            }

            // Select a random index
            int randomIndex = random.Next(0, textFiles.Length);

            // Return the full path of the randomly selected text file
            return textFiles[randomIndex];
        }

    }
}
