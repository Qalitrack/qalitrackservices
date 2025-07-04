namespace ArchiveService.Core.Interfaces
{
    public interface IDataCompressionService
    {
        Task<byte[]> CompressDataAsync<T>(IEnumerable<T> data, string compressionType = "GZIP", CancellationToken cancellationToken = default);
        Task<T[]> DecompressDataAsync<T>(byte[] compressedData, string compressionType = "GZIP", CancellationToken cancellationToken = default);
        Task<byte[]> CompressJsonAsync(string jsonData, string compressionType = "GZIP", CancellationToken cancellationToken = default);
        Task<string> DecompressJsonAsync(byte[] compressedData, string compressionType = "GZIP", CancellationToken cancellationToken = default);
        Task<byte[]> CompressFileAsync(string filePath, string compressionType = "GZIP", CancellationToken cancellationToken = default);
        Task<string> DecompressFileAsync(byte[] compressedData, string outputPath, string compressionType = "GZIP", CancellationToken cancellationToken = default);
        decimal GetCompressionRatio(long originalSize, long compressedSize);
        Task<string> GenerateChecksumAsync(byte[] data, string checksumType = "SHA256", CancellationToken cancellationToken = default);
        Task<bool> ValidateChecksumAsync(byte[] data, string checksum, string checksumType = "SHA256", CancellationToken cancellationToken = default);
        Task<long> GetCompressedSizeAsync(byte[] data, string compressionType = "GZIP", CancellationToken cancellationToken = default);
        IEnumerable<string> GetSupportedCompressionTypes();
        IEnumerable<string> GetSupportedChecksumTypes();
    }
}