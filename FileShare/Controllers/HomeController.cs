using System.Reactive.Linq;
using System.Threading.Tasks;

using FileShare.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Minio;

namespace FileShare.Controllers;

[ApiController]
[Route("[action]")]
[Route("[controller]/[action]")]
class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MinioClient _client;

    private const string BucketName = "files";

    public HomeController(ILogger<HomeController> logger, MinioClient client)
    {
        _logger = logger;
        _client = client;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("{bucket}")]
    public async Task<IActionResult> List(string bucket)
    {
        var bucketExists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
        if (bucketExists == false)
            return BadRequest($"Bucket {bucket} does not exist");

        var objects = _client.ListObjectsAsync(new ListObjectsArgs().WithBucket(bucket));
        var keys = await objects.Select(item => item.Key).ToList();

        return Json(keys);
    }
    [HttpGet("{bucket}")]
    public async Task<IActionResult> Add(string bucket)
    {
        var bucketExists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
        if (bucketExists)
            return BadRequest($"Bucket {bucket} already exists");

        await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket));

        return Ok($"Bucket {bucket} created");
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file == null)
            return BadRequest("No file to upload");

        var bucketExists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
        if (bucketExists == false)
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));

        await using var fileStream = file.OpenReadStream();
        var putObject = new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(file.Name)
            .WithContentType(file.ContentType)
            .WithStreamData(fileStream);

        var response = await _client.PutObjectAsync(putObject);
        var uploadedFile = new UploadedFile(
            response.Etag,
            response.ObjectName,
            response.Size
        );

        _logger.LogInformation($"File {uploadedFile.Name} [{uploadedFile.Size}] uploaded with etag {uploadedFile.Id}");

        return Ok(uploadedFile);
    }
}