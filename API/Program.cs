WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Assembly assembly = Assembly.GetExecutingAssembly();
IConfiguration configuration = builder.Configuration;

_ = builder.AddAppConfigurations();

_ = builder.AddAutofacConfiguration();

_ = builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            MSerilogAction.Configure(context, services, loggerConfiguration, false);
        });

Log.Information("Starting ", builder.Environment.ApplicationName + " API up");
try
{
    IServiceCollection services = builder.Services;
    _ = services.AddApplication(assembly);
    _ = services.AddInfrastructure<Program>(configuration);
    _ = services.AddDatabaseConfig(configuration);
    _ = services.SwaggerConfig(builder.Environment.ApplicationName);
    _ = services.AddScopeServices(typeof(ApiDbContext).Assembly);
    _ = services.AddValidateBearerToken<MTokenInfo>();
    _ = services.AddDbContextConfigure<ApiDbContext>(configuration, dbType: nameof(DbTypes.SqlServer));

    WebApplication app = builder.Build();
    _ = app.AddLocalization(assembly);
    _ = app.UseRouting();
    _ = app.UseAuthentication();
    _ = app.UseAuthorization();
    _ = app.ConfigureEndpoints();
    _ = app.MigrateDatabase<ApiDbContext>();
    _ = app.UseDefaultMiddleware();
    await app.RunAsync();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;

    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }

    Log.Fatal(ex, "Unhandled exception: ", ex.Message);
}
finally
{
    Log.Information("Shut down ", builder.Environment.ApplicationName + " complete");
    await Log.CloseAndFlushAsync();
}