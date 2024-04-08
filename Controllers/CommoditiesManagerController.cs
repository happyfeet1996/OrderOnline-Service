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
        public IActionResult Post([FromQuery] CommodityDto commodityDto)
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
        public IActionResult Post([FromQuery] CommodityDto2 commodityDto)
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
    }
}
