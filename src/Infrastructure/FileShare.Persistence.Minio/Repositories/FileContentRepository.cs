using System.Net.Mime;
using System.Reactive.Linq;

using FileShare.Application.Repositories;
using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;

namespace FileShare.Persistence.Minio.Repositories;

public class FileContentRepository : IFileContentRepository
{
    private readonly BucketContext _context;
    private readonly IFileMetadataRepository _metadataRepository;
    public FileContentRepository(BucketContext context, IFileMetadataRepository metadataRepository)
    {
        _context = context;
        _metadataRepository = metadataRepository;
    }

    public async Task<Identity?> UploadAsync(FileContent content)
    {
        var metadata = content.Metadata;
        var putObjectResponse = await _context.UploadObjectAsync(
            metadata.Name,
            content.Stream,
            content.Type.ToString(),
            metadata.Size.SizeInBytes
        ).ConfigureAwait(false);

        if (putObjectResponse is null) return null;

        var etag = putObjectResponse.Etag!.Trim('"');

        await RemoveDuplicatesAsync(metadata.Name, etag)
            .ConfigureAwait(false);

        return new Identity(etag);
    }
    public async Task<FileContent?> DownloadAsync(Identity identity)
    {
        var metadataResponse = await _metadataRepository.GetAsync(identity).ConfigureAwait(false);
        if (metadataResponse.HasValue == false) return null;

        var metadata = metadataResponse.Value;
        var name = metadata.Name;
        
        var size = (int)metadata.Size.SizeInBytes;
        var buffer = new MemoryStream(size);

        var objectStat = await _context.GetObjectContentAsync(name, buffer).ConfigureAwait(false);
        if (objectStat is null) return null;

        var contentType = new ContentType(objectStat.ContentType);

        return new FileContent(metadata, contentType, buffer);
    }

    private async Task RemoveDuplicatesAsync(string objectName, string etag)
    {
        var duplicatesRequest = _context.ListObjects()
            .Where(item => item.ETag == etag && item.Key != objectName);

        var hasDuplicates = await duplicatesRequest.Any();
        if (hasDuplicates == false) return;

        var duplicates = duplicatesRequest.Select(item => item.Key);
        await _context.RemoveObjects(duplicates);
    }
}