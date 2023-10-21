using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;

namespace FileShare.Application.Repositories;

public interface IFileMetadataRepository
{
    Task<FileMetadata?> GetAsync(Identity identity);
}