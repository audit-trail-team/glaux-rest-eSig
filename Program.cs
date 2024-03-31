using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

app.MapPost("/submitSignature", (eSigEvent sigEvent) =>
{
    try
    {
        string dataDir = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "data";
        Directory.CreateDirectory(dataDir);
        string fileName = $"{DateTime.Now:yyMMdd-hhmmss}-{sigEvent.EvidenceUserID}-{Guid.NewGuid()}.json";
        string filePath = Path.Combine(dataDir, fileName);

        // Serialize the object to JSON and save it to a file
        string jsonContent = JsonSerializer.Serialize(sigEvent);
        File.WriteAllText(filePath, jsonContent);

        return Results.Ok($"Signature saved for EvidenceUserID: {sigEvent.EvidenceUserID}");
    }
    catch (Exception ex)
    {
        //Console.WriteLine(ex.ToString());
        return Results.Problem("Error processing request");
    }
});

app.MapGet("/", () => "eSigAttestation REST API up and running!");

app.Run();