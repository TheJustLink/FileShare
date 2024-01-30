using System.Reactive.Linq;

using FileShare.Persistence.Minio.Configuration;

using Microsoft.Extensions.Options;

using Minio;
using Minio.DataModel;

namespace FileShare.Persistence.Minio;

public sealed class BucketContext
{
    private readonly IMinioClient _client;
    private readonly string _bucketName;

    public BucketContext(IMinioClient client, IOptions<Settings> settings)
    {
        _client = client;
        _bucketName = settings.Value.FilesBucket;
    }

    public async Task EnsureBucketExistsAsync()
    {
        var isExists = await IsBucketExistsAsync(_bucketName);
        if (isExists) return;

        await MakeBucketAsync(_bucketName);
    }

    public Task<ObjectStat> GetObjectContentAsync(string objectName, Stream contentBuffer)
    {
        var args = new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithCallbackStream(async (stream, cancellationToken) =>
            {
                await stream.CopyToAsync(contentBuffer, cancellationToken);
                contentBuffer.Position = 0;
            });

        return _client.GetObjectAsync(args);
    }
    public Task<PutObjectResponse> UploadObjectAsync(string objectName, Stream content, string contentType, long size)
    {
        var putObject = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(content)
            .WithContentType(contentType)
            .WithObjectSize(size);

        return _client.PutObjectAsync(putObject);
    }

    public IObservable<Item> ListObjects()
    {
        var args = new ListObjectsArgs()
            .WithBucket(_bucketName);

        return _client.ListObjectsAsync(args);
    }
    public IObservable<Task> RemoveObjects(IObservable<string> objectNames)
    {
        return objectNames.Select(RemoveObjectAsync);
    }
    public Task RemoveObjectAsync(string objectName)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName);

        return _client.RemoveObjectAsync(args);
    }

    private Task<bool> IsBucketExistsAsync(string name)
    {
        var args = new BucketExistsArgs().WithBucket(name);

        return _client.BucketExistsAsync(args);
    }
    private Task MakeBucketAsync(string name)
    {
        var args = new MakeBucketArgs().WithBucket(name);

        return _client.MakeBucketAsync(args);
    }
}