using System.IO;
using System.Text;

namespace StreamUP
{
    partial class StreamUpLib
    {

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
    }
}