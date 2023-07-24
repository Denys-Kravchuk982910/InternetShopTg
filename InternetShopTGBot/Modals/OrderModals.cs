using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetShopTGBot.Modals
{
    public class OrderModal
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string ParentName { get; set; }
        public string Post { get; set; }
        public string ProductName { get; set; }
        public string ProductBrand { get; set; }
        public int ProductSize { get; set; }
    }
}
