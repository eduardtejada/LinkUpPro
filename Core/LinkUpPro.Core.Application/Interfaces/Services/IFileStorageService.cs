namespace LinkUpPro.Core.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string destinationFolder);

    bool DeleteFile(string? relativeFilePath);

    bool ValidateImage(string fileName, long lengthInBytes, out string errorMessage);
}
