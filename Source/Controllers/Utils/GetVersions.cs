﻿#region includes

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PoliFemoBackend.Source.Utils;

#endregion

namespace PoliFemoBackend.Source.Controllers.due;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[Route("[controller]")]
public class GetVersionsController : ControllerBase
{
    /// <summary>
    ///     Get the available versions of the API
    /// </summary>
    /// <returns></returns>
    [MapToApiVersion("1.0")]
    [HttpGet]
    [HttpPost]
    public ObjectResult GetVersions()
    {
        try
        {
            return Ok(JsonConvert.SerializeObject(new { versions = APIVersionsManager.ReadAPIVersions() }, Formatting.Indented));
        }
        catch (Exception ex)
        {
            return ResultUtil.ExceptionResult(ex);
        }
    }
}