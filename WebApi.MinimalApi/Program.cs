using System.Reflection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Serialization;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
        options.SuppressMapClientErrors = true;
    });

builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

builder.Services.AddControllers(options =>
    {
        options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
        options.ReturnHttpNotAcceptable = true;
        options.RespectBrowserAcceptHeader = true;
    })
    .ConfigureApiBehaviorOptions(o =>
    {
        o.SuppressModelStateInvalidFilter = true;
        o.SuppressMapClientErrors = true;
    })
    .AddNewtonsoftJson(o => { o.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); });

builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<UserEntity, UserDto>()
        .ForMember(d => d.FullName, opt => opt.MapFrom(s =>
            string.IsNullOrWhiteSpace(s.LastName) && string.IsNullOrWhiteSpace(s.FirstName)
                ? null
                : $"{s.LastName} {s.FirstName}".Trim()));

    cfg.CreateMap<CreateUserDto, UserEntity>()
        .ForMember(d => d.Id, o => o.Ignore())
        .ForMember(d => d.FirstName, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.FirstName) ? "John" : s.FirstName))
        .ForMember(d => d.LastName, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.LastName) ? "Doe" : s.LastName));
}, new System.Reflection.Assembly[0]);


var app = builder.Build();

app.MapControllers();

app.Run();