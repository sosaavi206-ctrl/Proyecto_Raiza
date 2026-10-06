using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class ClassKit_R : IClassKitI
    {
        private readonly DatabaseService _context;

        public ClassKit_R(DatabaseService context)
        {
            this._context = context;
        }

        public async Task<List<Class_Kit>> GetClassKits() =>
            await _context.Class_Kit.ToListAsync();

        public async Task<Class_Kit?> GetClassKitById(int id) =>
            await _context.Class_Kit.AsNoTracking().FirstOrDefaultAsync(c => c.idclass_kit == id);

        public async Task<bool> CreateClassKit(Class_Kit classKit)
        {
            await _context.Class_Kit.AddAsync(classKit);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateClassKit(Class_Kit classKit)
        {
            _context.Class_Kit.Update(classKit);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteClassKit(int id)
        {
            var classKit = await _context.Class_Kit.FirstOrDefaultAsync(c => c.idclass_kit == id);
            if (classKit == null) return false;

            _context.Class_Kit.Remove(classKit);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}