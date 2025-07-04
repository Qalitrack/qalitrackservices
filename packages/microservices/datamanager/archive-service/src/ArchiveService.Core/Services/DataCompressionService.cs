using ArchiveService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ArchiveService.Core.Services
{
    public class DataCompressionService : IDataCompressionService
    {
        private readonly ILogger<DataCompressionService> _logger;

        public DataCompressionService(ILogger<DataCompressionService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> CompressDataAsync<T>(IEnumerable<T> data, string compressionType = "GZIP", CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                return await CompressJsonAsync(json, compressionType, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing data with compression type {CompressionType}", compressionType);
                throw;
            }
        }

        public async Task<T[]> DecompressDataAsync<T>(byte[] compressedData, string compressionType = "GZIP", CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await DecompressJsonAsync(compressedData, compressionType, cancellationToken);
                return JsonSerializer.Deserialize<T[]>(json) ?? Array.Empty<T>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decompressing data with compression type {CompressionType}", compressionType);
                throw;
            }
        }

        public async Task<byte[]> CompressJsonAsync(string jsonData, string compressionType = "GZIP", CancellationToken cancellationToken = default)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(jsonData);

                using var compressedStream = new MemoryStream();
                
                switch (compressionType.ToUpper())
                {
                    case "GZIP":
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, true))
                        {
                            await gzipStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                        }
                        break;
                    
                    case "DEFLATE":
                        using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                        {
                            await deflateStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                        }
                        break;
                    
                    default:
                        throw new ArgumentException($"Unsupported compression type: {compressionType}");
                }

                return compressedStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing JSON data with compression type {CompressionType}", compressionType);
                throw;
            }
        }

        public async Task<string> DecompressJsonAsync(byte[] compressedData, string compressionType = "GZIP", CancellationToken cancellationToken = default)
        {
            try
            {
                using var compressedStream = new MemoryStream(compressedData);
                using var decompressedStream = new MemoryStream();

                switch (compressionType.ToUpper())
                {
                    case "GZIP":
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                        {
                            await gzipStream.CopyToAsync(decompressedStream, cancellationToken);
                        }
                        break;
                    
                    case "DEFLATE":
                        using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                        {
                            await deflateStream.CopyToAsync(decompressedStream, cancellationToken);
                        }
                        break;
                    
                    default:
                        throw new ArgumentException($"Unsupported compression type: {compressionType}");
                }

                return Encoding.UTF8.GetString(decompressedStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decompressing JSON data with compression type {CompressionType}", compressionType);
                throw;
            }
        }

        public async Task<byte[]> CompressFileAsync(string filePath, string compressionType = "GZIP", CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                
                using var compressedStream = new MemoryStream();
                
                switch (compressionType.ToUpper())
                {
                    case "GZIP":
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, true))
                        {
                            await gzipStream.WriteAsync(fileBytes, 0, fileBytes.Length, cancellationToken);
                        }
                        break;
                    
                    case "DEFLATE":
                        using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                        {
                            await deflateStream.WriteAsync(fileBytes, 0, fileBytes.Length, cancellationToken);
                        }
                        break;
                    
                    default:
                        throw new ArgumentException($"Unsupported compression type: {compressionType}");
                }

                return compressedStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing file {FilePath} with compression type {CompressionType}", filePath, compressionType);
                throw;
            }
        }

        public async Task<string> DecompressFileAsync(byte[] compressedData, string outputPath, string compressionType = "GZIP", CancellationToken cancellationToken = default)
        {
            try
            {
                using var compressedStream = new MemoryStream(compressedData);
                
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await using var outputFileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

                switch (compressionType.ToUpper())
                {
                    case "GZIP":
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                        {
                            await gzipStream.CopyToAsync(outputFileStream, cancellationToken);
                        }
                        break;
                    
                    case "DEFLATE":
                        using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                        {
                            await deflateStream.CopyToAsync(outputFileStream, cancellationToken);
                        }
                        break;
                    
                    default:
                        throw new ArgumentException($"Unsupported compression type: {compressionType}");
                }

                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decompressing file to {OutputPath} with compression type {CompressionType}", outputPath, compressionType);
                throw;
            }
        }

        public decimal GetCompressionRatio(long originalSize, long compressedSize)
        {
            if (originalSize == 0) return 0;
            return Math.Round((decimal)(originalSize - compressedSize) / originalSize * 100, 2);
        }

        public async Task<string> GenerateChecksumAsync(byte[] data, string checksumType = "SHA256", CancellationToken cancellationToken = default)
        {
            try
            {
                HashAlgorithm hashAlgorithm = checksumType.ToUpper() switch
                {
                    "SHA256" => SHA256.Create(),
                    "SHA1" => SHA1.Create(),
                    "MD5" => MD5.Create(),
                    _ => throw new ArgumentException($"Unsupported checksum type: {checksumType}")
                };

                using (hashAlgorithm)
                {
                    var hash = await Task.Run(() => hashAlgorithm.ComputeHash(data), cancellationToken);
                    return Convert.ToHexString(hash).ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating checksum with type {ChecksumType}", checksumType);
                throw;
            }
        }

        public async Task<bool> ValidateChecksumAsync(byte[] data, string checksum, string checksumType = "SHA256", CancellationToken cancellationToken = default)
        {
            try
            {
                var calculatedChecksum = await GenerateChecksumAsync(data, checksumType, cancellationToken);
                return string.Equals(calculatedChecksum, checksum, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating checksum with type {ChecksumType}", checksumType);
                throw;
            }
        }

        public async Task<long> GetCompressedSizeAsync(byte[] data, string compressionType = "GZIP", CancellationToken cancellationToken = default)
        {
            try
            {
                using var compressedStream = new MemoryStream();
                
                switch (compressionType.ToUpper())
                {
                    case "GZIP":
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, true))
                        {
                            await gzipStream.WriteAsync(data, 0, data.Length, cancellationToken);
                        }
                        break;
                    
                    case "DEFLATE":
                        using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                        {
                            await deflateStream.WriteAsync(data, 0, data.Length, cancellationToken);
                        }
                        break;
                    
                    default:
                        throw new ArgumentException($"Unsupported compression type: {compressionType}");
                }

                return compressedStream.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating compressed size with compression type {CompressionType}", compressionType);
                throw;
            }
        }

        public IEnumerable<string> GetSupportedCompressionTypes()
        {
            return new[] { "GZIP", "DEFLATE" };
        }

        public IEnumerable<string> GetSupportedChecksumTypes()
        {
            return new[] { "SHA256", "SHA1", "MD5" };
        }
    }
}