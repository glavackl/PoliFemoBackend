﻿#region

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PoliFemoBackend.Source.Data;
using PoliFemoBackend.Source.Objects.DbObjects;
using PoliFemoBackend.Source.Utils;
using PoliFemoBackend.Source.Utils.Database;

#endregion

namespace PoliFemoBackend.Source.Controllers.Articles;

[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Articles")]
[Route("v{version:apiVersion}/articles")]
[Route("/articles")]
public class ArticlesByParameters : ControllerBase
{
    /// <summary>
    ///     Search articles by parameters
    /// </summary>
    /// <param name="start" example="2022-05-18T12:15:00Z">Start time</param>
    /// <param name="end" example="2022-05-18T12:15:00Z">End time</param>
    /// <param name="tag" example="STUDENTI">Tag name</param>
    /// <param name="author_id" example="1">Author ID</param>
    /// <param name="title" example="Titolo...">Article title</param>
    /// <param name="limit" example="30">Limit of results (can be null)</param>
    /// <param name="pageOffset">Offset page for limit (can be null)</param>
    /// <param name="sort" example="date">Sort by column</param>
    /// <remarks>
    ///     At least one of the parameters must be specified.
    /// </remarks>
    /// <returns>A JSON list of articles</returns>
    /// <response code="200">Request completed successfully</response>
    /// <response code="404">No available articles</response>
    /// <response code="500">Can't connect to the server</response>

    [MapToApiVersion("1.0")]
    [HttpGet]
    public ObjectResult SearchArticlesByDateRange(DateTime? start, DateTime? end, string? tag, int? author_id,
        string? title, uint? limit, uint? pageOffset, string? sort)
    {
        if (start == null && end == null && tag == null && author_id == null)
            return new BadRequestObjectResult(new
            {
                error = "Invalid parameters"
            });

        var r = SearchArticlesByParamsAsJobject(start, end, tag, author_id, title, new LimitOffset(limit, pageOffset),
            sort);
        return r == null ? new NotFoundObjectResult("") : Ok(r);
    }

    private static JObject? SearchArticlesByParamsAsJobject(DateTime? start, DateTime? end, string? tag, int? author_id,
        string? title, LimitOffset limitOffset, string? sort)
    {
        var startDateTime = DateTimeUtil.ConvertToMySqlString(start ?? null);
        var endDateTime = DateTimeUtil.ConvertToMySqlString(end ?? null);
        var query = "SELECT * FROM ArticlesWithAuthors_View WHERE ";
        if (start != null) query += "publishTime >= @start AND ";
        if (end != null) query += "publishTime <= @end AND ";
        if (tag != null) query += "id_tag = @tag AND ";
        if (author_id != null) query += "id_author = @author_id AND ";
        if (title != null) query += "title LIKE @title AND ";

        query = query[..^4]; // remove last "and"

        if (sort == "date") query += "ORDER BY publishTime DESC ";

        query += limitOffset.GetLimitQuery();

        var results = Database.ExecuteSelect(
            query, // Remove last AND
            GlobalVariables.DbConfigVar,
            new Dictionary<string, object?>
            {
                { "@start", startDateTime },
                { "@end", endDateTime },
                { "@tag", tag },
                { "@author_id", author_id },
                { "@title", "%" + title + "%" }
            });
        if (results == null || results.Rows.Count == 0)
            return null;

        var resultsJArray = ArticleUtil.ArticleAuthorsRowsToJArray(results);

        var r = new JObject
        {
            ["results"] = resultsJArray,
            ["start"] = startDateTime,
            ["end"] = endDateTime,
            ["tag"] = tag,
            ["author_id"] = author_id,
            ["title"] = title
        };
        return r;
    }
}