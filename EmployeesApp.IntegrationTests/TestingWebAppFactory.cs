using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EmployeesApp.Models;

namespace EmployeesApp.IntegrationTests
{
    public class TestingWebAppFactory<T> : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                  d => d.ServiceType ==
                     typeof(DbContextOptions<EmployeeContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                var serviceProvider = new ServiceCollection()
                  .AddEntityFrameworkInMemoryDatabase()
                  .BuildServiceProvider();
                services.AddDbContext<EmployeeContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryEmployeeTest");
                    options.UseInternalServiceProvider(serviceProvider);
                });
                //iniettiamo mediante servicecollection info realtive a dettagli token
                services.AddAntiforgery(t =>
                {
                    t.Cookie.Name = AntiForgeryTokenExtractor.AntiForgeryCookieName;
                    t.FormFieldName = AntiForgeryTokenExtractor.AntiForgeryFieldName;
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    using (var appContext = scope.ServiceProvider.GetRequiredService<EmployeeContext>())
                    {
                        try
                        {
                            appContext.Database.EnsureCreated();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            //Log errors or do anything you think it's needed
                            throw;
                        }
                    }
                }
            });
        }
    }
}
