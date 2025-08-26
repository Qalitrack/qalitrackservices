using System.Text.Json;
using BackupService.Core.Dtos;
using BackupService.Core.Entities;
using BackupService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using BackupService.Core.Enums;

namespace BackupService.Core.Services
{
    public class JsonBackupMetadataService : IBackupMetadataService
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<JsonBackupMetadataService> _logger;
        private readonly string _metadataFilePath;
        private BackupMetadata _metadata;
        private bool _isLoaded = false;

        public JsonBackupMetadataService(
            IFileSystem fileSystem,
            ILogger<JsonBackupMetadataService> logger,
            string metadataDirectory)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _metadataFilePath = Path.Combine(metadataDirectory, "backup_metadata.json");
            _metadata = new BackupMetadata { Chains = new List<BackupChain>() };
        }

        public async Task LoadMetadataAsync(CancellationToken ct = default)
        {
            if (_isLoaded) return;

            try
            {
                if (_fileSystem.File.Exists(_metadataFilePath))
                {
                    var json = await _fileSystem.File.ReadAllTextAsync(_metadataFilePath, ct);
                    _metadata = JsonSerializer.Deserialize<BackupMetadata>(json) ?? new BackupMetadata { Chains = new List<BackupChain>() };
                    _logger.LogInformation("Loaded backup metadata from {FilePath}", _metadataFilePath);
                }
                else
                {
                    _metadata = new BackupMetadata { Chains = new List<BackupChain>() };
                    _logger.LogInformation("No existing metadata found, creating new metadata");
                }
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load metadata from {FilePath}", _metadataFilePath);
                _metadata = new BackupMetadata { Chains = new List<BackupChain>() };
                _isLoaded = true;
            }
        }

        public async Task SaveMetadataAsync(BackupMetadata metadata, CancellationToken ct = default)
        {
            try
            {
                var directory = Path.GetDirectoryName(_metadataFilePath);
                if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
                {
                    _fileSystem.Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(metadata, options);
                await _fileSystem.File.WriteAllTextAsync(_metadataFilePath, json, ct);
                
                _metadata = metadata;
                _logger.LogDebug("Saved backup metadata to {FilePath}", _metadataFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save metadata to {FilePath}", _metadataFilePath);
                throw;
            }
        }

        // Updated to accept BackupResult with LSN and Timeline information
        public async Task UpdateMetadataWithFullBackupAsync(BackupResult backupResult, string microservice, CancellationToken ct = default)
        {
            await EnsureMetadataLoaded(ct);

            var chain = new BackupChain
            {
                Id = GenerateNewChainId(),
                MicroserviceName = microservice,
                FullBackupFile = backupResult.FileName,
                Timestamp = backupResult.CreatedAt,
                Incrementals = new List<string>(),
                Lsn = backupResult.Lsn,
                Timeline = backupResult.Timeline,
            };

            _metadata.Chains.Add(chain);
            await SaveMetadataAsync(_metadata, ct);
            _logger.LogInformation("Added full backup chain for {Microservice}: {BackupFile}, LSN: {Lsn}, Timeline: {Timeline}", 
                microservice, backupResult.FileName, backupResult.Lsn, backupResult.Timeline);
        }

        // Overload for backward compatibility
        public async Task UpdateMetadataWithFullBackupAsync(string backupFileName, string microservice, CancellationToken ct = default)
        {
            await EnsureMetadataLoaded(ct);

            var chain = new BackupChain
            {
                Id = GenerateNewChainId(),
                MicroserviceName = microservice,
                FullBackupFile = backupFileName,
                Timestamp = DateTime.UtcNow,
                Incrementals = new List<string>()
            };

            _metadata.Chains.Add(chain);
            await SaveMetadataAsync(_metadata, ct);
            _logger.LogWarning("Added full backup chain for {Microservice}: {BackupFile} without LSN/Timeline information", 
                microservice, backupFileName);
        }

        
        
        
     

        public async Task<List<BackupChain>> GetAllBackupChainsAsync(string? microservice = null, CancellationToken ct = default)
        {
            await EnsureMetadataLoaded(ct);
            return microservice == null 
                ? _metadata.Chains 
                : _metadata.Chains.Where(c => c.MicroserviceName == microservice).ToList();
        }


        public async Task<List<BackupFileInfo>> GetAvailableBackupsAsync(string? microservice = null, CancellationToken ct = default)
        {
            await EnsureMetadataLoaded(ct);
            
            var chains = microservice == null 
                ? _metadata.Chains 
                : _metadata.Chains.Where(c => c.MicroserviceName == microservice).ToList();

            var result = new List<BackupFileInfo>();
            
            foreach (var chain in chains)
            {
                result.Add(new BackupFileInfo
                {
                    BackupId = chain.Id.ToString(),
                    FileName = chain.FullBackupFile,
                    BackupType = BackupType.Full,
                    CreatedAt = chain.Timestamp,
                    ChainId = chain.ChainId ?? chain.Id.ToString(),
                    IsLatest = IsLatestChain(chain),
                    ServiceName = chain.MicroserviceName
                });

                foreach (var inc in chain.Incrementals)
                {
                    result.Add(new BackupFileInfo
                    {
                        BackupId = Path.GetFileNameWithoutExtension(inc),
                        FileName = inc,
                        CreatedAt = chain.Timestamp, // Use chain timestamp as approximation
                        ChainId = chain.ChainId ?? chain.Id.ToString(),
                        IsLatest = IsLatestChain(chain),
                        ServiceName = chain.MicroserviceName
                    });
                }
            }
            
            return result;
        }
        


        public async Task ValidateMetadataIntegrityAsync(string? microservice = null, CancellationToken ct = default)
        {
            await EnsureMetadataLoaded(ct);
            
            var chains = microservice == null 
                ? _metadata.Chains 
                : _metadata.Chains.Where(c => c.MicroserviceName == microservice).ToList();

            foreach (var chain in chains)
            {
                if (string.IsNullOrEmpty(chain.FullBackupFile) && chain.Incrementals.Count > 0)
                {
                    _logger.LogWarning("Invalid chain {ChainId}: No full backup", chain.Id);
                }

                if (string.IsNullOrEmpty(chain.Lsn) || chain.Timeline == null)
                {
                    _logger.LogWarning("Chain {ChainId} missing LSN or Timeline information", chain.Id);
                }
            }
        }

        
    

        public void Dispose()
        {
            // Cleanup if needed
        }

        private async Task EnsureMetadataLoaded(CancellationToken ct = default)
        {
            if (!_isLoaded)
            {
                await LoadMetadataAsync(ct);
            }
        }

        private int GenerateNewChainId()
        {
            return _metadata.Chains.Any() ? _metadata.Chains.Max(c => c.Id) + 1 : 1;
        }

        private bool IsLatestChain(BackupChain chain)
        {
            return _metadata.Chains
                .Where(c => c.MicroserviceName == chain.MicroserviceName)
                .OrderByDescending(c => c.Timestamp)
                .FirstOrDefault()?.Id == chain.Id;
        }
    }
}