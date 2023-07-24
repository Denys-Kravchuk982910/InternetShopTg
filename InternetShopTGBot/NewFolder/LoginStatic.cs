using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetShopTGBot.NewFolder
{
    public static class LoginStatic
    {
        public static string Login { get; set; }
        public static string Password { get; set; }
        public static bool IsLogin { get; set; } = false;
        public static string Token { get; set; }
    }

    public static class UserDataStatic
    {
        public static List<PersonInfo> People { get; set; } = new List<PersonInfo>();
    }

    public static class UrlInfo
    {
        public static string URL = "https://backend.crosshoprv.live/";
    }

    public class PersonInfo
    {
        public long UserId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public bool IsLogin { get; set; }
        public bool IsAddProduct { get; set; } = false;
        public bool IsAddProductImages { get; set; } = false;
        public bool IsGetAllProduct { get; set; } = false;
        public bool IsDeleteProduct { get; set; } = false;
        public bool IsEditProduct { get; set; } = false;
        public bool IsAddFilter { get; set; } = false;
        public bool IsAcceptOrder { get; set; } = false;
        public bool IsAddFilterManually { get; set; } = false;
        public bool IsDeleteFilter { get; set; } = false;
        public bool IsAddStory { get; set; } = false;
        public bool IsAddPost { get; set; } = false;
        public bool IsDelPost { get; set; } = false;
        public bool IsDelStory { get; set; } = false;

        public ProductInfo Product { get; set; } = null;

        public AcceptModal Accept { get; set; } = null;

        public AddFilterModal AddFilter { get; set; } = null;

        public AddStoryModal StoryModal { get; set; } = null;
        public AddPostModal AddPostModal { get; set; }

    }

    public class ProductInfo
    {
        public int Id { get; set; } = 0;
        public string Title { get; set; }
        public string Description { get; set; }
        public int Price { get; set; } = 0;
        public int Count { get; set; }
        public string Brand { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public string Filters { get; set; } = "";
    }

    public class AcceptModal
    {
        public bool Accept { get; set; }
        public int Id { get; set; }
    }

    public class AddFilterModal
    {
        public int ParentId { get; set; }
        public string Title { get; set; }
    }

    public class AddStoryModal
    {
        public string Image { get; set; }
        public string Title { get; set; }
    }

    public class AddPostModal
    {
        public string Image { get; set; }
    }
} 
