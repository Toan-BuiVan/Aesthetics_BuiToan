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
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class ClinicRepository : BaseApplicationService, IClinicRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		private ITypeProductsOfServicesRepository _productsOfServicesRepository;
		public ClinicRepository(DB_Context context, IConfiguration configuration,
			IServiceProvider serviceProvider,
			ITypeProductsOfServicesRepository productsOfServicesRepository) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
			_productsOfServicesRepository = productsOfServicesRepository;
		}

		public async Task<Clinic> GetClinicByName(string? name)
		{
			return await _context.Clinic.Where(s => s.ClinicName == name && s.ClinicStatus == 1).FirstOrDefaultAsync();
		}

		public async Task<Clinic> GetClinicByID(int? ClinicID)
		{
			return await _context.Clinic.Where(s => s.ClinicID == ClinicID && s.ClinicStatus == 1).FirstOrDefaultAsync();
		}
		    
		public async Task<ResponesClinic_Loggin> Insert_Clinic(ClinicRequest insert_)
		{
			var returnData = new ResponesClinic_Loggin();
			var listClinics = new List<Clinic_Loggin>();
			try
			{
				if (!Validation.CheckString(insert_.ClinicName) || !Validation.CheckXSSInput(insert_.ClinicName))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"Dữ liệu đầu vào {insert_.ClinicName} không hợp lệ!";
					return returnData;
				}

				if (await GetClinicByName(insert_.ClinicName) != null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"{insert_.ClinicName} đã tồn tại. Vui lòng nhập ClinicName khác!";
					return returnData;
				}

				if (insert_.ProductsOfServicesID <= 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"Dữ liệu đầu vào {insert_.ProductsOfServicesID} không hợp lệ!";
					return returnData;
				}
				if (await _productsOfServicesRepository
					.GetTypeProductsOfServicesIDByID(insert_.ProductsOfServicesID) == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"Không tồn tại ProductsOfServicesID: {insert_.ProductsOfServicesID}";
					return returnData;
				} 
				if (!Validation.CheckString(insert_.ProductsOfServicesName))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"Dữ liệu đầu vào {insert_.ProductsOfServicesName} không hợp lệ!";
					return returnData; 
				}
				if (!Validation.CheckXSSInput(insert_.ProductsOfServicesName))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"Dữ liệu {insert_.ProductsOfServicesName} chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (await _productsOfServicesRepository
					.GetTypeProductsOfServicesByName(insert_.ProductsOfServicesName) == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"Không tồn tại ProductsOfServices có Name: {insert_.ProductsOfServicesName}";
					return returnData;
				}

				var newClinic = new Clinic
				{
					ClinicName = insert_.ClinicName,
					ProductsOfServicesID = insert_.ProductsOfServicesID,
					ProductsOfServicesName = insert_.ProductsOfServicesName,
					ClinicStatus = 1
				};
				await _context.Clinic.AddAsync(newClinic);
				await _context.SaveChangesAsync();
				listClinics.Add(new Clinic_Loggin
				{
					ClinicID = newClinic.ClinicID,
					ClinicName = newClinic.ClinicName,
					ProductsOfServicesID = newClinic.ProductsOfServicesID,
					ProductsOfServicesName = newClinic.ProductsOfServicesName,
					ClinicStatus = 1
				});
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Insert thành công Clinic!";
				returnData.listClinics = listClinics;
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error Insert_Clinic Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponesClinic_Loggin> Update_Clinic(Update_Clinic update_)
		{
			var returnData = new ResponesClinic_Loggin();
			var listClinics = new List<Clinic_Loggin>();
			try
			{
				var _clinic = await GetClinicByID(update_.ClinicID);
				if (update_.ClinicID <= 0 || _clinic == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"Dữ liệu đầu vào ClinicID không hợp lệ || Không tồn tại Clinic có ID: {update_.ClinicID}!";
					return returnData;
				}
				_clinic.ClinicID = update_.ClinicID;
				if (update_.ClinicName != null)
				{
					if (!Validation.CheckString(update_.ClinicName) || !Validation.CheckXSSInput(update_.ClinicName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Dữ liệu {update_.ClinicName} không hợp lệ!";
						return returnData;
					}

					if (await GetClinicByName(update_.ClinicName) != null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"{update_.ClinicName} đã tồn tại. Vui lòng nhập ClinicName khác!";
						return returnData;
					}
					_clinic.ClinicName = update_.ClinicName;
				}
				if (update_.ProductsOfServicesID != null)
				{
					if (update_.ProductsOfServicesID <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Dữ liệu đầu vào {update_.ProductsOfServicesID} không hợp lệ!";
						return returnData;
					}
					if (await _productsOfServicesRepository
						.GetTypeProductsOfServicesIDByID(update_.ProductsOfServicesID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Không tồn tại ProductsOfServicesID: {update_.ProductsOfServicesID}";
						return returnData;
					}
					_clinic.ProductsOfServicesID = update_.ProductsOfServicesID ?? 0;
				}
				if (update_.ProductsOfServicesName != null)
				{
					if (!Validation.CheckString(update_.ProductsOfServicesName) 
						|| !Validation.CheckXSSInput(update_.ProductsOfServicesName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Dữ liệu đầu vào {update_.ProductsOfServicesName} không hợp lệ!";
						return returnData;
					}

					if (await _productsOfServicesRepository
						.GetTypeProductsOfServicesByName(update_.ProductsOfServicesName) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Không tồn tại ProductsOfServices có Name: {update_.ProductsOfServicesName}";
						return returnData;
					}
					_clinic.ProductsOfServicesName = update_.ProductsOfServicesName;
				}
				_context.Clinic.Update(_clinic);
				await _context.SaveChangesAsync();
				listClinics.Add(new Clinic_Loggin
				{
					ClinicID = _clinic.ClinicID,
					ClinicName = _clinic.ClinicName ?? null,
					ProductsOfServicesID = _clinic.ProductsOfServicesID,
					ProductsOfServicesName = _clinic.ProductsOfServicesName,
					ClinicStatus = 1
				});
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Update thành công Clinic!";
				returnData.listClinics = listClinics;
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error Update_Clinic Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponesClinic_DeleteLoggin> Delete_Clinic(Delete_Clinic delete_)
		{
			var returnData = new ResponesClinic_DeleteLoggin();
			var clinic_Loggins = new List<Clinic_Loggin>();
			var booking_AssignmentLoggins = new List<Booking_AssignmentLoggin>();
			var clinic_Staff_Loggins = new List<Clinic_Staff_Loggin>();
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				if (delete_.ClinicID <= 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "ClinicID không hợp lệ!";
					return returnData;
				}
				var dataClinic = await _context.Clinic
					.Include(s => s.Clinic_Staff)
					.Include(s => s.BookingAssignment)
					.AsSplitQuery()
					.FirstOrDefaultAsync(v => v.ClinicID == delete_.ClinicID);
				if (dataClinic != null)
				{
					// 1. Cập nhật ClinicStatus và lưu thay đổi
					dataClinic.ClinicStatus = 0;
					clinic_Loggins.Add(new Clinic_Loggin
					{
						ClinicID = dataClinic.ClinicID,
						ClinicName = dataClinic.ClinicName,
						ProductsOfServicesID = dataClinic.ProductsOfServicesID,
						ProductsOfServicesName = dataClinic.ProductsOfServicesName,
						ClinicStatus = dataClinic.ClinicStatus
					});
					await _context.SaveChangesAsync(); 

					// 2. Xóa Clinic_Staff liên quan đến ClinicID
					var clinic_Staff = dataClinic.Clinic_Staff
						.Where(s => s.ClinicID == dataClinic.ClinicID).ToList();
					if (clinic_Staff.Any())
					{
						_context.Clinic_Staff.RemoveRange(clinic_Staff); 
						await _context.SaveChangesAsync(); 
						foreach (var clinStaf in clinic_Staff)
						{
							clinic_Staff_Loggins.Add(new Clinic_Staff_Loggin
							{
								ClinicStaffID = clinStaf.ClinicStaffID,
								ClinicID = clinStaf.ClinicID,
								UserID = clinStaf.UserID,
							});
						}
					}
					var bookingAssignment = dataClinic.BookingAssignment
						.Where(s => s.ClinicID == dataClinic.ClinicID && s.AssignedDate > DateTime.Today).ToList();
					if (bookingAssignment.Any())
					{
						foreach (var bookAssign in bookingAssignment)
						{
							bookAssign.DeleteStatus = 0;
							booking_AssignmentLoggins.Add(new Booking_AssignmentLoggin
							{
								AssignmentID = bookAssign.AssignmentID,
								BookingID = bookAssign.BookingID,
								ClinicID = bookAssign.ClinicID,
								ProductsOfServicesID = bookAssign.ProductsOfServicesID,
								UserName = bookAssign.UserName,
								ServiceName = bookAssign.ServiceName,
								NumberOrder = bookAssign.NumberOrder,
								AssignedDate = bookAssign.AssignedDate,
								Status = bookAssign.Status,
								DeleteStatus = bookAssign.DeleteStatus
							});
						}
						await _context.SaveChangesAsync();
					}
					await transaction.CommitAsync();

					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Xóa Clinic thành công!";
					returnData.clinic_Loggins = clinic_Loggins;
					returnData.booking_AssignmentLoggins = booking_AssignmentLoggins;
					returnData.clinic_Staff_Loggins = clinic_Staff_Loggins;
					return returnData;
				}
				returnData.ResponseCode = -1;
				returnData.ResposeMessage = "Không tìm thấy Clinic nào!";
				return returnData;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error Delete_Clinic Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponesClinic_Loggin> GetList_SearchClinic(GetList_Search getList_)
		{
			var responseData = new ResponesClinic_Loggin();
			var listClinic = new List<ResponesClinic>();
			try
			{
				if (getList_.ClinicID != null)
				{
					if (getList_.ClinicID <= 0)
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = "Dữ liệu đầu vào ClinicID không hợp lệ!";
						return responseData;
					}
					if (await GetClinicByID(getList_.ClinicID) == null)
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = $"Không tồn tại Clinic có ID: {getList_.ClinicID}";
						return responseData;
					}
				}
				if (getList_.ClinicName != null)
				{
					if (!Validation.CheckString(getList_.ClinicName))
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = $"Dữ liệu đầu vào {getList_.ClinicName} không hợp lệ!";
						return responseData;
					}
					if (!Validation.CheckXSSInput(getList_.ClinicName))
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = $"Dữ liệu {getList_.ClinicName} chứa kí tự không hợp lệ!";
						return responseData;
					}

					if (await GetClinicByName(getList_.ClinicName) != null)
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = $"{getList_.ClinicName} đã tồn tại. Vui lòng nhập ClinicName khác!";
						return responseData;
					}
				}
				if (getList_.ProductsOfServicesID != null)
				{
					if (getList_.ProductsOfServicesID <= 0)
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = "Dữ liệu đầu vào ProductsOfServicesID không hợp lệ!";
						return responseData;
					}
					if (await _productsOfServicesRepository
					.GetTypeProductsOfServicesIDByID(getList_.ProductsOfServicesID) == null)
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = $"Không tồn tại Clinic có ProductsOfServices: {getList_.ProductsOfServicesID}";
						return responseData;
					}
				}
				if (getList_.ProductsOfServicesName != null)
				{
					if (!Validation.CheckString(getList_.ProductsOfServicesName))
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = $"Dữ liệu đầu vào {getList_.ProductsOfServicesName} không hợp lệ!";
						return responseData;
					}
					if (!Validation.CheckXSSInput(getList_.ProductsOfServicesName))
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = $"Dữ liệu {getList_.ProductsOfServicesName} chứa kí tự không hợp lệ!";
						return responseData;
					}
					if (await _productsOfServicesRepository
						.GetTypeProductsOfServicesByName(getList_.ProductsOfServicesName) == null)
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = $"Không tồn tại ProductsOfServices có Name: {getList_.ProductsOfServicesName}";
						return responseData;
					}
				}
				var parameters = new DynamicParameters();
				parameters.Add("@ClinicID",getList_.ClinicID ?? null);
				parameters.Add("@ClinicName", getList_.ClinicName ?? null);
				parameters.Add("@ProductsOfServicesID", getList_.ProductsOfServicesID ?? null);
				parameters.Add("@ProductsOfServicesName", getList_.ProductsOfServicesName ?? null);
				var result = await DbConnection.QueryAsync<ResponesClinic>("GetList_SearchLinic", parameters);
				if (result != null && result.Any())
				{
					foreach (var clinic in result)
					{
						clinic.ClinicStatus = clinic.ClinicStatus == "Hoạt động" ? "Hoạt động" : "Dừng hoạt động";
					}
					responseData.ResponseCode = 1;
					responseData.ResposeMessage = "Lấy thành công danh sách";
					responseData.Data = result.ToList();
					return responseData;
				}
				else
				{
					responseData.ResponseCode = 0;
					responseData.ResposeMessage = "Không tìm thấy Clinic nào.";
					return responseData;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetList_SearchClinic Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
	}

}
