namespace FileShare.Domain.ValueObjects;

public readonly record struct Size(long SizeInBytes)
{
    private const double BytesToMB = 1d / (1024d * 1024d);

    public double SizeInMB => SizeInBytes * BytesToMB;
}