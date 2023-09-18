using System.Reactive.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Minio;

namespace FileShare.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MinioClient _client;

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
}