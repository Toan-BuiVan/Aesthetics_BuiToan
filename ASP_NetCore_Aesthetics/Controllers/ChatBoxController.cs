using Aesthetics.DataAccess.NetCore.Repositories.Implement;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using ASP_NetCore_Aesthetics.Filter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Text;
using FuzzySharp;
using System.Globalization;
using Aesthetics.DTO.NetCore.DataObject.Model;
using System.Text.RegularExpressions;
using System.Security.Claims;
using Aesthetics.DTO.NetCore.RequestData;

namespace ASP_NetCore_Aesthetics.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ChatBoxController : ControllerBase
	{
		private readonly IProductsRepository _productsRepository;
		private readonly IServicessRepository _servicesRepository;
		private readonly IBookingsRepository _bookingsRepository;
		private readonly string _apiKey;

		public ChatBoxController(IProductsRepository productsRepository, IServicessRepository servicesRepository, IBookingsRepository bookingsRepository, IConfiguration configuration)
		{
			_productsRepository = productsRepository;
			_servicesRepository = servicesRepository;
			_bookingsRepository = bookingsRepository;
			_apiKey = configuration["OpenAI:ApiKey"];
		}

		[HttpPost("ChatBox")]
		public async Task<IActionResult> ChatBox(ChatBoxRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.searchText))
			{
				return BadRequest("Câu hỏi không được để trống.");
			}

			string answer = string.Empty;
			try
			{
				var lowerText = request.searchText.ToLower();
				string productInfo = string.Empty;
				string serviceInfo = string.Empty;
				string fullPrompt;

				// Thêm logic đặt lịch
				string bookingResult = await HandleBookingRequest(request.searchText, request.UserId);
				if (!string.IsNullOrEmpty(bookingResult))
				{
					// Nếu đặt lịch thành công, thêm vào prompt để OpenAI xử lý thêm nếu cần
					request.searchText += $" {bookingResult}";
					return Ok(bookingResult);
				}

				bool isProductQuery = IsProductQuery(lowerText);
				bool isServiceQuery = IsServiceQuery(lowerText);

				if (isProductQuery && !isServiceQuery)
				{
					productInfo = await GetRelatedProductInfoAsync(request.searchText);
					fullPrompt = string.IsNullOrEmpty(productInfo)
						? $"Trả lời câu hỏi về sản phẩm thẩm mỹ viện: {request.searchText}. Nếu không tìm thấy, xin lỗi khách hàng và gợi ý các sản phẩm liên quan phổ biến (ví dụ: kem dưỡng, serum chống lão hóa)."
						: $"{productInfo}Dựa trên thông tin sản phẩm từ database trên, tư vấn cụ thể dựa trên yêu cầu khách hàng (ví dụ: loại da dầu, lấy lợi ích từ mô tả như 'kiềm dầu 12h'). Trình bày bước tư vấn: 1. Match yêu cầu với Name/Desc, Câu hỏi: {request.searchText}";
				}
				else if (isServiceQuery && !isProductQuery)
				{
					serviceInfo = await GetRelatedServiceInfoAsync(request.searchText);
					fullPrompt = string.IsNullOrEmpty(serviceInfo)
						? $"Trả lời câu hỏi về dịch vụ thẩm mỹ viện: {request.searchText}. Nếu không tìm thấy, xin lỗi khách hàng và gợi ý các dịch vụ liên quan phổ biến (ví dụ: nâng mũi, điều trị nám)."
						: $"{serviceInfo}Dựa trên thông tin dịch vụ từ database trên, tư vấn cụ thể dựa trên yêu cầu khách hàng (ví dụ: nâng mũi, lấy lợi ích từ mô tả như 'tự nhiên hài hòa'). Trình bày bước tư vấn: 1. Match yêu cầu với Name/Desc, Câu hỏi: {request.searchText}";
				}
				else
				{
					productInfo = await GetRelatedProductInfoAsync(request.searchText);
					serviceInfo = await GetRelatedServiceInfoAsync(request.searchText);
					fullPrompt = string.IsNullOrEmpty(productInfo) && string.IsNullOrEmpty(serviceInfo)
						? $"Trả lời câu hỏi về thẩm mỹ viện hoặc sản phẩm/dịch vụ liên quan: {request.searchText}. Nếu không tìm thấy, xin lỗi khách hàng và gợi ý các sản phẩm/dịch vụ phổ biến."
						: $"{productInfo}{serviceInfo}Dựa trên thông tin sản phẩm/dịch vụ từ database trên, trả lời câu hỏi: {request.searchText}";
				}

				var client = new ChatClient(model: "gpt-3.5-turbo", apiKey: _apiKey);
				var messages = new List<ChatMessage> { new UserChatMessage(fullPrompt) };
				var options = new ChatCompletionOptions { MaxOutputTokenCount = 1024 };
				ChatCompletion completion = await client.CompleteChatAsync(messages, options);

				if (completion.Content.Count > 0)
				{
					answer = completion.Content[0].Text;
				}
				else
				{
					answer = "Không nhận được phản hồi từ OpenAI.";
				}

				return Ok(answer);
			}
			catch (Exception ex)
			{
				return BadRequest($"Lỗi: {ex.Message}");
			}
		}

		private async Task<string> HandleBookingRequest(string searchText, int? userId)
		{
			// Pattern để match "Tôi muốn đặt lịch Dịch vụ [tên dịch vụ] vào ngày [dd/MM/yyyy]"
			var pattern = @"đặt lịch (?<serviceName>.+?) vào ngày (?<date>\d{2}/\d{2}/\d{4})";
			var match = Regex.Match(searchText, pattern, RegexOptions.IgnoreCase);

			if (!match.Success)
			{
				return string.Empty;
			}

			var serviceName = match.Groups["serviceName"].Value.Trim();
			var dateStr = match.Groups["date"].Value;

			// Parse ngày
			if (!DateTime.TryParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime scheduledDate))
			{
				return "Ngày không hợp lệ.";
			}

			

			// Tìm dịch vụ bằng tên
			var service = await _servicesRepository.GetServicessByServicesName(serviceName);
			if (service == null)
			{
				return "Không tìm thấy dịch vụ.";
			}

			// Lấy UserID từ HttpContext
			if (userId == null || userId <= 0)
			{
				return "Vui lòng đăng nhập để đặt lịch.";
			}

			// Tạo BookingRequest
			var bookingRequest = new BookingRequest
			{
				UserID = userId.Value,
				ServiceIDs = new List<int> { service },
				ScheduledDate = scheduledDate
			};

			// Gọi Insert_Booking
			var response = await _bookingsRepository.Insert_Booking(bookingRequest);
			if (response.ResponseCode != 1)
			{
				return $"Lỗi đặt lịch: {response.ResposeMessage}";
			}

			// Trả về thông báo thành công
			return $"Đã đặt lịch thành công cho dịch vụ {service} vào ngày {dateStr}. Mã đặt lịch: {response.Booking_AssData.FirstOrDefault()?.BookingID}.";
		}

		private bool IsProductQuery(string lowerText)
		{
			var productKeywords = new[] { "sản phẩm", "kem", "serum", "dưỡng", "chống nắng", "sữa rửa mặt", "nước hoa hồng", "tẩy trang", "gel", "mỹ phẩm" };
			return productKeywords.Any(k => lowerText.Contains(k));
		}

		private bool IsServiceQuery(string lowerText)
		{
			var serviceKeywords = new[] { "dịch vụ", "nâng", "mũi", "mông", "ngực", "phẫu thuật", "laser", "sụn", "điều trị", "dáng", "bọc", "cấu trúc", "sẹo", "rỗ", "độn", "line", "gò", "tiềm", "nám", "trẻ hóa", "botox", "filler", "bọc răng", "dán sứ", "tẩy trắng", "niềng", "trồng", "hàn" };
			return serviceKeywords.Any(k => lowerText.Contains(k));
		}

		private async Task<string> GetRelatedProductInfoAsync(string searchText)
		{
			var searchKeywords = ExtractKeywordsImproved(searchText);

			var products = await _productsRepository.SearchProductsAsync(searchKeywords);

			products = products.Where(p =>
				searchKeywords.Split(' ').Any(kw =>
					Fuzz.PartialRatio(p.ProductName.ToLower(), kw) > 80 ||
					Fuzz.PartialRatio(p.ProductDescription.ToLower(), kw) > 80
				)
			).ToList();

			if (!products.Any())
			{
				var fallbackProducts = await _productsRepository.SearchProductsAsync(searchKeywords);
				fallbackProducts = fallbackProducts.Where(p =>
					searchKeywords.Split(' ').Any(kw =>
						Fuzz.PartialRatio(p.ProductName.ToLower(), kw) > 60 ||
						Fuzz.PartialRatio(p.ProductDescription.ToLower(), kw) > 60
					)
				).ToList();

				if (fallbackProducts.Any())
				{
					var infoBuilder = new StringBuilder("Xin lỗi quý khách, không tìm thấy sản phẩm chính xác phù hợp với yêu cầu. Chúng tôi gợi ý các sản phẩm liên quan:\n");
					foreach (var product in fallbackProducts.Take(3))
					{
						var matchingDesc = ExtractMatchingPart(product.ProductDescription, searchKeywords);
						infoBuilder.AppendLine($"- Mã: {product.ProductID}, Tên: {product.ProductName}, Hình ảnh: {product.ProductImages}, Giá: {product.SellingPrice} VND, Mô tả: {product.ProductDescription}");
					}
					return infoBuilder.ToString();
				}
				return string.Empty;
			}

			var infoBuilderMain = new StringBuilder("Thông tin sản phẩm từ Aesthetics liên quan đến yêu cầu:\n");
			foreach (var product in products.Take(3))
			{
				var matchingDesc = ExtractMatchingPart(product.ProductDescription, searchKeywords);
				infoBuilderMain.AppendLine($"- Mã: {product.ProductID}, Tên: {product.ProductName}, Hình ảnh: {product.ProductImages}, Giá: {product.SellingPrice} VND, Mô tả đầy đủ: {product.ProductDescription}");
			}

			return infoBuilderMain.ToString();
		}

		private async Task<string> GetRelatedServiceInfoAsync(string searchText)
		{
			var searchKeywords = ExtractKeywordsImproved(searchText);

			var services = await _servicesRepository.SearchServicesAsync(searchKeywords);

			services = services.Where(s =>
				searchKeywords.Split(' ').Any(kw =>
					Fuzz.PartialRatio(s.ServiceName.ToLower(), kw) > 80 ||
					Fuzz.PartialRatio(s.Description.ToLower(), kw) > 80
				)
			).ToList();

			if (!services.Any())
			{
				var fallbackServices = await _servicesRepository.SearchServicesAsync(searchKeywords);
				fallbackServices = fallbackServices.Where(s =>
					searchKeywords.Split(' ').Any(kw =>
						Fuzz.PartialRatio(s.ServiceName.ToLower(), kw) > 60 ||
						Fuzz.PartialRatio(s.Description.ToLower(), kw) > 60
					)
				).ToList();

				if (fallbackServices.Any())
				{
					var infoBuilder = new StringBuilder("Xin lỗi quý khách, không tìm thấy dịch vụ chính xác phù hợp với yêu cầu. Chúng tôi gợi ý các dịch vụ liên quan:\n");
					foreach (var service in fallbackServices.Take(3))
					{
						var matchingDesc = ExtractMatchingPart(service.Description, searchKeywords);
						infoBuilder.AppendLine($"- Mã: {service.ServiceID}, Tên: {service.ServiceName}, Giá: {service.PriceService} VND, Mô tả: {service.Description}");
					}
					return infoBuilder.ToString();
				}
				return string.Empty;
			}

			var infoBuilderMain = new StringBuilder("Thông tin dịch vụ từ Aesthetics liên quan đến yêu cầu:\n");
			foreach (var service in services.Take(3))
			{
				var matchingDesc = ExtractMatchingPart(service.Description, searchKeywords);
				infoBuilderMain.AppendLine($"- Mã: {service.ServiceID}, Tên: {service.ServiceName}, Giá: {service.PriceService} VND, Mô tả đầy đủ: {service.Description}");
			}

			return infoBuilderMain.ToString();
		}

		private string ExtractKeywordsImproved(string searchText)
		{
			var stopWords = new[] { "thông tin", "giá", "sản phẩm", "về", "là", "gì", "tư vấn", "dành", "các", "cho tôi", "dịch vụ", "làm đẹp", "tại", "cửa", "hàng", "cửa hàng", "chi tiết", "của" };

			var cleanedText = searchText.ToLower();
			foreach (var stopWord in stopWords)
			{
				cleanedText = cleanedText.Replace(stopWord, "");
			}

			var words = cleanedText.Split(new[] { ' ', '.', ',', '?' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(w => w.Trim())
				.Where(w => !string.IsNullOrEmpty(w))
				.ToArray();

			return string.Join(" ", words);
		}

		private string ExtractMatchingPart(string description, string keywords)
		{
			var kwList = keywords.Split(' ');
			var sentences = description.Split(new[] { '.', '!' }, StringSplitOptions.RemoveEmptyEntries);
			var matching = sentences.FirstOrDefault(s => kwList.Any(kw => s.ToLower().Contains(kw)));
			return matching?.Trim() ?? "Không có phần cụ thể match, nhưng liên quan tổng thể.";
		}
	}
}