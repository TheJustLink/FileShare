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

    public async Task<Identity> UploadAsync(FileContent content)
    {
        var metadata = content.Metadata;
        var putObjectResponse = await _context.UploadObjectAsync(
            metadata.Name,
            content.Stream,
            content.Type.ToString(),
            metadata.Size.SizeInBytes
        );

        var etag = putObjectResponse.Etag!.Trim('"');

        // Minio doesn't remove duplicates when uploading files with the same etag,
        // so we need to remove them manually, technically it's the same file, we can't check it in advance

        await RemoveDuplicatesAsync(metadata.Name, etag);

        return new Identity(etag);
    }
    public async Task<FileContent> DownloadAsync(Identity identity)
    {
        var metadata = await _metadataRepository.GetAsync(identity);
        var name = metadata.Name;

        var size = (int)metadata.Size.SizeInBytes;
        var buffer = new MemoryStream(size);

        var objectStat = await _context.GetObjectContentAsync(name, buffer);
        var contentType = new ContentType(objectStat.ContentType);

        return new FileContent(metadata, contentType, buffer);
    }

    private async ValueTask RemoveDuplicatesAsync(string objectName, string etag)
    {
        var duplicatesRequest = _context.ListObjects()
            .Where(item => item.ETag == etag && item.Key != objectName)
            .Select(static item => item.Key);

        var hasDuplicates = await duplicatesRequest.Any();
        if (hasDuplicates == false) return;

        await _context.RemoveObjects(duplicatesRequest);
    }
}