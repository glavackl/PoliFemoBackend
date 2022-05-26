#region includes

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PoliFemoBackend;
using PoliFemoBackend.Source.Test;
using PoliFemoBackend.Source.Utils;
using Database = PoliFemoBackend.Source.Utils.Database;

#endregion

if (args.Length > 0 && args[0] == "test")
{
    Test.TestMain();
    return;
}

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers().AddNewtonsoftJson();

    builder.Services.AddApiVersioning(setup =>
    {
        setup.DefaultApiVersion = new ApiVersion(1, 0);
        setup.AssumeDefaultVersionWhenUnspecified = true;
        setup.ReportApiVersions = true;
    });
    builder.Services.AddVersionedApiExplorer(setup =>
    {
        setup.GroupNameFormat = "'v'VVV";
        setup.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddSwaggerGen();
    builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        if (app.Services.GetService(typeof(IApiVersionDescriptionProvider)) is IApiVersionDescriptionProvider provider)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint("/swagger/" + description.GroupName + "/swagger.json", "PoliFemoBackend API " + description.GroupName.ToUpperInvariant());
                options.RoutePrefix = "swagger";
            }
        }
        else
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "PoliFemoBackend API V1");
            options.RoutePrefix = "swagger";
        }
    });
    DbConfig.InitializeDbConfig();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}