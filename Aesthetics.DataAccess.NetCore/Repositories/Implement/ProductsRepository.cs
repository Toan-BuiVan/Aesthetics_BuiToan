﻿using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using BE_102024.DataAces.NetCore.CheckConditions;
using BE_102024.DataAces.NetCore.Dapper;
using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class ProductsRepository : BaseApplicationService, IProductsRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		private static List<Products> _listProducts;
		private ISupplierRepository _supplierRepository;
		private IUserRepository _userRepository;
		public ProductsRepository(DB_Context context, IConfiguration configuration, 
			IServiceProvider serviceProvider, ISupplierRepository supplierRepository, 
			IUserRepository userRepository) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
			_listProducts = new List<Products>();
			_supplierRepository = supplierRepository;
			_userRepository = userRepository;
		}
		public async Task<string> BaseProcessingFunction64(string? ProductsImage)
		{
			try
			{
				var path = "FilesImages/Products";

				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}
				string imageName = Guid.NewGuid().ToString() + ".png";
				var imgPath = Path.Combine(path, imageName);

				if (ProductsImage.Contains("data:image"))
				{
					ProductsImage = ProductsImage.Substring(ProductsImage.LastIndexOf(',') + 1);
				}

				byte[] imageBytes = Convert.FromBase64String(ProductsImage);
				MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
				ms.Write(imageBytes, 0, imageBytes.Length);
				System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);

				image.Save(imgPath, System.Drawing.Imaging.ImageFormat.Png);

				return imageName;
			}
			catch (Exception ex)
			{
				throw new Exception("Lỗi khi lưu file: " + ex.Message);
			}
		}

		public async Task<Products> GetProductsByProductID(int? ProductID)
		{
			return await _context.Products.Where(s => s.ProductID == ProductID && s.DeleteStatus == 1).FirstOrDefaultAsync();
		}

		public async Task<TypeProductsOfServices> GetProductOfServicesByID(int? ProductsOfServicesID)
		{
			return await _context.TypeProductsOfServices.Where(s => s.ProductsOfServicesID == ProductsOfServicesID
				&& s.ProductsOfServicesType == "Products"
				&& s.DeleteStatus == 1).FirstOrDefaultAsync();
		}

		public async Task<ResponseProducts_Loggin> Insert_Products(ProductRequest products_)
		{
			var returnData = new ResponseProducts_Loggin();
			var product_Loggins = new List<Products_Loggin>();
			var invoice_Loggin_Inputs = new List<Invoice_Loggin_Input>();
			var invoiceDetail_Loggin_Inputs = new List<InvoiceDetail_Loggin_Input>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				if (products_.ProductsOfServicesID <= 0 
					|| await GetProductOfServicesByID(products_.ProductsOfServicesID) == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "ProductsOfServicesID không hợp lệ || không tồn tại!";
					return returnData;
				}

				if (products_.SupplierID <= 0 
					|| await _supplierRepository.GetSupplierBySupplierID(products_.SupplierID) == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "SupplierID không hợp lệ || không tồn tại!";
					return returnData;
				}

				if (!Validation.CheckString(products_.ProductName) || !Validation.CheckXSSInput(products_.ProductName))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "ProductName không hợp lệ || chứa kí tự không hợp lệ!";
					return returnData;
				}

				if (!Validation.CheckString(products_.ProductDescription) || !Validation.CheckXSSInput(products_.ProductDescription))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "ProductDescription không hợp lệ || chứa kí tự không hợp lệ!";
					return returnData;
				}

				if (products_.SellingPrice < 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "SellingPrice không hợp lệ!";
					return returnData;
				}

				if (products_.Quantity < 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "SellingPrice không hợp lệ!";
					return returnData;
				}

				if (!Validation.CheckXSSInput(products_.ProductImages) || !Validation.CheckString(products_.ProductImages))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "ProductImages không hợp lệ || chứa kí tự không hợp lệ!";
					return returnData;
				}
				var employee = await _userRepository.GetUserByUserID(products_.EmployeeID);
				if (employee == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "EmployeeID không tồn tại!";
					return returnData;
				}

				//1. Thêm sản phẩm
				var imagePathProducts = await BaseProcessingFunction64(products_.ProductImages);
				var newProduct = new Products
				{
					ProductsOfServicesID = products_.ProductsOfServicesID,
					SupplierID = products_.SupplierID,
					ProductName = products_.ProductName,
					ProductDescription = products_.ProductDescription,
					SellingPrice = products_.SellingPrice,
					Quantity = products_.Quantity,
					ProductImages = imagePathProducts,
					DeleteStatus = 1
				};
				_context.Products.Add(newProduct);
				await _context.SaveChangesAsync();
				product_Loggins.Add(new Products_Loggin
				{
					ProductID = newProduct.ProductID,
					ProductsOfServicesID = products_.ProductsOfServicesID,
					SupplierID = products_.SupplierID,
					ProductName = products_.ProductName,
					ProductDescription = products_.ProductDescription,
					SellingPrice = products_.SellingPrice,
					Quantity = products_.Quantity,
					ProductImages = imagePathProducts,
					DeleteStatus = 1
				});

				//2.Tạo hóa đơn nhập
				var newInvoice = new Invoice
				{
					EmployeeID = employee?.UserID,
					DateCreated = DateTime.Now,
					DeleteStatus = 1,
					TotalMoney = products_.Quantity * products_.SellingPrice,
					Type = "Input"
				};
				_context.Invoice.Add(newInvoice);
				await _context.SaveChangesAsync();
				invoice_Loggin_Inputs.Add(new Invoice_Loggin_Input
				{
					InvoiceID = newInvoice.InvoiceID,
					EmployeeID = newInvoice.EmployeeID,
					DateCreated = newInvoice.DateCreated,
					DeleteStatus = newInvoice.DeleteStatus,
					TotalMoney = newInvoice.TotalMoney,
					Type = newInvoice.Type,
				});

				//3.Tạo chi tiết hóa đơn nhập
				var newInvoiceDetail = new InvoiceDetail
				{
					InvoiceID = newInvoice.InvoiceID,
					EmployeeID = newInvoice.EmployeeID,
					EmployeeName = employee?.UserName,
					ProductID = newProduct.ProductID,
					ProductName = products_.ProductName,
					PriceProduct = products_.SellingPrice,
					TotalQuantityProduct = products_.Quantity,
					TotalMoney = products_.Quantity * products_.SellingPrice,
					DeleteStatus = 1,
					Type = newInvoice.Type
				};
				_context.InvoiceDetail.Add(newInvoiceDetail);
				await _context.SaveChangesAsync();
				invoiceDetail_Loggin_Inputs.Add(new InvoiceDetail_Loggin_Input
				{
					InvoiceDetailID = newInvoiceDetail.InvoiceDetailID,
					InvoiceID = newInvoiceDetail.InvoiceID,
					EmployeeID = newInvoiceDetail.EmployeeID,
					EmployeeName = newInvoiceDetail.EmployeeName,
					ProductID = newInvoiceDetail.ProductID,
					ProductName = newInvoiceDetail.ProductName,
					PriceProduct = newInvoiceDetail.PriceProduct,
					TotalQuantityProduct = newProduct.Quantity,
					TotalMoney = newInvoiceDetail.TotalMoney,
					DeleteStatus = 1,
					Type = newInvoice.Type
				});
				await transaction.CommitAsync();

				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Thêm mới sản phẩm thành công!";
				returnData.products_Loggins = product_Loggins;
				returnData.invoice_Loggin_Inputs = invoice_Loggin_Inputs;
				returnData.invoiceDetail_Loggin_Inputs = invoiceDetail_Loggin_Inputs;
				return returnData;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error Insert_Products Inner Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}

		}

		public async Task<ResponseProducts_Loggin> Update_Products(Update_Products update_)
		{
			var returnData = new ResponseProducts_Loggin();
			var product_Loggins = new List<Products_Loggin>();
			try
			{
				var products = await _context.Products
						.Where(s => s.ProductID == update_.ProductID && s.DeleteStatus == 1).FirstOrDefaultAsync();
				if (update_.ProductID <= 0 || products == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "ProductID không hợp lệ || không tồn tại!";
					return returnData;
				}
				if (update_.ProductName != null)
				{
					if (!Validation.CheckString(update_.ProductName) || !Validation.CheckXSSInput(update_.ProductName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductName không hợp lệ || chứa kí tự không hợp lệ!";
						return returnData;
					}
					products.ProductName = update_.ProductName;
				}
				if (update_.ProductsOfServicesID != null)
				{
					if (await GetProductOfServicesByID(update_.ProductsOfServicesID) == null 
						|| update_.ProductsOfServicesID <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductsOfServicesID không hợp lệ || không tồn tại!";
						return returnData;
					}
					products.ProductsOfServicesID = update_.ProductsOfServicesID;
				}
				if (update_.SupplierID != null)
				{
					if (update_.SupplierID <= 0 ||  await _supplierRepository.GetSupplierBySupplierID(update_.SupplierID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "SupplierID không hợp lệ || không tồn tại!";
						return returnData;
					}
					products.SupplierID = update_.SupplierID;
				}
				if (update_.ProductDescription != null)
				{
					if (!Validation.CheckString(update_.ProductDescription) || !Validation.CheckXSSInput(update_.ProductDescription))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductDescription không hợp lệ || chứa kí tự không hợp lệ!";
						return returnData;
					}
					products.ProductDescription = update_.ProductDescription;
				}
				if (update_.SellingPrice != null)
				{
					if (update_.SellingPrice < 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "SellingPrice không hợp lệ!";
						return returnData;
					}
					products.SellingPrice = update_.SellingPrice;
				}
				if (update_.Quantity != null)
				{
					if (update_.Quantity < 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "SellingPrice không hợp lệ!";
						return returnData;
					}
					products.Quantity = update_.Quantity;
				}
				if (update_.ProductImages != null)
				{
					if (!Validation.CheckXSSInput(update_.ProductImages) || !Validation.CheckString(update_.ProductImages))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductImages không hợp lệ || chứa kí tự không hợp lệ!";
						return returnData;
					}
					var imagePathProducts = await BaseProcessingFunction64(update_.ProductImages);
					products.ProductImages = imagePathProducts;
				}
				_context.Products.Update(products);
				await _context.SaveChangesAsync();
				product_Loggins.Add(new Products_Loggin
				{
					ProductID = products.ProductID,
					ProductsOfServicesID = products.ProductsOfServicesID,
					SupplierID = products.SupplierID,
					ProductName = products.ProductName,
					ProductDescription = products.ProductDescription,
					SellingPrice = products.SellingPrice,
					Quantity = products.Quantity,
					ProductImages = products.ProductImages,
					DeleteStatus = 1
				});
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Update Product thành công!";
				returnData.products_Loggins = product_Loggins;
				return returnData;
			}
			catch(Exception ex) 
			{
				throw new Exception($"Error Update_Products Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseProducts_LogginDelete> Delete_Products(Delete_Products delete_)
		{
			var returnData = new ResponseProducts_LogginDelete();
			var product_Loggins = new List<Products_Loggin>();
			var comment_Loggins = new List<Comment_Loggin>();
			var cartProducts_Loggins = new List<CartProducts_Loggin>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var products = await _context.Products
					.Include(a => a.Comments)
					.Include(b => b.CartProducts)
					.AsSplitQuery()
					.Where(s => s.ProductID == delete_.ProductID && s.DeleteStatus == 1).FirstOrDefaultAsync();
				if (products != null)
				{
					//1.Xóa Products
					products.DeleteStatus = 0;
					product_Loggins.Add(new Products_Loggin
					{
						ProductID = products.ProductID,
						ProductsOfServicesID = products.ProductsOfServicesID,
						SupplierID = products.SupplierID,
						ProductName = products.ProductName,
						ProductDescription = products.ProductDescription,
						SellingPrice = products.SellingPrice,
						Quantity = products.Quantity,
						ProductImages = products.ProductImages,
						DeleteStatus = 0
					});

					//2.Xóa Comment
					var comments = products.Comments
						.Where(s => s.ProductID == products.ProductID).ToList();
					if (comments != null && comments.Any())
					{
						foreach (var item in comments)
						{
							_context.Comments.Remove(item);
							//Lưu log comment
							comment_Loggins.Add(new Comment_Loggin
							{
								CommentID = item.CommentID,
								ProductID = item.ProductID,
								ServiceID = item.ServiceID,
								UserID = item.UserID,
								Comment_Content = item.Comment_Content,
								CreationDate = item.CreationDate,
							});
						}
					}

					//3.Xóa CartProducts
					var cartProducts = products.CartProducts
						.Where(s => s.ProductID == products.ProductID).ToList();
					if (cartProducts != null && cartProducts.Any())
					{
						foreach (var item in cartProducts)
						{
							_context.CartProduct.Remove(item);
							//Lưu log cartProducts
							cartProducts_Loggins.Add(new CartProducts_Loggin
							{
								CartProductID = item.CartProductID,
								CartID = item.CartID,
								ProductID = item.ProductID,
								Quantity = item.Quantity,
								CreateDay = item.CreateDay
							});
						}
					}

					await transaction.CommitAsync();
					await _context.SaveChangesAsync();
					
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Delete Product thành công!";
					returnData.products_Loggins = product_Loggins;
					returnData.comment_Loggins = comment_Loggins;
					returnData.cartProducts_Loggins = cartProducts_Loggins;
					return returnData;
				}

				returnData.ResponseCode = -1;
				returnData.ResposeMessage = "ProductID không hợp lệ || không tồn tại!";
				return returnData;
			}
			catch (Exception ex) 
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error Delete_Products Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseProducts_LogginGetList> GetList_SearchProducts(GetList_SearchProducts getList_)
		{
			var returnData = new ResponseProducts_LogginGetList();
			try
			{
				if (getList_.ProductID != null)
				{
					if (getList_.ProductID <= 0 || await GetProductsByProductID(getList_.ProductID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductID không hợp lệ || không tồn tại!";
						return returnData;
					}
				}
				if (getList_.ProductsOfServicesName != null)
				{
					if (!Validation.CheckString(getList_.ProductsOfServicesName) || !Validation.CheckXSSInput(getList_.ProductsOfServicesName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductsOfServicesName không hợp lệ || Chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.SupplierName != null)
				{
					if (!Validation.CheckString(getList_.SupplierName) || !Validation.CheckXSSInput(getList_.SupplierName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "SupplierName không hợp lệ || Chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.ProductName != null)
				{
					if (!Validation.CheckString(getList_.ProductName) || !Validation.CheckXSSInput(getList_.ProductName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductName không hợp lệ || Chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				var parametters = new DynamicParameters();
				parametters.Add("@ProductID", getList_.ProductID ?? null);
				parametters.Add("@ProductName", getList_.ProductName ?? null);
				parametters.Add("@ProductsOfServicesName", getList_.ProductsOfServicesName ?? null);
				parametters.Add("@SupplierName", getList_.SupplierName ?? null);

				var result = await DbConnection.QueryAsync<ResponseGetListProducts>("GetList_SearchProduct", parametters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Get list Products thành công!";
					returnData.Data = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy Products nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetList_SearchProducts Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseData> ExportProductsToExcel(ExportProductExcel filePath)
		{
			var returnData = new ResponseData();
			try
			{
				if (filePath.ProductID != null)
				{
					if (filePath.ProductID <= 0 || await GetProductsByProductID(filePath.ProductID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductID không hợp lệ || không tồn tại!";
						return returnData;
					}
				}
				if (filePath.ProductsOfServicesName != null)
				{
					if (!Validation.CheckString(filePath.ProductsOfServicesName) || !Validation.CheckXSSInput(filePath.ProductsOfServicesName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductsOfServicesName không hợp lệ || Chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				if (filePath.SupplierName != null)
				{
					if (!Validation.CheckString(filePath.SupplierName) || !Validation.CheckXSSInput(filePath.SupplierName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "SupplierName không hợp lệ || Chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				if (filePath.ProductName != null)
				{
					if (!Validation.CheckString(filePath.ProductName) || !Validation.CheckXSSInput(filePath.ProductName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductName không hợp lệ || Chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				if (filePath == null || string.IsNullOrWhiteSpace(filePath.filePath))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Đường dẫn file không hợp lệ!";
					return returnData;
				}

				// Kiểm tra và làm sạch đường dẫn
				string cleanedPath = filePath.filePath
					.Replace("\u202A", "") // Loại bỏ ký tự Unicode ẩn (U+202A)
					.Replace("/", "\\") // Chuyển dấu / thành \ cho Windows
					.Trim(); // Loại bỏ khoảng trắng thừa

				// Chuẩn hóa đường dẫn
				string fullPath = Path.GetFullPath(cleanedPath);

				// Kiểm tra xem file có phải định dạng Excel không
				string extension = Path.GetExtension(fullPath);
				if (string.IsNullOrEmpty(extension) || (extension.ToLower() != ".xlsx" && extension.ToLower() != ".xls"))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "File không đúng định dạng Excel (.xlsx hoặc .xls)!";
					return returnData;
				}

				// Đảm bảo thư mục cha tồn tại
				string directory = Path.GetDirectoryName(fullPath);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}

				var parametters = new DynamicParameters();
				parametters.Add("@ProductID", filePath.ProductID ?? null);
				parametters.Add("@ProductName", filePath.ProductName ?? null);
				parametters.Add("@ProductsOfServicesName", filePath.ProductsOfServicesName ?? null);
				parametters.Add("@SupplierName", filePath.SupplierName ?? null);
				var listProduct = await DbConnection.QueryAsync<ResponseGetListProducts>("GetList_SearchProduct", parametters);

				if (listProduct != null && listProduct.Any())
				{
					using (var workBook = new XLWorkbook())
					{
						var workSheet = workBook.AddWorksheet("List_Products");

						workSheet.Cell(1, 1).Value = "ProductID";
						workSheet.Cell(1, 2).Value = "ProductName";
						workSheet.Cell(1, 3).Value = "SellingPrice";
						workSheet.Cell(1, 4).Value = "Quantity";
						workSheet.Cell(1, 5).Value = "ProductImages";
						workSheet.Cell(1, 6).Value = "ProductDescription";
						workSheet.Cell(1, 7).Value = "ProductsOfServicesName";
						workSheet.Cell(1, 8).Value = "SupplierName";
						workSheet.Cell(1, 9).Value = "DeleteStatus";

						int row = 2;
						foreach (var item in listProduct)
						{
							workSheet.Cell(row, 1).Value = item.ProductID;
							workSheet.Cell(row, 2).Value = item.ProductName;
							workSheet.Cell(row, 3).Value = item.SellingPrice;
							workSheet.Cell(row, 4).Value = item.Quantity;
							workSheet.Cell(row, 5).Value = item.ProductImages;
							workSheet.Cell(row, 6).Value = item.ProductDescription;
							workSheet.Cell(row, 7).Value = item.ProductsOfServicesName;
							workSheet.Cell(row, 8).Value = item.SupplierName;
							workSheet.Cell(row, 9).Value = item.DeleteStatus;
							row++;
						}

						workBook.SaveAs(fullPath);

						returnData.ResponseCode = 1;
						returnData.ResposeMessage = "Xuất file Excel thành công!";
						return returnData;
					}
				}

				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không có dữ liệu để xuất!";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error ExportProductsToExcel Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task UpdateQuantityPro(int productID, int quantity)
		{
			try
			{
				var product = await _context.Products.Where(s => s.ProductID == productID && s.DeleteStatus == 1).FirstOrDefaultAsync();
				if (product != null)
				{
					product.Quantity = product.Quantity - quantity;
					await _context.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error UpdateQuantityPro Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseProducts_LogginGetList> GetSortedPagedProducts(SortListProducts sortList_)
		{
			var returnData = new ResponseProducts_LogginGetList();
			try
			{
				// Kiểm tra điều kiện đầu vào
				if (sortList_.PageIndex != null)
				{
					if (sortList_.PageIndex <= 0 || sortList_.PageSize <= 0 || sortList_.PageSize == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "PageIndex || PageSize không hợp lệ!";
						return returnData;
					}
				}
				if (sortList_.PageSize != null)
				{
					if (sortList_.PageSize <= 0 || sortList_.PageIndex <= 0 || sortList_.PageIndex == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "PageIndex || PageSize không hợp lệ!";
						return returnData;
					}
				}
				if (sortList_.MinPrice != null)
				{
					if (sortList_.MinPrice <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "MinPrice không hợp lệ!";
						return returnData;
					}
				}
				if (sortList_.MaxPrice != null)
				{
					if (sortList_.MaxPrice <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "MaxPrice không hợp lệ!";
						return returnData;
					}
				}

				// Khai báo tham số cho stored procedure
				var parameters = new DynamicParameters();
				parameters.Add("@PageIndex", sortList_.PageIndex ?? null);
				parameters.Add("@PageSize", sortList_.PageSize ?? null);
				parameters.Add("@MinPrice", sortList_.MinPrice ?? null);
				parameters.Add("@MaxPrice", sortList_.MaxPrice ?? null);
				parameters.Add("@SupplierName", sortList_.SupplierName ?? null);
				parameters.Add("@ProductsOfServicesName", sortList_.ProductsOfServicesName ?? null);
				// Thêm tham số đầu ra @TotalCount
				parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

				// Gọi stored procedure
				var result = await DbConnection.QueryAsync<ResponseGetListProducts>(
					"GetSortedPagedProducts",
					parameters,
					commandType: CommandType.StoredProcedure
				);

				// Lấy giá trị đầu ra từ @TotalCount
				var totalCount = parameters.Get<int>("@TotalCount");

				// Xử lý kết quả
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "GetSortedPagedProducts sản phẩm thành công!";
					returnData.CountProducts = totalCount;
					returnData.Data = result.ToList();
					return returnData;
				}

				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy sản phẩm nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error SortListProduct Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
	}
}
