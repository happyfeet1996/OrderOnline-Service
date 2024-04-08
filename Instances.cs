using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OrderOnline
{
    public class Commodity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        [NotMapped]
        public string[] ImagePaths { get; set; } = { };

        [Column("ImagePaths")]
        public string ImagePathsJson
        {
            get => ImagePaths != null ? JsonSerializer.Serialize(ImagePaths) : null;
            set => ImagePaths = value != null ? JsonSerializer.Deserialize<string[]>(value) : null;
        }
        public int Sales {  get; set; }  //销量
        public int Inventory { get; set; }  //库存
    }

    public class CommodityDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        [NotMapped]
        public string[] ImagePaths { get; set; } = { };
        public int Sales { get; set; }  //销量
        public int Inventory { get; set; }  //库存
    }

    public class CommodityDto2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        [NotMapped]
        public string[] ImagePaths { get; set; } = { };
        public int Sales { get; set; }  //销量
        public int Inventory { get; set; }  //库存
    }

    public class User
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Adress { get; set; }
    }

    public class UserDto
    {
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Adress { get; set; }
    }

    public class Result
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public dynamic? Data { get; set; }
        public bool Success { get; set; }
    }

}
