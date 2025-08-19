using System.Text.Json;
using Messaging.Contracts.Messaging;
using Messaging.Contracts.Messaging.contracts;
using Messaging.Contracts.Messaging.contracts.Enums;
using Messaging.Contracts.Messaging.contracts.Events.MaintenanceEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UserService.Infrastructure.Backup;

 public class BackupMetadataService : IBackupMetadataService
    {
        private readonly ILogger<BackupMetadataService> _logger;
        private readonly IOptions<BackupOptions> _options;
        private readonly string _hostBackupPath;
        private readonly string _metadataFilePath;
        private readonly SemaphoreSlim _metadataLock = new(1, 1);

        public BackupMetadataService(
            IConfiguration config,
            ILogger<BackupMetadataService> logger,
            IOptions<BackupOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            
            _hostBackupPath = _options.Value.Path ?? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "UserService", "Backups");
            
            _metadataFilePath = Path.Combine(_hostBackupPath, "backups_metadata.json");
            
            InitializeMetadata();
        }

        public async Task<BackupMetadata> LoadMetadataAsync()
        {
            await _metadataLock.WaitAsync();
            
            try
            {
                if (!File.Exists(_metadataFilePath))
                {
                    _logger.LogWarning("Metadata file not found, creating new metadata");
                    return new BackupMetadata { Chains = new List<BackupChain>() };
                }

                var json = await File.ReadAllTextAsync(_metadataFilePath);
                var metadata = JsonSerializer.Deserialize<BackupMetadata>(json) ?? new BackupMetadata();
                
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load metadata, creating new metadata");
                return new BackupMetadata { Chains = new List<BackupChain>() };
            }
            finally
            {
                _metadataLock.Release();
            }
        }

        public async Task SaveMetadataAsync(BackupMetadata metadata)
        {
            await _metadataLock.WaitAsync();
            
            try
            {
                var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_metadataFilePath, json);
                
                _logger.LogDebug("Metadata saved successfully to {MetadataPath}", _metadataFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save metadata to {MetadataPath}", _metadataFilePath);
                throw new InvalidOperationException("Failed to save backup metadata", ex);
            }
            finally
            {
                _metadataLock.Release();
            }
        }

        public async Task UpdateMetadataWithFullBackupAsync(string backupFileName, CancellationToken cancellationToken = default)
        {
            var metadata = await LoadMetadataAsync();
            
            metadata.Chains.Add(new BackupChain 
            { 
                FullBackupFile = backupFileName, 
                Timestamp = DateTime.UtcNow, 
                Incrementals = new List<string>() 
            });
            
            await SaveMetadataAsync(metadata);
            
            _logger.LogInformation("Updated metadata with new full backup: {BackupFileName}", backupFileName);
        }

        public async Task UpdateMetadataWithIncrementalBackupAsync(string backupFileName, CancellationToken cancellationToken = default)
        {
            var metadata = await LoadMetadataAsync();
            var latestChain = metadata.Chains.OrderByDescending(c => c.Timestamp).FirstOrDefault();
            
            if (latestChain != null)
            {
                latestChain.Incrementals.Add(backupFileName);
                await SaveMetadataAsync(metadata);
                
                _logger.LogInformation("Updated metadata with new incremental backup: {BackupFileName} for chain: {ChainId}", 
                    backupFileName, GetChainId(latestChain));
            }
            else
            {
                _logger.LogWarning("No backup chain found for incremental backup: {BackupFileName}", backupFileName);
            }
        }

        public async Task<BackupChain?> GetLatestFullBackupChainAsync()
        {
            var metadata = await LoadMetadataAsync();
            return metadata.Chains.OrderByDescending(c => c.Timestamp).FirstOrDefault();
        }

        public async Task<List<BackupChainInfo>> GetAllBackupChainsAsync()
        {
            var metadata = await LoadMetadataAsync();
            var latestChain = metadata.Chains.OrderByDescending(c => c.Timestamp).FirstOrDefault();
            
            return metadata.Chains
                .OrderByDescending(c => c.Timestamp)
                .Select(chain => 
                {
                    var info = MapToBackupChainInfo(chain);
                    info.IsLatest = chain == latestChain;
                    return info;
                })
                .ToList();
        }

        public async Task<BackupChainInfo?> GetBackupChainInfoAsync(string identifier)
        {
            var chain = await GetBackupChainAsync(identifier);
            if (chain == null) return null;
            
            var info = MapToBackupChainInfo(chain);
            var latestChain = await GetLatestFullBackupChainAsync();
            info.IsLatest = chain == latestChain;
            
            return info;
        }

        public async Task<List<BackupFileInfo>> GetAvailableBackupsAsync()
        {
            var metadata = await LoadMetadataAsync();
            var backups = new List<BackupFileInfo>();
            var latestChain = metadata.Chains.OrderByDescending(c => c.Timestamp).FirstOrDefault();

            foreach (var chain in metadata.Chains.OrderByDescending(c => c.Timestamp))
            {
                var chainId = GetChainId(chain);
                
                // Add full backup
                var fullBackupPath = Path.Combine(_hostBackupPath, chain.FullBackupFile);
                if (File.Exists(fullBackupPath))
                {
                    var fullFileInfo = new FileInfo(fullBackupPath);
                    backups.Add(new BackupFileInfo
                    {
                        BackupId = Path.GetFileNameWithoutExtension(chain.FullBackupFile),
                        FileName = chain.FullBackupFile,
                        BackupType = BackupType.Full.ToString(),
                        CreatedAt = chain.Timestamp,
                        FileSizeBytes = fullFileInfo.Length,
                        ChainId = chainId,
                        IsLatest = chain == latestChain,
                        ServiceName = "UserService"
                    });
                }

                // Add incremental backups
                foreach (var incFile in chain.Incrementals)
                {
                    var incPath = Path.Combine(_hostBackupPath, incFile);
                    if (File.Exists(incPath))
                    {
                        var incFileInfo = new FileInfo(incPath);
                        backups.Add(new BackupFileInfo
                        {
                            BackupId = Path.GetFileNameWithoutExtension(incFile),
                            FileName = incFile,
                            BackupType = BackupType.Incremental.ToString(),
                            CreatedAt = incFileInfo.CreationTimeUtc,
                            FileSizeBytes = incFileInfo.Length,
                            ChainId = chainId,
                            IsLatest = false,
                            ServiceName = "UserService"
                        });
                    }
                }
            }

            return backups;
        }

        public async Task<BackupChain?> GetBackupChainAsync(string backupId)
        {
            var metadata = await LoadMetadataAsync();
            
            // If requesting latest, return most recent
            if (backupId == "latest")
                return metadata.Chains.OrderByDescending(c => c.Timestamp).FirstOrDefault();
                
            // Find chain by full backup filename or backup ID
            var byFullBackup = metadata.Chains.FirstOrDefault(c => 
                c.FullBackupFile == backupId || 
                Path.GetFileNameWithoutExtension(c.FullBackupFile) == backupId);
            if (byFullBackup != null) return byFullBackup;
            
            // Find chain containing incremental backup
            return metadata.Chains.FirstOrDefault(c => 
                c.Incrementals.Contains(backupId) || 
                c.Incrementals.Any(inc => Path.GetFileNameWithoutExtension(inc) == backupId));
        }

        public async Task<BackupChain?> FindChainForIncrementalAsync(string incrementalFile)
        {
            var metadata = await LoadMetadataAsync();
            return metadata.Chains.FirstOrDefault(c => 
                c.Incrementals.Contains(incrementalFile) ||
                c.Incrementals.Any(inc => Path.GetFileNameWithoutExtension(inc) == incrementalFile));
        }

        public async Task RemoveBackupChainAsync(string chainId)
        {
            var metadata = await LoadMetadataAsync();
            var chainToRemove = metadata.Chains.FirstOrDefault(c => GetChainId(c) == chainId);
            
            if (chainToRemove != null)
            {
                metadata.Chains.Remove(chainToRemove);
                await SaveMetadataAsync(metadata);
                
                _logger.LogInformation("Removed backup chain from metadata: {ChainId}", chainId);
            }
        }

        public async Task ValidateMetadataIntegrityAsync()
        {
            var metadata = await LoadMetadataAsync();
            var hasChanges = false;
            var chainsToRemove = new List<BackupChain>();

            foreach (var chain in metadata.Chains)
            {
                // Check if full backup file exists
                var fullBackupPath = Path.Combine(_hostBackupPath, chain.FullBackupFile);
                if (!File.Exists(fullBackupPath))
                {
                    _logger.LogWarning("Full backup file missing for chain {ChainId}: {FullBackupFile}", 
                        GetChainId(chain), chain.FullBackupFile);
                    chainsToRemove.Add(chain);
                    continue;
                }

                // Check incremental backup files and remove missing ones
                var missingIncrementals = new List<string>();
                foreach (var incFile in chain.Incrementals)
                {
                    var incPath = Path.Combine(_hostBackupPath, incFile);
                    if (!File.Exists(incPath))
                    {
                        _logger.LogWarning("Incremental backup file missing: {IncrementalFile}", incFile);
                        missingIncrementals.Add(incFile);
                        hasChanges = true;
                    }
                }

                // Remove missing incremental files from chain
                foreach (var missing in missingIncrementals)
                {
                    chain.Incrementals.Remove(missing);
                }
            }

            // Remove chains with missing full backups
            foreach (var chainToRemove in chainsToRemove)
            {
                metadata.Chains.Remove(chainToRemove);
                hasChanges = true;
            }

            if (hasChanges)
            {
                await SaveMetadataAsync(metadata);
                _logger.LogInformation("Metadata integrity validation completed with {RemovedChains} chains removed and missing incrementals cleaned up", 
                    chainsToRemove.Count);
            }
            else
            {
                _logger.LogDebug("Metadata integrity validation completed - no issues found");
            }
        }

        public string GetChainId(BackupChain chain)
        {
            // Create a consistent chain ID based on the full backup filename
            return Path.GetFileNameWithoutExtension(chain.FullBackupFile);
        }

        public BackupChainInfo MapToBackupChainInfo(BackupChain chain)
        {
            var fullBackupPath = Path.Combine(_hostBackupPath, chain.FullBackupFile);
            var totalSize = 0L;
            var isValid = true;

            if (File.Exists(fullBackupPath))
            {
                totalSize += new FileInfo(fullBackupPath).Length;
            }
            else
            {
                isValid = false;
            }

            foreach (var incFile in chain.Incrementals)
            {
                var incPath = Path.Combine(_hostBackupPath, incFile);
                if (File.Exists(incPath))
                {
                    totalSize += new FileInfo(incPath).Length;
                }
                else
                {
                    isValid = false;
                }
            }

            return new BackupChainInfo
            {
                ChainId = GetChainId(chain),
                FullBackupFile = chain.FullBackupFile,
                FullBackupTimestamp = chain.Timestamp,
                IncrementalFiles = chain.Incrementals.ToList(),
                TotalFiles = 1 + chain.Incrementals.Count,
                TotalSizeBytes = totalSize,
                IsValid = isValid,
                IsLatest = false, // Will be set by caller if needed
                ServiceName = "UserService"
            };
        }

        private void InitializeMetadata()
        {
            try
            {
                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(_metadataFilePath);
                if (!Directory.Exists(directory))
                {
                    _logger.LogInformation("Creating backup directory: {Directory}", directory);
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(_metadataFilePath))
                {
                    var metadata = new BackupMetadata { Chains = new List<BackupChain>() };
                    var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_metadataFilePath, json);
            
                    _logger.LogInformation("Initialized backup metadata file: {MetadataPath}", _metadataFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize metadata file: {MetadataPath}", _metadataFilePath);
                throw;
            }
        }

        public void Dispose()
        {
            _metadataLock?.Dispose();
        }
    }

   