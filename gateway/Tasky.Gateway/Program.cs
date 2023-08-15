var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
var app = builder.Build();
//redirect api incoming and outgoing 
app.MapReverseProxy();
//app.MapGet("/", () => "Hello World!");
app.Run();

