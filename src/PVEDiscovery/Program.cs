using PVEDiscovery.Common;
using PVEDiscovery.Services;
using PVEDiscovery.Common;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<PVESettings>(builder.Configuration.GetSection("PVE"));
builder.Services.AddSwaggerGen();
// https://stackoverflow.com/questions/56850413/how-to-pass-httpclienthandler-to-httpclientfactory-explicitly
// https://stackoverflow.com/questions/12553277/allowing-untrusted-ssl-certificates-with-httpclient
// builder.Services.AddHttpClient<MyCustomHttpClient>()
//     .ConfigurePrimaryHttpMessageHandler(
//                                        _ => new HttpClientHandler
//                                        {
//                                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//                                        });
builder.Services.AddSingleton<VirtualMachineService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
