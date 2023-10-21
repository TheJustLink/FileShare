using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;

using FileShare.Application.Repositories;

namespace FileShare.Persistence.Minio.Repositories;

public class FileMetadataRepository : IFileMetadataRepository
{
    private readonly BucketContext _context;

    public FileMetadataRepository(BucketContext context) => _context = context;

    public async Task<FileMetadata?> GetAsync(Identity identity)
    {
        var etag = identity.Key;

        var item = await _context.FindObjectAsync(etag);
        if (item is null) return null;

        var name = item.Key;
        var size = new Size((long)item.Size);
        var modificationTime = new ModificationTime(item.LastModifiedDateTime!.Value);

        return new FileMetadata(
            name,
            size,
            modificationTime
        );
    }
}