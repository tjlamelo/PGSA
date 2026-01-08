using PGSA_Licence3.Models;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Data;

namespace PGSA_Licence3.Services.Students
{
    public class JustificationStudentsService
    {
        private readonly ApplicationDbContext _db;

        public JustificationStudentsService(ApplicationDbContext db)
        {
            _db = db;
        }
    }
}