namespace FileShare.Application.Common;

public enum ResponseStatus : ushort
{
    BadRequest = 400,
    NotFound = 404,

    Ok = 200,
    Created = 201,
}

public static class ResponseStatusExtensions
{
    public static bool IsInformational(this ResponseStatus status) => (int) status is > 99 and < 200;
    public static bool IsSuccessful(this ResponseStatus status) => (int) status is > 199 and < 300;
    public static bool IsFailure(this ResponseStatus status) => !status.IsSuccessful();
    public static bool IsRedirection(this ResponseStatus status) => (int) status is > 299 and < 400;
    public static bool IsClientError(this ResponseStatus status) => (int) status is > 399 and < 500;
    public static bool IsServerError(this ResponseStatus status) => (int) status is > 499 and < 600;
}