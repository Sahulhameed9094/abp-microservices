using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Tasky.AdministrationService.EntityFrameworkCore;
using Tasky.Shared.Hosting;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace Tasky.AdministrationService;

[DependsOn(
    typeof(TaskyHostingModule),
    typeof(AdministrationServiceApplicationModule),
    typeof(AdministrationServiceEntityFrameworkCoreModule),
    typeof(AdministrationServiceHttpApiModule)
    )]
public class AdministrationServiceHttpApiHostModule : AbpModule
{

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        //Configure<AbpDbContextOptions>(options =>
        //{
        //    options.UseSqlServer();
        //});

        //Configure<AbpMultiTenancyOptions>(options =>
        //{
        //    options.IsEnabled = MultiTenancyConsts.IsEnabled;
        //});

        //if (hostingEnvironment.IsDevelopment())
        //{
        //    Configure<AbpVirtualFileSystemOptions>(options =>
        //    {
        //        options.FileSets.ReplaceEmbeddedByPhysical<AdministrationServiceDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Tasky.AdministrationService.Domain.Shared", Path.DirectorySeparatorChar)));
        //        options.FileSets.ReplaceEmbeddedByPhysical<AdministrationServiceDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Tasky.AdministrationService.Domain", Path.DirectorySeparatorChar)));
        //        options.FileSets.ReplaceEmbeddedByPhysical<AdministrationServiceApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Tasky.AdministrationService.Application.Contracts", Path.DirectorySeparatorChar)));
        //        options.FileSets.ReplaceEmbeddedByPhysical<AdministrationServiceApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}Tasky.AdministrationService.Application", Path.DirectorySeparatorChar)));
        //    });
        //}

        context.Services.AddAbpSwaggerGenWithOAuth(
            configuration["AuthServer:Authority"],
            new Dictionary<string, string>
            {
                {"AdministrationService", "AdministrationService API"}
            },
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "AdministrationService API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });

        //Configure<AbpLocalizationOptions>(options =>
        //{
        //    options.Languages.Add(new LanguageInfo("ar", "ar", "العربية"));
        //    options.Languages.Add(new LanguageInfo("cs", "cs", "Čeština"));
        //    options.Languages.Add(new LanguageInfo("en", "en", "English"));
        //    options.Languages.Add(new LanguageInfo("en-GB", "en-GB", "English (UK)"));
        //    options.Languages.Add(new LanguageInfo("fi", "fi", "Finnish"));
        //    options.Languages.Add(new LanguageInfo("fr", "fr", "Français"));
        //    options.Languages.Add(new LanguageInfo("hi", "hi", "Hindi", "in"));
        //    options.Languages.Add(new LanguageInfo("is", "is", "Icelandic", "is"));
        //    options.Languages.Add(new LanguageInfo("it", "it", "Italiano", "it"));
        //    options.Languages.Add(new LanguageInfo("hu", "hu", "Magyar"));
        //    options.Languages.Add(new LanguageInfo("pt-BR", "pt-BR", "Português"));
        //    options.Languages.Add(new LanguageInfo("ro-RO", "ro-RO", "Română"));
        //    options.Languages.Add(new LanguageInfo("ru", "ru", "Русский"));
        //    options.Languages.Add(new LanguageInfo("sk", "sk", "Slovak"));
        //    options.Languages.Add(new LanguageInfo("tr", "tr", "Türkçe"));
        //    options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
        //    options.Languages.Add(new LanguageInfo("zh-Hant", "zh-Hant", "繁體中文"));
        //    options.Languages.Add(new LanguageInfo("de-DE", "de-DE", "Deutsch"));
        //    options.Languages.Add(new LanguageInfo("es", "es", "Español"));
        //    options.Languages.Add(new LanguageInfo("el", "el", "Ελληνικά"));
        //});

        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = "AdministrationService";
            });

        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "AdministrationService:";
        });

        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("AdministrationService");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "AdministrationService-Protection-Keys");
        }

        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        //if (MultiTenancyConsts.IsEnabled)
        //{
        app.UseMultiTenancy();
        //}
        app.UseAbpRequestLocalization();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Support APP API");

            var configuration = context.GetConfiguration();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
            options.OAuthScopes("AdministrationService");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
