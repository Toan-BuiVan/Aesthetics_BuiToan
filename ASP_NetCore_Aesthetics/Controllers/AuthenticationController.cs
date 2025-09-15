using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.TokenModel;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using BE_102024.DataAces.NetCore.DataOpject.RequestData;
using BE_102024.DataAces.NetCore.DataOpject.TokenModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASP_NetCore_Aesthetics.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private IAccountRepository _accountRepository;
		private IUserSessionRepository _userSession;
		private IConfiguration _configuration;
		private DB_Context _context;
		private readonly IDistributedCache _cache;
		private IUserRepository _userRepository;
		private readonly ILoggerManager _loggerManager;
		public AuthenticationController(IAccountRepository accountRepository,
			IConfiguration configuration, DB_Context context, IDistributedCache cache, 
			IUserSessionRepository userSession, IUserRepository userRepository, ILoggerManager loggerManager)
		{
			_accountRepository = accountRepository;
			_configuration = configuration;
			_context = context;
			_cache = cache;
			_userSession = userSession;
			_userRepository = userRepository;
			_loggerManager = loggerManager;
		}

		[HttpPost("Login_Account")]
		public async Task<IActionResult> Login_Account(AccountLoginRequestData loginRequestData)
		{
			var responseData = new UserLoginResponseData();
			try
			{
				var user = await _accountRepository.UserLogin(loginRequestData);
				if (user == null)
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Đăng Nhập Thất Bại, Vui Lòng Kiểm Tra Lại UserName || PassWord!";
					_loggerManager.LogWarn($"Response: Code={responseData.ResponseCode}, Message={responseData.ResposeMessage}");
					return Ok(responseData);
				}
				//Tạo Token
				var authClaims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(ClaimTypes.PrimarySid, user.UserID.ToString()),
					new Claim(ClaimTypes.Role, user.TypePerson ?? string.Empty),
					new Claim(ClaimTypes.Authentication, user.RefeshToken ?? string.Empty)
				};

				//Lưu RefreshToken
				var newToken = await _accountRepository.CreateToken(authClaims);
				_ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
				var refeshToken = await _accountRepository.GenerateRefreshToken();
				await _accountRepository.UserUpdate_RefeshToken(user.UserID, refeshToken, DateTime.Now.AddDays(refreshTokenValidityInDays));

				//Lấy tên thiết bị
				var DeviceName = await _accountRepository.GetDeviceName();

				//Lấy địa chỉ IP
				var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;

				//Lưu vào RedisCaching
				var cachKey = "User_" + user.UserID + "-" + DeviceName;

				//Lưu vào db
				var user_Session = new Aesthetics.DTO.NetCore.DataObject.Model.UserSession
				{
					UserID = user.UserID,
					Token = new JwtSecurityTokenHandler().WriteToken(newToken),
					DeviceName = DeviceName,
					Ip = remoteIpAddress.ToString(),
					CreateTime = DateTime.Now,
					DeleteStatus = 1
				};
				await _userSession.Insert_Sesion(user_Session);

				//Xét vào caching
				var user_SessionCach = new Aesthetics.DTO.NetCore.DataObject.Model.UserSession
				{
					UserID = user.UserID,
					Token = new JwtSecurityTokenHandler().WriteToken(newToken),
					DeviceName = DeviceName,
					Ip = remoteIpAddress.ToString(),
					CreateTime = DateTime.Now
				};
				//1.Chuyển thành dạng Json
				var dataCachingJson = JsonConvert.SerializeObject(user_SessionCach);

				//2.Chuyển dataCachingJson thành byte 
				var dataToCache = Encoding.UTF8.GetBytes(dataCachingJson);

				//3.Xét thời gian sống của Token trong Caching 
				DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
			   .SetAbsoluteExpiration(DateTime.Now.AddMinutes(3));

				_cache.Set(cachKey, dataToCache, options);

				//Trả về token & thông tin
				responseData.ResponseCode = 1;
				responseData.ResposeMessage = "Đăng nhập thành công!";
				responseData.DeviceName = DeviceName;
				responseData.UserID = user.UserID;
				responseData.TypePerson = user.TypePerson;
				responseData.UserName = user.UserName;
				responseData.Token = new JwtSecurityTokenHandler().WriteToken(newToken);
				responseData.RefreshToken = refeshToken;
				_loggerManager.LogInfo($"Login successful: {responseData}");
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				responseData.ResponseCode = -99;
				responseData.ResposeMessage = ex.Message;
				_loggerManager.LogError($"Login Exception Error: '{ex.Message}'. StackTrace: {ex.StackTrace}");
				return Ok(responseData);
			}
		}

		[HttpPost("Check_Token")]
		public async Task<IActionResult> Check_Token(CheckTokenRequestData requestData)
		{
			var responeData = new Aesthetics.DataAccess.NetCore.CheckConditions.Response.CheckTokenReponsData();
			try
			{
				//1.Kiểm tra thời hạn của token 
				var cachKey = "User_" + requestData.UserID + "-" + requestData.DeviceName;
				byte[] cacheData = await _cache.GetAsync(cachKey);
				if (cacheData == null)
				{
					//2. Kiểm tra thời hạn RefreshToken Expried Time:
					var userDetail = await _accountRepository.User_GetByID(requestData.UserID);
					if (userDetail == null)
					{
						responeData.ResponseCode = -1;
						responeData.ResposeMessage = $"Token của User: {requestData.UserID} không tồn tại!";
						return Ok(responeData);
					}
					//2.1 Hết hạn => đăng nhập lại
					if (userDetail.TokenExprired < DateTime.Now)
					{
						responeData.ResponseCode = -1;
						responeData.ResposeMessage = "Token hết hạn, Vui lòng đăng nhập lại!";
						_loggerManager.LogWarn($"Response: Code={responeData.ResponseCode}, Message={responeData.ResposeMessage}");
						return Ok(responeData);
					}

					//2.2 Còn hạn => Tạo Token mới:
					//2.2.1 Giải mã Token truyền lên để lấy claims
					var principal = await _accountRepository.GetPrincipalFromExpiredToken(requestData.AccessToken);
					if (principal == null)
					{
						responeData.ResponseCode = -1;
						responeData.ResposeMessage = "Token không hợp lệ!";
						_loggerManager.LogWarn($"Response: Code={responeData.ResponseCode}, Message={responeData.ResposeMessage}");
						return Ok(responeData);
					}
					//2.2.1 Check RefreshToken và ngày hết hạn
					var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(principal.FindFirst("exp").Value));
					DateTime result = exp.UtcDateTime.AddHours(7);
					string userName = principal.Identity.Name;

					//Gọi db lấy theo UserName
					var user = _accountRepository.GetUser_ByUserName(userName);

					//Nếu ngày hết hạn < thời gian hiện tại || RefreshToken truyền lên khác RefreshToken trong db
					if (user == null || user.RefeshToken != requestData.RefreshToken || user.TokenExprired <= DateTime.Now)
					{
						responeData.ResponseCode = -1;
						responeData.ResposeMessage = "Token không hợp lệ!";
						_loggerManager.LogWarn($"Response: Code={responeData.ResponseCode}, Message={responeData.ResposeMessage}");
						return Ok(responeData);
					}
					var newToken = await _accountRepository.CreateToken(principal.Claims.ToList());
					var newRefreshToken = await _accountRepository.GenerateRefreshToken();

					//3. Tạo Token và RefreshToken mới
					_ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
					await _accountRepository.UserUpdate_RefeshToken(user.UserID, newRefreshToken, DateTime.Now.AddDays(refreshTokenValidityInDays));
					responeData.ResponseCode = 1;
					responeData.ResposeMessage = "Tạo mới Token thành công";
					responeData.Token = new JwtSecurityTokenHandler().WriteToken(newToken);
					responeData.RefreshToken = newRefreshToken;
					_loggerManager.LogInfo($"Refreash token successful responseData: {responeData}");
					return Ok(responeData);
				}
				responeData.ResponseCode = 1;
				responeData.ResposeMessage = "Token vẫn còn thời hạn hoạt động!";
				_loggerManager.LogWarn($"Response: Code={responeData.ResponseCode}, Message={responeData.ResposeMessage}");
				return Ok(responeData);
			}
			catch (Exception ex)
			{
				responeData.ResponseCode = -99;
				responeData.ResposeMessage = ex.Message;
				_loggerManager.LogError($"Refreah Token Exception Error: '{ex.Message}'. StackTrace: {ex.StackTrace}");
				return Ok(responeData);
			}
		}

		[HttpPost("Refresh_Token")]
		public async Task<IActionResult> Refresh_Token(TokenModel tokenModel)
		{
			var responseData = new UserLoginResponseData();
			try
			{
				if (tokenModel == null || string.IsNullOrEmpty(tokenModel.AccessToken)
					|| string.IsNullOrEmpty(tokenModel.RefreshToken))
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Dữ liệu đầu vào không hợp lệ";
					return Ok(responseData);
				}

				if (tokenModel == null)
				{
					return BadRequest("Yêu cầu Token không hợp lệ");
				}
				string? accessToken = tokenModel.AccessToken;
				string? refreshToken = tokenModel.RefreshToken;

				//Bước 1: Giai mã Token truyền lên để lấy claims
				var principal = await _accountRepository.GetPrincipalFromExpiredToken(tokenModel.AccessToken);
				if (principal == null)
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Token không hợp lệ";
					return Ok(responseData);
				}

				//Bước 2: Check RefresToken và ngày hết hạn
				var exp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(principal.FindFirst("exp").Value));
				DateTime result = exp.UtcDateTime.AddHours(7);
				string userName = principal.Identity.Name;

				//Gọi dataBase để lấy theo userName
				var user = _accountRepository.GetUser_ByUserName(userName);

				//Nếu ngày hết hạn < thời gian hiện tại || RefreshToken truyền lên khác RefreshToken
				if (user == null || user.RefeshToken != refreshToken || user.TokenExprired <= DateTime.Now)
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Token không hợp lệ";
					return Ok(responseData);
				}

				var newToken = await _accountRepository.CreateToken(principal.Claims.ToList());
				var newRefeshToken = await _accountRepository.GenerateRefreshToken();

				//Bước 3: Tạo Token mới và RefresToken mới
				_ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
				await _accountRepository.UserUpdate_RefeshToken(user.UserID, newRefeshToken, DateTime.Now.AddDays(refreshTokenValidityInDays));

				responseData.ResponseCode = 1;
				responseData.ResposeMessage = "Tạo mới RefreshToken thành công";
				responseData.Token = new JwtSecurityTokenHandler().WriteToken(newToken);
				responseData.RefreshToken = newRefeshToken;
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				
				return Ok(ex);
			}
		}

		[HttpPost("LogOut_Account")]
		public async Task<IActionResult> LogOut_Account(TokenLogOutModel token)
		{
			var responseData = new UserLogOutResponseData();
			try
			{
				if (token == null || string.IsNullOrEmpty(token.AccessToken))
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Token không hợp lệ";
					_loggerManager.LogWarn($"Response LogOut_Account: Code={responseData.ResponseCode}, Message={responseData.ResposeMessage}");
					return Ok(responseData);
				}
				//1.Xóa Token trong Caching:
				//1.1 Giải mã Token truyền lên
				var principal = await _accountRepository.GetPrincipalFromExpiredToken(token.AccessToken);
				if (principal == null)
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Token không hợp lệ";
					_loggerManager.LogWarn($"Response LogOut_Account: Code={responseData.ResponseCode}, Message={responseData.ResposeMessage}");
					return Ok(responseData);
				}

				string userName = principal.Identity.Name;
				var user = _accountRepository.GetUser_ByUserName(userName);
				if (user == null)
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Token truyền lên không hợp lệ";
					_loggerManager.LogWarn($"Response LogOut_Account: Code={responseData.ResponseCode}, Message={responseData.ResposeMessage}");
					return Ok(responseData);
				}
				//Lấy dữ liệu từ Redis => LogOut trên 1 thiết bị
				var DeviceName = await _accountRepository.GetDeviceName();
				var cachKey = "User_" + user.UserID + "-" + DeviceName;
				_cache.Remove(cachKey);
				await _userSession.Delele_Session(token.AccessToken, user.UserID);
				responseData.ResponseCode = 1;
				responseData.ResposeMessage = "Đăng xuất thành công";
				_loggerManager.LogInfo($"Response LogOut_Account: Code={responseData.ResponseCode}, Message={responseData.ResposeMessage}");
				return Ok(responseData);

			}
			catch (Exception ex)
			{
				responseData.ResponseCode = -99;
				responseData.ResposeMessage = ex.Message;
				_loggerManager.LogError($"LogOut_Account Error: '{ex.Message}'. StackTrace: {ex.StackTrace}");
				return Ok(responseData);
			}
		}

		[HttpPost("LogOutAll_Account")]
		public async Task<IActionResult> LogOutAll_Account(TokenLogOutModel tokenLogOut)
		{
			var responseData = new UserLogOutResponseData();
			try
			{
				if (tokenLogOut == null || string.IsNullOrEmpty(tokenLogOut.AccessToken))
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Đăng xuất thất bại, Kiểm tra lại UserName, Password";
					_loggerManager.LogWarn($"Response LogOutAll_Account: Code={responseData.ResponseCode}, Message={responseData.ResposeMessage}");
					return Ok(responseData);
				}
				//Thực hiện xóa Token trong Caching:
				//Bước 1: Giải mã token truyền lên
				var principal = await _accountRepository.GetPrincipalFromExpiredToken(tokenLogOut.AccessToken);
				if (principal == null)
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Token không hợp lệ";
					_loggerManager.LogWarn($"Response LogOutAll_Account: Code={responseData.ResponseCode}, Message={responseData.ResposeMessage}");
					return Ok(responseData);
				}

				//Bước 2: Check RefreshToken và ngày hết hạn 
				string userName = principal.Identity.Name;
				var user = _accountRepository.GetUser_ByUserName(userName);
				if (user == null)
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Token truyền lên không hợp lệ";
					_loggerManager.LogWarn($"Response LogOutAll_Account: Code={responseData.ResponseCode}, Message={responseData.ResposeMessage}");
					return Ok(responseData);
				}

				//Bước 3: Lấy dữ liệu từ RedisCaching và LogOut trên mọi thiết bị 
				var DeviceName = await _accountRepository.GetDeviceName();
				var cachKey = "User_" + user.UserID + "-" + DeviceName;
				using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true"))
				{
					IDatabase db = redis.GetDatabase();
					var server = redis.GetServer("localhost", 6379);

					// Lấy tất cả các keys từ Redis theo pattern User_*
					var keys = server.Keys(pattern: "User_*");

					foreach (var key in keys)
					{
						// Kiểm tra nếu key khớp với user.UserID
						if (key.ToString().Contains(user.UserID.ToString()))
						{
							Console.WriteLine($"Deleting key: {key}");

							// Xóa key khỏi Redis
							db.KeyDelete(key);
						}
					}
					//Xóa trong db
					await _userSession.DeleleAll_Session(user.UserID);
				}
				responseData.ResponseCode = 1;
				responseData.ResposeMessage = "Đăng xuất thành công";
				_loggerManager.LogInfo($"Response LogOutAll_Account: Code={responseData.ResponseCode}, Message={responseData.ResposeMessage}");
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				responseData.ResponseCode = -99;
				responseData.ResposeMessage = ex.Message;
				_loggerManager.LogError($"LogOut_Account Error: '{ex.Message}'. StackTrace: {ex.StackTrace}");
				return Ok(responseData);
			}
		}

		[HttpGet("login-google")]
		public IActionResult LoginGoogle()
		{
			var properties = new AuthenticationProperties
			{
				RedirectUri = "/api/Authentication/google-response"
			};
			return Challenge(properties, GoogleDefaults.AuthenticationScheme);
		}

		[HttpGet("google-response")]
		public async Task<IActionResult> GoogleResponse()
		{
			var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			if (result?.Principal != null)
			{
				// (Giữ nguyên logic hiện có để lấy user, tạo token, v.v.)
				var claims = result.Principal.Claims.Select(claim => new
				{
					claim.Issuer,
					claim.OriginalIssuer,
					claim.Type,
					claim.Value
				});

				var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
				if (email == null)
				{
					return BadRequest(new { message = "Không thể lấy email từ Google." });
				}

				string emailName = email.Split('@')[0];
				var user = await _userRepository.GetUserByUserName(emailName);
				if (user == null)
				{
					var newAccount = new User_CreateAccount
					{
						UserName = emailName,
						PassWord = "123456",
						ReferralCode = null,
						TypePerson = "Customer"
					};
					var createResult = await _userRepository.CreateAccount(newAccount);
					if (createResult.ResponseCode != 1)
					{
						return BadRequest(new { message = "Tạo tài khoản thất bại: " + createResult.ResposeMessage });
					}
					user = await _userRepository.GetUserByUserName(emailName);
					if (user != null)
					{
						return BadRequest(new { message = "Tạo tài khoản thất bại: "});
					}
				}

				var authClaims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(ClaimTypes.PrimarySid, user.UserID.ToString()),
					new Claim(ClaimTypes.Role, user.TypePerson ?? string.Empty),
					new Claim(ClaimTypes.Authentication, user.RefeshToken ?? string.Empty)
				};

				var newToken = await _accountRepository.CreateToken(authClaims);
				var refeshToken = await _accountRepository.GenerateRefreshToken();
				await _accountRepository.UserUpdate_RefeshToken(user.UserID, refeshToken, DateTime.Now.AddDays(int.Parse(_configuration["JWT:RefreshTokenValidityInDays"])));

				var DeviceName = await _accountRepository.GetDeviceName();
				var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;

				var cachKey = "User_" + user.UserID + "-" + DeviceName;
				var user_Session = new Aesthetics.DTO.NetCore.DataObject.Model.UserSession
				{
					UserID = user.UserID,
					Token = new JwtSecurityTokenHandler().WriteToken(newToken),
					DeviceName = DeviceName,
					Ip = remoteIpAddress.ToString(),
					CreateTime = DateTime.Now,
					DeleteStatus = 1
				};
				await _userSession.Insert_Sesion(user_Session);

				var user_SessionCach = new Aesthetics.DTO.NetCore.DataObject.Model.UserSession
				{
					UserID = user.UserID,
					Token = new JwtSecurityTokenHandler().WriteToken(newToken),
					DeviceName = DeviceName,
					Ip = remoteIpAddress.ToString(),
					CreateTime = DateTime.Now
				};
				var dataCachingJson = JsonConvert.SerializeObject(user_SessionCach);
				var dataToCache = Encoding.UTF8.GetBytes(dataCachingJson);
				DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
					.SetAbsoluteExpiration(DateTime.Now.AddMinutes(3));
				_cache.Set(cachKey, dataToCache, options);

				var responseData = new UserLoginResponseData
				{
					ResponseCode = 1,
					ResposeMessage = "Đăng nhập thành công!",
					DeviceName = DeviceName,
					UserID = user.UserID,
					TypePerson = user.TypePerson,
					UserName = user.UserName,
					Token = new JwtSecurityTokenHandler().WriteToken(newToken),
					RefreshToken = refeshToken
				};

				// Trả về HTML với script gửi dữ liệu về frontend
				var jsonData = JsonConvert.SerializeObject(responseData);
				Console.WriteLine("Response data: " + jsonData);
				var script = $"<script>window.opener.postMessage({jsonData}, 'http://localhost:3000'); window.close();</script>";
				return Content(script, "text/html");
			}
			return Content("<script>window.close();</script>", "text/html");
		}
	}
}
