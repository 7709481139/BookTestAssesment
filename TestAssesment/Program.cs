using TestAssesment.Configs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<FileSettings>(builder.Configuration.GetSection("FileSettings"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Books API",
        Version = "v1"
    });
});
var app = builder.Build();


app.UseMiddleware<GlobalExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Books API v1");
        c.RoutePrefix = "swagger"; // https://localhost:{port}/swagger
    });
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
