using Rocky.Data;
using Rocky.Models;
using Rocky_DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky_DataAccess.Repository
{
    public class ApplicationTypeRepository : Repository<ApplicationType>, IApplicationTypeRepository
    {
        private readonly ApplicationDbContext _db;
        public ApplicationTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ApplicationType applicationType)
        {
            // Find application type in db
            var dbApplicationType = base.FirstOrDefault(x => x.Id == applicationType.Id);

            // Update if found
            if (dbApplicationType != null)
            {
                dbApplicationType.Name = applicationType.Name;
            }
        }
    }
}
