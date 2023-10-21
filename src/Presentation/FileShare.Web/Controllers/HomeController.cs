using System.Net.Mime;

using FileShare.Application.Services;
using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;
using FileShare.Web.Models;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace FileShare.Web.Controllers;

[ApiController]
[Route("[action]")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IFileService _fileService;

    private const double MaxFileSizeInMB = 25;
    private const long MaxRequestSizeInBytes = FormOptions.DefaultMultipartBodyLengthLimit;

    public HomeController(ILogger<HomeController> logger, IFileService fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    [HttpGet]
    [Route("/")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Index([FromQuery] string? msg) => View(msg as object);

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [RequestSizeLimit(MaxRequestSizeInBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestSizeInBytes)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file)
    {
        if (file is null)
            return RedirectToIndexWithMessage("No file selected");

        var size = new Size(file.Length);

        var isOutOfSizeLimit = size.SizeInMB > MaxFileSizeInMB;
        if (isOutOfSizeLimit)
            return RedirectToIndexWithMessage("File is too big, size limit - 25 MB!");

        var metadata = new FileMetadata(file.FileName, new Size(file.Length));
        var contentType = new ContentType(file.ContentType);

        await using var fileStream = file.OpenReadStream();
        var content = new FileContent(metadata, contentType, fileStream);

        var response = await _fileService.UploadAsync(content);
        if (response.IsFailure)
            return RedirectToIndexWithMessage(response.Message);

        var id = response.Value.Key;

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

        var response = await _fileService.GetMetadataAsync(new Identity(id));
        if (response.IsFailure)
            return NotFound();

        var metadata = response.Value;
        var viewModel = new FileViewModel(
            id,
            metadata.Name,
            metadata.Size.SizeInMB,
            metadata.ModificationTime.ElapsedTime
        );

        return View(viewModel);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Download([FromRoute] string? id)
    {
        if (id is null) return NotFound();

        var response = await _fileService.DownloadAsync(new Identity(id));
        if (response.IsFailure)
            return NotFound();

        var content = response.Value;

        return File(
            content.Stream,
            content.Type.ToString(),
            content.Metadata.Name,
            false
        );
    }

    private IActionResult RedirectToIndexWithMessage(string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Index), new { msg = message });
    }
}