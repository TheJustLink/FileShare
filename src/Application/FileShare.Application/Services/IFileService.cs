﻿using FileShare.Application.Common;
using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;

namespace FileShare.Application.Services;

public interface IFileService
{
    Task<Response<Identity>> UploadAsync(FileContent content);
    Task<Response<FileMetadata>> GetMetadataAsync(Identity identity);
    Task<Response<FileContent>> DownloadAsync(Identity identity);
}