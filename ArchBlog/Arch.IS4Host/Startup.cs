// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Arch.IS4Host.Data;
using Arch.IS4Host.Models;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Arch.IS4Host
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Store connection string as a var
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            // Store assembley migrations
            var migrationsAssembley = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // Replace DbContext database from SqLite in template to Postgres
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            var builder = services.AddIdentityServer()
                // Use our Postgres Database for storing configuration data
               .AddConfigurationStore(configDb => {
                   configDb.ConfigureDbContext = db => db.UseNpgsql(connectionString,
                   sql => sql.MigrationsAssembly(migrationsAssembley));
               })
                

            // Use our Postgres Database for storing operational data
               .AddOperationalStore(operationalDb => {
                   operationalDb.ConfigureDbContext = db => db.UseNpgsql(connectionString,
                   sql => sql.MigrationsAssembly(migrationsAssembley));
               })
               .AddAspNetIdentity<ApplicationUser>();

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com";
                    options.ClientSecret = "wdfPY6t8H8cecgjlxud__4Gh";
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }

        private void InitializeDatabase(IApplicationBuilder app) {
            using (var serviceScope = app.ApplicationServices.
            GetService<IServiceScopeFactory>().CreateScope())
            {
                var persistedGrantDbContext =  serviceScope.ServiceProvider
                .GetRequiredService<PersistedGrantDbContext>();
                persistedGrantDbContext.Database.Migrate();

                // Addition of initial population of db test
                var configDbContext =  serviceScope.ServiceProvider
                .GetRequiredService<ConfigurationDbContext>();
                configDbContext.Database.Migrate();

                if(!configDbContext.Clients.Any()) 
                {
                    foreach(var client in Config.GetClients()){
                        configDbContext.Clients.Add(client.ToEntity());
                    }
                    configDbContext.SaveChanges();
                }
                if(!configDbContext.IdentityResources.Any()) 
                {
                    foreach(var res in Config.GetIdentityResources()){
                        configDbContext.IdentityResources.Add(res.ToEntity());
                    }
                    configDbContext.SaveChanges();
                }
            }
        }
    }
}
