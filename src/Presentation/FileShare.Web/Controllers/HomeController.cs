using FileShare.Application.Services;
using FileShare.Domain.Aggregates;
using FileShare.Domain.ValueObjects;
using FileShare.Web.Extensions;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace FileShare.Web.Controllers;

[ApiController]
[Route("[action]")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IFileService _fileService;

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

        await using var fileStream = file.OpenReadStream();
        var fileContent = file.MapToFileContent(fileStream);

        var response = await _fileService.UploadAsync(fileContent);
        if (response.IsFailure)
            return RedirectToIndexWithMessage(response.Message);

        var fileId = response.Value.Key;

        _logger.LogInformation("File {Id}:{Name}[{Size}] uploaded", fileId, file.FileName, fileContent.Metadata.Size.SizeInMB);

        return RedirectToAction(nameof(File), new { id = fileId });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> File([FromRoute] string? id)
    {
        if (id is null) return NotFound();

        var response = await _fileService.GetMetadataAsync(new Identity(id));
        if (response.IsFailure) return NotFound();

        var fileMetadata = response.Value;
        var viewModel = fileMetadata.MapToViewModel(id);

        return View(viewModel);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Download([FromRoute] string? id)
    {
        if (id is null) return NotFound();

        var response = await _fileService.DownloadAsync(new Identity(id));
        if (response.IsFailure) return NotFound();

        var fileContent = response.Value;

        return File(fileContent);
    }

    private FileStreamResult File(FileContent content) => File
    (
        content.Stream,
        content.Type.ToString(),
        content.Metadata.Name,
        true
    );

    private IActionResult RedirectToIndexWithMessage(string? message) =>
        string.IsNullOrWhiteSpace(message)
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Index), new { msg = message });
}