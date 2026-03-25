namespace bikey.Common
{
    /// <summary>
    /// Utility class for common string operations used across the application.
    /// </summary>
    public static class StringHelpers
    {
        /// <summary>
        /// Normalizes an email address by trimming whitespace and converting to lowercase invariant.
        /// </summary>
        /// <param name="email">Email address to normalize</param>
        /// <returns>Normalized email address</returns>
        public static string NormalizeEmail(string email)
        {
            return email?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        /// <summary>
        /// Normalizes image URL/file name for web display.
        /// Ensures proper path formatting and http/https prefix handling.
        /// </summary>
        /// <param name="tenFile">Image file name or URL</param>
        /// <returns>Normalized image URL with leading slash, or null if input is null/empty</returns>
        public static string? NormalizeImageUrl(string? tenFile)
        {
            if (string.IsNullOrWhiteSpace(tenFile))
                return null;

            // If it already contains http/https, return as is
            if (tenFile.StartsWith("http://") || tenFile.StartsWith("https://"))
                return tenFile;

            // Ensure leading slash for local paths
            return tenFile.StartsWith("/") ? tenFile : $"/{tenFile}";
        }

        /// <summary>
        /// Generates a URL-friendly slug from a given text.
        /// </summary>
        /// <param name="text">Text to convert to slug</param>
        /// <returns>URL-friendly slug</returns>
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var slug = text.ToLowerInvariant().Trim();
            // Replace spaces with hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
            // Remove special characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^\w\-]", "");
            // Remove multiple hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            // Trim hyphens from start/end
            return slug.Trim('-');
        }
    }
}
