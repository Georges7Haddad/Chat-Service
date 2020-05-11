using System;
using Aub.Eece503e.ChatService.Web.Services;
using Aub.Eece503e.ChatService.Web.Store;
using Aub.Eece503e.ChatService.Web.Store.Azure;
using Aub.Eece503e.ChatService.Web.Store.DocumentDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aub.Eece503e.ChatService.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddSingleton<IProfileStore,InMemoryUserStore>();
            
            services.AddControllers();
            services.AddSingleton<IProfileStore, AzureTableProfileStore>();
            services.AddSingleton<IImagesStore, AzureBlobProfilePictureStore>();
            services.AddSingleton<IMessageStore, DocumentDbMessageStore>();
            services.AddSingleton<IConversationStore, DocumentDbConversationStore>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<IImageService, ImageService>();
            services.AddSingleton<IConversationService, ConversationService>();
            services.AddOptions();
            
            services.Configure<AzureStorageSettings>(Configuration.GetSection("AzureStorageSettings"));
            
            string instrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
            services.AddApplicationInsightsTelemetry(instrumentationKey);
            
            services.Configure<DocumentDbSettings>(Configuration.GetSection(nameof(DocumentDbSettings)));
            services.AddSingleton<IDocumentClient>(sp =>
            {
                var settings = GetSettings<DocumentDbSettings>();
                return new DocumentClient(new Uri(settings.EndpointUrl), Environment.GetEnvironmentVariable("PrimaryKey"),
                    new ConnectionPolicy
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        ConnectionProtocol = Protocol.Tcp,
                        MaxConnectionLimit = settings.MaxConnectionLimit
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiddleware<ExceptionMiddleware>();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        
        private T GetSettings<T>() where T : new()
        {
            var config = Configuration.GetSection(typeof(T).Name);
            T settings = new T();
            config.Bind(settings);
            return settings;
        }
    }
}
