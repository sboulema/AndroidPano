using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http.Headers;
using WebVRPano.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IPanoService, PanoService>();
builder.Services.AddTransient<IXmlService, XmlService>();

builder.Services.AddHttpClient("funda", client =>
{
    client.BaseAddress = new Uri("http://partnerapi.funda.nl");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.Run();
