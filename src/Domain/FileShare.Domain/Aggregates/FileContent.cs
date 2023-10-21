using System.Net.Mime;

namespace FileShare.Domain.Aggregates;

public readonly record struct FileContent
(
    FileMetadata Metadata,
    ContentType Type,
    Stream Stream
);