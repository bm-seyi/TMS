namespace TMS.Application.Extensions
{
    public static class StringExtension
    {
        extension(string text)
        {
           public string Sanitize() => text.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }
    }
}