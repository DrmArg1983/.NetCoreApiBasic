using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using netCoreWebApi.Proxies;
using Polly;
using Polly.Registry;
using Refit;


namespace netCoreWebApi
{
    public class Startup
    {
        IPolicyRegistry<string> policyRegistry;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<PostsApiOptions>(Configuration.GetSection("PostsApiOptions"));

            ConfigurePolicies(services);
            ConfigureHttpClients(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        void ConfigurePolicies(IServiceCollection services)
        {
            policyRegistry = services.AddPolicyRegistry();
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(1500));
            policyRegistry.Add("timeout", timeoutPolicy);
        }

        void ConfigureHttpClients(IServiceCollection services)
        {
            services.AddHttpClient("PostsClient", options =>
            {
                options.BaseAddress = new Uri(Configuration["PostsApiOptions:BaseUrl"]);
                options.Timeout = TimeSpan.FromMilliseconds(15000);          
                options.DefaultRequestHeaders.Add("ClientFactory", "Check");
            })
            .AddPolicyHandlerFromRegistry("timeout")
            .AddTransientHttpErrorPolicy(p => p.RetryAsync(3))
            .AddTypedClient(client => RestService.For<IPostsClient>(client));
        }
    }
}
