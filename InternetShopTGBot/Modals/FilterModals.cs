using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetShopTGBot.Modals
{
    public class FilterItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class FilterTree
    {
        public int Key { get; set; }
        public string Name { get; set; }
        public List<FilterItem> Items { get; set; }
    }

}
