﻿#region includes

using Microsoft.AspNetCore.Mvc;
using PoliFemoBackend.Source.Utils;
using Database = PoliFemoBackend.Source.Utils.Database;

#endregion

namespace PoliFemoBackend.Source.Controllers.Articles;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[controller]")]
public class ArticlesTargetingTheFuture : ControllerBase
{
    [MapToApiVersion("1.0")]
    [HttpGet]
    [HttpPost]
    public ObjectResult SearchArticles()
    {
        try
        {
            var (articlesToSearchInto, exception) = ArticleUtil.GetArticles();
            return articlesToSearchInto == null
                ? ResultUtil.ExceptionResult(exception)
                : Ok(ArticleUtil.FilterByTargetingTheFuture(articlesToSearchInto));
        }
        catch (Exception ex)
        {
            return ResultUtil.ExceptionResult(ex);
        }
    }

    //date today    
    public ObjectResult SearchArticlesByTargetingTheFuture()
    {
        var results = Database.ExecuteSelect(
            "SELECT * FROM article WHERE publishTime > NOW()",
            GlobalVariables.DbConfigVar);

        return Ok(results);
    }
}