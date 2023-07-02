#region

using System.IdentityModel.Tokens.Jwt;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using PoliFemoBackend.Source.Utils.Database;

#endregion

namespace PoliFemoBackend.Source.Data;

public static class GlobalVariables
{
    public static readonly DateTime Start = DateTime.Now;
    private static JObject? _secrets;
    public static WebApplication? App;
    public static string? BasePath { get; set; }
    public static int LogLevel { get; set; }
    public static DbConfig? DbConfigVar { get; set; }
    public static MySqlConnection? DbConnection { get; set; }
    public static JwtSecurityTokenHandler? TokenHandler { get; set; }
    public static bool? SkipDbSetup { get; set; }

    public static JToken? GetSecrets(string? v)
    {
        return v != null ? _secrets?[v] : null;
    }

    public static void SetSecrets(JObject? jObject)
    {
        _secrets = jObject;
    }

    public static DbConfig? GetDbConfig()
    {
        if (DbConfigVar != null)
            return DbConfigVar;

        DbConfig.InitializeDbConfig();
        return DbConfigVar;
    }
}