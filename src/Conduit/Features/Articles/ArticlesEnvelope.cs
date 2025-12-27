using System.Collections.Generic;
using Conduit.Domain;

namespace Conduit.Features.Articles;

public class ArticlesEnvelope
{
    public List<Article> Articles { get; set; } = new();

    public int ArticlesCount { get; set; }

    /// <summary>
    /// Cursor for fetching the next page of results.
    /// Null if there are no more results.
    /// </summary>
    public string? NextCursor { get; set; }

    /// <summary>
    /// Indicates if there are more results after this page.
    /// </summary>
    public bool HasMore { get; set; }
}
