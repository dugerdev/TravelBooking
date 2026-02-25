using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Web.Helpers;

/// <summary>
/// Normalizes and resolves image URLs so they work in img src (supports ~/, /, relative, and http).
/// </summary>
public static class ImageUrlHelper
{
    /// <summary>
    /// Normalize ImageUrl before saving: relative paths become root-relative (/assets/...).
    /// Call this in Create/Edit POST before sending to API.
    /// </summary>
    public static string NormalizeForSave(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return string.Empty;
        var s = imageUrl.Trim();
        // Leave full URLs as-is
        if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return s;
        // Ensure root-relative: /assets/img/...
        if (s.StartsWith("~/"))
            s = "/" + s[2..].TrimStart('/');
        else if (!s.StartsWith("/"))
            s = "/" + s.TrimStart('/');
        return s;
    }

    /// <summary>
    /// Resolve image URL for display in img src. Supports ~/, /, relative, http, and API-relative paths.
    /// Use this overload for Car/Hotel images from API - pass apiBaseUrl to resolve API-relative paths.
    /// </summary>
    public static string GetImageSrc(IUrlHelper urlHelper, string? imageUrl, string defaultPath, string? apiBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return urlHelper.Content(defaultPath) ?? "";
        var s = imageUrl.Trim();
        if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return s;
        // Local assets (~/assets/... or /assets/...) are always served from the web app
        if (s.StartsWith("~/assets/", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("/assets/", StringComparison.OrdinalIgnoreCase))
        {
            var localPath = s.StartsWith("~/") ? s : "~" + s;
            return urlHelper.Content(localPath) ?? localPath;
        }
        // When apiBaseUrl is provided, resolve all other relative paths against the API
        if (!string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            var baseUrl = apiBaseUrl.TrimEnd('/');
            var path = s.StartsWith("/") ? s : "/" + s;
            return baseUrl + path;
        }
        var fallbackPath = s.StartsWith("~/") ? s : s.StartsWith("/") ? "~" + s : "~/" + s.TrimStart('/');
        return urlHelper.Content(fallbackPath) ?? fallbackPath;
    }

    /// <summary>
    /// Resolve image URL for display in img src (3-param overload for non-API images).
    /// </summary>
    public static string ImageSrc(this IUrlHelper urlHelper, string? imageUrl, string defaultPath = "~/assets/img/placeholder.svg")
    {
        return GetImageSrc(urlHelper, imageUrl, defaultPath, null);
    }
}
