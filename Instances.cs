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
        public string? RefreshToken { get; set; }
    }

    public class Admin
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? RefreshToken { get; set;}
    }

    public class UserResult
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Adress { get; set; }
    }

    public class AdminResult
    {
        public int Id { get; set; }
        public string UserName { get; set; }
    }

    public class UserDto
    {
        public string captchaId { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Adress { get; set; }
        public string captcha {  get; set; }
    }

    public class Result
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }
    }

    public class ResultWithData<T>: Result
    {
        public T? Data { get; set; }
    }

    public class ResultWithDataAndToken<T>: Result
    {
        public T? Data { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class ResultWithToken: Result
    {
        public string? Token { get; set; }
    }

    public enum OrderStatus
    {
        New = 0,
        Processing = 1,
        Finished = 2,
        Canceled = 3
    }

    public class Order { 
        public string Id { get; set; }
        public OrderStatus Status { get; set; }
        public int CustomerId { get; set; }
        public DateTime Date {  get; set; }
    }

    public class OrderDetails
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public int CommodityId { get; set; }
        public decimal Count { get; set; }
    }

    public class OrderDetailsDto
    {
        public int CommodityId { get; set; }
        public decimal Count { get; set; }
    }

}
