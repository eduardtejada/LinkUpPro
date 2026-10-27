using LinkUpPro.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace LinkUpPro.Infrastructure.Shared.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<FileStorageService> _logger;

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 Megabytes

    public FileStorageService(IWebHostEnvironment webHostEnvironment, ILogger<FileStorageService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string destinationFolder)
    {
        var webRootPath = _webHostEnvironment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadsPath = Path.Combine(webRootPath, "uploads", destinationFolder);
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullFilePath = Path.Combine(uploadsPath, uniqueFileName);

        using (var outputStream = new FileStream(fullFilePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputStream);
        }

        return $"/uploads/{destinationFolder}/{uniqueFileName}".Replace("\\", "/");
    }

    public bool DeleteFile(string? relativeFilePath)
    {
        if (string.IsNullOrWhiteSpace(relativeFilePath))
        {
            return false;
        }

        var webRootPath = _webHostEnvironment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        // Remover barra inicial si existe para combinar correctamente
        var cleanRelative = relativeFilePath.TrimStart('/').TrimStart('\\');
        var fullPath = Path.Combine(webRootPath, cleanRelative);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Archivo eliminado: {Path}", fullPath);
            return true;
        }

        return false;
    }

    public bool ValidateImage(string fileName, long lengthInBytes, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(fileName) || lengthInBytes <= 0)
        {
            errorMessage = "El archivo seleccionado está vacío o no es válido.";
            return false;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
        {
            errorMessage = $"Formato no permitido. Solo se admiten: {string.Join(", ", AllowedImageExtensions)}.";
            return false;
        }

        if (lengthInBytes > MaxFileSizeBytes)
        {
            errorMessage = "El tamaño de la imagen no puede exceder los 5 MB.";
            return false;
        }

        return true;
    }
}
