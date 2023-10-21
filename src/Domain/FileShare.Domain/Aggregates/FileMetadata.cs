using FileShare.Domain.ValueObjects;

namespace FileShare.Domain.Aggregates;

public readonly record struct FileMetadata
(
    string Name,
    Size Size,
    ModificationTime ModificationTime
)
{
    public FileMetadata(string name, Size size)
        : this(name, size, ModificationTime.Now) { }
}