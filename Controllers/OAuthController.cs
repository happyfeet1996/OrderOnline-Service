using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrderOnline.Controllers
{
    [Route("api/oauth")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        public OAuthController(IMemoryCache cache) 
        {
            _cache = cache;
        }

        [HttpGet]
        [Route("token")]
        public ActionResult GetToken(string phoneNumber, string captchaId, string password, string captcha)
        {
            /*var localCaptcha = HttpContext.Session.GetString(captchaId + "_Captcha");*/
            if (_cache.TryGetValue(captchaId + "_Captcha", out string value))
            {
                if (value != captcha)
                {
                    _cache.Remove(captchaId + "_Captcha");
                    /*HttpContext.Session.Remove(captchaId + "_Captcha");*/
                    return Ok(new Result
                    {
                        Code = 503,
                        Success = false,
                        Message = "Invalid Captcha."
                    });
                }
                Result checkRes = UsersManager.CheckLogin(phoneNumber, password);
                _cache.Remove(captchaId + "_Captcha");
                /*HttpContext.Session.Remove(captchaId + "_Captcha");*/
                if (!checkRes.Success)
                {
                    return Ok(checkRes);
                }
                string pn = (checkRes as ResultWithDataAndToken<UserResult>).Data.PhoneNumber;
                var claims = new[]
                {
                new Claim(ClaimTypes.MobilePhone, pn),
                new Claim(ClaimTypes.Role, "")
            };
                string token = GenerateToken(claims);
                string refreshToken = GenerateRefreshToken(phoneNumber);
                (checkRes as ResultWithDataAndToken<UserResult>).Token = token;
                (checkRes as ResultWithDataAndToken<UserResult>).RefreshToken = refreshToken;
                return Ok(checkRes);
            }
            _cache.Remove(captchaId + "_Captcha");
            /*HttpContext.Session.Remove(captchaId + "_Captcha");*/
            return Ok(new Result
            {
                Code = 501,
                Success = false,
            });
        }

        private static string GenerateToken(Claim[] claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenParameter.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtToken = new JwtSecurityToken(TokenParameter.Issuer, TokenParameter.Audience, claims, expires: DateTime.UtcNow.AddMinutes(TokenParameter.AccessExpiration), signingCredentials: credentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return token;
        }

        private static string GenerateRefreshToken(string phoneNumber)
        {
            Guid newGuid = Guid.NewGuid();
            string guid = newGuid.ToString();
            string refreshToken = guid + "_" + phoneNumber;
            UsersManager.WriteRefreshToken(phoneNumber, refreshToken);
            return refreshToken;
        }

        [HttpGet]
        [Route("refreshToken")]
        public ActionResult GetRefreshToken(string oldToken,string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(oldToken);
            var phoneNumber = principal.Claims.First().Value;
            if (principal == null||phoneNumber == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            var refreshTokenRes = UsersManager.GetRefreshToken(phoneNumber);
            if (refreshTokenRes == null||(refreshToken != refreshTokenRes))
            {
                return Unauthorized(new { message = "Invalid token" });
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.MobilePhone, phoneNumber),
                new Claim(ClaimTypes.Role, "")
            };
            // 生成新的 token
            string newToken = GenerateToken(claims);
            string newRefreshToken = GenerateRefreshToken(phoneNumber);

            return Ok(new {token = newToken,refreshToken = newRefreshToken});
        }

        [HttpGet]
        [Route("getCaptcha")]
        public IActionResult GetCaptcha(string captchaId, int width, int height, int count)
        {
            try
            {
                CaptchaGenerator generator = new CaptchaGenerator();
                string captchaText = generator.GenerateRandomText(count);
                string captchaStr = generator.GenerateCaptcha(width, height, captchaText);
                _cache.Remove(captchaId + "_Captcha");
                _cache.Set(captchaId + "_Captcha", captchaText, TimeSpan.FromMinutes(3));
                /*HttpContext.Session.Remove(captchaId + "_Captcha");
                HttpContext.Session.SetString(captchaId + "_Captcha", captchaText);*/
                return Ok(new {src = $"data:image/png;base64,{captchaStr}" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getRegisterCaptcha")]
        public IActionResult GetRegisterCaptcha(string captchaId, int width, int height, int count)
        {
            try
            {
                CaptchaGenerator generator = new CaptchaGenerator();
                string captchaText = generator.GenerateRandomText(count);
                string captchaStr = generator.GenerateCaptcha(width, height, captchaText);
                _cache.Remove(captchaId + "_RCaptcha");
                _cache.Set(captchaId + "_RCaptcha", captchaText, TimeSpan.FromMinutes(3));
                /*HttpContext.Session.Remove(captchaId + "_RCaptcha");
                HttpContext.Session.SetString(captchaId + "_RCaptcha", captchaText);*/
                return Ok(new { src = $"data:image/png;base64,{captchaStr}" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("registerUser")]
        public IActionResult PostRegisterUser([FromBody] UserDto userDto)
        {
            /*var localCaptcha = HttpContext.Session.GetString(userDto.captchaId + "_RCaptcha");*/
            if (_cache.TryGetValue(userDto.captchaId + "_RCaptcha", out string value))
            {
                if(value != userDto.captcha)
                {
                    return Ok(new Result
                    {
                        Success = false,
                        Code = 501,
                        Message = "Invalid Captcha!"
                    });
                }
                try
                {
                    Result res = UsersManager.RegisterUser(userDto);
                    return Ok(res);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            return Ok(new Result
            {
                Success = false,
                Code = 501,
                Message = "Invalid Captcha!"
            });
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();


            try
            {
                // 解析 token
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                // 从 token 中提取用户信息
                var claims = jwtToken.Claims;

                // 构造 ClaimsIdentity
                var identity = new ClaimsIdentity(claims, "jwt");

                // 构造 ClaimsPrincipal
                var principal = new ClaimsPrincipal(identity);

                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
