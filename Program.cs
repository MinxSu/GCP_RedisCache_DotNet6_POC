var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
RedisConnection.Init();
app.MapGet("/get", (string key) =>
{
    RedisModel redis = new RedisModel();
    return redis.getKey(key).Result;
})
.WithName("GetValue");

app.MapGet("/set", (string key, string value) =>
{
    RedisModel redis = new RedisModel();
    redis.setKey(key, value);
    return Results.Ok();
})
.WithName("SetValue");

app.Run("http://0.0.0.0:8080");