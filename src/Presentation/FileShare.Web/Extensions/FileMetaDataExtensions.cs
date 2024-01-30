using FileShare.Domain.Aggregates;
using FileShare.Web.Models;

namespace FileShare.Web.Extensions;

public static class FileMetaDataExtensions
{
    public static FileViewModel MapToViewModel(this FileMetadata metadata, string id) => new
    (
        id,
        metadata.Name,
        metadata.Size.SizeInMB,
        metadata.ModificationTime.ElapsedTime
    );
}