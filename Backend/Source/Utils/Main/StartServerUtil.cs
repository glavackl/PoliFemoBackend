﻿using System.IdentityModel.Tokens.Jwt;
using PoliFemoBackend.Source.Data;
using PoliFemoBackend.Source.Utils.Main.WebApplicationUtil;

namespace PoliFemoBackend.Source.Utils.Main;

public static class StartServerUtil
{
    internal static void StartServer(string[] args, ArgumentsUtil au)
    {
        var app = CreateApplicationUtil.CreateWebApplication(args);
        if (app == null)
        {
            Console.WriteLine("Error in creating the web application.");
            return;
        }

        GlobalVariables.App = app;

        GlobalVariables.TokenHandler = new JwtSecurityTokenHandler();

        WebApplicationConfigUtil.AppConfigPreServerThreads(app);

        try
        {
            Start.Start.StartThings(au.UseNews);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        WebApplicationConfigUtil.AppConfigPostServerThreads(app);

        app.Run();
    }
}