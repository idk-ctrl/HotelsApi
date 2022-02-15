namespace WebApi.Data;

public interface IHotelRepository : IDisposable
{
    Task<List<Hotel>> GetHotelsAsync();
    Task<List<Hotel>> GetHotelsAsync(string name);
    Task<List<Hotel>> GetHotelsAsync(Coordinate coordinate);
    Task<Hotel> GetHotelByIdAsync(int id);
    Task InsertHotelAsync(Hotel hotel);
    Task UpdateHotelAsync(Hotel hotel);
    Task DeleteHotelAsync(int id);
    Task SaveAsync();
}

public class HotelRepository : IHotelRepository
{
    private readonly HotelDb _context;

    public HotelRepository(HotelDb context)
    {
        _context = context;
    }

    public async Task<List<Hotel>> GetHotelsAsync() => 
        await _context.Hotels.ToListAsync();

    public async Task<List<Hotel>> GetHotelsAsync(string name) => 
        await _context.Hotels.Where(x=>x.Name.Contains(name)).ToListAsync();
    
    public async Task<List<Hotel>> GetHotelsAsync(Coordinate coordinate) => 
        await _context.Hotels.Where(hotel =>
            hotel.Latitude > coordinate.Latitude - 1 &&
            hotel.Longitude < coordinate.Longitude + 1 &&
            hotel.Longitude > coordinate.Longitude - 1 &&
            hotel.Latitude < coordinate.Latitude + 1
        ).ToListAsync();

    public async Task<Hotel> GetHotelByIdAsync(int id) => await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id);

    public async Task InsertHotelAsync(Hotel hotel) =>await _context.Hotels.AddAsync(hotel);
   

    public async Task UpdateHotelAsync(Hotel hotel)
    {
        var hotelFromDb = await _context.Hotels.FindAsync(new object[] { hotel.Id});

        if (hotelFromDb == null) return;

        hotelFromDb.Latitude = hotel.Latitude;
        hotelFromDb.Longitude = hotel.Longitude;
        hotelFromDb.Name = hotel.Name;
    }

    public async Task DeleteHotelAsync(int id)
    {
        var hotelFromDb = await _context.Hotels.FindAsync(new object[] { id });

        if (hotelFromDb == null) return;

        _context.Hotels.Remove(hotelFromDb);
    }

    public async Task SaveAsync() =>await _context.SaveChangesAsync();


    private bool _disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if(disposing)
            {
                _context.Dispose();
            }
        }

        _disposed = true;
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}