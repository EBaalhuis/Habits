namespace Habits.Util;

internal static class Extensions
{
    public static string ToShortVersionString(this string versionString)
    {
        var split = versionString.Split('.');
        if (split.Length >= 2)
        {
            return $"{split[0]}.{split[1]}";
        }
        return versionString;
    }
}
