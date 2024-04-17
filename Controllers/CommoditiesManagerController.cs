using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace OrderOnline.Controllers
{
    [Route("api/commoditiesManager")]
    [ApiController]
    //[Authorize]
    public class CommoditiesManagerController : ControllerBase
    {
        [HttpGet]
        [Route("checkName")]
        public IActionResult Get(string name)
        {
            return Ok(CommoditiesManager.CheckName(name));
        }

        [HttpPost]
        [Route("addCommodity")]
        public IActionResult Post([FromBody] CommodityDto commodityDto)
        {
            var commodity = new Commodity 
            { 
                Name = commodityDto.Name,
                Description = commodityDto.Description,
                Price = commodityDto.Price,
                ImagePaths = commodityDto.ImagePaths,
                Inventory = commodityDto.Inventory,
                Sales = commodityDto.Sales,
            };
            try
            {
                CommoditiesManager.AddCommodity(commodity);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("modifyCommodity")]
        public IActionResult Post([FromBody] CommodityDto2 commodityDto)
        {
            var commodity = new Commodity
            {
                Id = commodityDto.Id,
                Name = commodityDto.Name,
                Description = commodityDto.Description,
                Price = commodityDto.Price,
                ImagePaths = commodityDto.ImagePaths,
                Inventory = commodityDto.Inventory,
                Sales = commodityDto.Sales,
            };
            try
            {
                CommoditiesManager.ModifyCommodity(commodity);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("deleteCommodity")]
        public IActionResult Delete(int id)
        {
            try
            {
                CommoditiesManager.DeleteCommodity(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getCommodities")]
        public IActionResult Get(int skip, int limit) 
        {
            try
            {
                var res = CommoditiesManager.GetCommodities(limit, skip);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("uploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            try
            {
                string uploadsFolder = DataManager.GetImagesPath();
                Directory.CreateDirectory(uploadsFolder);

                // 生成唯一文件名
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                // 保存文件到服务器文件系统中
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return Ok(new { filePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("getImage")]
        public IActionResult GetImage(string imagePath)
        {
            try
            {
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, "image/" + imagePath.Split(".").Last()); // 返回图片文件
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }
    }
}
