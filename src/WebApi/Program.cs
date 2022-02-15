var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddDbContext<HotelDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));

});
var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db  = scope.ServiceProvider.GetRequiredService<HotelDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/hotels", async (IHotelRepository repository) => 
    Results.Extensions.Xml(await repository.GetHotelsAsync()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");

app.MapGet("/hotels/{id}", async (int id, IHotelRepository repository) => 
    await repository.GetHotelByIdAsync(id) is Hotel hotel 
    ? Results.Ok(hotel) 
    : Results.NotFound())
    .Produces<Hotel>(StatusCodes.Status200OK)
    .WithName("GetHotel")
    .WithTags("Getters");

app.MapPost("/hotels",async ([FromBody] Hotel hotel, IHotelRepository repository) =>
    {
        await repository.InsertHotelAsync(hotel);
        await repository.SaveAsync();

        return Results.Created($"/hotels/{hotel.Id}", hotel);
    })
    .Accepts<Hotel>("application/json")
    .Produces<Hotel>(StatusCodes.Status201Created)
    .WithName("CreateHotel")
    .WithTags("Creators");

app.MapPut("/hotels", async([FromBody] Hotel hotel, IHotelRepository repository) =>
    {
        await repository.UpdateHotelAsync(hotel);
        await repository.SaveAsync();

        return Results.NoContent();
    })
    .Accepts<Hotel>("application/json")
    .WithName("UpdateHotel")
    .WithTags("Updaters");

app.MapDelete("/hotel/{id}", async (int id, IHotelRepository repository) =>
    {
        await repository.DeleteHotelAsync(id);
        await repository.SaveAsync();

        return Results.NoContent();
    })
    .WithName("DeleteHotel")
    .WithTags("Deleters");

app.MapGet("/hotels/search/name/{query}", async (string query, IHotelRepository repository) =>
    await repository.GetHotelsAsync(query) is IEnumerable<Hotel> hotels
    ? Results.Ok(hotels)
    : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotels")
    .WithTags("Getters")
    .ExcludeFromDescription();

app.MapGet("/hotels/search/location/{coordinate}", async (Coordinate coordinate, IHotelRepository repository) =>
    await repository.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
    ? Results.Ok(hotels)
    : Results.NotFound(Array.Empty<Hotel>()))
    .ExcludeFromDescription();

app.UseHttpsRedirection();
app.Run();


public record Coordinate(double Latitude, double Longitude)
{
    public static bool TryParse(string input, out Coordinate? coordinate)
    {
        coordinate = default;
        var splitArray = input.Split(',', 2);
        if (splitArray.Length != 2) return false;
        if(!double.TryParse(splitArray[0], out var lat)) return false;
        if (!double.TryParse(splitArray[1], out var lon)) return false;

        coordinate = new Coordinate(lat, lon);
       
        return true;
    }

    public static async ValueTask<Coordinate> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    {
        var input = httpContext.GetRouteValue(parameter.Name!) as string ?? string.Empty;
        TryParse(input, out var coordinate);

        return coordinate;
    }
}