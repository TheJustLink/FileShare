using System;

namespace FileShare.Extensions;

public static class NumberExtensions
{
    private const double BytesToMB = 1024 * 1024;

    public static double GetSizeInMB<T>(this T number) where T : IConvertible =>
        double.Round(number.ToDouble(null) / BytesToMB, 1);
}