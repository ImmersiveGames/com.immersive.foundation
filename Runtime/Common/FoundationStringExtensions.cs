using System;

namespace Immersive.Foundation.Common
{
    /// <summary>
    /// Small runtime-only string normalization primitives shared by packages.
    /// </summary>
    public static class FoundationStringExtensions
    {
        public static string NormalizeText(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public static string NormalizeTextOrFallback(this string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback.NormalizeText() : value.Trim();
        }
    }
}
