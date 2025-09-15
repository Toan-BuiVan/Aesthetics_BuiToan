using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Implement;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using Aesthetics.DTO.NetCore.ResponsePermission;
using ASP_NetCore_Aesthetics.Filter;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;
using XAct.Library.Settings;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
		private IPermissionRepository _permissionRepository;
		private readonly IDistributedCache _cache;
		private readonly ILoggerManager _loggerManager;
		private DB_Context _context;
		public PermissionController(IPermissionRepository permissionRepository, IDistributedCache cache
			, ILoggerManager loggerManager, DB_Context context)
		{
			_permissionRepository = permissionRepository;
			_cache = cache;
			_loggerManager = loggerManager;
			_context = context;
		}

		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Update_Permission")]
		[HttpPost("Update_Permission")]
		public async Task<IActionResult> Update_Permission(Update_Permission permission)
		{
			try
			{
				//1.Update_Permission
				var responseData = await _permissionRepository.Update_Permission(permission);
				//2. Lưu log request
				_loggerManager.LogInfo("Update_Permission Request: " + JsonConvert.SerializeObject(permission));
				//3. Lưu log data response
				//_loggerManager.LogInfo("Update_Permission Response data: " + JsonConvert.SerializeObject(responseData.Data));
				if (responseData.ResponseCode == 1)
				{
					var cacheKey = "GetPermission_Cache";
					await _cache.RemoveAsync(cacheKey);
				}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Update_Permission} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Delete_Permission")]
		[HttpDelete("Delete_Permission")]
		public async Task<IActionResult> Delete_Permission(PermissionRequest delete_)
		{
			try
			{
				//1.Delete_Permission
				var responseData = await _permissionRepository.Delete_Permission(delete_);
				//2. Lưu log request
				_loggerManager.LogInfo("Delete_Permission Request: " + JsonConvert.SerializeObject(delete_));
				//3. Lưu log data response
				//_loggerManager.LogInfo("Delete_Permission Response data: " + JsonConvert.SerializeObject(responseData.Data));
				if (responseData.ResponseCode == 1)
				{
					var cacheKey = "GetPermission_Cache";
					await _cache.RemoveAsync(cacheKey);
				}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Delete_Permission} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}


		[Filter_Authorization("GetList_SearchPermission")]
		[HttpPost("GetList_SearchPermission")]
		public async Task<IActionResult> GetList_SearchPermission(PermissionRequest request)
		{
			try
			{
				var listPermission = new List<ResponseData_Permission>();
				//Khóa để lưu dữ liệu trong Redis caching
				var cacheKey = "GetPermission_Cache";

				//1. Lấy dữ liệu trong Redis caching
				byte[] cachedData = await _cache.GetAsync(cacheKey);

				//1.1 Lưu log request
				_loggerManager.LogInfo("GetList_SearchPermission Request: " + JsonConvert.SerializeObject(request));

				//1.2 Nếu Cache có dữ liệu, giải mã trả về Client
				if (cachedData != null)
				{
					var cachedDataString = Encoding.UTF8.GetString(cachedData);
					listPermission = JsonConvert.DeserializeObject<List<ResponseData_Permission>>(cachedDataString);

					if (request.UserID != null)
					{
						listPermission = listPermission.Where(p => p.UserID == request.UserID).ToList();
					}

					//1.4 Lưu log dữ liệu Permission trả về trong cache
					_loggerManager.LogInfo("GetList_SearchPermission cache: " + cachedDataString);

					//1.5 Lưu log kết quả dữ liệu khi có request
					_loggerManager.LogInfo("GetList_SearchPermission cache Request: " + JsonConvert.SerializeObject(listPermission));

					return Ok(listPermission);
				}
				else
				{
					//2. Nếu cache không có dữ liệu gọi vào db
					var responseData = await _permissionRepository.GetList_SearchPermission(request);

					//2.1 Chuyển đổi dữ liệu lấy từ DB thành danh sách đối tượng ResponseServicess
					listPermission = responseData?.Data_Permission?.Select(s => new ResponseData_Permission
					{
						PermissionID = s.PermissionID,
						FunctionID = s.FunctionID,
						FunctionName = s.FunctionName,
						UserID = s.UserID,
						UserName = s.UserName,
						StatusPermission = s.StatusPermission
					}).ToList() ?? new List<ResponseData_Permission>();

					//2.2. Chuyển danh sách dịch vụ thành chuỗi JSON để lưu vào cache
					var cachedDataString = JsonConvert.SerializeObject(listPermission);
					var dataToCache = Encoding.UTF8.GetBytes(cachedDataString);

					//2.3 Cấu hình thời gian hết hạn cho Redis Cache
					DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
						.SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
						.SetSlidingExpiration(TimeSpan.FromMinutes(3));

					//2.4. Lưu log dữ liệu Supplier trong db
					_loggerManager.LogInfo("GetList_SearchPermission db: " + cachedDataString);

					//2.5. Kiểm tra nếu lấy thành công danh sách thì lưu cache
					if (responseData.ResponseCode == 1)
					{
						await _cache.SetAsync(cacheKey, dataToCache, options);
					}
					return Ok(responseData);
				}
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetList_SearchSupplier} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		//[Filter_Authorization("GetListTyperson")]
		[HttpPost("GetListTyperson")]
		public async Task<IActionResult> GetListTyperson()
		{
			try
			{
				var responseData = await _permissionRepository.GetListTyperson();
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetListTyperson} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		//[Filter_Authorization("GroupByPermission")]
		[HttpPost("GroupByPermission")]
		public async Task<IActionResult> GroupByPermission(Update_Permission update_)
		{
			try
			{
				var responseData = await _permissionRepository.GroupByPermission(update_);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GroupByPermission} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
		//[Filter_Authorization("GetPermissionByUserID")]
		[HttpPost("GetPermissionByUserID")]
		public async Task<IActionResult> GetPermissionByUserID(Update_Permission userID)
		{
			try
			{
				var responseData = await _permissionRepository.GetPermissionByUserID(userID);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetPermissionByUserID} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
