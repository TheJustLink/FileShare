using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

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

    public HomeController(ILogger<HomeController> logger, MinioClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet]
    [Route("/")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file == null)
            return BadRequest("No file to upload");

        await using var fileStream = file.OpenReadStream();

        await EnsureBucketExists(FilesBucketName);
        var uploadedFile = await UploadObject(FilesBucketName, file.FileName, fileStream, file.ContentType, file.Length);

        _logger.LogInformation($"File {uploadedFile.Id}:{uploadedFile.Name}[{uploadedFile.Size}] uploaded");

        return Ok(uploadedFile);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Download(string? id)
    {
        if (id is null)
            return NotFound();

        await EnsureBucketExists(FilesBucketName);

        var item = await FindObjectBy(id);
        if (item is null)
            return NotFound();

        _logger.LogInformation($"Download request {item.ETag}:{item.Key}[{item.Size}]");

        return await GetObjectContent(item.Key, (int)item.Size)
            .ConfigureAwait(false);
    }

    private async Task<FileStreamResult> GetObjectContent(string objectName, int size)
    {
        var contentStream = new MemoryStream(size);
        var args = new GetObjectArgs()
            .WithBucket(FilesBucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(contentStream);
                contentStream.Position = 0;
            });

        var response = await _client.GetObjectAsync(args);

        return File(contentStream, response.ContentType, objectName);
    }
    private async Task<Item?> FindObjectBy(string id)
    {
        var args = new ListObjectsArgs()
            .WithBucket(FilesBucketName);
        var items = _client.ListObjectsAsync(args);

        return await items
            .Where(i => i.ETag == id)
            .SingleOrDefaultAsync();
    }

    private async Task<UploadedFile> UploadObject(string bucketName, string objectName, Stream stream, string contentType, long size)
    {
        var putObject = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithContentType(contentType)
            .WithObjectSize(size);

        var response = await _client.PutObjectAsync(putObject);

        return new UploadedFile(
            response.Etag,
            response.ObjectName,
            response.Size
        );
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