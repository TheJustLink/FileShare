using System;

namespace FileShare.Models
{
    public record FileViewModel(string Id, string Name, double SizeInMB, DateTime LastModifiedTime);
}