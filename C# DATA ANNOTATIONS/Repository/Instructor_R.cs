using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RAIZA.Interfaces;
using RAIZA.Models;
using RAIZA.Data;

namespace RAIZA.Repositories
{
    public class Instructor_R : IInstructor_I
    {
        private readonly DatabaseService _context;

        public Instructor_R(DatabaseService context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Instructor>> GetInstructores() =>
            await _context.Instructor.ToListAsync();

        public async Task<Instructor?> GetInstructorById(int id) =>
            await _context.Instructor.AsNoTracking().FirstOrDefaultAsync(i => i.idinstructor == id);

        public async Task<bool> CreateInstructor(Instructor instructor)
        {
            await _context.Instructor.AddAsync(instructor);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateInstructor(Instructor instructor)
        {
            _context.Instructor.Update(instructor);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteInstructor(int id)
        {
            var instructor = await _context.Instructor.FirstOrDefaultAsync(i => i.idinstructor == id);
            if (instructor == null) return false;

            _context.Instructor.Remove(instructor);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}