using System.Text;
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

app.MapPost("/submitSignature", async (eSigEvent sigEvent) =>
{
    try
    {
        // Since we don't receive the SigType from eSignature Saturn we chose randomly between QES and QSeal
        if (string.IsNullOrEmpty(sigEvent.SigType))
        {
            eSigEvent fixedEvent = sigEvent with { SigType = new Random().Next(2).ToString() };
            sigEvent = fixedEvent;
        }

        string jsonData = JsonSerializer.Serialize(sigEvent);

        /* We pass the received JSON along to our backend. 
         * If that fails we save it as a file to a data/backend/failed where the backend will pick it up on restart
         */
        try
        {
            var url = "http://localhost:3001/create-audit-log";

            var httpClient = new HttpClient();
            HttpContent httpcontent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, httpcontent);
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Passed request for user {sigEvent.EvidenceUserID} to the backend");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to pass request for user {sigEvent.EvidenceUserID} to backend: {ex.Message}");
            string dataDir = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "data", "backend", "failed");
            Directory.CreateDirectory(dataDir);
            string fileName = $"{DateTime.Now:yyMMdd-hhmmss}-{sigEvent.EvidenceUserID}-{Guid.NewGuid()}.json";
            string filePath = Path.Combine(dataDir, fileName);

            // Save json to failed folder
            File.WriteAllText(filePath, jsonData);
            Console.WriteLine($"Saved request for user {sigEvent.EvidenceUserID} to the failed folder");
        }

        return Results.Ok($"Signature saved for user {sigEvent.EvidenceUserID}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error processing request:" + Environment.NewLine + ex.ToString());
        return Results.Problem("Error processing request");
    }
});

app.MapGet("/", () => "eSigAttestation REST API up and running!");

app.Run();
