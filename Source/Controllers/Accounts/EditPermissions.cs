﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PoliFemoBackend.Source.Data;
using PoliFemoBackend.Source.Objects.Permissions;
using PoliFemoBackend.Source.Utils;
using PoliFemoBackend.Source.Utils.Database;

namespace PoliFemoBackend.Source.Controllers.Accounts;

[ApiController]
[ApiExplorerSettings(GroupName = "Accounts")]
[Route("/accounts/{id}/permissions")]

public class EditPermissions : ControllerBase
{
    /// <summary>
    ///     Grant a permission to a user
    /// </summary>
    /// <remarks>
    ///     The body is a Grant object
    /// </remarks>
    /// <param name="id">User ID</param>
    /// <param name="grant">The Grant object</param>
    /// <response code="200">Request completed successfully</response>
    /// <response code="401">Authorization error</response>
    /// <response code="403">The user does not have enough permissions</response>
    /// <response code="400">A received grant is not valid</response>
    /// <response code="500">Can't connect to the server</response>
    
    [HttpPost]
    [Authorize]
    public ObjectResult GrantPermission(string id, [FromBody] Grant grant)
    {
        var canGrantPermissions = AuthUtil.HasPermission(AuthUtil.GetSubjectFromHttpRequest(Request),
            Constants.Permissions.PermissionsConst);
        if (!canGrantPermissions)
        {
            Response.StatusCode = 403;
            return new ObjectResult(new JObject
            {
                { "error", "You don't have enough permissions" }
            });
        }

        const string q =
            "INSERT INTO permission (grant_id, user_id, object_id) VALUES (@id_grant, @id_user, @id_object)";
        try
        {
            var count = Database.Execute(q, DbConfig.DbConfigVar, new Dictionary<string, object?>
            {
                { "@id_grant", grant.grant },
                { "@id_user", id },
                { "@id_object", grant.object_id }
            });
        }
        catch (Exception)
        {
            return new BadRequestObjectResult(new JObject
            {
                { "error", "An error occurred while granting the permission. Make sure the grant is valid" }
            });
        }

        return Ok("");
    }


    /// <summary>
    ///     Revoke a permission from a user
    /// </summary>
    /// <remarks>
    ///     The body is a Grant object
    /// </remarks>
    /// <param name="id">User ID</param>
    /// <param name="grant">The Grant object</param>
    /// <response code="200">Permissions updated successfully</response>
    /// <response code="403">The user does not have enough permissions</response>
    
    [HttpDelete]
    [Authorize]
    public ObjectResult RevokePermission(string id, [FromBody] Grant grant)
    {
        var canRevokePermissions = AuthUtil.HasPermission(AuthUtil.GetSubjectFromHttpRequest(Request),
            Constants.Permissions.PermissionsConst);
        if (!canRevokePermissions)
        {
            Response.StatusCode = 403;
            return new ObjectResult(new JObject
            {
                { "error", "You don't have enough permissions" }
            });
        }

        const string q =
            "DELETE FROM permission WHERE grant_id= @id_grant AND user_id = @id_user AND object_id = @id_object";
        var count = Database.Execute(q, DbConfig.DbConfigVar, new Dictionary<string, object?>
        {
            { "@id_grant", grant.grant },
            { "@id_user", id },
            { "@id_object", grant.object_id }
        });
        return Ok("");
    }
}