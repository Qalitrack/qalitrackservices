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
        private readonly string _backupDirectory;
        private BackupMetadata _metadata;
        private bool _isLoaded = false;

        public JsonBackupMetadataService(
            IFileSystem fileSystem,
            ILogger<JsonBackupMetadataService> logger,
            string metadataDirectory = "/app/backup-metadata",
            // Was hardcoded to "/app/backups" (lowercase) inside GetAvailableBackupsAsync,
            // independent of this constructor — see BackupCreationService for the same
            // fix and why the case mismatch against the docker-compose volume mattered.
            string backupDirectory = "/app/backups")
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _backupDirectory = backupDirectory;

            // Ensure the directory exists
            if (!_fileSystem.Directory.Exists(metadataDirectory))
            {
                _fileSystem.Directory.CreateDirectory(metadataDirectory);
            }

            _metadataFilePath = Path.Combine(metadataDirectory, "backup_metadata.json");
            _metadata = new BackupMetadata { Chains = new List<BackupChain>() };

            _logger.LogInformation("Using metadata file at: {MetadataPath}", _metadataFilePath);
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
                Timestamp = backupResult.Timestamp,
                Incrementals = new List<string>(),
                Lsn = backupResult.Lsn,
                LastLsn = backupResult.Lsn,
                Timeline = backupResult.Timeline,
                IsPhysical = true,
                FullBackupSizeBytes = backupResult.FileSizeBytes,
            };

            _metadata.Chains.Add(chain);
            await SaveMetadataAsync(_metadata, ct);
            _logger.LogInformation("Added full backup chain for {Microservice}: {BackupFile}, LSN: {Lsn}, Timeline: {Timeline}",
                microservice, backupResult.FileName, backupResult.Lsn, backupResult.Timeline);
        }

        public async Task UpdateMetadataWithIncrementalBackupAsync(BackupResult backupResult, string microservice, CancellationToken ct = default)
        {
            await EnsureMetadataLoaded(ct);

            var chain = _metadata.Chains
                .Where(c => c.MicroserviceName == microservice && c.IsPhysical)
                .OrderByDescending(c => c.Timestamp)
                .FirstOrDefault();

            if (chain == null)
            {
                throw new InvalidOperationException(
                    $"No full backup chain found for {microservice} — an incremental backup needs a full backup to attach to.");
            }

            chain.Incrementals.Add(backupResult.FileName);
            chain.IncrementalSizesBytes.Add(backupResult.FileSizeBytes);
            chain.IncrementalTimestamps.Add(backupResult.Timestamp);
            chain.LastLsn = backupResult.Lsn;

            await SaveMetadataAsync(_metadata, ct);
            _logger.LogInformation("Added incremental backup {BackupFile} to chain {ChainId} for {Microservice}, LSN: {Lsn}",
                backupResult.FileName, chain.ChainId, microservice, backupResult.Lsn);
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
            var backupRoot = _backupDirectory;

            // Physical (pgBackRest) chains live inside the pgbackrest repo volume, which
            // this service has no direct filesystem access to (only docker exec/run) — so
            // their availability is trusted from the chain record itself, not File.Exists.
            var physicalChains = chains.Where(c => c.IsPhysical).ToList();

            // Legacy pg_dump chains: metadata can outlive the file it describes — e.g. the
            // backups volume gets wiped/reset without also clearing this JSON — so a chain
            // only counts as "available" once we confirm its full-backup file actually
            // exists on disk. Otherwise the API reports phantom backups that a user can see
            // and try to restore/download but that don't exist.
            var legacyChains = chains.Except(physicalChains).ToList();
            var validLegacyChains = legacyChains
                .Where(c => _fileSystem.File.Exists(Path.Combine(backupRoot, c.FullBackupFile)))
                .ToList();

            foreach (var missing in legacyChains.Except(validLegacyChains))
            {
                _logger.LogWarning(
                    "Skipping backup chain {ChainId} for {Microservice} — file not found: {Path}",
                    missing.Id, missing.MicroserviceName, Path.Combine(backupRoot, missing.FullBackupFile));
            }

            var validChains = physicalChains.Concat(validLegacyChains).ToList();

            foreach (var chain in physicalChains)
            {
                result.Add(new BackupFileInfo
                {
                    BackupId = chain.Id.ToString(),
                    FileName = $"pgbackrest:{chain.FullBackupFile}",
                    BackupType = BackupType.Full,
                    CreatedAt = chain.Timestamp,
                    FileSizeBytes = chain.FullBackupSizeBytes,
                    ChainId = chain.ChainId ?? chain.Id.ToString(),
                    IsLatest = IsLatestChain(chain, validChains),
                    ServiceName = chain.MicroserviceName,
                    IsPhysical = true,
                });

                for (var i = 0; i < chain.Incrementals.Count; i++)
                {
                    result.Add(new BackupFileInfo
                    {
                        BackupId = chain.Incrementals[i],
                        FileName = $"pgbackrest:{chain.Incrementals[i]}",
                        BackupType = BackupType.Incremental,
                        // Falls back to the full backup's timestamp only for chains recorded
                        // before this field existed — must be the incremental's own timestamp
                        // for "restore latest" (which orders by CreatedAt) to resolve correctly.
                        CreatedAt = i < chain.IncrementalTimestamps.Count ? chain.IncrementalTimestamps[i] : chain.Timestamp,
                        FileSizeBytes = i < chain.IncrementalSizesBytes.Count ? chain.IncrementalSizesBytes[i] : 0,
                        ChainId = chain.ChainId ?? chain.Id.ToString(),
                        IsLatest = IsLatestChain(chain, validChains),
                        ServiceName = chain.MicroserviceName,
                        IsPhysical = true,
                    });
                }
            }

            foreach (var chain in validLegacyChains)
            {
                var fullBackupPath = Path.Combine(backupRoot, chain.FullBackupFile);
                result.Add(new BackupFileInfo
                {
                    BackupId = chain.Id.ToString(),
                    FileName = fullBackupPath, // Store full path instead of just filename
                    BackupType = BackupType.Full,
                    CreatedAt = chain.Timestamp,
                    ChainId = chain.ChainId ?? chain.Id.ToString(),
                    IsLatest = IsLatestChain(chain, validChains),
                    ServiceName = chain.MicroserviceName
                });

                foreach (var inc in chain.Incrementals)
                {
                    var fullIncPath = Path.Combine(backupRoot, inc);
                    if (!_fileSystem.File.Exists(fullIncPath))
                    {
                        _logger.LogWarning(
                            "Skipping incremental backup {File} for chain {ChainId} — file not found: {Path}",
                            inc, chain.Id, fullIncPath);
                        continue;
                    }
                    result.Add(new BackupFileInfo
                    {
                        BackupId = Path.GetFileNameWithoutExtension(inc),
                        FileName = fullIncPath,
                        CreatedAt = chain.Timestamp, // Use chain timestamp as approximation
                        ChainId = chain.ChainId ?? chain.Id.ToString(),
                        IsLatest = IsLatestChain(chain, validChains),
                        ServiceName = chain.MicroserviceName
                    });
                }
            }

            return result;
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

        private bool IsLatestChain(BackupChain chain, List<BackupChain> candidates)
        {
            return candidates
                .Where(c => c.MicroserviceName == chain.MicroserviceName)
                .OrderByDescending(c => c.Timestamp)
                .FirstOrDefault()?.Id == chain.Id;
        }
    }
}