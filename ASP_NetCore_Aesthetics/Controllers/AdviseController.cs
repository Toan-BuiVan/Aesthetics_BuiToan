using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using Aesthetics.DTO.NetCore.ResponseAdvise;
using ASP_NetCore_Aesthetics.Filter;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdviseController : ControllerBase
    {
        private IAdviseRepository _adviseRepository;
		private readonly ILoggerManager _loggerManager;
		private readonly IDistributedCache _cache;
		public AdviseController(IAdviseRepository adviseRepository, ILoggerManager loggerManager, IDistributedCache cache)
		{
			_adviseRepository = adviseRepository;
			_loggerManager = loggerManager;
			_cache = cache;
		}
		[HttpPost("Insert_Advise")]
		public async Task<IActionResult> Insert_Advise(AdviseRequest adviseRequest)
		{
			try
			{
				var responseData = await _adviseRepository.InsertAdvise(adviseRequest);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Insert_Advise} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Update_Advise")]
		[HttpPost("Update_Advise")]
		public async Task<IActionResult> Update_Advise(Update_Advise update_Advise)
		{
			try
			{
				var responseData = await _adviseRepository.UpdateAdvise(update_Advise);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Update_Advise} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Delete_Advise")]
		[HttpDelete("Delete_Advise")]
		public async Task<IActionResult> Delete_Advise(Update_Advise delete_)
		{
			try
			{
				var responseData = await _adviseRepository.DeleteAdvise(delete_);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Delete_Advise} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		//[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("GetList_SearchAdvise")]
		[HttpPost("GetList_SearchAdvise")]
		public async Task<IActionResult> GetList_SearchAdvise(GetLisr_SearchAdvise getLisr_)
		{
			try
			{
				var listAdvise = new List<AdviseResponse>();
				//Khóa để lưu dữ liệu trong Redis caching
				var cacheKey = "GetAdvise_Cache";

				//1. Lấy dữ liệu trong Redis caching
				byte[] cachedData = await _cache.GetAsync(cacheKey);

				//1.1 Lưu log request
				_loggerManager.LogInfo("GetList_SearchAdvise Request: " + JsonConvert.SerializeObject(getLisr_));

				//1.2 Nếu Cache có dữ liệu, giải mã trả về Client
				if (cachedData != null)
				{
					var cachedDataString = Encoding.UTF8.GetString(cachedData);
					listAdvise = JsonConvert.DeserializeObject<List<AdviseResponse>>(cachedDataString);

					//1.3 Lọc dữ liệu khi có request 
					listAdvise = listAdvise
								.Where(s =>
									(string.IsNullOrEmpty(getLisr_.FullName) || s.FullName.Contains(getLisr_.FullName, StringComparison.OrdinalIgnoreCase)) &&
									(
										(getLisr_.StartDate == null && getLisr_.EndDate == null) ||
										(getLisr_.StartDate != null && getLisr_.EndDate == null && s.CreationDate >= getLisr_.StartDate) ||
										(getLisr_.StartDate == null && getLisr_.EndDate != null && s.CreationDate <= getLisr_.EndDate) ||
										(getLisr_.StartDate != null && getLisr_.EndDate != null && s.CreationDate >= getLisr_.StartDate && s.CreationDate <= getLisr_.EndDate)
									)
								).ToList();
					// Kiểm tra nếu danh sách rỗng sau khi lọc
					if (listAdvise.Count == 0)
					{
						var errorResponse = new
						{
							responseCode = -1,
							responseMessage = "Không tìm thấy dữ liệu phù hợp với yêu cầu tìm kiếm."
						};
						return Ok(errorResponse);
					}

					//1.4 Lưu log dữ liệu Supplier trả về trong cache
					_loggerManager.LogInfo("GetList_SearchAdvise cache: " + cachedDataString);

					//1.5 Lưu log kết quả dữ liệu khi có request
					_loggerManager.LogInfo("GetList_SearchAdvise cache Request: " + JsonConvert.SerializeObject(listAdvise));

					return Ok(listAdvise);
				}
				else
				{
					var responseData = await _adviseRepository.GetList_SreachAdvise(getLisr_);
					listAdvise = responseData?.Data?.Select(s => new AdviseResponse
					{
						AdviseID = s.AdviseID,
						FullName = s.FullName,
						Phone = s.Phone,
						Email = s.Email,
						Content = s.Content,
						ConsultingStatus = s.ConsultingStatus,
						CreationDate = s.CreationDate
					}).ToList() ?? new List<AdviseResponse>();

					//2.2. Chuyển danh sách dịch vụ thành chuỗi JSON để lưu vào cache
					var cachedDataString = JsonConvert.SerializeObject(listAdvise);
					var dataToCache = Encoding.UTF8.GetBytes(cachedDataString);

					//2.3 Cấu hình thời gian hết hạn cho Redis Cache
					DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
						.SetAbsoluteExpiration(DateTime.Now.AddMinutes(5))
						.SetSlidingExpiration(TimeSpan.FromMinutes(3));

					//2.4. Lưu log dữ liệu Supplier trong db
					_loggerManager.LogInfo("GetList_SearchAdvise db: " + cachedDataString);

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
				_loggerManager.LogError("{Error GetList_SearchAdvise} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
