namespace DailyRugby.Application.Utilitaries;

public static class IsInHelper
{
    public static bool IsIn<T>(this T item, params T[] array)
        => array.Contains(item);
}