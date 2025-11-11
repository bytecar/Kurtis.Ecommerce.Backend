
using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL.Interfaces;
using Kurtis.DAL.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Kurtis.DAL;
using Microsoft.EntityFrameworkCore;

namespace Kurtis.Api.Catalog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandsController : ControllerBase
    {
        private readonly KurtisDbContext _db;
        public BrandsController(KurtisDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Brands.AsNoTracking().ToListAsync());

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Brand model)
        {
            _db.Brands.Add(model); await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var b = await _db.Brands.FindAsync(id);
            if (b==null) return NotFound();
            return Ok(b);
        }
    }
}
