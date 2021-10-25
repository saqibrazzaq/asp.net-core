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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Category category)
        {
            // Find category in db
            var dbCategory = base.FirstOrDefault(x => x.Id == category.Id);

            // If found, then update
            if (dbCategory != null)
            {
                dbCategory.Name = category.Name;
                dbCategory.DisplayOrder = category.DisplayOrder;
            }
        }
    }
}
