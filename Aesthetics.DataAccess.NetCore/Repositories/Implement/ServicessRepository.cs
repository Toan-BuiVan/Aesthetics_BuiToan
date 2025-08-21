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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class ServicessRepository : BaseApplicationService, IServicessRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		private static List<Servicess> _listSevicess;
		public ServicessRepository(DB_Context context, IConfiguration configuration,
			 IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
			_listSevicess = new List<Servicess>();
		}

		public async Task<string> BaseProcessingFunction64(string? ServicessImage)
		{
			try
			{
				var path = "FilesImages/Servicess";

				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}
				string imageName = Guid.NewGuid().ToString() + ".png";
				var imgPath = Path.Combine(path, imageName);

				if (ServicessImage.Contains("data:image"))
				{
					ServicessImage = ServicessImage.Substring(ServicessImage.LastIndexOf(',') + 1);
				}

				byte[] imageBytes = Convert.FromBase64String(ServicessImage);
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

		public async Task<Servicess> GetServicessByServicesID(int? ServicesID)
		{
			return await _context.Servicess.Where(s => s.ServiceID == ServicesID && s.DeleteStatus == 1).FirstOrDefaultAsync();
		}

		public async Task<TypeProductsOfServices> GetProductOfServicesByID(int? ProductsOfServicesID)
		{
			return await _context.TypeProductsOfServices.Where(s => s.ProductsOfServicesID == ProductsOfServicesID
				&& s.ProductsOfServicesType == "Servicess"
				&& s.DeleteStatus == 1).FirstOrDefaultAsync();
		}

		public async Task<ResponseServicess_Loggin> Insert_Servicess(ServicessRequest servicess_)
		{
			var returnData = new ResponseServicess_Loggin();
			var servicess_Loggins = new List<Servicess_Loggin>();
			try
			{
				if (servicess_.ProductsOfServicesID <= 0
					|| await GetProductOfServicesByID(servicess_.ProductsOfServicesID) == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Dữ liệu đầu vào ProductsOfServicesID không hợp lệ || Không tồn tại!";
					return returnData;
				}
				if (!Validation.CheckString(servicess_.ServiceName) || !Validation.CheckXSSInput(servicess_.ServiceName))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Dữ liệu đầu vào ServiceName không hợp lệ || Dữ liệu ServiceName chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (!Validation.CheckString(servicess_.Description) || !Validation.CheckXSSInput(servicess_.Description))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Dữ liệu đầu vào Description không hợp lệ || Dữ liệu Description chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (servicess_.PriceService < 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Dữ liệu đầu vào PriceService không hợp lệ!";
					return returnData;
				}
				if (!Validation.CheckXSSInput(servicess_.ServiceImage) || !Validation.CheckString(servicess_.ServiceImage))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Dữ liệu ServiceImage chứa kí tự không hợp lệ || Dữ liệu đầu vào ServiceImage không hợp lệ!";
					return returnData;
				}

				var imagePathServicess = await BaseProcessingFunction64(servicess_.ServiceImage);
				var parameters = new DynamicParameters();
				parameters.Add("@ProductsOfServicesID",servicess_.ProductsOfServicesID);
				parameters.Add("@ServiceName", servicess_.ServiceName);
				parameters.Add("@Description", servicess_.Description);
				parameters.Add("@ServiceImage", imagePathServicess);
				parameters.Add("@PriceService", servicess_.PriceService);
				parameters.Add("@ServiceID", dbType: DbType.Int32, direction: ParameterDirection.Output);
				await DbConnection.ExecuteAsync("Insert_Servicess", parameters);
				var newServicessID = parameters.Get<int>("@ServiceID");
				servicess_Loggins.Add(new Servicess_Loggin
				{
					ServiceID = newServicessID,
					ProductsOfServicesID = servicess_.ProductsOfServicesID,
					ServiceName = servicess_.ServiceName,
					Description = servicess_.Description,
					ServiceImage = imagePathServicess,
					PriceService = servicess_.PriceService,
					DeleteStatus = 1
				});
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Insert thành công Service!";
				returnData.servicess_Loggins = servicess_Loggins;
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error Insert_Servicess Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseServicess_Loggin> Update_Servicess(Update_Servicess update_)
		{
			var returnData = new ResponseServicess_Loggin();
			var servicess_Loggins = new List<Servicess_Loggin>();
			try
			{
				if (update_.ServiceID <= 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Dữ liệu đầu vào ServiceID không hợp lệ!";
					return returnData;
				}
				if (update_.ProductsOfServicesID != null)
				{
					if (update_.ProductsOfServicesID <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào ProductsOfServicesID không hợp lệ!";
						return returnData;
					}
					if (await GetProductOfServicesByID(update_.ProductsOfServicesID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "ProductsOfServicesID không tồn tại. Vui lòng nhập lại!";
						return returnData;
					}
				}
	
				if (update_.ServiceName != null) 
				{
					if (!Validation.CheckString(update_.ServiceName) || !Validation.CheckXSSInput(update_.ServiceName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào ServiceName không hợp lệ || Dữ liệu ServiceName chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				if (update_.Description != null) 
				{
					if (!Validation.CheckString(update_.Description) || !Validation.CheckXSSInput(update_.Description))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào Description không hợp lệ || Dữ liệu Description chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				if (update_.PriceService != null)
				{
					if (update_.PriceService < 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào PriceService không hợp lệ!";
						return returnData;
					}
				}
				if (update_.ServiceImage != null)
				{
					if (!Validation.CheckXSSInput(update_.ServiceImage) || !Validation.CheckString(update_.ServiceImage))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu ServiceImage chứa kí tự không hợp lệ || Dữ liệu đầu vào ServiceImage không hợp lệ!";
						return returnData;
					}
				}
				var imagePathServicess = update_.ServiceImage != null 
					? await BaseProcessingFunction64(update_.ServiceImage) : null;
				var parameters = new DynamicParameters();
				parameters.Add("@ServiceID", update_.ServiceID);
				parameters.Add("@ProductsOfServicesID", update_.ProductsOfServicesID ?? null);
				parameters.Add("@ServiceName", update_.ServiceName ?? null);
				parameters.Add("@Description", update_.Description ?? null);
				parameters.Add("@ServiceImage",imagePathServicess ?? null);
				parameters.Add("@PriceService", update_.PriceService ?? null);
				await DbConnection.ExecuteAsync("Update_Servicess", parameters);
				servicess_Loggins.Add(new Servicess_Loggin
				{
					ServiceID = update_.ServiceID,
					ProductsOfServicesID = update_.ProductsOfServicesID ?? 0,
					ServiceName = update_.ServiceName ?? null,
					Description = update_.Description ?? null,
					ServiceImage = imagePathServicess ?? null,
					PriceService = update_.PriceService ?? 0,
					DeleteStatus = 1
				});
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Update thành công Service!";
				returnData.servicess_Loggins = servicess_Loggins;
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error Update_Servicess Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseServicess_LogginDelete> Delete_Servicess(Delete_Servicess delete_)
		{
			var returnData = new ResponseServicess_LogginDelete();
			var servicess_Loggins = new List<Servicess_Loggin>();
			var comment_Loggins = new List<Comment_Loggin>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				if(delete_.ServiceID <=0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Dữ liệu đầu vào ServiceID không hợp lệ!";
					return returnData;
				}
				var servicess = await _context.Servicess
					.Include(b => b.Comments)
					.AsSplitQuery()
					.FirstOrDefaultAsync(s => s.ServiceID == delete_.ServiceID && s.DeleteStatus == 1);
				if (servicess != null)
				{
					//1. Xóa Sevicess nếu tồn tại
					servicess.DeleteStatus = 0;
					servicess_Loggins.Add(new Servicess_Loggin
					{
						ServiceID = servicess.ServiceID,
						ProductsOfServicesID = servicess.ProductsOfServicesID,
						ServiceName = servicess.ServiceName,
						Description = servicess.Description,
						ServiceImage = servicess.ServiceImage,
						PriceService = servicess.PriceService,
						DeleteStatus = servicess.DeleteStatus,
					});


					//2. Xóa Comment liên quan đến ServiceID
					var comments = servicess.Comments
						.Where(s => s.ServiceID == servicess.ServiceID).ToList();
					if (comments != null && comments.Any())
					{
						foreach (var item in comments)
						{
							_context.Comments.Remove(item);
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
					await _context.SaveChangesAsync();
					//Commit transaction nếu thành công
					await transaction.CommitAsync();

					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Xóa thành công Service!";
					returnData.servicess_Loggins = servicess_Loggins;
					returnData.comment_Loggins = comment_Loggins;
					return returnData;
				}
				returnData.ResponseCode = -1;
				returnData.ResposeMessage = "ServiceID không tồn tại. Vui lòng nhập lại!";
				return returnData;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error Delete_Servicess Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseServicess_Loggin> GetList_SearchServicess(GetList_SearchServicess getList_)
		{
			var returnData = new ResponseServicess_Loggin();
			var listData = new List<ResponseServicess>();
			try
			{
				if (getList_.ServiceID != null)
				{
					if (getList_.ServiceID <=0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào ServiceID không hợp lệ!";
						return returnData;
					}
					if (await GetServicessByServicesID(getList_.ServiceID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Danh sách không tồn tại Service: {getList_.ServiceID}!";
						return returnData;
					}
				}
				if (getList_.ServiceName != null)
				{
					if (!Validation.CheckString(getList_.ServiceName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào ServiceName không hợp lệ!";
						return returnData;
					}
					if (!Validation.CheckXSSInput(getList_.ServiceName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu ServiceName chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				if (getList_.ProductsOfServicesID != null)
				{

					if (getList_.ProductsOfServicesID <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào ProductsOfServicesID không hợp lệ!";
						return returnData;
					}
					if (await GetProductOfServicesByID(getList_.ProductsOfServicesID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Danh sách Service không tồn tại Service có ProductsOfServicesID: {getList_.ProductsOfServicesID}!";
						return returnData;
					}
				}
			
				var parameters = new DynamicParameters();
				parameters.Add("@ServiceID", getList_.ServiceID ?? null);
				parameters.Add("@ServiceName", getList_.ServiceName ?? null);
				parameters.Add("@ProductsOfServicesID", getList_.ProductsOfServicesID ?? null);
				var result = await DbConnection.QueryAsync<ResponseServicess>("GetList_SearchServicess", parameters);
				if (result != null && result.Any()) 
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách Services thành công!";
					returnData.Data = result.ToList();
					return returnData;
				}
				else
				{
					returnData.ResponseCode = 0;
					returnData.ResposeMessage = "Không tìm thấy Services nào.";
					return returnData;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetList_SearchServicess Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseData> ExportServicessToExcel(ExportSevicessExcel filePath)
		{
			var returnData = new ResponseData();
			try
			{
				if (filePath.ServiceID != null)
				{
					if (filePath.ServiceID <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào ServiceID không hợp lệ!";
						return returnData;
					}
					if (await GetServicessByServicesID(filePath.ServiceID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Danh sách không tồn tại Service: {filePath.ServiceID}!";
						return returnData;
					}
				}
				if (filePath.ServiceName != null)
				{
					if (!Validation.CheckString(filePath.ServiceName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào ServiceName không hợp lệ!";
						return returnData;
					}
					if (!Validation.CheckXSSInput(filePath.ServiceName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu ServiceName chứa kí tự không hợp lệ!";
						return returnData;
					}
				}
				if (filePath.ProductsOfServicesID != null)
				{
					if (filePath.ProductsOfServicesID <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Dữ liệu đầu vào ProductsOfServicesID không hợp lệ!";
						return returnData;
					}
					if (await GetProductOfServicesByID(filePath.ProductsOfServicesID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Danh sách Service không tồn tại Service có ProductsOfServicesID: {filePath.ProductsOfServicesID}!";
						return returnData;
					}
				}
				if (filePath == null || string.IsNullOrWhiteSpace(filePath.filePath))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Đường dẫn file không hợp lệ!";
					return returnData;
				}

				var parameters = new DynamicParameters();
				parameters.Add("@ServiceID", filePath.ServiceID ?? null);
				parameters.Add("@ServiceName", filePath.ServiceName ?? null);
				parameters.Add("@ProductsOfServicesID", filePath.ProductsOfServicesID ?? null);
				var listServicess = await DbConnection.QueryAsync<ResponseServicess>("GetList_SearchServicess", parameters);
				if (listServicess == null || !listServicess.Any())
				{
					returnData.ResponseCode = 0;
					returnData.ResposeMessage = "Không có dữ liệu để xuất!";
					return returnData;
				}

				using (var workBook = new XLWorkbook())
				{
					var workSheet = workBook.AddWorksheet("List_Servicess");
					workSheet.Cell(1, 1).Value = "ServiceID";
					workSheet.Cell(1, 2).Value = "ProductsOfServicesName";
					workSheet.Cell(1, 3).Value = "ServiceName";
					workSheet.Cell(1, 4).Value = "Description";
					workSheet.Cell(1, 5).Value = "ServiceImage";
					workSheet.Cell(1, 6).Value = "PriceService";

					int row = 2;
					foreach (var item in listServicess)
					{
						workSheet.Cell(row, 1).Value = item.ServiceID;
						workSheet.Cell(row, 2).Value = item.ProductsOfServicesName;
						workSheet.Cell(row, 3).Value = item.ServiceName;
						workSheet.Cell(row, 4).Value = item.Description;
						workSheet.Cell(row, 5).Value = item.ServiceImage;
						workSheet.Cell(row, 6).Value = item.PriceService;
						row++;
					}

					// Làm sạch và chuẩn hóa đường dẫn
					string cleanedPath = filePath.filePath
						.Replace("\u202A", "") // Loại bỏ ký tự Unicode ẩn (U+202A)
						.Replace("/", "\\") // Chuyển dấu / thành \ cho Windows
						.Trim(); // Loại bỏ khoảng trắng thừa
					string fullPath = Path.GetFullPath(cleanedPath);

					// Đảm bảo thư mục cha tồn tại
					string directory = Path.GetDirectoryName(fullPath);
					if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					// Thêm đuôi .xlsx nếu chưa có
					if (!fullPath.EndsWith(".xlsx"))
					{
						fullPath += ".xlsx";
					}

					workBook.SaveAs(fullPath);
				}

				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Xuất file Excel thành công!";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error ExportServicessToExcel Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseServicess_Loggin> GetSortedPagedServicess(SortListSevicess sortList_)
		{
			var returnData = new ResponseServicess_Loggin();
			var listData = new List<ResponseServicess>();
			try
			{
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
					if (sortList_.MinPrice <=0)
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
				var parameters = new DynamicParameters();
				parameters.Add("@PageIndex", sortList_.PageIndex ?? null);
				parameters.Add("@PageSize", sortList_.PageSize ?? null);
				parameters.Add("@MinPrice", sortList_.MinPrice ?? null);
				parameters.Add("@MaxPrice", sortList_.MaxPrice ?? null);
				parameters.Add("@ProductsOfServicesName", sortList_.ProductsOfServicesName ?? null);
				// Thêm tham số đầu ra @TotalCount
				parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
				// Gọi stored procedure
				var result = await DbConnection.QueryAsync<ResponseServicess>(
					"GetSortedPagedServicess",
					parameters,
					commandType: CommandType.StoredProcedure
				);

				// Lấy giá trị đầu ra từ @TotalCount
				var totalCount = parameters.Get<int>("@TotalCount");

				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "GetSortedPagedServicess Servicess thành công!";
					returnData.CountServices = totalCount;
					returnData.Data = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy Servicess nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error SortListService Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
	}
}
