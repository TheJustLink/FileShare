namespace FileShare.Application.Common;

public readonly record struct Response<T>(ResponseStatus Status, string? Message = default, T? Value = default)
{
    public bool IsInformational => Status.IsInformational();
    public bool IsSuccessful => Status.IsSuccessful();
    public bool IsFailure => Status.IsFailure();
    public bool IsRedirection => Status.IsRedirection();
    public bool IsClientError => Status.IsClientError();
    public bool IsServerError => Status.IsServerError();

    public static Response<T> BadRequest(string? message = default) => new(ResponseStatus.BadRequest, message);
    public static Response<T> NotFound(string? message = default) => new(ResponseStatus.NotFound, message);

    public static Response<T> Ok(T? value = default) => new(ResponseStatus.Ok, Value: value);
    public static Response<T> Created(T? value = default) => new(ResponseStatus.Created, Value: value);
}