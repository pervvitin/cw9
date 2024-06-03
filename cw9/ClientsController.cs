using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using cw9;
using cw9.Models;
using TravelAPI.Models;

namespace TravelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public ClientsController(TravelDbContext context)
        {
            _context = context;
        }

        // DELETE: api/clients/{idClient}
        [HttpDelete("{idClient}")]
        public async Task<ActionResult> DeleteClient(int idClient)
        {
            var client = await _context.Clients
                .Include(c => c.ClientTrips)
                .FirstOrDefaultAsync(c => c.IdClient == idClient);

            if (client == null)
            {
                return NotFound("Client not found.");
            }

            if (client.ClientTrips.Any())
            {
                return BadRequest("Client has assigned trips.");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/trips/{idTrip}/clients
        [HttpPost("trips/{idTrip}/clients")]
        public async Task<ActionResult> AddClientToTrip(int idTrip, [FromBody] ClientTripRequest request)
        {
            if (await _context.Clients.AnyAsync(c => c.Pesel == request.Pesel))
            {
                return BadRequest("Client with this PESEL already exists.");
            }

            if (await _context.ClientTrips.AnyAsync(ct => ct.IdClientNavigation.Pesel == request.Pesel && ct.IdTrip == idTrip))
            {
                return BadRequest("Client already registered for this trip.");
            }

            var trip = await _context.Trips.FindAsync(idTrip);
            if (trip == null || trip.DateFrom <= DateTime.Now)
            {
                return BadRequest("Trip not found or already started.");
            }

            var client = new Client
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Telephone = request.Telephone,
                Pesel = request.Pesel
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            var clientTrip = new ClientTrip
            {
                IdClient = client.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = request.PaymentDate
            };

            _context.ClientTrips.Add(clientTrip);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
