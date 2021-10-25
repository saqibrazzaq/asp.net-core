using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Models.ViewModels
{
    public class ProductUserVM
    {
        public ProductUserVM()
        {
            // Initialize product list
            Products = new List<Product>();
        }
        public ApplicationUser ApplicationUser { get; set; }
        public IList<Product> Products { get; set; }
    }
}
