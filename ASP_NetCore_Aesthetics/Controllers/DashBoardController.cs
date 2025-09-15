using Aesthetics.DataAccess.NetCore.Repositories.Implement;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
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
	public class DashBoardController : ControllerBase
	{
		private IDashBoardRepository _dashBoardRepository;
		private readonly ILoggerManager _loggerManager;
		public DashBoardController(IDashBoardRepository dashBoardRepository, ILoggerManager loggerManager)
		{
			_dashBoardRepository = dashBoardRepository;
			_loggerManager = loggerManager;
		}
		[Filter_Authorization("GetMonthlyStatistics")]
		[HttpPost("GetMonthlyStatistics")]
		public async Task<IActionResult> GetMonthlyStatistics(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetMonthlyStatistics(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetMonthlyStatistics Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetList_SearchInvoiceDetail response
				_loggerManager.LogInfo("GetMonthlyStatistics Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetMonthlyStatistics} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetMonthlyImportStatistics")]
		[HttpPost("GetMonthlyImportStatistics")]
		public async Task<IActionResult> GetMonthlyImportStatistics(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyImportStatistics 
				var responseData = await _dashBoardRepository.GetMonthlyImportStatistics(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetMonthlyImportStatistics Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetList_SearchInvoiceDetail response
				_loggerManager.LogInfo("GetMonthlyImportStatistics Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetMonthlyImportStatistics} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetTopProductsBySales")]
		[HttpPost("GetTopProductsBySales")]
		public async Task<IActionResult> GetTopProductsBySales(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetTopProductsBySales(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetTopProductsBySales Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetList_SearchInvoiceDetail response
				_loggerManager.LogInfo("GetTopProductsBySales Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetTopProductsBySales} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetTopServicesBySales")]
		[HttpPost("GetTopServicesBySales")]
		public async Task<IActionResult> GetTopServicesBySales(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetTopServicesBySales(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetTopServicesBySales Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetTopServicesBySales response
				_loggerManager.LogInfo("GetTopServicesBySales Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetTopServicesBySales} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetReturningCustomerRate")]
		[HttpPost("GetReturningCustomerRate")]
		public async Task<IActionResult> GetReturningCustomerRate()
		{
			try
			{
				//1.GetReturningCustomerRate 
				var responseData = await _dashBoardRepository.GetReturningCustomerRate();
				//3. Lưu log data GetReturningCustomerRate response
				_loggerManager.LogInfo("GetReturningCustomerRate Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetReturningCustomerRate} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("CountProcessedOrders")]
		[HttpPost("CountProcessedOrders")]
		public async Task<IActionResult> CountProcessedOrders(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.CountProcessedOrders(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("CountProcessedOrders Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data CountProcessedOrders response
				_loggerManager.LogInfo("CountProcessedOrders Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error CountProcessedOrders} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetTotalBookings")]
		[HttpPost("GetTotalBookings")]
		public async Task<IActionResult> GetTotalBookings(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetTotalBookings(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetTotalBookings Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetTotalBookings response
				_loggerManager.LogInfo("GetTotalBookings Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetTotalBookings} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetTopEmployeesByQuantityProduct")]
		[HttpPost("GetTopEmployeesByQuantityProduct")]
		public async Task<IActionResult> GetTopEmployeesByQuantityProduct(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetTopEmployeesByQuantityProduct(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetTopEmployeesByQuantityProduct Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetTopEmployeesByQuantityProduct response
				_loggerManager.LogInfo("GetTopEmployeesByQuantityProduct Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetTopEmployeesByQuantityProduct} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetTopEmployeesByQuantityServices")]
		[HttpPost("GetTopEmployeesByQuantityServices")]
		public async Task<IActionResult> GetTopEmployeesByQuantityServices(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetTopEmployeesByQuantityServices(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetTopEmployeesByQuantityServices Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetTopEmployeesByQuantityServices response
				_loggerManager.LogInfo("GetTopEmployeesByQuantityServices Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetTopEmployeesByQuantityServices} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetMonthlyBookingReport")]
		[HttpPost("GetMonthlyBookingReport")]
		public async Task<IActionResult> GetMonthlyBookingReport(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetMonthlyBookingReport(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetMonthlyBookingReport Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetMonthlyBookingReport response
				_loggerManager.LogInfo("GetMonthlyBookingReport Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetMonthlyBookingReport} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetTop3ProductTypesByRevenue")]
		[HttpPost("GetTop3ProductTypesByRevenue")]
		public async Task<IActionResult> GetTop3ProductTypesByRevenue(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetTop3ProductTypesByRevenue(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetTop3ProductTypesByRevenue Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetTop3ProductTypesByRevenue response
				_loggerManager.LogInfo("GetTop3ProductTypesByRevenue Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetTop3ProductTypesByRevenue} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetTop3ClinicsByRevenue")]
		[HttpPost("GetTop3ClinicsByRevenue")]
		public async Task<IActionResult> GetTop3ClinicsByRevenue(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetTop3ClinicsByRevenue(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetTop3ClinicsByRevenue Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetTop3ClinicsByRevenue response
				_loggerManager.LogInfo("GetTop3ClinicsByRevenue Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetTop3ClinicsByRevenue} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetVoucherStatistics")]
		[HttpPost("GetVoucherStatistics")]
		public async Task<IActionResult> GetVoucherStatistics(DashBoardRequest dashBoard)
		{
			try
			{
				//1.GetMonthlyStatistics 
				var responseData = await _dashBoardRepository.GetVoucherStatistics(dashBoard);
				//2. Lưu log request
				_loggerManager.LogInfo("GetVoucherStatistics Request: " + JsonConvert.SerializeObject(dashBoard));
				//3. Lưu log data GetVoucherStatistics response
				_loggerManager.LogInfo("GetVoucherStatistics Response data: " + JsonConvert.SerializeObject(responseData.ListDashBoard));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetVoucherStatistics} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
