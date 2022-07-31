using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuotesApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connStrBuilder = new SqlConnectionStringBuilder(
    builder.Configuration.GetConnectionString("QuotesDb-safe"));
connStrBuilder.Password = builder.Configuration["Password"];
var connection = connStrBuilder.ConnectionString;
builder.Services.AddDbContext<QuotesDbContext>(opt => opt.UseSqlServer(connection));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// =============================================================================
// Add Requests here:

app.MapGet("/", () => "Welcome! This is a basic Quotes API");                                                   // Define Index (NOT NEEDED) //

app.MapPost("/quotes", async (QuoteDTO quoteDTO, QuotesDbContext db) =>                                            // Create new quote (POST) //
{
    var quote = new Quote
    {
        Content = quoteDTO.Content,
        Source = quoteDTO.Source,
        SubSource = quoteDTO.SubSource
    };
    db.Quotes.Add(quote);
    await db.SaveChangesAsync();

    return Results.Created($"/api/quotes/{quote.Id}", new QuoteDTO(quote)); //returns contents of quote created
});

app.MapGet("/quotes", async (QuotesDbContext db) =>                                                                // Read all quotes (GET) //
    // await db.Quotes.ToListAsync());         //without DTO
    await db.Quotes.Select(x => new QuoteDTO(x)).ToListAsync());

app.MapGet("/quotes/{id}", async (int id, QuotesDbContext db) =>                                                   // Read a quote by Id (GET) //
    await db.Quotes.FindAsync(id)
        is Quote quote
            ? Results.Ok(new QuoteDTO(quote))     //without DTO is just Results.Ok(quote)
            : Results.NotFound());

app.MapGet("/quotes/search/{query}", async (string query, QuotesDbContext db) =>                                 // Return list of quotes from query string
{
    var _selectedQuotes = db.Quotes.Where(x => x.Content.ToLower().Contains(query.ToLower())).ToList();
    return _selectedQuotes.Count>0? Results.Ok(_selectedQuotes): Results.NotFound(Array.Empty<Quote>());
})
.Produces<List<QuoteDTO>>(StatusCodes.Status200OK)
.WithName("Search").WithTags("Getters");

app.MapPut("/quotes/{id}", async (int id, QuoteDTO quoteDTO, QuotesDbContext db) =>                                // Update a quote by Id (PUT) //
{
    var quote = await db.Quotes.FindAsync(id); //looks in db for quote by Id, then points to it

    if (quote is null) return Results.NotFound(); //if Id doesnt exist, 404 response

    quote.Content = quoteDTO.Content;
    quote.Source = quoteDTO.Source;
    quote.SubSource = quoteDTO.SubSource;

    await db.SaveChangesAsync();

    return Results.NoContent(); 
    //this Results.NoContent() is the reason it returns a 204 response rather than the content modified like in POST ??
});

app.MapDelete("/quotes/{id}", async (int id, QuotesDbContext db) =>                                                // Delete a quote by Id //
{
    if (await db.Quotes.FindAsync(id) is Quote quote) //if it can find and point to the quote by it's id:
    {
        db.Quotes.Remove(quote); //remove it
        await db.SaveChangesAsync(); //save changes to the db
        return Results.Ok(new QuoteDTO(quote)); //return only the DTO details of what was deleted
    }

    return Results.NotFound(); //else 404 response
});
// =============================================================================

app.Run();

