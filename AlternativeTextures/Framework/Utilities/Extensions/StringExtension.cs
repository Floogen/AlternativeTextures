using System;

namespace AlternativeTextures.Framework.Utilities.Extensions
{
    public static class StringExtension
    {
        public static string ReplaceLastInstance(this string source, string target, string replacement)
        {
            int index = source.LastIndexOf(target, StringComparison.OrdinalIgnoreCase);

            if (index == -1)
            {
                return source;
            }

            return source.Remove(index, target.Length).Insert(index, replacement);
        }
    }
}
