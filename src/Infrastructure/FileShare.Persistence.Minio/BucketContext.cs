using System.Reactive.Linq;

using FileShare.Persistence.Minio.Configuration;

using Minio;
using Minio.DataModel;

namespace FileShare.Persistence.Minio;

public class BucketContext
{
    private readonly IMinioClient _client;
    private readonly string _bucketName;

    public BucketContext(MinioClient client, Settings settings)
    {
        _client = client;
        _bucketName = settings.FilesBucket;
    }

    public async Task EnsureBucketExistsAsync()
    {
        var isExists = await IsBucketExistsAsync(_bucketName).ConfigureAwait(false);
        if (isExists) return;

        await MakeBucketAsync(_bucketName).ConfigureAwait(false);
    }

    public async Task<Item?> FindObjectAsync(string etag)
    {
        var items = ListObjects();

        return await items
            .Where(i => i.ETag == etag)
            .SingleOrDefaultAsync();
    }
    public Task<ObjectStat?> GetObjectContentAsync(string objectName, Stream contentBuffer)
    {
        var args = new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(contentBuffer);
                contentBuffer.Position = 0;
            });

        return _client.GetObjectAsync(args);
    }
    public Task<PutObjectResponse?> UploadObjectAsync(string objectName, Stream content, string contentType, long size)
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