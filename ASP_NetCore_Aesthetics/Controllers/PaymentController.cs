using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.Model.Momo;
using Aesthetics.DTO.NetCore.DataObject.Model.VnPay;
using ASP_NetCore_Aesthetics.Services.MomoServices;
using ASP_NetCore_Aesthetics.Services.SenderMail;
using ASP_NetCore_Aesthetics.Services.VnPaySevices;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
		private readonly IVnPayService _vnPayService;
		private IMomoService _momoService;
		private IUserRepository _userRepository;
		private IInvoiceRepository _invoiceRepository;
		private IProductsRepository _productsRepository;
		private IEmailSender _emailSender;
		private IBookingsRepository _bookingsRepository;
		public PaymentController(IVnPayService vnPayService, IMomoService momoService, 
			IUserRepository userRepository, IInvoiceRepository invoiceRepository, 
			IProductsRepository productsRepository, IEmailSender emailSender, IBookingsRepository bookingsRepository)
		{
			_vnPayService = vnPayService;
			_momoService = momoService;
			_userRepository = userRepository;
			_invoiceRepository = invoiceRepository;
			_productsRepository = productsRepository;
			_emailSender = emailSender;
			_bookingsRepository = bookingsRepository;
		}
		[HttpPost("CreatePaymentUrlVnPay")]
		public IActionResult CreatePaymentUrlVnPay([FromBody] PaymentInformationModel model)
		{
			try
			{
				// Logic tạo URL thanh toán VnPay
				var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
				return Ok(new { paymentUrl = url });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = ex.Message });
			}
		}

		[HttpGet("CallBackVnPay")]
		public async Task<IActionResult> PaymentCallbackVnpay()
		{
			try
			{
				var response = _vnPayService.PaymentExecute(Request.Query);
				string status;
				string invoiceId;

				// Lấy OrderInfo từ query string
				string orderInfo = Request.Query["vnp_OrderInfo"];
				var match = Regex.Match(orderInfo, @"OrderID:(\d+)");
				if (!match.Success || !int.TryParse(match.Groups[1].Value, out int orderId))
				{
					return BadRequest("Không tìm được OrderID từ vnp_OrderInfo.");
				}
				invoiceId = orderId.ToString();
				Console.WriteLine($"VnPayResponseCode: '{response.VnPayResponseCode}'");
				if (response.VnPayResponseCode == "00")
				{
					// Thanh toán thành công
					var invoice = await _invoiceRepository.GetInvoiceByInvoiceID(orderId);
					if (invoice != null)
					{
						//Cập nhật trạng thái hóa đơn
						await _invoiceRepository.UpdateStatusInvoice(orderId);
						//Cập nhật điểm mua hàng cho khách hàng
						await _userRepository.UpdateRatingPoints_Customer(invoice.CustomerID ?? 0);
						if (invoice.EmployeeID != null)
						{
							//Cập nhật điểm bán hàng cho nhân viên
							await _userRepository.UpdateSalesPoints(invoice.EmployeeID ?? 0, invoice.TotalMoney ?? 0);
						}

						//Cập nhật trạng thái hóa đơn chi tiết
						//await _invoiceRepository.UpdateStatusInvoiceDetail(invoice.InvoiceID);

						//Cập nhật trạng thái thanh toán của Bookings_BookingAssignmet
						await _bookingsRepository.UpdatePaymentStatus(invoice.InvoiceID);

						var invoiceDetail = await _invoiceRepository.InvoiceDetailByInvoiceID(invoice.InvoiceID);
						if (invoiceDetail != null && invoiceDetail.Any())
						{
							for (int i = 0; i < invoiceDetail.Count; i++)
							{
								var detail = invoiceDetail[i];
								if (detail.ProductID != null)
								{
									//Cập nhật số lượng sản phẩm
									await _productsRepository.UpdateQuantityPro(detail.ProductID ?? 0, detail.TotalQuantityProduct ?? 0);
								}
							}
						}
						//Gửi mail
						var customer = await _userRepository.GetUserByUserID(invoice.CustomerID);
						if (customer != null && customer.Email != null)
						{
							var subject = "Thanh Toán Hóa Đơn Thành Công!";
							var message = $"Kính gửi Quý Khách," +
								$"\n\nChúng tôi xin thông báo: Hóa đơn {orderId}." +
								$"\nTổng số tiền thanh toán: {invoice.TotalMoney:N0} VND." +
								$"\n\nChân thành cảm ơn Quý Khách đã tin tưởng và sử dụng dịch vụ của chúng tôi." +
								$"\n\nTrân trọng!";
							await _emailSender.SendEmailAsync(customer.Email, subject, message);
						}

						
						if (invoice.EmployeeID == null)
						{
							//Cập nhật trạng thái vận chuyển đơn hàng nếu không mua online
							await _invoiceRepository.AutoUpdateOrderStatus(invoice.InvoiceID);
						}
						else
						{
							//Cập nhật trạng thái vận chuyển đơn hàng nếu không mua tại của hàng
							await _invoiceRepository.AutoUpdateOrderStatusEmployee(invoice.InvoiceID);
						}

					}
					status = "success";
				}
				else
				{
					// Thanh toán thất bại
					var invoice = await _invoiceRepository.GetInvoiceByInvoiceID(orderId);
					if (invoice != null)
					{
						await _invoiceRepository.UpdateStatusInvoiceFail(orderId);
						await _invoiceRepository.UpdateStatusInvoiceDetailFail(invoice.InvoiceID);
					}
					status = "failure";
				}
				// Chuyển hướng về client với status và invoiceId
				string redirectUrl = $"http://localhost:3000/payment-result?status={status}&invoiceId={invoiceId}";
				return Redirect(redirectUrl);
			}
			catch (Exception ex)
			{

				string redirectUrl = $"http://localhost:3000/payment-result?status=error&message={ex.Message}";
				return Redirect(redirectUrl);
			}
		}

		[HttpPost]
		[Route("CreatePaymentUrl")]
		public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
		{
			var response = await _momoService.CreatePaymentAsync(model);
			return Ok(new { paymentUrl = response.PayUrl});

			//var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
			//return Ok(new { paymentUrl = url });
		}

		[HttpGet("CallBackMomo")]
		public IActionResult PaymentCallPack()
		{
			var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
			return Ok(response);
		}
	}
}
