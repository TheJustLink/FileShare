using FileShare.Application.Common;
using FileShare.Application.Configuration;
using FileShare.Application.Repositories;

using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;

using Microsoft.Extensions.Options;

namespace FileShare.Application.Services;

sealed class FileService : IFileService, IDisposable
{
    private readonly IFileMetadataRepository _metadataRepository;
    private readonly IFileContentRepository _contentRepository;

    private readonly IDisposable? _settingsMonitorSubscription;
    private Settings _settings;

    public FileService(IFileMetadataRepository metadataRepository, IFileContentRepository contentRepository, IOptionsMonitor<Settings> settingsMonitor)
    {
        _metadataRepository = metadataRepository;
        _contentRepository = contentRepository;

        _settingsMonitorSubscription = settingsMonitor.OnChange(settings => _settings = settings);
        _settings = settingsMonitor.CurrentValue;
    }

    public void Dispose()
    {
        _settingsMonitorSubscription?.Dispose();
    }

    public async Task<Response<FileMetadata>> GetMetadataAsync(Identity identity)
    {
        var hasMetadata = await _metadataRepository.ExistsAsync(identity);
        if (hasMetadata == false)
            return Response<FileMetadata>.NotFound("File not found");

        var metadata = await _metadataRepository.GetAsync(identity);

        return Response<FileMetadata>.Ok(metadata);
    }
    public async Task<Response<Identity>> UploadAsync(FileContent content)
    {
        if (content.Metadata.Size.SizeInMB > _settings.MaxFileSizeInMB)
            return Response<Identity>.BadRequest($"File is too big, size limit - {_settings.MaxFileSizeInMB} MB!");

        var identity = await _contentRepository.UploadAsync(content);

        return Response<Identity>.Ok(identity);
    }
    public async Task<Response<FileContent>> DownloadAsync(Identity identity)
    {
        var hasMetadata = await _metadataRepository.ExistsAsync(identity);
        if (hasMetadata == false)
            return Response<FileContent>.NotFound("File not found");

        var content = await _contentRepository.DownloadAsync(identity);

        return Response<FileContent>.Ok(content);
    }
}