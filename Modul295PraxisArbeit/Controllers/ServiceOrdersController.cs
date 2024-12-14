using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modul295PraxisArbeit.Data;
using Modul295PraxisArbeit.Models;

namespace Modul295PraxisArbeit.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceOrder>>> GetServiceOrders()
        {
            return await _context.ServiceOrders.ToListAsync();
        }

        // GET: api/ServiceOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceOrder>> GetServiceOrder(int id)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);

            if (serviceOrder == null)
            {
                return NotFound();
            }

            return serviceOrder;
        }

        // POST: api/ServiceOrders
        [HttpPost]
        public async Task<ActionResult<ServiceOrder>> PostServiceOrder(ServiceOrder serviceOrder)
        {
            // Set default values for Status and AssignedUserId if not provided
            if (string.IsNullOrEmpty(serviceOrder.Status))
            {
                serviceOrder.Status = "Offen"; // Set default status to "Offen"
            }

            var userName = HttpContext.User.Identity.Name; // Normalerweise der Benutzername
            Console.WriteLine($"Name: {userName}");


            if (serviceOrder.AssignedUserId == null)
            {
                serviceOrder.AssignedUserId = null; // Ensure the AssignedUserId is null for new orders
            }

            // Validierung hinzufügen (optional)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ServiceOrders.Add(serviceOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServiceOrder), new { id = serviceOrder.OrderId }, serviceOrder);
        }

        // PUT: api/ServiceOrders/5
        [Authorize] // Autorisierung erforderlich
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceOrder(int id, ServiceOrder serviceOrder)
        {
            var dbService = await _context.ServiceOrders.Include(s => s.AssignedUser).FirstOrDefaultAsync(s => s.OrderId == id);
            //FindAsync(id);
            if (dbService == null)
            {
                return NotFound();
            }

            dbService.name = serviceOrder.name;
            dbService.email = serviceOrder.email;
            dbService.phone = serviceOrder.phone;
            dbService.priority = serviceOrder.priority;
            dbService.service = serviceOrder.service;
            //dbService.assignedUser = serviceOrder.assignedUser;
          
            // Update the AssignedUser if the AssignedUserId has been provided
            if (serviceOrder.AssignedUserId.HasValue)
            {
                var assignedUser = await _context.Users.FindAsync(serviceOrder.AssignedUserId.Value);
                if (assignedUser != null)
                {
                    dbService.AssignedUser = assignedUser; // Assign the user to the service order
                }
            }
            else
            {
                dbService.AssignedUser = null; // If no AssignedUserId, set AssignedUser to null
            }

            // Update the status if provided
            if (!string.IsNullOrEmpty(serviceOrder.Status))
            {
                dbService.Status = serviceOrder.Status; // Set the new status
            }


            _context.Entry(dbService).State = EntityState.Modified;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceOrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ServiceOrders/5
        [Authorize] // Autorisierung erforderlich
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceOrder(int id)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder == null)
            {
                return NotFound();
            }

            _context.ServiceOrders.Remove(serviceOrder);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // PUT: api/ServiceOrders/Claim/5
        [Authorize] // Authorization required
        [HttpPut("Claim/{id}")]
        public async Task<IActionResult> ClaimServiceOrder(int id, int userId)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder == null)
            {
                return NotFound();
            }

            // Ensure the order is still "Offen" before claiming
            if (serviceOrder.Status != "Offen")
            {
                return BadRequest("Only open orders can be claimed.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("Invalid user ID.");
            }

            // Update the status and assign the user
            serviceOrder.AssignedUserId = userId;
            serviceOrder.AssignedUser = user;
            serviceOrder.Status = "InArbeit"; // Set the status to "InArbeit"

            _context.Entry(serviceOrder).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Successfully claimed
        }

        // PUT: api/ServiceOrders/Finish/5
        [Authorize] // Authorization required
        [HttpPut("Finish/{id}")]
        public async Task<IActionResult> FinishServiceOrder(int id)
        {
            var serviceOrder = await _context.ServiceOrders.FindAsync(id);
            if (serviceOrder == null)
            {
                return NotFound();
            }

            // Ensure the order is assigned before finishing
            if (serviceOrder.AssignedUserId == null)
            {
                return BadRequest("Only claimed orders can be finished.");
            }

            // Update the status and clear the assigned user
            serviceOrder.AssignedUserId = null;
            serviceOrder.AssignedUser = null;
            serviceOrder.Status = "Abgeschlossen"; // Set the status to "Abgeschlossen"

            _context.Entry(serviceOrder).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent(); // Successfully finished
        }

        // Helper method to check if a ServiceOrder exists
        private bool ServiceOrderExists(int id)
        {
            return _context.ServiceOrders.Any(e => e.OrderId == id);
        }


        /*
        // Helper-Methode, um zu prüfen, ob ein ServiceOrder existiert
        private bool ServiceOrderExists(int id)
        {
            return _context.ServiceOrders.Any(e => e.OrderId == id);
        }
        */
    }
}


/* 
    ********************************************************************
    TO DO:
    ********************************************************************
    - Struktur 
    - Datenbank Limit ändern von Max auf 50
    - Frontend Besser Gestalten
    - Token Funktionen überprüfen 
    - Rollen zu Bestimmten Usern Zu teilen 
    - Email Hinzufügen bei register/ Login 

*/