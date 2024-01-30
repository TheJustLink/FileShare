using System.Net.Mime;

using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;

namespace FileShare.Web.Extensions;

static class FormFileExtensions
{
    public static FileContent MapToFileContent(this IFormFile file, Stream stream)
    {
        var size = new Size(file.Length);
        var metadata = new FileMetadata(file.FileName, size);
        var contentType = new ContentType(file.ContentType);

        return new FileContent(metadata, contentType, stream);
    }
}