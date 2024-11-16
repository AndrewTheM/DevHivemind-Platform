using BlogPlatform.Shared.Common.Filters;
using BlogPlatform.Shared.Services.Contracts;
using System.Collections.Specialized;

namespace BlogPlatform.Shared.Services;

public class UriService : IUriService
{
    public string UriBase { get; set; }

    public UriService(string uriBase)
    {
        UriBase = uriBase;
    }

    public Uri GetPostsPageUri(PostFilter filter = null)
    {
        string endpoint = "api/posts";
        NameValueCollection parameters = null;

        if (filter is not null)
        {
            parameters = new()
            {
                ["title"] = filter.Title,
                ["author"] = filter.Author,
                ["year"] = filter.Year.ToString(),
                ["month"] = filter.Month.ToString(),
                ["day"] = filter.Day.ToString(),
                ["tag"] = filter.Tag
            };
        }

        return BuildPageUri(endpoint, filter, parameters);
    }

    public Uri GetCommentsPageUri(Guid postId, CommentFilter filter = null)
    {
        string endpoint = $"api/posts/{postId}/comments";
        NameValueCollection parameters = null;

        if (filter is not null)
        {
            parameters = new()
            {
                ["content"] = filter.Content
            };
        }

        return BuildPageUri(endpoint, filter, parameters);
    }

    private Uri BuildPageUri(string endpoint,
                             PaginationFilter filter = null,
                             NameValueCollection specificParameters = null)
    {
        UriBuilder uriBuilder = new($"{UriBase}/{endpoint}");

        if (filter is null)
        {
            return uriBuilder.Uri;
        }

        var queryParameters = new NameValueCollection
        {
            ["pageNumber"] = filter.PageNumber.ToString(),
            ["pageSize"] = filter.PageSize.ToString()
        };

        if (specificParameters is not null)
        {
            queryParameters.Add(specificParameters);
        }

        foreach (string name in queryParameters)
        {
            string value = queryParameters[name];

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(uriBuilder.Query))
            {
                uriBuilder.Query += "&";
            }

            uriBuilder.Query += $"{name}={value}";
        }

        return uriBuilder.Uri;
    }
}
