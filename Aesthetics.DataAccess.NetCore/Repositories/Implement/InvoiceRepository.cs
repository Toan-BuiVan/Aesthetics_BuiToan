using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponseInvoice_Loggin;
using BE_102024.DataAces.NetCore.CheckConditions;
using BE_102024.DataAces.NetCore.Dapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class InvoiceRepository : BaseApplicationService, IInvoiceRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		private IUserRepository _userRepository;
		private IVouchersRepository _vouchersRepository;
		private IProductsRepository _productsRepository;
		private IServicessRepository _servicessRepository;
		public InvoiceRepository(DB_Context context, IConfiguration configuration,
			IServiceProvider serviceProvider, IUserRepository userRepository,
			IVouchersRepository vouchersRepository, 
			IProductsRepository productsRepository,
			IServicessRepository servicessRepository) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
			_userRepository = userRepository;
			_vouchersRepository = vouchersRepository;
			_productsRepository = productsRepository;
			_servicessRepository = servicessRepository;
		}

		//public async Task<ResponseInvoice_Loggin> Insert_Invoice(InvoiceRequest insert_)
		//{
		//	//Tổng giá trị đơn hàng trước khi áp dụng voucher

		//	decimal totalMoney = 0;
		//	var returnData = new ResponseInvoice_Loggin();
		//	var invoiceOut_Loggin = new List<Invoice_Loggin_Ouput>();
		//	var invoiceDetailOut_Loggin = new List<InvoiceDetail_Loggin_Ouput>();
		//	using var transaction = await _context.Database.BeginTransactionAsync();
		//	try
		//	{
		//		Users empployee = null;
		//		Vouchers vouchers = null;
		//		var customer = await _userRepository.GetUserByUserID(insert_.CustomerID);

		//		//1. Kiểm tra employee
		//		if (insert_.EmployeeID != null)
		//		{
		//			empployee = await _userRepository.GetUserByUserID(insert_.EmployeeID);
		//			if (empployee == null || empployee.TypePerson != "Employee")
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "EmployeeID không tồn tại!";
		//				return returnData;
		//			}
		//		}

		//		//2. Kiểm tra Customer
		//		if (customer == null)
		//		{
		//			returnData.ResponseCode = -1;
		//			returnData.ResposeMessage = "CustomerID không tồn tại!";
		//			return returnData;
		//		}

		//		//3. Kiểm tra danh sách đầu vào của Product & Services
		//		if (insert_.ProductIDs == null && insert_.ServicesIDs == null)
		//		{
		//			returnData.ResponseCode = -1;
		//			returnData.ResposeMessage = "Vui lòng chọn ít nhất 1 Product || Servicess!";
		//			return returnData;
		//		}

		//		//4. Tính tổng giá trị đơn hàng
		//		if (insert_.ProductIDs != null)
		//		{
		//			for (int i = 0; i < insert_.ProductIDs.Count; i++)
		//			{
		//				var product = await _productsRepository.GetProductsByProductID(insert_.ProductIDs[i]);
		//				if (product == null) continue;
		//				totalMoney += insert_.QuantityProduct[i] * product.SellingPrice ?? 0;
		//			}
		//		}
		//		if (insert_.ServicesIDs != null)
		//		{
		//			for (int i = 0; i < insert_.ServicesIDs.Count; i++)
		//			{
		//				var service = await _servicessRepository.GetServicessByServicesID(insert_.ServicesIDs[i]);
		//				if (service == null) continue;
		//				totalMoney += insert_.QuantityServices[i] * service.PriceService ?? 0;
		//			}
		//		}

		//		//5. Kiểm tra VouchersID
		//		if (insert_.VoucherID != null)
		//		{
		//			vouchers = await _vouchersRepository.GetVouchersByVouchersID(insert_.VoucherID ?? 0);
		//			var wallets = await _context.Wallets.Where(s => s.VoucherID == insert_.VoucherID && s.UserID == insert_.CustomerID).FirstOrDefaultAsync();
		//			if (wallets == null)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = $"Bạn chưa sở hữu VouchersID: {insert_.VoucherID}!";
		//				return returnData;
		//			}
		//			if (vouchers == null)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "VoucherID không tồn tại!";
		//				return returnData;
		//			}
		//			if (vouchers.EndDate < DateTime.Now)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "Voucher đã hết hạn!";
		//				return returnData;
		//			}
		//			if (totalMoney < vouchers.MinimumOrderValue)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = $"Voucher chỉ áp dụng cho đơn hàng tối thiểu {vouchers.MinimumOrderValue}!";
		//				return returnData;
		//			}
		//		}

		//		//1. Tạo hóa đơn
		//		var discount = vouchers?.DiscountValue ?? 0; 
		//		var maxDiscountValue = vouchers?.MaxValue ?? 0;
		//		var totalMonyVoucher = totalMoney * ((decimal)discount / 100);
		//		var finalDiscount = totalMonyVoucher > maxDiscountValue ? maxDiscountValue : totalMonyVoucher;
		//		var newInvoice = new Invoice
		//		{
		//			EmployeeID = empployee?.UserID,
		//			CustomerID = customer.UserID,
		//			VoucherID = vouchers?.VoucherID,
		//			Code = vouchers?.Code,
		//			DiscountValue = vouchers?.DiscountValue,
		//			TotalMoney = totalMoney - finalDiscount,
		//			DateCreated = DateTime.Now,
		//			Status = "Pending",
		//			DeleteStatus = 1,
		//			Type = "Output",
		//			OrderStatus = "Mặc Định"
		//		};
		//		await _context.Invoice.AddAsync(newInvoice);
		//		await _context.SaveChangesAsync();
		//		invoiceOut_Loggin.Add(new Invoice_Loggin_Ouput
		//		{
		//			InvoiceID = newInvoice.InvoiceID,
		//			EmployeeID = newInvoice.EmployeeID,
		//			CustomerID = newInvoice.CustomerID,
		//			VoucherID = newInvoice.VoucherID,
		//			Code = vouchers?.Code,
		//			DiscountValue = vouchers?.DiscountValue,
		//			TotalMoney = newInvoice.TotalMoney,
		//			DateCreated = newInvoice.DateCreated,
		//			Status = newInvoice.Status,
		//			DeleteStatus = newInvoice.DeleteStatus,
		//			Type = newInvoice.Type,
		//			OrderStatus = newInvoice.OrderStatus
		//		});

		//		//2. Tạo chi tiết hóa đơn
		//		//2.1 Tạo chi tiết hóa đơn khi có Product & Servicess
		//		if (insert_.ProductIDs != null && insert_.ServicesIDs != null && insert_.ProductIDs.Count > 0 && insert_.ServicesIDs.Count > 0)
		//		{
		//			int minCount = Math.Min(insert_.ProductIDs.Count, insert_.ServicesIDs.Count);
		//			for (int i = 0; i < minCount; i++)
		//			{
		//				var productID = insert_.ProductIDs[i];
		//				var quantityProduct = insert_.QuantityProduct[i];
		//				var product = await _productsRepository.GetProductsByProductID(productID);

		//				var servicesID = insert_.ServicesIDs[i];
		//				var quantityService = insert_.QuantityServices[i];
		//				var service = await _servicessRepository.GetServicessByServicesID(servicesID);
		//				if (product == null)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ProductID: {productID} không hợp lệ!";
		//					return returnData;
		//				}
		//				if (quantityProduct <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Quantity ProductID: {productID} không hợp lệ!";
		//					return returnData;
		//				}
		//				if (quantityProduct > product.Quantity)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Quantity ProductID: {productID} vượt quá số lượng của cửa hàng hiện có!";
		//					return returnData;
		//				}
		//				if (service == null)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ServiceID: {servicesID} không hợp lệ!";
		//					return returnData;
		//				}
		//				if (quantityService <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Quantity ServiceID: {servicesID} không hợp lệ!";
		//					return returnData;
		//				}
		//				var newInvoiceDetail = new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = empployee?.UserName,
		//					ProductID = product.ProductID,
		//					ProductName = product.ProductName,
		//					ServiceID = service.ServiceID,
		//					ServiceName = service.ServiceName,
		//					VoucherID = vouchers?.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceProduct = product.SellingPrice,
		//					PriceService = service.PriceService,
		//					TotalQuantityProduct = quantityProduct,
		//					TotalQuantityService = quantityService,
		//					TotalMoney = (quantityProduct * product.SellingPrice)
		//								+ (quantityService * service.PriceService),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//					StatusComment = 1
		//				};
		//				await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
		//				await _context.SaveChangesAsync();
		//				invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
		//				{
		//					InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoiceDetail.CustomerID,
		//					CustomerName = newInvoiceDetail.CustomerName,
		//					EmployeeID = newInvoiceDetail.EmployeeID,
		//					EmployeeName = newInvoiceDetail.EmployeeName,
		//					ProductID = newInvoiceDetail.ProductID,
		//					ProductName = newInvoiceDetail.ProductName,
		//					ServiceID = newInvoiceDetail.ServiceID,
		//					ServiceName = newInvoiceDetail.ServiceName,
		//					VoucherID = newInvoiceDetail.VoucherID ?? 0,
		//					Code = newInvoiceDetail.Code,
		//					DiscountValue = newInvoiceDetail.DiscountValue,
		//					PriceProduct = newInvoiceDetail.PriceProduct,
		//					PriceService = newInvoiceDetail.PriceService,
		//					TotalQuantityService = newInvoiceDetail.TotalQuantityService,
		//					TotalQuantityProduct = newInvoiceDetail.TotalQuantityProduct,
		//					TotalMoney = newInvoiceDetail.TotalMoney,
		//					DeleteStatus = newInvoiceDetail.DeleteStatus,
		//					Status = newInvoiceDetail.Status,
		//					Type = newInvoiceDetail.Type,
		//					StatusComment = newInvoiceDetail.StatusComment
		//				});
		//			}

		//			// Nếu có sản phẩm dư, tạo bản ghi riêng
		//			for (int i = minCount; i < insert_.ProductIDs.Count; i++)
		//			{
		//				var productID = insert_.ProductIDs[i];
		//				var quantityProduct = insert_.QuantityProduct[i];
		//				var product = await _productsRepository.GetProductsByProductID(productID);
		//				if (product == null)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ProductID: {productID} không hợp lệ!";
		//					return returnData;
		//				}
		//				if (quantityProduct <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Quantity ProductID: {productID} không hợp lệ!";
		//					return returnData;
		//				}
		//				var newInvoiceDetail = new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = empployee?.UserName,
		//					ProductID = product.ProductID,
		//					ProductName = product.ProductName,
		//					VoucherID = newInvoice.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceProduct = product.SellingPrice,
		//					TotalQuantityProduct = quantityProduct,
		//					TotalMoney = quantityProduct * product.SellingPrice,
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//				};
		//				await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
		//				await _context.SaveChangesAsync();
		//				invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
		//				{
		//					InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoiceDetail.CustomerID,
		//					CustomerName = newInvoiceDetail.CustomerName,
		//					EmployeeID = newInvoiceDetail.EmployeeID,
		//					EmployeeName = newInvoiceDetail.EmployeeName,
		//					ProductID = newInvoiceDetail.ProductID,
		//					ProductName = newInvoiceDetail.ProductName,
		//					VoucherID = newInvoiceDetail.VoucherID ?? 0,
		//					Code = newInvoiceDetail.Code,
		//					DiscountValue = newInvoiceDetail.DiscountValue,
		//					PriceProduct = newInvoiceDetail.PriceProduct,
		//					TotalQuantityProduct = newInvoiceDetail.TotalQuantityProduct,
		//					TotalMoney = newInvoiceDetail.TotalMoney,
		//					DeleteStatus = newInvoiceDetail.DeleteStatus,
		//					Status = newInvoiceDetail.Status,
		//					Type = newInvoiceDetail.Type,
		//				});
		//			}

		//			// Nếu có dịch vụ dư, tạo bản ghi riêng
		//			for (int i = minCount; i < insert_.ServicesIDs.Count; i++)
		//			{
		//				var servicesID = insert_.ServicesIDs[i];
		//				var quantityService = insert_.QuantityServices[i];
		//				var service = await _servicessRepository.GetServicessByServicesID(servicesID);
		//				if (service == null)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ServiceID: {servicesID} không hợp lệ!";
		//					return returnData;
		//				}
		//				if (quantityService <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Quantity ServiceID: {servicesID} không hợp lệ!";
		//					return returnData;
		//				}

		//				var newInvoiceDetail = new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = empployee?.UserName,
		//					ServiceID = service.ServiceID,
		//					ServiceName = service.ServiceName,
		//					VoucherID = newInvoice.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceService = service.PriceService,
		//					TotalQuantityService = quantityService,
		//					TotalMoney = quantityService * service.PriceService,
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//				};
		//				await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
		//				await _context.SaveChangesAsync();
		//				invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
		//				{
		//					InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoiceDetail.CustomerID,
		//					CustomerName = newInvoiceDetail.CustomerName,
		//					EmployeeID = newInvoiceDetail.EmployeeID,
		//					EmployeeName = newInvoiceDetail.EmployeeName,
		//					ServiceID = newInvoiceDetail.ServiceID,
		//					ServiceName = newInvoiceDetail.ServiceName,
		//					VoucherID = newInvoiceDetail.VoucherID ?? 0,
		//					Code = newInvoiceDetail.Code,
		//					DiscountValue = newInvoiceDetail.DiscountValue,
		//					PriceService = newInvoiceDetail.PriceService,
		//					TotalQuantityService = newInvoiceDetail.TotalQuantityService,
		//					TotalMoney = newInvoiceDetail.TotalMoney,
		//					DeleteStatus = newInvoiceDetail.DeleteStatus,
		//					Status = newInvoiceDetail.Status,
		//					Type = newInvoiceDetail.Type,
		//				});
		//			}
		//		}

		//		//2.2 Tạo chi tiết hóa đơn khi có Product không có Services
		//		if (insert_.ProductIDs != null && insert_.ProductIDs.Count >= 1 && (insert_.ServicesIDs == null || insert_.ServicesIDs.Count == 0))
		//		{
		//			int productCount = insert_.ProductIDs.Count;

		//			for (int i = 0; i < productCount; i++)
		//			{
		//				var productID = insert_.ProductIDs[i];
		//				var quantity = insert_.QuantityProduct[i];

		//				var product = await _productsRepository.GetProductsByProductID(productID);
		//				if (product == null)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ProductID: {productID} không tồn tại!";
		//					return returnData;
		//				}
		//				if (quantity <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Quantity ProductID: {productID} không hợp lệ!";
		//					return returnData;
		//				}
		//				if ( quantity > product.Quantity)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Quantity ProductID: {productID} vượt quá số lượng của cửa hàng hiện có!";
		//					return returnData;
		//				}

		//				var newInvoiceDetail = new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = empployee?.UserName,
		//					ProductID = product.ProductID,
		//					ProductName = product.ProductName,
		//					VoucherID = newInvoice.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceProduct = product.SellingPrice,
		//					TotalQuantityProduct = quantity,
		//					TotalMoney = quantity * product.SellingPrice,
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//					StatusComment = 1
		//				};

		//				await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
		//				await _context.SaveChangesAsync();

		//				invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
		//				{
		//					InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
		//					CustomerID = newInvoiceDetail.CustomerID,
		//					CustomerName = newInvoiceDetail.CustomerName,
		//					EmployeeID = newInvoiceDetail.EmployeeID,
		//					EmployeeName = newInvoiceDetail.EmployeeName,
		//					ProductID = newInvoiceDetail.ProductID,
		//					ProductName = newInvoiceDetail.ProductName,
		//					VoucherID = newInvoiceDetail.VoucherID ?? 0,
		//					Code = newInvoiceDetail.Code,
		//					DiscountValue = newInvoiceDetail.DiscountValue,
		//					PriceProduct = newInvoiceDetail.PriceProduct,
		//					TotalQuantityProduct = newInvoiceDetail.TotalQuantityProduct,
		//					TotalMoney = newInvoiceDetail.TotalMoney,
		//					DeleteStatus = newInvoiceDetail.DeleteStatus,
		//					Status = newInvoiceDetail.Status,
		//					Type = newInvoiceDetail.Type,
		//					StatusComment = newInvoiceDetail.StatusComment
		//				});
		//			}
		//		}

		//		// 2.3 Tạo chi tiết hóa đơn khi có Services không có Product
		//		if (insert_.ServicesIDs != null && insert_.ServicesIDs.Count > 0 && (insert_.ProductIDs == null || insert_.ProductIDs.Count == 0))
		//		{
		//			if (insert_.QuantityServices == null || insert_.ServicesIDs.Count != insert_.QuantityServices.Count)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "Số lượng dịch vụ không khớp với danh sách dịch vụ.";
		//				return returnData;
		//			}

		//			for (int i = 0; i < insert_.ServicesIDs.Count; i++)
		//			{
		//				var servicesID = insert_.ServicesIDs[i];
		//				var quantity = insert_.QuantityServices[i];

		//				var servicess = await _servicessRepository.GetServicessByServicesID(servicesID);
		//				if (servicess == null)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ServiceID: {servicesID} không tồn tại!";
		//					return returnData;
		//				}

		//				if (quantity <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Vui lòng nhập lại số lượng của ServiceID: {servicesID}!";
		//					return returnData;
		//				}

		//				var newInvoiceDetail = new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = empployee?.UserName,
		//					ServiceID = servicess.ServiceID,
		//					ServiceName = servicess.ServiceName,
		//					VoucherID = insert_.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceService = servicess.PriceService,
		//					TotalQuantityService = quantity,
		//					TotalMoney = quantity * servicess.PriceService,
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//					StatusComment = 1
		//				};

		//				await _context.InvoiceDetail.AddAsync(newInvoiceDetail);
		//				await _context.SaveChangesAsync();

		//				invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
		//				{
		//					InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
		//					CustomerID = newInvoiceDetail.CustomerID,
		//					CustomerName = newInvoiceDetail.CustomerName,
		//					EmployeeID = newInvoiceDetail.EmployeeID,
		//					EmployeeName = newInvoiceDetail.EmployeeName,
		//					ServiceID = newInvoiceDetail.ServiceID,
		//					ServiceName = newInvoiceDetail.ServiceName,
		//					VoucherID = newInvoiceDetail.VoucherID ?? 0,
		//					Code = newInvoiceDetail.Code,
		//					DiscountValue = newInvoiceDetail.DiscountValue,
		//					PriceService = newInvoiceDetail.PriceService,
		//					TotalQuantityService = newInvoiceDetail.TotalQuantityService,
		//					TotalMoney = newInvoiceDetail.TotalMoney,
		//					DeleteStatus = newInvoiceDetail.DeleteStatus,
		//					Status = newInvoiceDetail.Status,
		//					Type = newInvoiceDetail.Type,
		//					StatusComment = newInvoiceDetail.StatusComment
		//				});
		//			}
		//		}
		//		if (insert_.ProductIDs != null)
		//		{
		//			var cartProductsToDelete = await _context.CartProduct
		//				.Where(cp => insert_.ProductIDs.Contains(cp.ProductID.Value))
		//				.ToListAsync();

		//			_context.CartProduct.RemoveRange(cartProductsToDelete);
		//		}

		//		await _context.SaveChangesAsync();
		//		await transaction.CommitAsync();
		//		returnData.ResponseCode = 1;
		//		returnData.ResposeMessage = $"Insert_Invoice thành công!";
		//		returnData.InvoiceID = newInvoice.InvoiceID;
		//		returnData.TotalMoney = totalMoney;
		//		returnData.invoiceOut_Loggin = invoiceOut_Loggin;
		//		returnData.invoiceDetailOut_Loggin = invoiceDetailOut_Loggin;
		//		return returnData;

		//	}
		//	catch (Exception ex)
		//	{
		//		await transaction.RollbackAsync();
		//		throw new Exception($"Error in Insert_Invoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
		//	}
		//}

		//public async Task<ResponseInvoice_Loggin> Insert_Invoice(InvoiceRequest insert_)
		//{
		//	// Tổng giá trị đơn hàng trước khi áp dụng voucher
		//	decimal totalMoney = 0;
		//	var returnData = new ResponseInvoice_Loggin();
		//	var invoiceOut_Loggin = new List<Invoice_Loggin_Ouput>();
		//	var invoiceDetailOut_Loggin = new List<InvoiceDetail_Loggin_Ouput>();

		//	using var transaction = await _context.Database.BeginTransactionAsync();
		//	try
		//	{
		//		Users employee = null;
		//		Vouchers vouchers = null;
		//		var customer = await _userRepository.GetUserByUserID(insert_.CustomerID);

		//		// 1. Kiểm tra Employee
		//		if (insert_.EmployeeID != null)
		//		{
		//			employee = await _userRepository.GetUserByUserID(insert_.EmployeeID);
		//			if (employee == null || employee.TypePerson != "Employee")
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "EmployeeID không tồn tại!";
		//				return returnData;
		//			}
		//		}

		//		// 2. Kiểm tra Customer
		//		if (customer == null)
		//		{
		//			returnData.ResponseCode = -1;
		//			returnData.ResposeMessage = "CustomerID không tồn tại!";
		//			return returnData;
		//		}

		//		// 3. Kiểm tra danh sách Product & Services đầu vào
		//		if ((insert_.ProductIDs == null || insert_.ProductIDs.Count == 0) &&
		//			(insert_.ServicesIDs == null || insert_.ServicesIDs.Count == 0))
		//		{
		//			returnData.ResponseCode = -1;
		//			returnData.ResposeMessage = "Vui lòng chọn ít nhất 1 Product hoặc Service!";
		//			return returnData;
		//		}

		//		// 4. Lấy thông tin Product và Service
		//		var products = insert_.ProductIDs != null && insert_.ProductIDs.Count > 0
		//			? await GetProductsByProductIDs(insert_.ProductIDs)
		//			: new List<Products>();
		//		var services = insert_.ServicesIDs != null && insert_.ServicesIDs.Count > 0
		//			? await GetServicessByServicesIDs(insert_.ServicesIDs)
		//			: new List<Servicess>();

		//		var productDict = products.ToDictionary(p => p.ProductID, p => p);
		//		var serviceDict = services.ToDictionary(s => s.ServiceID, s => s);

		//		// 5. Tính tổng giá trị đơn hàng
		//		if (insert_.ProductIDs != null && insert_.ProductIDs.Count > 0)
		//		{
		//			for (int i = 0; i < insert_.ProductIDs.Count; i++)
		//			{
		//				var productId = insert_.ProductIDs[i];
		//				if (productDict.TryGetValue(productId, out var product))
		//				{
		//					totalMoney += insert_.QuantityProduct[i] * (product.SellingPrice ?? 0);
		//				}
		//			}
		//		}
		//		if (insert_.ServicesIDs != null && insert_.ServicesIDs.Count > 0)
		//		{
		//			for (int i = 0; i < insert_.ServicesIDs.Count; i++)
		//			{
		//				var serviceId = insert_.ServicesIDs[i];
		//				if (serviceDict.TryGetValue(serviceId, out var service))
		//				{
		//					totalMoney += insert_.QuantityServices[i] * (service.PriceService ?? 0);
		//				}
		//			}
		//		}

		//		// 6. Kiểm tra Voucher
		//		if (insert_.VoucherID != null)
		//		{
		//			vouchers = await _vouchersRepository.GetVouchersByVouchersID(insert_.VoucherID ?? 0);
		//			var wallets = await _context.Wallets
		//				.Where(s => s.VoucherID == insert_.VoucherID && s.UserID == insert_.CustomerID)
		//				.FirstOrDefaultAsync();
		//			if (wallets == null)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = $"Bạn chưa sở hữu VouchersID: {insert_.VoucherID}!";
		//				return returnData;
		//			}
		//			if (vouchers == null)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "VoucherID không tồn tại!";
		//				return returnData;
		//			}
		//			if (vouchers.EndDate < DateTime.Now)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "Voucher đã hết hạn!";
		//				return returnData;
		//			}
		//			if (totalMoney < vouchers.MinimumOrderValue)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = $"Voucher chỉ áp dụng cho đơn hàng tối thiểu {vouchers.MinimumOrderValue}!";
		//				return returnData;
		//			}
		//		}

		//		// 7. Tạo hóa đơn mới
		//		var discount = vouchers?.DiscountValue ?? 0;
		//		var maxDiscountValue = vouchers?.MaxValue ?? 0;
		//		var totalMoneyVoucher = totalMoney * ((decimal)discount / 100);
		//		var finalDiscount = totalMoneyVoucher > maxDiscountValue ? maxDiscountValue : totalMoneyVoucher;

		//		var newInvoice = new Invoice
		//		{
		//			EmployeeID = employee?.UserID,
		//			CustomerID = customer.UserID,
		//			VoucherID = vouchers?.VoucherID,
		//			Code = vouchers?.Code,
		//			DiscountValue = vouchers?.DiscountValue,
		//			TotalMoney = totalMoney - finalDiscount,
		//			DateCreated = DateTime.Now,
		//			Status = "Pending",
		//			DeleteStatus = 1,
		//			Type = "Output",
		//			OrderStatus = "Mặc Định",
		//			PaymentMethod = "Mặc Định"
		//		};
		//		await _context.Invoice.AddAsync(newInvoice);
		//		await _context.SaveChangesAsync();

		//		invoiceOut_Loggin.Add(new Invoice_Loggin_Ouput
		//		{
		//			InvoiceID = newInvoice.InvoiceID,
		//			EmployeeID = newInvoice.EmployeeID,
		//			CustomerID = newInvoice.CustomerID,
		//			VoucherID = newInvoice.VoucherID,
		//			Code = vouchers?.Code,
		//			DiscountValue = vouchers?.DiscountValue,
		//			TotalMoney = newInvoice.TotalMoney,
		//			DateCreated = newInvoice.DateCreated,
		//			Status = newInvoice.Status,
		//			DeleteStatus = newInvoice.DeleteStatus,
		//			Type = newInvoice.Type,
		//			OrderStatus = newInvoice.OrderStatus,
		//			PaymentMethod = newInvoice.PaymentMethod
		//		});

		//		// 8. Tạo danh sách InvoiceDetail
		//		var invoiceDetails = new List<InvoiceDetail>();

		//		// 8.1 Trường hợp có cả Product và Service
		//		if (insert_.ProductIDs != null && insert_.ServicesIDs != null &&
		//			insert_.ProductIDs.Count > 0 && insert_.ServicesIDs.Count > 0)
		//		{
		//			int minCount = Math.Min(insert_.ProductIDs.Count, insert_.ServicesIDs.Count);
		//			for (int i = 0; i < minCount; i++)
		//			{
		//				var productId = insert_.ProductIDs[i];
		//				var quantityProduct = insert_.QuantityProduct[i];
		//				var product = productDict.ContainsKey(productId) ? productDict[productId] : null;

		//				var serviceId = insert_.ServicesIDs[i];
		//				var quantityService = insert_.QuantityServices[i];
		//				var service = serviceDict.ContainsKey(serviceId) ? serviceDict[serviceId] : null;

		//				if (product == null || service == null || quantityProduct <= 0 || quantityService <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Dữ liệu ProductID: {productId} hoặc ServiceID: {serviceId} không hợp lệ!";
		//					return returnData;
		//				}
		//				if (quantityProduct > product.Quantity)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Số lượng ProductID: {productId} vượt quá số lượng hiện có!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ProductID = product.ProductID,
		//					ProductName = product.ProductName,
		//					ServiceID = service.ServiceID,
		//					ServiceName = service.ServiceName,
		//					VoucherID = vouchers?.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceProduct = product.SellingPrice,
		//					PriceService = service.PriceService,
		//					TotalQuantityProduct = quantityProduct,
		//					TotalQuantityService = quantityService,
		//					TotalMoney = (quantityProduct * (product.SellingPrice ?? 0)) + (quantityService * (service.PriceService ?? 0)),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//					StatusComment = 1
		//				});
		//			}

		//			// Xử lý Product dư
		//			for (int i = minCount; i < insert_.ProductIDs.Count; i++)
		//			{
		//				var productId = insert_.ProductIDs[i];
		//				var quantityProduct = insert_.QuantityProduct[i];
		//				var product = productDict.ContainsKey(productId) ? productDict[productId] : null;

		//				if (product == null || quantityProduct <= 0 || quantityProduct > product.Quantity)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ProductID: {productId} không hợp lệ hoặc số lượng vượt quá!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ProductID = product.ProductID,
		//					ProductName = product.ProductName,
		//					VoucherID = newInvoice.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceProduct = product.SellingPrice,
		//					TotalQuantityProduct = quantityProduct,
		//					TotalMoney = quantityProduct * (product.SellingPrice ?? 0),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type
		//				});
		//			}

		//			// Xử lý Service dư
		//			for (int i = minCount; i < insert_.ServicesIDs.Count; i++)
		//			{
		//				var serviceId = insert_.ServicesIDs[i];
		//				var quantityService = insert_.QuantityServices[i];
		//				var service = serviceDict.ContainsKey(serviceId) ? serviceDict[serviceId] : null;

		//				if (service == null || quantityService <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ServiceID: {serviceId} không hợp lệ!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ServiceID = service.ServiceID,
		//					ServiceName = service.ServiceName,
		//					VoucherID = newInvoice.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceService = service.PriceService,
		//					TotalQuantityService = quantityService,
		//					TotalMoney = quantityService * (service.PriceService ?? 0),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type
		//				});
		//			}
		//		}

		//		// 8.2 Trường hợp chỉ có Product
		//		else if (insert_.ProductIDs != null && insert_.ProductIDs.Count > 0)
		//		{
		//			for (int i = 0; i < insert_.ProductIDs.Count; i++)
		//			{
		//				var productId = insert_.ProductIDs[i];
		//				var quantity = insert_.QuantityProduct[i];
		//				var product = productDict.ContainsKey(productId) ? productDict[productId] : null;

		//				if (product == null || quantity <= 0 || quantity > product.Quantity)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ProductID: {productId} không hợp lệ hoặc số lượng vượt quá!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ProductID = product.ProductID,
		//					ProductName = product.ProductName,
		//					VoucherID = newInvoice.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceProduct = product.SellingPrice,
		//					TotalQuantityProduct = quantity,
		//					TotalMoney = quantity * (product.SellingPrice ?? 0),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//					StatusComment = 1
		//				});
		//			}
		//		}

		//		// 8.3 Trường hợp chỉ có Service
		//		else if (insert_.ServicesIDs != null && insert_.ServicesIDs.Count > 0)
		//		{
		//			for (int i = 0; i < insert_.ServicesIDs.Count; i++)
		//			{
		//				var serviceId = insert_.ServicesIDs[i];
		//				var quantity = insert_.QuantityServices[i];
		//				var service = serviceDict.ContainsKey(serviceId) ? serviceDict[serviceId] : null;

		//				if (service == null || quantity <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ServiceID: {serviceId} không hợp lệ!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ServiceID = service.ServiceID,
		//					ServiceName = service.ServiceName,
		//					VoucherID = insert_.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceService = service.PriceService,
		//					TotalQuantityService = quantity,
		//					TotalMoney = quantity * (service.PriceService ?? 0),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//					StatusComment = 1
		//				});
		//			}
		//		}

		//		// 9. Lưu tất cả InvoiceDetail
		//		await _context.InvoiceDetail.AddRangeAsync(invoiceDetails);
		//		await _context.SaveChangesAsync();

		//		// 10. Cập nhật dữ liệu trả về invoiceDetailOut_Loggin
		//		foreach (var detail in invoiceDetails)
		//		{
		//			invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
		//			{
		//				InvoiceDetailID = detail.InvoiceDetailID,
		//				InvoiceID = detail.InvoiceID,
		//				CustomerID = detail.CustomerID,
		//				CustomerName = detail.CustomerName,
		//				EmployeeID = detail.EmployeeID,
		//				EmployeeName = detail.EmployeeName,
		//				ProductID = detail.ProductID,
		//				ProductName = detail.ProductName,
		//				ServiceID = detail.ServiceID,
		//				ServiceName = detail.ServiceName,
		//				VoucherID = detail.VoucherID ?? 0,
		//				Code = detail.Code,
		//				DiscountValue = detail.DiscountValue,
		//				PriceProduct = detail.PriceProduct,
		//				PriceService = detail.PriceService,
		//				TotalQuantityProduct = detail.TotalQuantityProduct,
		//				TotalQuantityService = detail.TotalQuantityService,
		//				TotalMoney = detail.TotalMoney,
		//				DeleteStatus = detail.DeleteStatus,
		//				Status = detail.Status,
		//				Type = detail.Type,
		//				StatusComment = detail.StatusComment
		//			});
		//		}

		//		// 11. Xóa CartProduct nếu có
		//		if (insert_.ProductIDs != null)
		//		{
		//			var cartProductsToDelete = await _context.CartProduct
		//				.Where(cp => insert_.ProductIDs.Contains(cp.ProductID.Value))
		//				.ToListAsync();
		//			_context.CartProduct.RemoveRange(cartProductsToDelete);
		//		}

		//		await _context.SaveChangesAsync();
		//		await transaction.CommitAsync();

		//		// 12. Trả về kết quả thành công
		//		returnData.ResponseCode = 1;
		//		returnData.ResposeMessage = "Insert_Invoice thành công!";
		//		returnData.InvoiceID = newInvoice.InvoiceID;
		//		returnData.TotalMoney = totalMoney;
		//		returnData.invoiceOut_Loggin = invoiceOut_Loggin;
		//		returnData.invoiceDetailOut_Loggin = invoiceDetailOut_Loggin;
		//		return returnData;
		//	}
		//	catch (Exception ex)
		//	{
		//		await transaction.RollbackAsync();
		//		throw new Exception($"Lỗi trong Insert_Invoice: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
		//	}
		//}

		//public async Task<ResponseInvoice_Loggin> Insert_Invoice(InvoiceRequest insert_)
		//{
		//	decimal totalMoney = 0;
		//	var returnData = new ResponseInvoice_Loggin();
		//	var invoiceOut_Loggin = new List<Invoice_Loggin_Ouput>();
		//	var invoiceDetailOut_Loggin = new List<InvoiceDetail_Loggin_Ouput>();

		//	using var transaction = await _context.Database.BeginTransactionAsync();
		//	try
		//	{
		//		Users employee = null;
		//		Vouchers vouchers = null;
		//		var customer = await _userRepository.GetUserByUserID(insert_.CustomerID);

		//		// 1. Kiểm tra Employee
		//		if (insert_.EmployeeID != null)
		//		{
		//			employee = await _userRepository.GetUserByUserID(insert_.EmployeeID);
		//			if (employee == null || employee.TypePerson != "Employee")
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "EmployeeID không tồn tại!";
		//				return returnData;
		//			}
		//		}

		//		// 2. Kiểm tra Customer
		//		if (customer == null)
		//		{
		//			returnData.ResponseCode = -1;
		//			returnData.ResposeMessage = "CustomerID không tồn tại!";
		//			return returnData;
		//		}

		//		// 3. Kiểm tra danh sách Product & Services đầu vào
		//		if ((insert_.ProductIDs == null || insert_.ProductIDs.Count == 0) &&
		//			(insert_.ServicesIDs == null || insert_.ServicesIDs.Count == 0))
		//		{
		//			returnData.ResponseCode = -1;
		//			returnData.ResposeMessage = "Vui lòng chọn ít nhất 1 Product hoặc Service!";
		//			return returnData;
		//		}

		//		// 4. Lấy thông tin Product và Service
		//		var products = insert_.ProductIDs != null && insert_.ProductIDs.Count > 0
		//			? await GetProductsByProductIDs(insert_.ProductIDs)
		//			: new List<Products>();
		//		var services = insert_.ServicesIDs != null && insert_.ServicesIDs.Count > 0
		//			? await GetServicessByServicesIDs(insert_.ServicesIDs)
		//			: new List<Servicess>();

		//		var productDict = products.ToDictionary(p => p.ProductID, p => p);
		//		var serviceDict = services.ToDictionary(s => s.ServiceID, s => s);

		//		// 5. Tính tổng giá trị đơn hàng bằng LINQ
		//		if (insert_.ProductIDs != null && insert_.ProductIDs.Count > 0)
		//		{
		//			totalMoney += insert_.ProductIDs
		//				.Zip(insert_.QuantityProduct, (id, qty) => new { id, qty })
		//				.Where(x => productDict.ContainsKey(x.id))
		//				.Sum(x => x.qty * (productDict[x.id].SellingPrice ?? 0));
		//		}

		//		if (insert_.ServicesIDs != null && insert_.ServicesIDs.Count > 0)
		//		{
		//			totalMoney += insert_.ServicesIDs
		//				.Zip(insert_.QuantityServices, (id, qty) => new { id, qty })
		//				.Where(x => serviceDict.ContainsKey(x.id))
		//				.Sum(x => x.qty * (serviceDict[x.id].PriceService ?? 0));
		//		}

		//		// 6. Kiểm tra Voucher
		//		if (insert_.VoucherID != null)
		//		{
		//			vouchers = await _vouchersRepository.GetVouchersByVouchersID(insert_.VoucherID ?? 0);
		//			var wallets = await _context.Wallets
		//				.Where(s => s.VoucherID == insert_.VoucherID && s.UserID == insert_.CustomerID)
		//				.FirstOrDefaultAsync();
		//			if (wallets == null)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = $"Bạn chưa sở hữu VouchersID: {insert_.VoucherID}!";
		//				return returnData;
		//			}
		//			if (vouchers == null)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "VoucherID không tồn tại!";
		//				return returnData;
		//			}
		//			if (vouchers.EndDate < DateTime.Now)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = "Voucher đã hết hạn!";
		//				return returnData;
		//			}
		//			if (totalMoney < vouchers.MinimumOrderValue)
		//			{
		//				returnData.ResponseCode = -1;
		//				returnData.ResposeMessage = $"Voucher chỉ áp dụng cho đơn hàng tối thiểu {vouchers.MinimumOrderValue}!";
		//				return returnData;
		//			}
		//		}

		//		// 7. Tạo hóa đơn mới
		//		var discount = vouchers?.DiscountValue ?? 0;
		//		var maxDiscountValue = vouchers?.MaxValue ?? 0;
		//		var totalMoneyVoucher = totalMoney * ((decimal)discount / 100);
		//		var finalDiscount = totalMoneyVoucher > maxDiscountValue ? maxDiscountValue : totalMoneyVoucher;

		//		//var paymentMethod = insert_.PaymentMethod == null ? "Mặc Định" : insert_.PaymentMethod;

		//		//var newInvoice = new Invoice
		//		//{
		//		//	EmployeeID = employee?.UserID,
		//		//	CustomerID = customer.UserID,
		//		//	VoucherID = vouchers?.VoucherID,
		//		//	Code = vouchers?.Code,
		//		//	DiscountValue = vouchers?.DiscountValue,	
		//		//	TotalMoney = totalMoney - finalDiscount,
		//		//	DateCreated = DateTime.Now,
		//		//	Status = "Pending",
		//		//	DeleteStatus = 1,
		//		//	Type = "Output",
		//		//	OrderStatus = "Mặc Định",
		//		//	PaymentMethod = paymentMethod
		//		//};
		//		var paymentMethod = insert_.PaymentMethod == null ? "Mặc Định" : insert_.PaymentMethod;

		//		var newInvoice = new Invoice
		//		{
		//			EmployeeID = employee?.UserID,
		//			CustomerID = customer.UserID,
		//			VoucherID = vouchers?.VoucherID,
		//			Code = vouchers?.Code,
		//			DiscountValue = vouchers?.DiscountValue,
		//			TotalMoney = totalMoney - finalDiscount,
		//			DateCreated = DateTime.Now,
		//			Status = paymentMethod == "Thanh Toán Trực Tiếp" ? "Paid" : "Pending",
		//			DeleteStatus = 1,
		//			Type = "Output",
		//			OrderStatus = paymentMethod == "Thanh Toán Trực Tiếp" ? "Đã Nhận Hàng" : "Mặc Định",
		//			PaymentMethod = paymentMethod
		//		};
		//		await _context.Invoice.AddAsync(newInvoice);
		//		await _context.SaveChangesAsync();

		//		invoiceOut_Loggin.Add(new Invoice_Loggin_Ouput
		//		{
		//			InvoiceID = newInvoice.InvoiceID,
		//			EmployeeID = newInvoice.EmployeeID,
		//			CustomerID = newInvoice.CustomerID,
		//			VoucherID = newInvoice.VoucherID,
		//			Code = vouchers?.Code,
		//			DiscountValue = vouchers?.DiscountValue,
		//			TotalMoney = newInvoice.TotalMoney,
		//			DateCreated = newInvoice.DateCreated,
		//			Status = newInvoice.Status,
		//			DeleteStatus = newInvoice.DeleteStatus,
		//			Type = newInvoice.Type,
		//			OrderStatus = newInvoice.OrderStatus,
		//			PaymentMethod = newInvoice.PaymentMethod
		//		});

		//		// 8. Tạo danh sách InvoiceDetail
		//		var invoiceDetails = new List<InvoiceDetail>();

		//		if (insert_.ProductIDs != null && insert_.ServicesIDs != null &&
		//			insert_.ProductIDs.Count > 0 && insert_.ServicesIDs.Count > 0)
		//		{
		//			int minCount = Math.Min(insert_.ProductIDs.Count, insert_.ServicesIDs.Count);
		//			for (int i = 0; i < minCount; i++)
		//			{
		//				var productId = insert_.ProductIDs[i];
		//				var quantityProduct = insert_.QuantityProduct[i];
		//				var product = productDict.ContainsKey(productId) ? productDict[productId] : null;

		//				var serviceId = insert_.ServicesIDs[i];
		//				var quantityService = insert_.QuantityServices[i];
		//				var service = serviceDict.ContainsKey(serviceId) ? serviceDict[serviceId] : null;

		//				if (product == null || service == null || quantityProduct <= 0 || quantityService <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Dữ liệu ProductID: {productId} hoặc ServiceID: {serviceId} không hợp lệ!";
		//					return returnData;
		//				}
		//				if (quantityProduct > product.Quantity)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"Số lượng ProductID: {productId} vượt quá số lượng hiện có!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ProductID = product.ProductID,
		//					ProductName = product.ProductName,
		//					ServiceID = service.ServiceID,
		//					ServiceName = service.ServiceName,
		//					VoucherID = vouchers?.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceProduct = product.SellingPrice,
		//					PriceService = service.PriceService,
		//					TotalQuantityProduct = quantityProduct,
		//					TotalQuantityService = quantityService,
		//					TotalMoney = (quantityProduct * (product.SellingPrice ?? 0)) + (quantityService * (service.PriceService ?? 0)),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//					StatusComment = 1
		//				});
		//			}

		//			for (int i = minCount; i < insert_.ProductIDs.Count; i++)
		//			{
		//				var productId = insert_.ProductIDs[i];
		//				var quantityProduct = insert_.QuantityProduct[i];
		//				var product = productDict.ContainsKey(productId) ? productDict[productId] : null;

		//				if (product == null || quantityProduct <= 0 || quantityProduct > product.Quantity)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ProductID: {productId} không hợp lệ hoặc số lượng vượt quá!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ProductID = product.ProductID,
		//					ProductName = product.ProductName,
		//					VoucherID = newInvoice.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceProduct = product.SellingPrice,
		//					TotalQuantityProduct = quantityProduct,
		//					TotalMoney = quantityProduct * (product.SellingPrice ?? 0),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type
		//				});
		//			}

		//			for (int i = minCount; i < insert_.ServicesIDs.Count; i++)
		//			{
		//				var serviceId = insert_.ServicesIDs[i];
		//				var quantityService = insert_.QuantityServices[i];
		//				var service = serviceDict.ContainsKey(serviceId) ? serviceDict[serviceId] : null;

		//				if (service == null || quantityService <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ServiceID: {serviceId} không hợp lệ!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ServiceID = service.ServiceID,
		//					ServiceName = service.ServiceName,
		//					VoucherID = newInvoice.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceService = service.PriceService,
		//					TotalQuantityService = quantityService,
		//					TotalMoney = quantityService * (service.PriceService ?? 0),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type
		//				});
		//			}
		//		}
		//		else if (insert_.ProductIDs != null && insert_.ProductIDs.Count > 0)
		//		{
		//			for (int i = 0; i < insert_.ProductIDs.Count; i++)
		//			{
		//				var productId = insert_.ProductIDs[i];
		//				var quantity = insert_.QuantityProduct[i];
		//				var product = productDict.ContainsKey(productId) ? productDict[productId] : null;

		//				if (product == null || quantity <= 0 || quantity > product.Quantity)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ProductID: {productId} không hợp lệ hoặc số lượng vượt quá!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ProductID = product.ProductID,
		//					ProductName = product.ProductName,
		//					VoucherID = newInvoice.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceProduct = product.SellingPrice,
		//					TotalQuantityProduct = quantity,
		//					TotalMoney = quantity * (product.SellingPrice ?? 0),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//					StatusComment = 1
		//				});
		//			}
		//		}
		//		else if (insert_.ServicesIDs != null && insert_.ServicesIDs.Count > 0)
		//		{
		//			for (int i = 0; i < insert_.ServicesIDs.Count; i++)
		//			{
		//				var serviceId = insert_.ServicesIDs[i];
		//				var quantity = insert_.QuantityServices[i];
		//				var service = serviceDict.ContainsKey(serviceId) ? serviceDict[serviceId] : null;

		//				if (service == null || quantity <= 0)
		//				{
		//					returnData.ResponseCode = -1;
		//					returnData.ResposeMessage = $"ServiceID: {serviceId} không hợp lệ!";
		//					return returnData;
		//				}

		//				invoiceDetails.Add(new InvoiceDetail
		//				{
		//					InvoiceID = newInvoice.InvoiceID,
		//					CustomerID = newInvoice.CustomerID,
		//					CustomerName = customer.UserName,
		//					EmployeeID = newInvoice.EmployeeID,
		//					EmployeeName = employee?.UserName,
		//					ServiceID = service.ServiceID,
		//					ServiceName = service.ServiceName,
		//					VoucherID = insert_.VoucherID,
		//					Code = newInvoice.Code,
		//					DiscountValue = newInvoice.DiscountValue,
		//					PriceService = service.PriceService,
		//					TotalQuantityService = quantity,
		//					TotalMoney = quantity * (service.PriceService ?? 0),
		//					DeleteStatus = 1,
		//					Status = newInvoice.Status,
		//					Type = newInvoice.Type,
		//					StatusComment = 1
		//				});
		//			}
		//		}

		//		// 9. Lưu tất cả InvoiceDetail
		//		await _context.InvoiceDetail.AddRangeAsync(invoiceDetails);
		//		await _context.SaveChangesAsync();

		//		// 10. Cập nhật dữ liệu trả về invoiceDetailOut_Loggin
		//		foreach (var detail in invoiceDetails)
		//		{
		//			invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
		//			{
		//				InvoiceDetailID = detail.InvoiceDetailID,
		//				InvoiceID = detail.InvoiceID,
		//				CustomerID = detail.CustomerID,
		//				CustomerName = detail.CustomerName,
		//				EmployeeID = detail.EmployeeID,
		//				EmployeeName = detail.EmployeeName,
		//				ProductID = detail.ProductID,
		//				ProductName = detail.ProductName,
		//				ServiceID = detail.ServiceID,
		//				ServiceName = detail.ServiceName,
		//				VoucherID = detail.VoucherID ?? 0,
		//				Code = detail.Code,
		//				DiscountValue = detail.DiscountValue,
		//				PriceProduct = detail.PriceProduct,
		//				PriceService = detail.PriceService,
		//				TotalQuantityProduct = detail.TotalQuantityProduct,
		//				TotalQuantityService = detail.TotalQuantityService,
		//				TotalMoney = detail.TotalMoney,
		//				DeleteStatus = detail.DeleteStatus,
		//				Status = detail.Status,
		//				Type = detail.Type,
		//				StatusComment = detail.StatusComment
		//			});
		//		}

		//		// 11. Xóa CartProduct nếu có
		//		if (insert_.ProductIDs != null)
		//		{
		//			_context.CartProduct.RemoveRange(_context.CartProduct.Where(cp => insert_.ProductIDs.Contains(cp.ProductID.Value)));
		//		}

		//		await _context.SaveChangesAsync();
		//		await transaction.CommitAsync();

		//		// 12. Trả về kết quả thành công
		//		returnData.ResponseCode = 1;
		//		returnData.ResposeMessage = "Insert_Invoice thành công!";
		//		returnData.InvoiceID = newInvoice.InvoiceID;
		//		returnData.TotalMoney = totalMoney;
		//		returnData.invoiceOut_Loggin = invoiceOut_Loggin;
		//		returnData.invoiceDetailOut_Loggin = invoiceDetailOut_Loggin;
		//		return returnData;
		//	}
		//	catch (Exception ex)
		//	{
		//		await transaction.RollbackAsync();
		//		throw new Exception($"Lỗi trong Insert_Invoice: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
		//	}
		//}

		public async Task<ResponseInvoice_Loggin> Insert_Invoice(InvoiceRequest insert_)
		{
			decimal totalMoney = 0;
			var returnData = new ResponseInvoice_Loggin();
			var invoiceOut_Loggin = new List<Invoice_Loggin_Ouput>();
			var invoiceDetailOut_Loggin = new List<InvoiceDetail_Loggin_Ouput>();

			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				// 1. Lấy thông tin tuần tự thay vì song song
				var customer = await _userRepository.GetUserByUserID(insert_.CustomerID);
				if (customer == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "CustomerID không tồn tại!";
					return returnData;
				}

				var employee = insert_.EmployeeID != null
					? await _userRepository.GetUserByUserID(insert_.EmployeeID)
					: null;
				if (employee != null && employee.TypePerson != "Employee")
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "EmployeeID không tồn tại!";
					return returnData;
				}

				var vouchers = insert_.VoucherID != null
					? await _vouchersRepository.GetVouchersByVouchersID(insert_.VoucherID ?? 0)
					: null;

				// 2. Kiểm tra danh sách đầu vào
				if ((insert_.ProductIDs == null || !insert_.ProductIDs.Any()) &&
					(insert_.ServicesIDs == null || !insert_.ServicesIDs.Any()))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Vui lòng chọn ít nhất 1 Product hoặc Service!";
					return returnData;
				}

				// 3. Lấy thông tin sản phẩm và dịch vụ
				var products = insert_.ProductIDs?.Any() == true
					? await GetProductsByProductIDs(insert_.ProductIDs)
					: new List<Products>();
				var services = insert_.ServicesIDs?.Any() == true
					? await GetServicessByServicesIDs(insert_.ServicesIDs)
					: new List<Servicess>();

				var productDict = products.ToDictionary(p => p.ProductID);
				var serviceDict = services.ToDictionary(s => s.ServiceID);

				// 4. Tính tổng tiền bằng LINQ
				totalMoney += insert_.ProductIDs?.Any() == true
					? insert_.ProductIDs
						.Select((id, i) => new { id, qty = insert_.QuantityProduct[i] })
						.Where(x => productDict.ContainsKey(x.id) && x.qty > 0 && x.qty <= productDict[x.id].Quantity)
						.Sum(x => x.qty * (productDict[x.id].SellingPrice ?? 0))
					: 0;

				totalMoney += insert_.ServicesIDs?.Any() == true
					? insert_.ServicesIDs
						.Select((id, i) => new { id, qty = insert_.QuantityServices[i] })
						.Where(x => serviceDict.ContainsKey(x.id) && x.qty > 0)
						.Sum(x => x.qty * (serviceDict[x.id].PriceService ?? 0))
					: 0;

				// 5. Kiểm tra voucher
				if (insert_.VoucherID != null)
				{
					if (vouchers == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "VoucherID không tồn tại!";
						return returnData;
					}

					var wallet = await _context.Wallets
						.FirstOrDefaultAsync(w => w.VoucherID == insert_.VoucherID && w.UserID == insert_.CustomerID);
					if (wallet == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Bạn chưa sở hữu VoucherID: {insert_.VoucherID}!";
						return returnData;
					}

					if (vouchers.EndDate < DateTime.Now)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Voucher đã hết hạn!";
						return returnData;
					}

					if (totalMoney < vouchers.MinimumOrderValue)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Voucher chỉ áp dụng cho đơn hàng tối thiểu {vouchers.MinimumOrderValue}!";
						return returnData;
					}
				}

				// 6. Tính giảm giá và tạo hóa đơn
				var discount = vouchers?.DiscountValue ?? 0;
				var maxDiscountValue = vouchers?.MaxValue ?? 0;
				var totalMoneyVoucher = totalMoney * ((decimal)discount / 100m);
				var finalDiscount = totalMoneyVoucher > maxDiscountValue ? maxDiscountValue : totalMoneyVoucher;

				var paymentMethod = insert_.PaymentMethod ?? "Mặc Định";
				var newInvoice = new Invoice
				{
					EmployeeID = employee?.UserID,
					CustomerID = customer.UserID,
					VoucherID = vouchers?.VoucherID,
					Code = vouchers?.Code,
					DiscountValue = discount,
					TotalMoney = totalMoney - finalDiscount,
					DateCreated = DateTime.Now,
					Status = paymentMethod == "Thanh Toán Trực Tiếp" ? "Paid" : "Pending",
					DeleteStatus = 1,
					Type = "Output",
					OrderStatus = paymentMethod == "Thanh Toán Trực Tiếp" ? "Đã Nhận Hàng" : "Mặc Định",
					PaymentMethod = paymentMethod
				};
				await _context.Invoice.AddAsync(newInvoice);
				await _context.SaveChangesAsync();

				invoiceOut_Loggin.Add(new Invoice_Loggin_Ouput
				{
					InvoiceID = newInvoice.InvoiceID,
					EmployeeID = newInvoice.EmployeeID,
					CustomerID = newInvoice.CustomerID,
					VoucherID = newInvoice.VoucherID,
					Code = newInvoice.Code,
					DiscountValue = newInvoice.DiscountValue,
					TotalMoney = newInvoice.TotalMoney,
					DateCreated = newInvoice.DateCreated,
					Status = newInvoice.Status,
					DeleteStatus = newInvoice.DeleteStatus,
					Type = newInvoice.Type,
					OrderStatus = newInvoice.OrderStatus,
					PaymentMethod = newInvoice.PaymentMethod
				});

				// 7. Tạo danh sách InvoiceDetail bằng LINQ
				var invoiceDetails = new List<InvoiceDetail>();

				if (insert_.ProductIDs?.Any() == true)
				{
					invoiceDetails.AddRange(
						insert_.ProductIDs
							.Select((id, i) => new { id, qty = insert_.QuantityProduct[i] })
							.Where(x => productDict.ContainsKey(x.id) && x.qty > 0 && x.qty <= productDict[x.id].Quantity)
							.Select(x => new InvoiceDetail
							{
								InvoiceID = newInvoice.InvoiceID,
								CustomerID = customer.UserID,
								CustomerName = customer.UserName,
								EmployeeID = employee?.UserID,
								EmployeeName = employee?.UserName,
								ProductID = x.id,
								ProductName = productDict[x.id].ProductName,
								VoucherID = vouchers?.VoucherID,
								Code = newInvoice.Code,
								DiscountValue = discount,
								PriceProduct = productDict[x.id].SellingPrice,
								TotalQuantityProduct = x.qty,
								TotalMoney = x.qty * (productDict[x.id].SellingPrice ?? 0),
								DeleteStatus = 1,
								Status = newInvoice.Status,
								Type = newInvoice.Type,
								StatusComment = 1
							})
					);
				}

				if (insert_.ServicesIDs?.Any() == true)
				{
					invoiceDetails.AddRange(
						insert_.ServicesIDs
							.Select((id, i) => new { id, qty = insert_.QuantityServices[i] })
							.Where(x => serviceDict.ContainsKey(x.id) && x.qty > 0)
							.Select(x => new InvoiceDetail
							{
								InvoiceID = newInvoice.InvoiceID,
								CustomerID = customer.UserID,
								CustomerName = customer.UserName,
								EmployeeID = employee?.UserID,
								EmployeeName = employee?.UserName,
								ServiceID = x.id,
								ServiceName = serviceDict[x.id].ServiceName,
								VoucherID = vouchers?.VoucherID,
								Code = newInvoice.Code,
								DiscountValue = discount,
								PriceService = serviceDict[x.id].PriceService,
								TotalQuantityService = x.qty,
								TotalMoney = x.qty * (serviceDict[x.id].PriceService ?? 0),
								DeleteStatus = 1,
								Status = newInvoice.Status,
								Type = newInvoice.Type,
								StatusComment = 1
							})
					);
				}

				await _context.InvoiceDetail.AddRangeAsync(invoiceDetails);
				await _context.SaveChangesAsync();

				// 8. Cập nhật dữ liệu trả về
				invoiceDetailOut_Loggin = invoiceDetails.Select(d => new InvoiceDetail_Loggin_Ouput
				{
					InvoiceDetailID = d.InvoiceDetailID,
					InvoiceID = d.InvoiceID,
					CustomerID = d.CustomerID,
					CustomerName = d.CustomerName,
					EmployeeID = d.EmployeeID,
					EmployeeName = d.EmployeeName,
					ProductID = d.ProductID,
					ProductName = d.ProductName,
					ServiceID = d.ServiceID,
					ServiceName = d.ServiceName,
					VoucherID = d.VoucherID ?? 0,
					Code = d.Code,
					DiscountValue = d.DiscountValue,
					PriceProduct = d.PriceProduct,
					PriceService = d.PriceService,
					TotalQuantityProduct = d.TotalQuantityProduct,
					TotalQuantityService = d.TotalQuantityService,
					TotalMoney = d.TotalMoney,
					DeleteStatus = d.DeleteStatus,
					Status = d.Status,
					Type = d.Type,
					StatusComment = d.StatusComment
				}).ToList();

				// 9. Xóa CartProduct nếu cần
				if (insert_.ProductIDs?.Any() == true)
				{
					var cartProducts = _context.CartProduct.Where(cp => insert_.ProductIDs.Contains(cp.ProductID.Value));
					_context.CartProduct.RemoveRange(cartProducts);
					await _context.SaveChangesAsync();
				}

				await transaction.CommitAsync();

				// 10. Trả về kết quả
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Insert_Invoice thành công!";
				returnData.InvoiceID = newInvoice.InvoiceID;
				returnData.TotalMoney = totalMoney;
				returnData.invoiceOut_Loggin = invoiceOut_Loggin;
				returnData.invoiceDetailOut_Loggin = invoiceDetailOut_Loggin;
				return returnData;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Lỗi trong Insert_Invoice: {ex.Message}");
			}
		}

		public async Task<ResponseInvoice_Loggin> Delete_Invoice(Delete_Invoice delete_)
		{
			var returnData = new ResponseInvoice_Loggin();
			var invoiceOut_Loggin = new List<Invoice_Loggin_Ouput>();
			var invoiceDetailOut_Loggin = new List<InvoiceDetail_Loggin_Ouput>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(s => s.InvoiceID == delete_.InvoiceID && s.DeleteStatus == 1)
					.FirstOrDefaultAsync();
				if (invoice != null)
				{
					//1.Xóa Ivoice
					invoice.DeleteStatus = 0;
					invoiceOut_Loggin.Add(new Invoice_Loggin_Ouput
					{
						InvoiceID = invoice.InvoiceID,
						EmployeeID = invoice.EmployeeID,
						CustomerID = invoice.CustomerID,
						VoucherID = invoice.VoucherID,
						Code = invoice?.Code,
						DiscountValue = invoice?.DiscountValue,
						DateCreated = invoice.DateCreated,
						Status = invoice.Status,
						DeleteStatus = invoice.DeleteStatus,
						Type = invoice.Type,
						OrderStatus = invoice.OrderStatus
					});


					//2. Xóa InvoiceDetail
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1);
					if (invoiceDetail != null)
					{
						foreach (var item in invoiceDetail)
						{
							item.DeleteStatus = 0;
							invoiceDetailOut_Loggin.Add(new InvoiceDetail_Loggin_Ouput
							{
								InvoiceDetailID = item.InvoiceDetailID,
								InvoiceID = item.InvoiceID,
								CustomerID = item.CustomerID,
								CustomerName = item.CustomerName,
								EmployeeID = item.EmployeeID,
								EmployeeName = item.EmployeeName,
								ProductID = item.ProductID,
								ProductName = item.ProductName,
								ServiceID = item.ServiceID,
								ServiceName = item.ServiceName,
								VoucherID = item.VoucherID ?? 0,
								Code = item.Code,
								DiscountValue = item.DiscountValue,
								PriceProduct = item.PriceProduct,
								PriceService = item.PriceService,
								TotalQuantityService = item.TotalQuantityService,
								TotalQuantityProduct = item.TotalQuantityProduct,
								TotalMoney = item.TotalMoney,
								DeleteStatus = item.DeleteStatus,
								Status = item.Status,
								Type = item.Type,
							});
						}
					}
					await transaction.CommitAsync();
					await _context.SaveChangesAsync();
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = $"Delete_Invoice thành công!";
					returnData.invoiceOut_Loggin = invoiceOut_Loggin;
					returnData.invoiceDetailOut_Loggin = invoiceDetailOut_Loggin;
					return returnData;
				}
				returnData.ResponseCode = -1;
				returnData.ResposeMessage = "InvoiceID không hợp lệ!";
				return returnData;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error in Delete_Invoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseGetListInvoice> GetList_SearchInvoice(GetList_Invoice getList_)
		{
			var returnData = new ResponseGetListInvoice();
			try
			{
				if (getList_.InvoiceID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.InvoiceID == getList_.InvoiceID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.EmployeeID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.EmployeeID == getList_.EmployeeID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "EmployeeID chưa có hóa đơn nào!";
						return returnData;
					}
					if (await _userRepository.GetUserByUserID(getList_.EmployeeID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "EmployeeID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.CustomerID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.CustomerID == getList_.CustomerID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "CustomerID chưa có hóa đơn nào!";
						return returnData;
					}
					if (await _userRepository.GetUserByUserID(getList_.CustomerID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "CustomerID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.InvoiceType != null)
				{
					if (!Validation.CheckString(getList_.InvoiceType) || !Validation.CheckXSSInput(getList_.InvoiceType))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceType không hợp lệ!";
						return returnData;
					}
					if (!string.Equals(getList_.InvoiceType, "Input", StringComparison.OrdinalIgnoreCase) &&
						!string.Equals(getList_.InvoiceType, "Output", StringComparison.OrdinalIgnoreCase))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceDetailType chỉ chấp nhận Input || Output";
						return returnData;
					}
				}
				if (getList_.StartDate != null && getList_.EndDate != null)
				{
					if (getList_.EndDate < getList_.StartDate)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Ngày kết thúc không được nhỏ hơn ngày bắt đầu!";
						return returnData;
					}
				}
				var parameters = new DynamicParameters();
				parameters.Add("@InvoiceID", getList_.InvoiceID ?? null);
				parameters.Add("@EmployeeID", getList_.EmployeeID ?? null);
				parameters.Add("@CustomerID", getList_.CustomerID ?? null);
				parameters.Add("@InvoiceType", getList_.InvoiceType ?? null);
				parameters.Add("@StartDate", getList_.StartDate ?? null);
				parameters.Add("@EndDate", getList_.EndDate ?? null);
				parameters.Add("@Status", getList_.Status ?? null);
				parameters.Add("@PaymentMethod", getList_.PaymentMethod ?? null);
				var _listInvoice = await DbConnection.QueryAsync<GetList_Invoice_Out>("GetList_SearchInvoice", parameters);
				if (_listInvoice != null && _listInvoice.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách Invoice thành công!";
					returnData.Data = _listInvoice.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy Invoice nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetList_SearchInvoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseGetListInvoice> GetList_SearchInvoicee(GetList_Invoice getList_)
		{
			var returnData = new ResponseGetListInvoice();
			try
			{
				if (getList_.InvoiceID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.InvoiceID == getList_.InvoiceID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.EmployeeID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.EmployeeID == getList_.EmployeeID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "EmployeeID chưa có hóa đơn nào!";
						return returnData;
					}
					if (await _userRepository.GetUserByUserID(getList_.EmployeeID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "EmployeeID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.CustomerID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.CustomerID == getList_.CustomerID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "CustomerID chưa có hóa đơn nào!";
						return returnData;
					}
					if (await _userRepository.GetUserByUserID(getList_.CustomerID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "CustomerID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.InvoiceType != null)
				{
					if (!Validation.CheckString(getList_.InvoiceType) || !Validation.CheckXSSInput(getList_.InvoiceType))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceType không hợp lệ!";
						return returnData;
					}
					if (!string.Equals(getList_.InvoiceType, "Input", StringComparison.OrdinalIgnoreCase) &&
						!string.Equals(getList_.InvoiceType, "Output", StringComparison.OrdinalIgnoreCase))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceDetailType chỉ chấp nhận Input || Output";
						return returnData;
					}
				}
				if (getList_.StartDate != null && getList_.EndDate != null)
				{
					if (getList_.EndDate < getList_.StartDate)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Ngày kết thúc không được nhỏ hơn ngày bắt đầu!";
						return returnData;
					}
				}
				var parameters = new DynamicParameters();
				parameters.Add("@InvoiceID", getList_.InvoiceID ?? null);
				parameters.Add("@EmployeeID", getList_.EmployeeID ?? null);
				parameters.Add("@CustomerID", getList_.CustomerID ?? null);
				parameters.Add("@InvoiceType", getList_.InvoiceType ?? null);
				parameters.Add("@StartDate", getList_.StartDate ?? null);
				parameters.Add("@EndDate", getList_.EndDate ?? null);
				parameters.Add("@Status", getList_.Status ?? null);
				parameters.Add("@PaymentMethod", getList_.PaymentMethod ?? null);
				var _listInvoice = await DbConnection.QueryAsync<GetList_Invoice_Out>("GetList_SearchInvoicee", parameters);
				if (_listInvoice != null && _listInvoice.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách Invoice thành công!";
					returnData.Data = _listInvoice.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy Invoice nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetList_SearchInvoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseInvoice_Loggin> UpdateOrderStatus(UpdateOrderStatus orderStatus)
		{
			var returnData = new ResponseInvoice_Loggin();
			try
			{
				var invoice = await _context.Invoice
					.Where(v => v.InvoiceID == orderStatus.InvoiceID && v.DeleteStatus == 1).FirstOrDefaultAsync();
				if (invoice.EmployeeID != null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Đơn hàng đã được giao thành công!";
					return returnData;
				}

				if (orderStatus.Status != "Đang Giao" && orderStatus.Status != "Đã Nhận Hàng" && orderStatus.Status != "Shipper đã lấy hàng")
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Trạng thái đơn hàng không hợp lệ!";
					return returnData;
				}
				if (invoice != null)
				{
					invoice.OrderStatus = orderStatus.Status;

					if (orderStatus.Status != "Đã Nhận Hàng")
					{
						var invoiceDetail = await _context.InvoiceDetail
							.Where(s => s.InvoiceDetailID == orderStatus.InvoiceDetailID && s.DeleteStatus == 1).FirstOrDefaultAsync();
						if (invoiceDetail != null)
						{
							invoiceDetail.StatusComment = 1;
						}
					}
					await _context.SaveChangesAsync();
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Cập nhật trạng thái đơn hàng thành công!";
					return returnData;
				}
				else
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Kiểm tra lại đơn hàng || thanh toán!";
					return returnData;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in UpdateStatusInvoiceDetail Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseGetListInvoiceDetail> GetList_SearchInvoiceDetail(GetList_InvoiceDetail getList_)
		{
			var returnData = new ResponseGetListInvoiceDetail();
			try
			{
				if (getList_.InvoiceID != null)
				{
					var result = await _context.Invoice
						.Where(s => s.InvoiceID == getList_.InvoiceID && s.DeleteStatus == 1)
						.FirstOrDefaultAsync();
					if (result == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceID không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.InvoiceDetailType != null)
				{
					if (!Validation.CheckString(getList_.InvoiceDetailType) || !Validation.CheckXSSInput(getList_.InvoiceDetailType))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceDetailType không hợp lệ!";
						return returnData;
					}
					if (!string.Equals(getList_.InvoiceDetailType, "Input", StringComparison.OrdinalIgnoreCase) &&
						!string.Equals(getList_.InvoiceDetailType, "Ouput", StringComparison.OrdinalIgnoreCase))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "InvoiceDetailType chỉ chấp nhận Input || Ouput";
						return returnData;
					}

				}
				if (getList_.StartDate != null && getList_.EndDate != null)
				{
					if (getList_.EndDate < getList_.StartDate)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Ngày kết thúc không được nhỏ hơn ngày bắt đầu!";
						return returnData;
					}
				}
				var parameters = new DynamicParameters();
				parameters.Add("@InvoiceID", getList_.InvoiceID ?? null);
				parameters.Add("@InvoiceDetailType", getList_.InvoiceDetailType ?? null);
				parameters.Add("@StartDate", getList_.StartDate ?? null);
				parameters.Add("@EndDate", getList_.EndDate ?? null);
				var _listInvoice = await DbConnection.QueryAsync<GetList_InvoiceDetail_Out>("GetList_SearchInvoiceDetail", parameters);
				if (_listInvoice != null && _listInvoice.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách Invoice thành công!";
					returnData.Data = _listInvoice.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy Invoice nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetList_SearchInvoiceDetail Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<Invoice> GetInvoiceByInvoiceID(int InvoiceID)
		{
			return await _context.Invoice.Where(s => s.InvoiceID == InvoiceID && s.DeleteStatus == 1).FirstOrDefaultAsync();
		}

		public async Task UpdateStatusInvoice(int InvoiceID)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(a => a.InvoiceID == InvoiceID && a.DeleteStatus == 1)
					.FirstOrDefaultAsync();
				if (invoice != null)
				{
					invoice.Status = "Paid";
					invoice.PaymentMethod = "Chuyển Khoản Ngân Hàng";
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1).ToList();
					if (invoiceDetail != null)
					{
						foreach (var item in invoiceDetail)
						{
							item.Status = "Paid";
						}
					}
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
				}
				else
				{
					throw new Exception($"Không tồn tại InvoiceID: {InvoiceID}");
				}

			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error UpdateStatusInvoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task UpdateStatusInvoiceDetail(int InvoiceID)
		{
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(v => v.InvoiceID == InvoiceID && v.DeleteStatus == 1).FirstOrDefaultAsync();
				if (invoice != null)
				{
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1)
						.ToList();
					if (invoiceDetail != null && invoiceDetail.Any())
					{
						foreach (var detail in invoiceDetail)
						{
							detail.Status = "Paid";
						}
						await _context.SaveChangesAsync();
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in UpdateStatusInvoiceDetail Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task UpdateStatusInvoiceFail(int InvoiceID)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(a => a.InvoiceID == InvoiceID && a.DeleteStatus == 1)
					.FirstOrDefaultAsync();
				if (invoice != null)
				{
					invoice.Status = "Fail";
					invoice.PaymentMethod = "Chuyển Khoản Ngân Hàng";
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1).ToList();
					if (invoiceDetail != null)
					{
						foreach (var item in invoiceDetail)
						{
							item.Status = "Fail";
						}
					}
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
				}
				else
				{
					throw new Exception($"Không tồn tại InvoiceID: {InvoiceID}");
				}

			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error UpdateStatusInvoice Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task UpdateStatusInvoiceDetailFail(int InvoiceID)
		{
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(v => v.InvoiceID == InvoiceID && v.DeleteStatus == 1).FirstOrDefaultAsync();
				if (invoice != null)
				{
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1)
						.ToList();
					if (invoiceDetail != null && invoiceDetail.Any())
					{
						foreach (var detail in invoiceDetail)
						{
							detail.Status = "Fail";
						}
						await _context.SaveChangesAsync();
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in UpdateStatusInvoiceDetailFail Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<List<InvoiceDetail>> InvoiceDetailByInvoiceID(int invoiceID)
		{
			return await _context.InvoiceDetail.Where(s => s.InvoiceID == invoiceID && s.DeleteStatus == 1).ToListAsync();
		}

		public async Task AutoUpdateOrderStatus(int InvoiceID)
		{
			try
			{
				var invoice = await _context.Invoice
					.Where(v => v.InvoiceID == InvoiceID && v.DeleteStatus == 1).FirstOrDefaultAsync();
				if (invoice != null)
				{
					invoice.OrderStatus = "Đang chuẩn bị hàng";
					_context.Invoice.Update(invoice);
					await _context.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in AutoUpdateOrderStatus Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task AutoUpdateOrderStatusEmployee(int InvoiceID)
		{
			try
			{
				var invoice = await _context.Invoice
					.Where(v => v.InvoiceID == InvoiceID && v.DeleteStatus == 1).FirstOrDefaultAsync();
				if (invoice != null)
				{
					invoice.OrderStatus = "Đã Nhận Hàng";
					_context.Invoice.Update(invoice);
					await _context.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in AutoUpdateOrderStatusEmployee Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<List<Products>> GetProductsByProductIDs(List<int> productIds)
		{
			return await _context.Products
				.Where(p => productIds.Contains(p.ProductID))
				.ToListAsync();
		}

		public async Task<List<Servicess>> GetServicessByServicesIDs(List<int> serviceIds)
		{
			return await _context.Servicess
				.Where(s => serviceIds.Contains(s.ServiceID))
				.ToListAsync();
		}

		//public async Task<ResponseData> ProcessDirectPayment(Delete_Invoice delete_)
		//{
		//	var responseData = new ResponseData();
		//	try
		//	{
		//		var invoice = await _context.Invoice
		//			.Include(s => s.InvoiceDetails)
		//			.AsSplitQuery()
		//			.Where(a => a.InvoiceID == delete_.InvoiceID && a.DeleteStatus == 1)
		//			.FirstOrDefaultAsync();
		//		if (invoice != null)
		//		{
		//			invoice.Status = "Pending";
		//			invoice.OrderStatus = "Đang chuẩn bị hàng";
		//			invoice.PaymentMethod = "Thanh Toán Khi Nhận Hàng";
		//			var invoiceDetail = invoice.InvoiceDetails
		//				.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1).ToList();
		//			if (invoiceDetail != null)
		//			{
		//				foreach (var item in invoiceDetail)
		//				{
		//					item.Status = "Pending";
		//				}
		//			}
		//			await _context.SaveChangesAsync();
		//			responseData.ResponseCode = 1;
		//			responseData.ResposeMessage = "Mua Hàng Thành Công!";
		//			return responseData;
		//		}
		//		responseData.ResponseCode = -1;
		//		responseData.ResposeMessage = "Hóa Đơn Không Tồn Tại!";
		//		return responseData;
		//	}
		//	catch (Exception ex)
		//	{
		//		throw new Exception($"Error in ProcessDirectPayment Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
		//	}
		//}

		public async Task<ResponseData> ProcessDirectPayment(Delete_Invoice delete_)
		{
			var responseData = new ResponseData();
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(a => a.InvoiceID == delete_.InvoiceID && a.DeleteStatus == 1)
					.FirstOrDefaultAsync();
				if (invoice != null)
				{
					invoice.Status = "Pending";
					invoice.OrderStatus = "Đang chuẩn bị hàng";
					invoice.PaymentMethod = "Thanh Toán Khi Nhận Hàng";
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1).ToList();
					if (invoiceDetail != null)
					{
						foreach (var item in invoiceDetail)
						{
							item.Status = "Pending";
						}
					}
					await _context.SaveChangesAsync();

					// Lấy danh sách ServiceID từ InvoiceDetail
					var serviceIds = invoiceDetail
						.Where(d => d.ServiceID.HasValue)
						.Select(d => d.ServiceID.Value)
						.Distinct()
						.ToList();

					if (serviceIds.Any())
					{
						// Cập nhật PaymentStatus cho BookingAssignment
						var bookingAssignments = await _context.Booking_Assignment
							.Where(ba => serviceIds.Contains(ba.ServiceID))
							.ToListAsync();

						foreach (var ba in bookingAssignments)
						{
							ba.PaymentStatus = 2; // Đã thanh toán
						}
						await _context.SaveChangesAsync();

						// Cập nhật PaymentStatus cho Booking
						var bookingIds = bookingAssignments
							.Select(ba => ba.BookingID)
							.Distinct()
							.ToList();

						foreach (var bookingId in bookingIds)
						{
							var booking = await _context.Booking
								.Include(b => b.Booking_Assignment)
								.FirstOrDefaultAsync(b => b.BookingID == bookingId);

							if (booking != null)
							{
								// Kiểm tra xem tất cả BookingAssignment của Booking đã thanh toán chưa
								bool allPaid = booking.Booking_Assignment.All(ba => ba.PaymentStatus == 2);
								if (allPaid)
								{
									booking.PaymentStatus = 2; // Đã thanh toán
								}
							}
						}
						await _context.SaveChangesAsync();
					}

					responseData.ResponseCode = 1;
					responseData.ResposeMessage = "Mua Hàng Thành Công!";
					return responseData;
				}
				responseData.ResponseCode = -1;
				responseData.ResposeMessage = "Hóa Đơn Không Tồn Tại!";
				return responseData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in ProcessDirectPayment Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseData> ConfirmCodPaymentSuccess(Delete_Invoice delete_)
		{
			var responseData = new ResponseData();
			try
			{
				var invoice = await _context.Invoice
					.Include(s => s.InvoiceDetails)
					.AsSplitQuery()
					.Where(a => a.InvoiceID == delete_.InvoiceID && a.DeleteStatus == 1)
					.FirstOrDefaultAsync();
				if (invoice != null)
				{
					invoice.Status = "Paid";
					invoice.OrderStatus = "Đã Nhận Hàng";
					var invoiceDetail = invoice.InvoiceDetails
						.Where(s => s.InvoiceID == invoice.InvoiceID && s.DeleteStatus == 1).ToList();
					//var invoiceDetail = await _invoiceRepository.InvoiceDetailByInvoiceID(invoice.InvoiceID);
					if (invoiceDetail != null)
					{
						foreach (var item in invoiceDetail)
						{
							item.Status = "Paid";
						}
					}

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
					//Cập nhật điểm mua hàng cho khách hàng
					await _userRepository.UpdateRatingPoints_Customer(invoice.CustomerID ?? 0);
					await _context.SaveChangesAsync();
					responseData.ResponseCode = 1;
					responseData.ResposeMessage = "Thanh Toán Thành Công!";
					return responseData;
				}
				responseData.ResponseCode = -1;
				responseData.ResposeMessage = "Hóa Đơn Không Tồn Tại!";
				return responseData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in ProcessDirectPayment Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
	}
}
