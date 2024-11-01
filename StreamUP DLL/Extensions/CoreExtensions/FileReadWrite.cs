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

        //! This is Andi's thing
        public void WriteToTextFile(string textToSave, string fileName, string filePath)
        {
            string fileNameFull = $"{fileName}.txt";
            Directory.CreateDirectory(filePath);
            string completePath = Path.Combine(filePath, fileNameFull);

            // Use FileStream with FileShare.ReadWrite to allow other processes to read the file while it's being written.
            using (var fileStream = new FileStream(completePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
            {
                writer.WriteLine(textToSave);
            }
        }

        //!Admin Methods

        //# CreateFile  This method Creates a File
        public bool CreateFile(string fileLocation, string fileName)
        {
            try
            {
                // Perform checks using helper methods
                ValidatePath(fileLocation);
                ValidateFileName(fileName);

                string filePath = Path.Combine(fileLocation, fileName);
                ValidatePathLength(filePath);
                CheckWritePermission(fileLocation);

                // Ensure directory exists
                Directory.CreateDirectory(fileLocation);

                // Create file and immediately release handle
                using (File.Create(filePath))
                {
                    // File created successfully
                    LogInfo("File is Created");
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error creating file: {ex.Message}");
                return false;
            }
        }

        //# ClearFile - This method will clear a Text File
        //todo Revisit
        public bool ClearTextFile(string filePath, out int totalLines)
        {
            totalLines = -1;
            try
            {
                DoesFileExist(filePath);
                string[] users = File.ReadAllLines(filePath);
                totalLines = users.Length;
                File.WriteAllText(filePath, string.Empty);
                LogInfo("File Cleared");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error clearing text file: {ex.Message}");
                return false;
            }
        }

        //# Delete File - This method will delete the file
        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);

            if (!File.Exists(filePath))
            {
                LogInfo("File successfully deleted.");
            }
            else
            {
                LogError("File could not be deleted.");
            }
        }


        //# DeleteLines
        public bool DeleteLines(string filePath, int count = 1)
        {


            string[] lines = ReadAllLines(filePath);


            string[] linesRemoved = lines.Skip(count).ToArray();

            //!! File.WriteAllLines(filePath, linesRemoved);
            return true;
        }
        //# DeleteLine
        public bool DeleteLine(string filePath, int index = 0)
        {

            string[] users = ReadAllLines(filePath);
            List<string> list = users.ToList();
            if (index >= 0 && index < list.Count)
            {
                list.RemoveAt(index);
                //!! File.WriteAllLines(filePath, lines);
            }
            else
            {
                LogError("Error: Line index out of bounds");
            }



            return true;
        }

        //RemoveText(bool allInstances = false)

        //! Reading Methods
        //# ReadAllLines - This reads all the line and returns string[]
        public string[] ReadAllLines(string filePath)
        {
            if (!DoesFileExist(filePath))
            {
                LogError($"Error text file doesn't exist");
                return new string[0];
            }
            string[] lines = File.ReadAllLines(filePath);
            int totalLines = lines.Length;

            //? 0 check
            if (totalLines == 0)
            {
                LogError("Error File is Empty");
                return new string[0];
            }
            return lines;

        }

        //# GetLinesCount - this returns an int
        public int GetLinesCount(string filePath)
        {
            string[] users = ReadAllLines(filePath);
            return users.Length;
        }

        //# ReadAllText - This will read All the Text
        public string ReadText(string filePath)
        {
            if (!DoesFileExist(filePath))
            {
                LogError($"Error text file doesn't exist");
                return string.Empty;
            }
            string text = File.ReadAllText(filePath);

            //? 0 check
            if (string.IsNullOrEmpty(text))
            {
                LogError("Error File is Empty");
                return string.Empty;
            }

            return text;

        }




        //# ReadLines(int count) - This method will read the Top X amount of lines
        public string[] ReadLines(string filePath, int count = 1)
        {
            string[] lines = ReadAllLines(filePath);
            if (count > lines.Length)
            {
                LogError("Error Requested too many lines, pull what it can");
                count = lines.Length;
            }


            return lines.Take(count).ToArray();

        }




        //# ReadLine(int index)
        public string ReadLine(string filePath, int index = 0)
        {

            string[] lines = ReadAllLines(filePath);
            if (lines.Length == 0)
            {
                LogError("Error File is Empty");
                return string.Empty;
            }

            if (index >= 0 && index < lines.Length)
            {
                return lines[index];
            }
            else
            {
                LogError("Warning: Requested line index is out of bounds. Returning the first line instead.");
                return lines[0];
            }


        }

        //RandomLine(bool remove)
        public string ReadRandomLine(string filePath, bool remove = false)
        {

            string[] lines = ReadAllLines(filePath);
            int count = lines.Length;
            if (count == 0)
            {
                LogError("Error File is Empty");
                return string.Empty;
            }

            int i = random.Next(0, count);


            if (remove)
            {
                DeleteLine(filePath, i);
            }

            return lines[i];
        }


        //! Writing Methods
        //# TextFileAppend - Writes Text To File Appending
        public void TextFileAppend(string filePath, string text)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None);
                using var writer = new StreamWriter(fileStream);
                writer.WriteLine(text);
                LogInfo("Text successfully appended to file.");
            }
            catch (Exception ex)
            {
                LogError($"Unable to write, {ex.Message}");
            }

        }
        //# TextFileWrite - Writes Text to File Overwriting
        public void TextFileWrite(string filePath, string text)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var writer = new StreamWriter(fileStream);
                writer.WriteLine(text);
                LogInfo("Text successfully written to file.");
            }
            catch (Exception ex)
            {
                LogError($"Unable to write, {ex.Message}");
            }

        }

        //# WriteAtLine - This will write a line at a certain index
        public void WriteAtLine(string filePath, string text, int index = -1)
        {
            var lines = ReadAllLines(filePath).ToList();

            if (index >= 0 && index < lines.Count)
            {
                lines.Insert(index, text);
            }
            else
            {
                lines.Add(text);
            }


            WriteLines(filePath, lines.ToArray());
        }


        //# WriteLines - This will write Lines Overwriting
        public void WriteLines(string filePath, string[] lines)
        {
            string textString = String.Join(Environment.NewLine, lines);
            TextFileWrite(filePath, textString);

        }
        //# WriteLinesAppend - This will Append Lines
        public void WriteLinesAppend(string filePath, string[] lines)
        {
            string textString = String.Join(Environment.NewLine, lines);
            TextFileAppend(filePath, textString);

        }

        //WriteLinesAtLine - This will Write multiple Lines at an index

        public void WriteLinesAtLine(string filePath, string[] linesToInsert, int index = -1)
        {

            var lines = File.ReadAllLines(filePath).ToList();

            if (index >= 0 && index < lines.Count)
            {
                lines.InsertRange(index, linesToInsert);
            }
            else
            {
                lines.AddRange(linesToInsert);
            }

            string textString = String.Join(Environment.NewLine, lines);
            TextFileWrite(filePath, textString);

        }


        //! Other Methods

        //ShuffleLines

        public string[] ShuffleLines(string filePath)
        {
            var lines = ReadAllLines(filePath);
            var shuffledLines = lines.OrderBy(_ => random.Next()).ToArray();
            WriteLines(filePath, shuffledLines);
            return shuffledLines;
        }



        //GetIndex
        public int[] GetIndex(string filePath, string value, out int first)
        {
            string[] lines = ReadAllLines(filePath);
            int total = lines.Length;
            //? Check value
            List<string> list = lines.ToList();
            first = -1;
            if (!ContainsIgnoreCase(list, value))
            {
                //? Not in File
                return new int[0];
            }

            int placement = 1;
            int i = 1;

            List<int> positions = new List<int>();
            foreach (string line in list)
            {
                if (value.ToUpper() == line.ToUpper())
                {
                    _CPH.SetArgument("lineFound" + i, placement);
                    i++;
                    positions.Add(placement);
                    
                }

                placement++;
            }
            first = positions[0];
            int[] allPositions = positions.ToArray();
            return allPositions;
        }






        //! Helper Methods
        private bool DoesFileExist(string path)
        {
            return File.Exists(path);
        }
        private void ValidatePath(string path)
        {
            if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new ArgumentException("The file location contains invalid characters.");
            }
        }

        // Helper method to check for invalid characters in the file name
        private void ValidateFileName(string fileName)
        {
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("The file name contains invalid characters.");
            }
        }

        // Helper method to check the full path length
        private void ValidatePathLength(string fullPath)
        {
            if (fullPath.Length > 260) // Maximum path length for Windows
            {
                throw new PathTooLongException("The file path is too long.");
            }
        }
        private void CheckWritePermission(string path)
        {
            try
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                DirectorySecurity security = directory.GetAccessControl();
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException("Insufficient permissions to write to the specified directory.");
            }
        }
    }
}