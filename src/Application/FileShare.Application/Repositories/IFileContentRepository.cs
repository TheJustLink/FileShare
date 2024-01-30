using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;

namespace FileShare.Application.Repositories;

public interface IFileContentRepository
{
    Task<Identity> UploadAsync(FileContent content);
    Task<FileContent> DownloadAsync(Identity identity);
}