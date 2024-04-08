using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderOnline.Controllers
{
    [Route("api/oauth")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        [HttpGet]
        [Route("token")]
        public ActionResult GetAccessToken(string phoneNumber, string password, string captcha)
        {
            var localCaptcha = HttpContext.Session.GetString(phoneNumber + "_Captcha");
            if(localCaptcha.IsNullOrEmpty())
            {
                return Ok(new Result
                {
                    Code = 501,
                    Success = false,
                });
            }
            if(localCaptcha != captcha)
            {
                return Ok(new Result
                {
                    Code = 503,
                    Success = false,
                    Message = "Invalid Captcha."
                });
            }

            if (username != "admin" || password != "admin")
            {
                return BadRequest("Invalid Request");
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "")
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenParameter.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(TokenParameter.Issuer, TokenParameter.Audience, claims, expires: DateTime.UtcNow.AddMinutes(TokenParameter.AccessExpiration), signingCredentials: credentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return Ok(token);
        }

        [HttpGet]
        [Route("getCaptcha")]
        public IActionResult Get(string phoneNm, int width, int height, int count)
        {
            try
            {
                CaptchaGenerator generator = new CaptchaGenerator();
                string captchaText = generator.GenerateRandomText(count);
                string captchaStr = generator.GenerateCaptcha(width, height, captchaText);
                HttpContext.Session.Remove(phoneNm + "_Captcha");
                HttpContext.Session.SetString(phoneNm + "_Captcha", captchaText);
                return Ok($"data:image/png;base64,{captchaStr}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        [Route("registerUser")]
        public IActionResult Post([FromQuery] UserDto userDto)
        {
            try
            {
                Result res = UsersManager.RegisterUser(userDto);
                return Ok(res);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
