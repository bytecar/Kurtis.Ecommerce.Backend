
using Microsoft.AspNetCore.Mvc;
using Kurtis.Common.Models;
using Kurtis.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kurtis.Api.Catalog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly KurtisDbContext _db;
        public CategoriesController(KurtisDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _db.Categories.AsNoTracking().ToListAsync());

        [Authorize(Roles="admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category model)
        {
            _db.Categories.Add(model); await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c==null) return NotFound();
            return Ok(c);
        }
    }
}
