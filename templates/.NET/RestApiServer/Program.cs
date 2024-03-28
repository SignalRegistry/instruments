var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapPost("/", () => "Hello World!");
app.MapPut("/", () => "Hello World!");
app.MapDelete("/", () => "Hello World!");

app.Run();