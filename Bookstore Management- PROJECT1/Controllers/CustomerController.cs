using Bookstore_Management__PROJECT1.Data;
using Bookstore_Management__PROJECT1.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Bookstore_Management__PROJECT1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly BookstoreDbContext customerdbcontext;
        private readonly SieveProcessor sieveProcessor;

        public CustomerController(BookstoreDbContext customerdbcontext, SieveProcessor sieveProcessor, ILogger<CustomerController> _logger)

        {
            this.customerdbcontext = customerdbcontext;
            this.sieveProcessor = sieveProcessor;
            this._logger = _logger;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers([FromQuery] SieveModel sieveModel)
        {
            var customers = customerdbcontext.Customer.AsQueryable();
            customers = sieveProcessor.Apply(sieveModel, customers);

            return await customers.ToListAsync();
        }

        // GET /api/customers/{id} - Get customer by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await customerdbcontext.Customer.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // Return validation errors
                }

                var existingCustomer = await customerdbcontext.Customer.FirstOrDefaultAsync(c => c.Email == customer.Email);
                if (existingCustomer != null)
                {
                    ModelState.AddModelError("Email", "A customer with this email already exists.");
                    return BadRequest(ModelState); // Return duplicate email error
                }

                customerdbcontext.Customer.Add(customer);
                await customerdbcontext.SaveChangesAsync(); // Save changes to the database

                return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer); // Return success response
            }
            catch (DbUpdateException ex)
            {
                // Log detailed database update error information
                _logger.LogError($"Database update error: {ex.InnerException?.Message}");

                // Return a 500 Internal Server Error with specific message
                return StatusCode(500, "Failed to update database. Please contact support.");
            }
            catch (Exception ex)
            {
                // Log detailed general exception information
                _logger.LogError($"Failed to create customer: {ex.Message}\n{ex.StackTrace}");

                // Return a 500 Internal Server Error status with a more descriptive message
                return StatusCode(500, $"Internal server error occurred: {ex.Message}. Please try again later.");
            }
        }




        // PUT /api/customers/{id} - Update a customer
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            customerdbcontext.Entry(customer).State = EntityState.Modified;

            try
            {
                await customerdbcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // DELETE /api/customers/{id} - Delete a customer
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var customer = await customerdbcontext.Customer.FindAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }

                customerdbcontext.Customer.Remove(customer);
                await customerdbcontext.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                // Log the exception or handle it as needed
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the customer.");
            }
        }

        private bool CustomerExists(int id)
        {
            return customerdbcontext.Customer.Any(e => e.CustomerId == id);
        }
    }
}
    
