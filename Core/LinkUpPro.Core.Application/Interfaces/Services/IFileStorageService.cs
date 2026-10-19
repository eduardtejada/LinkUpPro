namespace LinkUpPro.Core.Application.Interfaces.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Guarda un archivo en el sistema de almacenamiento local y retorna la ruta web relativa.
    /// </summary>
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string destinationFolder);

    /// <summary>
    /// Elimina un archivo físico del almacenamiento local a partir de su ruta web relativa.
    /// </summary>
    bool DeleteFile(string? relativeFilePath);

    /// <summary>
    /// Valida que el archivo cumpla con las extensiones (.jpg, .jpeg, .png, .webp) y tamaño maximo (5 MB).
    /// </summary>
    bool ValidateImage(string fileName, long lengthInBytes, out string errorMessage);
}
