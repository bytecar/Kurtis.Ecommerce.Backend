using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kurtis.Common.Models;
using Kurtis.DAL;

namespace Kurtis.Api.Catalog.Controllers
{
/*    public class MetadatasController(KurtisDbContext context) : Controller
    {
        private readonly KurtisDbContext _context = context;

        public async Task<IActionResult?> GetColors()
        {
            return await _context.Metadatas.Select(p => p.Colors).ToListAsync() as IActionResult;
        }
        public async Task<IActionResult?> GetSizes()
        {
            return await _context.Metadatas.Select(p => p.Sizes).ToListAsync() as IActionResult;
        }
        public async Task<IActionResult?> GetRatings()
        {
            return await _context.Metadatas.Select(p => p.Reviews).ToListAsync() as IActionResult;
        }
        public async Task<IActionResult?> GetAllBrands()
        {
            return await _context.Metadatas.Select(p => p.Brands).ToListAsync() as IActionResult;
        }
        public async Task<IActionResult?> GetAllCategories()
        {
            return await _context.Metadatas.Select(p => p.Categories).ToListAsync() as IActionResult;
        }
        public async Task<IActionResult?> GetAllProducts()
        {
            return await _context.Metadatas.Select(p => p.Products).ToListAsync() as IActionResult;
        }
        // GET: Metadatas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var metadata = await _context.Metadatas.FirstOrDefaultAsync(m => m.Id == id);
            if (metadata == null)
            {
                return NotFound();
            }

            return View(metadata);
        }
        
        // POST: Metadatas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id")] Metadata metadata)
        {
            if (ModelState.IsValid)
            {
                _context.Add(metadata);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(metadata);
        }
        // POST: Metadatas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id")] Metadata metadata)
        {
            if (id != metadata.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(metadata);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MetadataExists(metadata.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(metadata);
        }

        // POST: Metadatas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var metadata = await _context.Metadatas.FindAsync(id);
            if (metadata != null)
            {
                _context.Metadatas.Remove(metadata);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MetadataExists(int id)
        {
            return _context.Metadatas.Any(e => e.Id == id);
        }
    }*/
}
