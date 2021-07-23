using GreenPipes;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace InMemoryOutboxTest.Api
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
            services.AddDbContext<TestSagaDbContext>(options =>
            {
                options.UseSqlServer("Data Source=.;Initial Catalog=InMemoryOutboxTest;Trusted_Connection=True;MultipleActiveResultSets=true");
            });

            MasstransitSetup(services);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "InMemoryOutboxTest.Api", Version = "v1" });

                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

            });

            services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "InMemoryOutboxTest.Api v1"));
            }

            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            serviceScope.ServiceProvider.GetRequiredService<TestSagaDbContext>().Database.Migrate();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void MasstransitSetup(IServiceCollection services)
        {
            services.AddMassTransit(config =>
            {
                config.SetKebabCaseEndpointNameFormatter();

                config.AddSagaStateMachine<SagaStateMachine, SagaState>()
                    .EntityFrameworkRepository(r =>
                    {
                        r.ExistingDbContext<TestSagaDbContext>();
                        r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    });

                config.UsingRabbitMq((context, cfg) =>
                {
                    // Rabbit Connection Settings
                    cfg.Host("localhost", 5672, "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                        h.RequestedChannelMax(40);

                        h.ConfigureBatchPublish(x =>
                        {
                            x.Timeout = TimeSpan.FromMilliseconds(50);
                        });

                        h.PublisherConfirmation = true;
                    });

                    // Sets Exchange name for when message type is produced or consumed.
                    cfg.Message<FirstEventReceived>(x =>
                    {
                        x.SetEntityName("ex.inMemoryOutboxTest.firstEventReceived");
                    });

                    cfg.Message<SecondEvent>(x =>
                    {
                        x.SetEntityName("ex.inMemoryOutboxTest.secondEvent");
                    });

                    cfg.ReceiveEndpoint("saga.inMemoryOutboxTest", e =>
                    {
                        e.UseMessageRetry(r => r.Interval(retryCount: 3, interval: TimeSpan.FromMilliseconds(100)));
                        e.UseInMemoryOutbox();

                        //e.ConcurrentMessageLimit = 10;
                        //e.PrefetchCount = 10;

                        e.ConfigureSaga<SagaState>(context, c =>
                        {
                            //var partition = c.CreatePartitioner(10);

                            //c.Message<FirstEventReceived>(x => x.UsePartitioner(partition, m => m.Message.ExecutionId));                            
                        });
                    });
                });
            });

            services.AddMassTransitHostedService();
        }
    }
}
