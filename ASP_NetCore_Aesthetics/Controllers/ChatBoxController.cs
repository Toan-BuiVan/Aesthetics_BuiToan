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

namespace ASP_NetCore_Aesthetics.Controllers 
{
	[Route("api/[controller]")] 
	[ApiController] 
	public class ChatBoxController : ControllerBase 
	{
		private IProductsRepository _productsRepository; 
		private IServicessRepository _servicesRepository;
		private IBookingsRepository _bookingRepository;
		private readonly string _apiKey; 

		public ChatBoxController(IProductsRepository productsRepository, IServicessRepository servicesRepository, 
			IConfiguration configuration, IBookingsRepository bookingRepository) 
		{
			_productsRepository = productsRepository; // Gán repository sản phẩm
			_servicesRepository = servicesRepository; // Gán repository dịch vụ
			_apiKey = configuration["OpenAI:ApiKey"]; // Lấy API key từ appsettings.json hoặc config source
			_bookingRepository = bookingRepository;
		}

		[HttpPost("ChatBox")] 
		public async Task<IActionResult> ChatBox(string searchText) 
		{
			if (string.IsNullOrWhiteSpace(searchText)) 
			{
				return BadRequest("Câu hỏi không được để trống."); 
			}

			string answer = string.Empty; // Khởi tạo biến lưu response từ AI
			try // Block try-catch để xử lý ngoại lệ
			{
				var lowerText = searchText.ToLower(); // Chuẩn hóa query về lowercase để matching case-insensitive
				string productInfo = string.Empty; // Biến lưu thông tin sản phẩm từ DB
				string serviceInfo = string.Empty; // Biến lưu thông tin dịch vụ từ DB
				string fullPrompt; // Biến lưu prompt đầy đủ gửi cho OpenAI

				bool isProductQuery = IsProductQuery(lowerText); // Kiểm tra nếu query liên quan sản phẩm dựa trên keyword
				bool isServiceQuery = IsServiceQuery(lowerText); // Kiểm tra nếu query liên quan dịch vụ dựa trên keyword

				if (isProductQuery && !isServiceQuery) // Nếu chỉ là query sản phẩm
				{
					productInfo = await GetRelatedProductInfoAsync(searchText); // Fetch async thông tin sản phẩm liên quan
					fullPrompt = string.IsNullOrEmpty(productInfo) // Xây dựng prompt động
						? $"Trả lời câu hỏi về sản phẩm thẩm mỹ viện: {searchText}. Nếu không tìm thấy, xin lỗi khách hàng và gợi ý các sản phẩm liên quan phổ biến (ví dụ: kem dưỡng, serum chống lão hóa)." 
						: $"{productInfo}Dựa trên thông tin sản phẩm từ database trên, tư vấn cụ thể dựa trên yêu cầu khách hàng (ví dụ: loại da dầu, lấy lợi ích từ mô tả như 'kiềm dầu 12h'). Trình bày bước tư vấn: 1. Match yêu cầu với Name/Desc, 2. Giải thích lợi ích cụ thể, 3. Gợi ý sử dụng. Câu hỏi: {searchText}"; // Prompt chi tiết nếu có data
				}
				else if (isServiceQuery && !isProductQuery) // Nếu chỉ là query dịch vụ
				{
					serviceInfo = await GetRelatedServiceInfoAsync(searchText); // Fetch async thông tin dịch vụ liên quan
					fullPrompt = string.IsNullOrEmpty(serviceInfo) // Xây dựng prompt động
						? $"Trả lời câu hỏi về dịch vụ thẩm mỹ viện: {searchText}. Nếu không tìm thấy, xin lỗi khách hàng và gợi ý các dịch vụ liên quan phổ biến (ví dụ: nâng mũi, điều trị nám)." 
						: $"{serviceInfo}Dựa trên thông tin dịch vụ từ database trên, tư vấn cụ thể dựa trên yêu cầu khách hàng (ví dụ: nâng mũi, lấy lợi ích từ mô tả như 'tự nhiên hài hòa'). Trình bày bước tư vấn: 1. Match yêu cầu với Name/Desc, 2. Giải thích lợi ích cụ thể, 3. Gợi ý sử dụng. Câu hỏi: {searchText}"; // Prompt chi tiết
				}
				else // Nếu cả hai hoặc không rõ loại
				{
					productInfo = await GetRelatedProductInfoAsync(searchText); // Fetch cả hai
					serviceInfo = await GetRelatedServiceInfoAsync(searchText);
					fullPrompt = string.IsNullOrEmpty(productInfo) && string.IsNullOrEmpty(serviceInfo) // Xây dựng prompt tổng hợp
						? $"Trả lời câu hỏi về thẩm mỹ viện hoặc sản phẩm/dịch vụ liên quan: {searchText}. Nếu không tìm thấy, xin lỗi khách hàng và gợi ý các sản phẩm/dịch vụ phổ biến." 
						: $"{productInfo}{serviceInfo}Dựa trên thông tin sản phẩm/dịch vụ từ database trên, trả lời câu hỏi: {searchText}"; // Prompt với data kết hợp
				}

				var client = new ChatClient(model: "gpt-3.5-turbo", apiKey: _apiKey); // Tạo client OpenAI với model GPT-3.5-turbo và API key
				var messages = new List<ChatMessage> { new UserChatMessage(fullPrompt) }; // Tạo list message chỉ với prompt user (không system prompt)
				var options = new ChatCompletionOptions { MaxOutputTokenCount = 1024 }; // Set options giới hạn output token để tránh response quá dài
				ChatCompletion completion = await client.CompleteChatAsync(messages, options); // Gọi async API OpenAI để generate response

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

		private bool IsProductQuery(string lowerText) // Hàm private kiểm tra query có phải về sản phẩm
		{
			var productKeywords = new[] { "sản phẩm", "kem", "serum", "dưỡng", "chống nắng", "sữa rửa mặt", "nước hoa hồng", "tẩy trang", "gel", "mỹ phẩm" }; // Mảng keyword sản phẩm tiếng Việt
			return productKeywords.Any(k => lowerText.Contains(k)); // Trả true nếu query chứa bất kỳ keyword (matching đơn giản, không fuzzy)
		}

		private bool IsServiceQuery(string lowerText) // Hàm private kiểm tra query có phải về dịch vụ
		{
			var serviceKeywords = new[] { "dịch vụ", "nâng", "mũi", "mông", "ngực", "phẫu thuật", "laser", "sụn", "điều trị", "dáng", "bọc", "cấu trúc", "sẹo", "rỗ", "độn", "line", "gò", "tiềm", "nám", "trẻ hóa", "botox", "filler", "bọc răng", "dán sứ", "tẩy trắng", "niềng", "trồng", "hàn" }; // Mảng keyword dịch vụ thẩm mỹ
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
				// Fallback: Lower threshold (>60) for related suggestions based on request keywords // Fallback với threshold lỏng hơn cho gợi ý liên quan
				var fallbackProducts = await _productsRepository.SearchProductsAsync(searchKeywords); // Fetch lại (có thể tối ưu bằng cách reuse products gốc)
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
						infoBuilder.AppendLine($"- Tên: {product.ProductName}, Mô tả: {product.ProductDescription}, Giá: {product.SellingPrice} VND."); 
					}
					return infoBuilder.ToString(); 
				}
				return string.Empty; 
			}

			var infoBuilderMain = new StringBuilder("Thông tin sản phẩm từ Aesthetics liên quan đến yêu cầu:\n"); 
			foreach (var product in products.Take(3)) 
			{
				var matchingDesc = ExtractMatchingPart(product.ProductDescription, searchKeywords); 
				infoBuilderMain.AppendLine($"-Mã: {product.ProductID}, Tên: {product.ProductName}, Mô tả đầy đủ: {product.ProductDescription}, Hình ảnh: {product.ProductImages}, Giá: {product.SellingPrice} VND, Phần liên quan yêu cầu: '{matchingDesc}'"); // Thêm info chi tiết
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
						infoBuilder.AppendLine($"- Mã: {service.ServiceID}, Tên: {service.ServiceName}, Mô tả: {service.Description}, Giá: {service.PriceService} VND.");
					}
					return infoBuilder.ToString();
				}
				return string.Empty;
			}

			var infoBuilderMain = new StringBuilder("Thông tin dịch vụ từ Aesthetics liên quan đến yêu cầu:\n");
			foreach (var service in services.Take(3))
			{
				var matchingDesc = ExtractMatchingPart(service.Description, searchKeywords);
				infoBuilderMain.AppendLine($"- Mã: {service.ServiceID}, Tên: {service.ServiceName}, Mô tả đầy đủ: {service.Description}, Giá: {service.PriceService} VND.");
			}

			return infoBuilderMain.ToString();
		}

		private string ExtractKeywordsImproved(string searchText) // Hàm trích xuất keyword cải tiến
		{
			var stopWords = new[] { "thông tin", "giá", "sản phẩm", "về", "là", "gì", "tư vấn", "dành", "các", "cho tôi", "dịch vụ", "làm đẹp", "tại", "cửa", "hàng", "cửa hàng", "chi tiết", "của" }; // Mảng stop words/phrases tiếng Việt

			// Replace stop phrases first to preserve compound keywords // Xóa stop phrases trước để giữ nguyên compound keyword (ví dụ: "nâng mũi" không bị tách)
			var cleanedText = searchText.ToLower(); // Lowercase
			foreach (var stopWord in stopWords)
			{
				cleanedText = cleanedText.Replace(stopWord, ""); // Xóa từng stop word
			}

			// Split and filter remaining single stop words // Split text thành words, loại bỏ empty
			var words = cleanedText.Split(new[] { ' ', '.', ',', '?' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(w => w.Trim())
				.Where(w => !string.IsNullOrEmpty(w))
				.ToArray();

			return string.Join(" ", words); // Join words thành string keyword
		}

		private string ExtractMatchingPart(string description, string keywords) // Hàm trích phần desc khớp với keyword
		{
			var kwList = keywords.Split(' '); // Split keyword thành list
			var sentences = description.Split(new[] { '.', '!' }, StringSplitOptions.RemoveEmptyEntries); // Split desc thành sentences dựa trên dấu câu
			var matching = sentences.FirstOrDefault(s => kwList.Any(kw => s.ToLower().Contains(kw))); // Tìm sentence đầu chứa bất kỳ keyword
			return matching?.Trim() ?? "Không có phần cụ thể match, nhưng liên quan tổng thể."; // Trả matching hoặc default message
		}
	}
}