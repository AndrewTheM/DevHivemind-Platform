using BlogPlatform.Posts.API.Extensions;
using BlogPlatform.Posts.BusinessLogic.Mapping;
using BlogPlatform.Posts.DataAccess.Context;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Globalization;

namespace BlogPlatform.Posts.API;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private IEnumerable<OpenApiInfo> GetApiVersionsInfo()
    {
        var versionsConfig = _configuration.GetSection("Versions");
        var apiInfos = versionsConfig.Get<OpenApiInfo[]>();
        return apiInfos;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddDbContext<BlogContext>(options =>
        {
            string connectionString = _configuration.GetConnectionString("LocalSqlServer");
            options.UseSqlServer(connectionString);
        });

        services.AddHttpContextAccessor();
        services.AddAutoMapper(typeof(BlogMappingProfile).Assembly);

        services.AddRepositories();
        services.AddBlogging();

        services.AddLocalization(opt => opt.ResourcesPath = "Resources");
        services.Configure<RequestLocalizationOptions>(opts =>
        {
            var supportedCultures = new List<CultureInfo> { new("en"), new("uk"), new("ru") };
            opts.DefaultRequestCulture = new RequestCulture("en", "en");
            opts.SupportedCultures = supportedCultures;
            opts.SupportedUICultures = supportedCultures;
        });

        var versionsInfo = GetApiVersionsInfo();
        services.AddSwaggerWithSecurityAndVersioning(versionsInfo);

        services.AddControllers()
            .AddFluentValidation(config =>
            {
                config.RegisterValidatorsFromAssemblyContaining<Startup>();
                config.DisableDataAnnotationsValidation = true;
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var apiInfo in GetApiVersionsInfo())
                {
                    c.SwaggerEndpoint($"/swagger/{apiInfo.Version}/swagger.json",
                                      $"{apiInfo.Title} {apiInfo.Version}");
                }
            });
        }

        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseRouting();

        // TODO: configure CORS properly
        app.UseCors(builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}