namespace FileShare.Web.Models;

public record FileViewModel(string Id, string Name, double SizeInMB, TimeSpan ElapsedTime);