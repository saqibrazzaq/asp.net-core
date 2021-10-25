using Microsoft.AspNetCore.Mvc.Rendering;
using Rocky;
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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<SelectListItem> GetAllDropdownList(string entity)
        {
            // Category dropdown
            if (entity.Equals(WC.CategoryName))
            {
                return _db.Categories.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
            }
            // Application type dropdown
            else if (entity.Equals(WC.ApplicationTypeName))
            {
                return _db.ApplicationTypes.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });
            }
            else
            {
                return null;
            }
        }

        public void Update(Product product)
        {
            _db.Products.Update(product);
        }
    }
}
