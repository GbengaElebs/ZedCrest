using System;
using System.Collections.Generic;
using Api.Middleware;
using Application.Interfaces;
using Application.UserHandler;
using Infrastructure.Documents;
using Infrastructure.Email;
using Infrastructure.RabbitMQ;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQConsumers.UserDocumentEmailConsumer1;
using RabbitMQConsumers.UserDocumentEmailConsumer2;
using ZedCrestTest.Persistence.DBContexts;

namespace ZedCrestTest
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

            services.AddControllers();
            
            services.AddDbContext<ZedCrestContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseMySql(Configuration.GetConnectionString("ZedCrestDBConn"),
                new MySqlServerVersion(new Version(8, 0, 19)));
            });
            services.AddMediatR(typeof(Register.Handler).Assembly);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZedCrestTest", Version = "v1" });
            });
            services.Configure<CloudinarySettings>(Configuration.GetSection("Cloudinary"));
            services.Configure<SendGridSettings>(Configuration.GetSection("SendGrid"));
            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));
            services.Configure<RabbitMqConfiguration>(Configuration.GetSection("RabbitMq"));

            services.AddScoped<IDocumentsAccessor, DocumentsAccessor>();
            services.AddTransient<ISendEmailServiceA, SenderEmailSendGrid>();
            services.AddTransient<ISendEmailServiceB, SendEmail>();
            services.AddTransient<IUserDocumentEmailPulisher, UserDocumentEmailPulisher>(); 

           

            //Two Background Services as the Consumers
            services.AddHostedService<UserDocumentEmailConsumer1>();
            services.AddHostedService<UserDocumentEmailConsumer2>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZedCrestTest v1"));
            }

            app.UseRouting();

            app.UseAuthorization();
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
