using System;
using System.Text;
using System.Text.Json;

namespace Conduit.Infrastructure;

/// <summary>
/// Helper class for encoding and decoding cursor-based pagination cursors.
/// Cursor format: Base64 encoded JSON containing CreatedAt and ArticleId.
/// </summary>
public static class CursorHelper
{
    public record CursorData(DateTime CreatedAt, int ArticleId);

    /// <summary>
    /// Encodes cursor data into a Base64 string.
    /// </summary>
    public static string Encode(DateTime createdAt, int articleId)
    {
        var data = new CursorData(createdAt, articleId);
        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Decodes a Base64 cursor string into cursor data.
    /// Returns null if the cursor is invalid.
    /// </summary>
    public static CursorData? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(cursor);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<CursorData>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates if a cursor string is properly formatted.
    /// </summary>
    public static bool IsValid(string? cursor)
    {
        return Decode(cursor) != null;
    }
}
