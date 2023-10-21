using FileShare.Application.Common;
using FileShare.Application.Repositories;
using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;

namespace FileShare.Application.Services;

class FileService : IFileService
{
    private readonly IFileMetadataRepository _metadataRepository;
    private readonly IFileContentRepository _contentRepository;

    public FileService(IFileMetadataRepository metadataRepository, IFileContentRepository contentRepository)
    {
        _metadataRepository = metadataRepository;
        _contentRepository = contentRepository;
    }

    public async Task<Response<Identity>> UploadAsync(FileContent content)
    {
        var identity = await _contentRepository.UploadAsync(content).ConfigureAwait(false);
        
        return identity.HasValue
            ? Response<Identity>.Ok(identity.Value)
            : Response<Identity>.BadRequest("Failed to upload file");
    }
    public async Task<Response<FileMetadata>> GetMetadataAsync(Identity identity)
    {
        var metadata = await _metadataRepository.GetAsync(identity).ConfigureAwait(false);

        return metadata.HasValue
            ? Response<FileMetadata>.Ok(metadata.Value)
            : Response<FileMetadata>.NotFound("File not found");
    }
    public async Task<Response<FileContent>> DownloadAsync(Identity identity)
    {
        var content = await _contentRepository.DownloadAsync(identity).ConfigureAwait(false);

        return content.HasValue
            ? Response<FileContent>.Ok(content.Value)
            : Response<FileContent>.NotFound("File not found");
    }
}