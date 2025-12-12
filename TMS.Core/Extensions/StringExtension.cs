namespace TMS.Core.Extensions
{
    public static class StringExtension
    {
        extension(string text)
        {
           public string Sanitize() => text.Replace("\r", "").Replace("\n", "");
        }
    }
}