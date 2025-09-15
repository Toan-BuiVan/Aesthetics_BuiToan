using Aesthetics.DataAccess.NetCore.Repositories.Implement;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.RequestData;
using ASP_NetCore_Aesthetics.Filter;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSessionController : ControllerBase
    {
		private IUserSessionRepository _userSession;
		private readonly ILoggerManager _loggerManager;
		public UserSessionController(IUserSessionRepository userSession, ILoggerManager loggerManager)
		{
			_userSession = userSession;
			_loggerManager = loggerManager;
		}

		[Filter_Authorization("GetList_SearchUserSession")]
		[HttpPost("GetList_SearchUserSession")]
		public async Task<IActionResult> GetList_SearchUserSession(UserSessionRequest userSession)
		{
			try
			{
				//1. GetList_SearchUserSession
				var responseData = await _userSession.GetList_SearchUserSession(userSession);
				//2. Lưu log request
				_loggerManager.LogInfo("GetList_SearchUserSession Requets: " + JsonConvert.SerializeObject(userSession));
				//3. Lưu log data trả về
				_loggerManager.LogInfo("GetList_SearchUserSession data: " + JsonConvert.SerializeObject(responseData.Data));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetList_SearchUserSession} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
