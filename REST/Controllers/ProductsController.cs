using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace REST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [HttpGet("books")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IEnumerable<string> GetAllBooks()
        {
            return new List<string>() {"Book 1", "Book 2", "Book 3" };
        }
        [HttpGet("pencils")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IEnumerable<string> GetAllPencils()
        {
            return new List<string>() { "Pencil 1", "Pencil 2", "Pencil 3" };
        }
    }
}
