using System.IO;

namespace Returnly.Services
{
    public class FileUploadService
    {
        private readonly string[] _supportedExtensions = [".pdf"];
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public ValidationResult ValidateFile(string filePath)
        {
            if (!File.Exists(filePath))
                return new ValidationResult(false, "File does not exist.");

            var extension = Path.GetExtension(filePath).ToLower();
            if (!Array.Exists(_supportedExtensions, ext => ext == extension))
                return new ValidationResult(false, "Only PDF files are supported. Please select a PDF file.");

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > _maxFileSize)
                return new ValidationResult(false, $"File size too large. Maximum allowed: {FormatFileSize(_maxFileSize)}");

            if (fileInfo.Length == 0)
                return new ValidationResult(false, "File is empty.");

            return new ValidationResult(true, "File is valid.");
        }

        public string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return $"{number:n1} {suffixes[counter]}";
        }

        public FileInfo GetFileInfo(string filePath)
        {
            return new FileInfo(filePath);
        }
    }

    public record ValidationResult(bool IsValid, string Message);
}