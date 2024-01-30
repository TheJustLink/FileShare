using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;

namespace FileShare.Application.Repositories;

public interface IFileMetadataRepository
{
    Task<bool> ExistsAsync(Identity identity);
    Task<FileMetadata> GetAsync(Identity identity);
}