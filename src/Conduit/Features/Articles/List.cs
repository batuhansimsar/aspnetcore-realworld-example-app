using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Articles;

public class List
{
    public record Query(
        string Tag,
        string Author,
        string FavoritedUsername,
        int? Limit,
        int? Offset,
        bool IsFeed = false
    ) : IRequest<ArticlesEnvelope>
    {
        /// <summary>
        /// Cursor for pagination. When provided, Offset is ignored.
        /// </summary>
        public string? Cursor { get; init; }
    }

    public class QueryHandler(ConduitContext context, ICurrentUserAccessor currentUserAccessor)
        : IRequestHandler<Query, ArticlesEnvelope>
    {
        public async Task<ArticlesEnvelope> Handle(
            Query message,
            CancellationToken cancellationToken
        )
        {
            var queryable = context.Articles.GetAllData();

            if (message.IsFeed && currentUserAccessor.GetCurrentUsername() != null)
            {
                var currentUser = await context
                    .Persons.Include(x => x.Following)
                    .FirstOrDefaultAsync(
                        x => x.Username == currentUserAccessor.GetCurrentUsername(),
                        cancellationToken
                    );

                if (currentUser is null)
                {
                    throw new RestException(
                        HttpStatusCode.NotFound,
                        new { User = Constants.NOT_FOUND }
                    );
                }
                queryable = queryable.Where(x =>
                    currentUser.Following.Select(y => y.TargetId).Contains(x.Author!.PersonId)
                );
            }

            if (!string.IsNullOrWhiteSpace(message.Tag))
            {
                var tag = await context.ArticleTags.FirstOrDefaultAsync(
                    x => x.TagId == message.Tag,
                    cancellationToken
                );
                if (tag != null)
                {
                    queryable = queryable.Where(x =>
                        x.ArticleTags.Select(y => y.TagId).Contains(tag.TagId)
                    );
                }
                else
                {
                    return new ArticlesEnvelope();
                }
            }

            if (!string.IsNullOrWhiteSpace(message.Author))
            {
                var author = await context.Persons.FirstOrDefaultAsync(
                    x => x.Username == message.Author,
                    cancellationToken
                );
                if (author != null)
                {
                    queryable = queryable.Where(x => x.Author == author);
                }
                else
                {
                    return new ArticlesEnvelope();
                }
            }

            if (!string.IsNullOrWhiteSpace(message.FavoritedUsername))
            {
                var author = await context.Persons.FirstOrDefaultAsync(
                    x => x.Username == message.FavoritedUsername,
                    cancellationToken
                );
                if (author != null)
                {
                    queryable = queryable.Where(x =>
                        x.ArticleFavorites.Any(y => y.PersonId == author.PersonId)
                    );
                }
                else
                {
                    return new ArticlesEnvelope();
                }
            }

            // Get total count before pagination
            var totalCount = await queryable.CountAsync(cancellationToken);

            // Order by CreatedAt descending, then by ArticleId for consistent ordering
            var orderedQuery = queryable.OrderByDescending(x => x.CreatedAt)
                                         .ThenByDescending(x => x.ArticleId);

            var limit = message.Limit ?? 20;

            // Apply cursor-based or offset-based pagination
            var cursorData = CursorHelper.Decode(message.Cursor);
            if (cursorData != null)
            {
                // Cursor-based: get items after the cursor position
                orderedQuery = (IOrderedQueryable<Domain.Article>)orderedQuery.Where(x =>
                    x.CreatedAt < cursorData.CreatedAt ||
                    (x.CreatedAt == cursorData.CreatedAt && x.ArticleId < cursorData.ArticleId)
                );
            }
            else if (message.Offset.HasValue && message.Offset > 0)
            {
                // Fallback to offset-based for backward compatibility
                orderedQuery = (IOrderedQueryable<Domain.Article>)orderedQuery.Skip(message.Offset.Value);
            }

            // Fetch one extra to determine if there are more results
            var articles = await orderedQuery
                .Take(limit + 1)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var hasMore = articles.Count > limit;
            if (hasMore)
            {
                articles = articles.Take(limit).ToList();
            }

            // Generate next cursor from the last article
            string? nextCursor = null;
            if (hasMore && articles.Count > 0)
            {
                var lastArticle = articles.Last();
                nextCursor = CursorHelper.Encode(lastArticle.CreatedAt, lastArticle.ArticleId);
            }

            return new ArticlesEnvelope 
            { 
                Articles = articles, 
                ArticlesCount = totalCount,
                NextCursor = nextCursor,
                HasMore = hasMore
            };
        }
    }
}
