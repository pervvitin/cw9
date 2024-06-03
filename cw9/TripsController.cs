using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using cw9.Models;
using TravelAPI.Models;

namespace TravelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public TripsController(TravelDbContext context)
        {
            _context = context;
        }

        // GET: api/trips
        [HttpGet]
        public async Task<ActionResult> GetTrips(int page = 1, int pageSize = 10)
        {
            var trips = await _context.Trips
                .OrderByDescending(t => t.DateFrom)
                .Include(t => t.CountryTrips)
                .ThenInclude(ct => ct.IdCountryNavigation)
                .Include(t => t.ClientTrips)
                .ThenInclude(ct => ct.IdClientNavigation)
                .ToListAsync();

            var pagedTrips = trips
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Name,
                    t.Description,
                    DateFrom = t.DateFrom.ToString("yyyy-MM-dd"),
                    DateTo = t.DateTo.ToString("yyyy-MM-dd"),
                    t.MaxPeople,
                    Countries = t.CountryTrips.Select(ct => new { ct.IdCountryNavigation.Name }),
                    Clients = t.ClientTrips.Select(ct => new { ct.IdClientNavigation.FirstName, ct.IdClientNavigation.LastName })
                });

            return Ok(new
            {
                pageNum = page,
                pageSize = pageSize,
                allPages = (int)Math.Ceiling((double)trips.Count / pageSize),
                trips = pagedTrips
            });
        }
    }
}