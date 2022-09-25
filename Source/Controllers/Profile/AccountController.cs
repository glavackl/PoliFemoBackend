﻿#region

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoliFemoBackend.Source.Utils;

#endregion

namespace PoliFemoBackend.Source.Controllers.Profile;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[ApiExplorerSettings(GroupName = "Account")]
[Route("v{version:apiVersion}/accounts/me")]
[Route("accounts/me")]
public class ArticleByIdController : ControllerBase
{
    [MapToApiVersion("1.0")]
    [HttpGet]
    public ObjectResult ProfileDetails()
    {
        string userid;
        var tempSub = AuthUtil.GetSubject(Request.Headers["Authorization"]);
        var sub = tempSub ?? "";
        var permissions = AuthUtil.getPermissions(sub);
        using (var sha256Hash = SHA256.Create())
        {
            //From String to byte array
            var sourceBytes = Encoding.UTF8.GetBytes(sub);
            var hashBytes = sha256Hash.ComputeHash(sourceBytes);
            userid = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }

        return new ObjectResult(new
        {
            id = userid.ToLower(), permissions
        });
    }
}