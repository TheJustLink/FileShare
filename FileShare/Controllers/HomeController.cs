using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

using FileShare.Extensions;
using FileShare.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Minio;
using Minio.DataModel;

namespace FileShare.Controllers;

[ApiController]
[Route("[action]")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MinioClient _client;

    private const string FilesBucketName = "files";
    private const double MaxFileSizeInMB = 25;

    public HomeController(ILogger<HomeController> logger, MinioClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet]
    [Route("/")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index([FromQuery] string? msg) => View(msg as object);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file)
    {
        if (file is null)
            return RedirectToIndexWithMessage("No file selected");

        var isOutOfSizeLimit = file.Length.GetSizeInMB() > MaxFileSizeInMB;
        if (isOutOfSizeLimit)
            return RedirectToIndexWithMessage("File is too big, size limit - 25 MB!");

        await using var fileStream = file.OpenReadStream();

        await EnsureBucketExists(FilesBucketName);
        var id = await UploadObject(FilesBucketName, file.FileName, fileStream, file.ContentType, file.Length);

        _logger.LogInformation($"File {id}:{file.FileName}[{file.Length}] uploaded");

        return RedirectToAction(nameof(File), new { id });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> File([FromRoute] string? id)
    {
        if (id is null)
            return NotFound();

        await EnsureBucketExists(FilesBucketName);

        var item = await FindObjectBy(id, FilesBucketName);
        if (item is null)
            return NotFound();

        var itemSizeInMB = item.Size.GetSizeInMB();
        var viewModel = new FileViewModel(item.ETag, item.Key, itemSizeInMB, item.LastModifiedDateTime!.Value);

        return View(viewModel);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Download([FromRoute] string? id)
    {
        if (id is null)
            return NotFound();

        await EnsureBucketExists(FilesBucketName);

        var item = await FindObjectBy(id, FilesBucketName);
        if (item is null)
            return NotFound();

        _logger.LogInformation($"Download request {item.ETag}:{item.Key}[{item.Size}]");

        return await GetObjectContent(item.Key, (int)item.Size, FilesBucketName)
            .ConfigureAwait(false);
    }

    private IActionResult RedirectToIndexWithMessage(string message) =>
        RedirectToAction(nameof(Index), new { msg = message });

    private async Task<FileStreamResult> GetObjectContent(string objectName, int size, string bucketName)
    {
        var contentStream = new MemoryStream(size);
        var args = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(contentStream);
                contentStream.Position = 0;
            });

        var response = await _client.GetObjectAsync(args);

        return File(contentStream, response.ContentType, objectName);
    }
    private async Task<Item?> FindObjectBy(string id, string bucketName)
    {
        var items = ListObjects(bucketName);

        return await items
            .Where(i => i.ETag == id)
            .SingleOrDefaultAsync();
    }

    private async Task<string> UploadObject(string bucketName, string objectName, Stream stream, string contentType, long size)
    {
        var putObject = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithContentType(contentType)
            .WithObjectSize(size);

        var response = await _client.PutObjectAsync(putObject);
        var id = response.Etag.Trim('"');

        var duplicates = ListObjects(bucketName)
            .Where(item => item.ETag == id && item.Key != objectName);
        
        var hasDuplicates = await duplicates.Any();
        if (hasDuplicates)
            await RemoveObjects(bucketName, duplicates.Select(item => item.Key));

        return id;
    }

    private IObservable<Task> RemoveObjects(string bucketName, IObservable<string> objectNames)
    {
        return objectNames.Select(
            objectName => RemoveObject(bucketName, objectName)
        );
    }
    private Task RemoveObject(string bucketName, string objectName)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);

        return _client.RemoveObjectAsync(args);
    }
    private IObservable<Item> ListObjects(string bucketName)
    {
        var args = new ListObjectsArgs()
            .WithBucket(bucketName);

        return _client.ListObjectsAsync(args);
    }
    private async Task EnsureBucketExists(string name)
    {
        var isBucketExists = await IsBucketExists(name);
        if (isBucketExists) return;

        await MakeBucket(name)
            .ConfigureAwait(false);
    }

    private Task MakeBucket(string name)
    {
        var args = new MakeBucketArgs().WithBucket(name);

        return _client.MakeBucketAsync(args);
    }
    private Task<bool> IsBucketExists(string name)
    {
        var args = new BucketExistsArgs().WithBucket(name);

        return _client.BucketExistsAsync(args);
    }
}