using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetShopTGBot.Modals
{
    public class AddProductModal
    {
        public string Title { get; set; } = "";
        public double Price { get; set; }
        public string Description { get; set; } = "";
        public int Count { get; set; }
        public string Brand { get; set; }
    }

    public class EditProductModal
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public string Brand { get; set; }
    }

    public class ProductId
    {
        public int Id { get; set; }
        public string Message { get; set; }
    }

    public class ProductImage
    {
        public string ImageBase64 { get; set; }
        public int ProductId { get; set; }
    }

    public class ProductFilter
    {
        public int FilterId { get; set; }
        public int ProductId { get; set; }
    }

    public class ProductItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public int Rating { get; set; }
        public string Brand { get; set; }
        public List<ProductItemImage> Images { get; set; }
    }

    public class DeleteProduct
    {
        public int Id { get; set; }
    }

    public class DeleteProductMessage
    {
        public string Message { get; set; }
    }

    public class ProductItemImage
    {
        public string Image { get; set; }
        public int Id { get; set; }
        public int ProductId { get; set; }
    }



}
