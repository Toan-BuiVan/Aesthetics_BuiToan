using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject;
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
	public class BookingsController : ControllerBase
	{
		private IBookingsRepository _bookingRepository;
		private readonly ILoggerManager _loggerManager;
		public BookingsController(IBookingsRepository bookingRepository, ILoggerManager loggerManager)
		{
			_bookingRepository = bookingRepository;
			_loggerManager = loggerManager;
		}

		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Insert_Booking", "INSERT")]
		[HttpPost("Insert_Booking")]
		public async Task<IActionResult> Insert_Booking(BookingRequest request)
		{
			try
			{
				//1. Insert_Booking
				var responseData = await _bookingRepository.Insert_Booking(request);
				//2. Lưu log Insert_Booking Request 
				_loggerManager.LogInfo("Insert_Booking Request: " + JsonConvert.SerializeObject(request));

				//3. Lưu log Insert_Booking_Assignment Request 
				_loggerManager.LogInfo("Insert_Booking_Assignment Request: " + JsonConvert.SerializeObject(responseData.Booking_AssData));
				return Ok(responseData);
			}
			catch (Exception ex) 
			{
				_loggerManager.LogError("{Error Insert_Booking} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("UpdateBooking_Assignment", "UPDATE")]
		[HttpPost("UpdateBooking_Assignment")]
		public async Task<IActionResult> UpdateBooking_Assignment(UpdateBooking_Assignment updateBooking_)
		{
			try
			{
				//1. UpdateBooking_Assignment
				var responseData = await _bookingRepository.UpdateStatusBooking_Assignment(updateBooking_);

				//2. Lưu log Insert_Booking Request 
				_loggerManager.LogInfo("UpdateBooking_Assignment Request: " + JsonConvert.SerializeObject(updateBooking_));

				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error UpdateBooking_Assignment} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.StackTrace);
			}
		}


		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("UpdateQuantity", "UPDATE")]
		[HttpPost("UpdateQuantity")]
		public async Task<IActionResult> UpdateQuantity(UpdateQuantityBookingAssignment updateBooking_)
		{
			try
			{
				//1. UpdateQuantity
				var responseData = await _bookingRepository.UpdateQuantity(updateBooking_);

				//2. Lưu log UpdateQuantity Request 
				_loggerManager.LogInfo("UpdateQuantity Request: " + JsonConvert.SerializeObject(updateBooking_));

				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error UpdateQuantity} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.StackTrace);
			}
		}


		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Delete_Booking", "DELETE")]
		[HttpDelete("Delete_Booking")]
		public async Task<IActionResult> Delete_Booking(Delete_Booking delete_)
		{
			try
			{
				//1. Delete_Booking
				var responseData = await _bookingRepository.Delete_Booking(delete_);

				//2. Lưu log Insert_Booking Request 
				_loggerManager.LogInfo("Delete_Booking Request: " + JsonConvert.SerializeObject(delete_));

				//3. Lưu log Delete_Booking_Assignment Request 
				_loggerManager.LogInfo("Delete_Booking_Assignment Request: " + JsonConvert.SerializeObject(responseData.Booking_AssData));


				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Delete_Booking} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.StackTrace);
			}
		}

		[Filter_Authorization("GetList_SearchBooking", "VIEW")]
		[HttpPost("GetList_SearchBooking")]
		public async Task<IActionResult> GetList_SearchBooking(GetList_SearchBooking getList_)
		{
			try
			{
				//1. GetList_SearchBooking
				var responseData = await _bookingRepository.GetList_SearchBooking(getList_);
				//2. Lưu log request
				_loggerManager.LogInfo("GetList_SearchBooking Requets: " + JsonConvert.SerializeObject(getList_));
				//3. Lưu log data trả về
				_loggerManager.LogInfo("GetList_SearchBooking data: " + JsonConvert.SerializeObject(responseData.Data));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetList_SearchBooking} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[Filter_Authorization("GetList_SearchBooking_Assignment", "VIEW")]
		[HttpPost("GetList_SearchBooking_Assignment")]
		public async Task<IActionResult> GetList_SearchBooking_Assignment(GetList_SearchBooking_Assignment getList_)
		{
			try
			{
				//1. GetList_SearchBooking_Assignment
				var responseData = await _bookingRepository.GetList_SearchBooking_Assignment(getList_);
				//2. Lưu log request
				_loggerManager.LogInfo("GetList_SearchBooking_Assignment Requets: " + JsonConvert.SerializeObject(getList_));
				//3. Lưu log data trả về
				_loggerManager.LogInfo("GetList_SearchBooking_Assignment data: " + JsonConvert.SerializeObject(responseData.Data));
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Insert Clinic} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Insert_BookingSer_Assi", "INSERT")]
		[HttpPost("Insert_BookingSer_Assi")]
		public async Task<IActionResult> Insert_BookingSer_Assi(Insert_Booking_Services request)
		{
			try
			{
				//1. Insert_BookingSer_Assi
				var responseData = await _bookingRepository.Insert_BookingSer_Assi(request);
				//2. Lưu log Insert_BookingSer_Assi Request 
				_loggerManager.LogInfo("Insert_Booking Request: " + JsonConvert.SerializeObject(request));

				//3. Lưu log Insert_BookingAssignment Request 
				_loggerManager.LogInfo("Insert_Booking_Assignment Request: " + JsonConvert.SerializeObject(responseData.Booking_AssData));

				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Insert_BookingSer_Assi} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Delete_BookingSer_Assi", "DELETE")]
		[HttpDelete("Delete_BookingSer_Assi")]
		public async Task<IActionResult> Delete_BookingSer_Assi(Delete_Booking_Services delete_)
		{
			try
			{
				//1. Delete_BookingSer_Assi
				var responseData = await _bookingRepository.Delete_BookingSer_Assi(delete_);

				//2. Lưu log Delete_BookingSer_Assi Request 
				_loggerManager.LogInfo("Delete_Booking Request: " + JsonConvert.SerializeObject(delete_));

				//3. Lưu log Delete_Booking_Assi data 
				_loggerManager.LogInfo("Delete_Booking_Assi Request: " + JsonConvert.SerializeObject(responseData.Booking_AssData));

				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Delete_BookingSer_Assi} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.StackTrace);
			}
		}

		//[HttpGet("GetList_SearchBooking_Services")]
		//public async Task<IActionResult> GetList_SearchBooking_Services(GetList_SearchBooking_Services getList_)
		//{
		//	try
		//	{
		//		//1. GetList_SearchBooking_Services
		//		var responseData = await _bookingRepository.GetList_SearchBooking_Services(getList_);
		//		//2. Lưu log request
		//		_loggerManager.LogInfo("GetList_SearchBooking_Services Requets: " + JsonConvert.SerializeObject(getList_));
		//		//3. Lưu log data trả về
		//		_loggerManager.LogInfo("GetList_SearchBooking_Services data: " + JsonConvert.SerializeObject(responseData.Data));
		//		return Ok(responseData);
		//	}
		//	catch (Exception ex)
		//	{
		//		_loggerManager.LogError("{Error Insert Clinic} Message: " + ex.Message +
		//			"|" + "Stack Trace: " + ex.StackTrace);
		//		return Ok(ex.Message);
		//	}
		//}
	}
}
