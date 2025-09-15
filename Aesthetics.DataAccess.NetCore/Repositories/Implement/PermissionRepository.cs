using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using Aesthetics.DTO.NetCore.ResponsePermission;
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
using System.Text.Json;
using System.Data;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class PermissionRepository : BaseApplicationService, IPermissionRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		public PermissionRepository(DB_Context context,
			IConfiguration configuration, IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<ResponsePermission_Loggin> Update_Permission(Update_Permission update_)
		{
			var returnData = new ResponsePermission_Loggin();
			try
			{
				if (update_.TypePerson != null && update_.UserID == null 
					&& update_.FunctionID != null && update_.Status != null)
				{
					var permissions = await _context.Permission
						.Where(p => p.Users.TypePerson == update_.TypePerson 
									&& p.FunctionID == update_.FunctionID)
						.ToListAsync();

					if (permissions.Any())
					{
						foreach (var permission in permissions)
						{
							permission.Status = update_.Status;
						}
						_context.Permission.UpdateRange(permissions);
						await _context.SaveChangesAsync();
						returnData.ResponseCode = 1;
						returnData.ResposeMessage = $"Cập nhật thông tin phân quyền thành " +
							$"công cho tất cả người dùng có TypePerson = {update_.TypePerson}!";
						return returnData;
					}
					else if (!permissions.Any())
					{
						var UserIDs = await _context.Users
							.Where(u => u.TypePerson == update_.TypePerson && u.DeleteStatus == 1)
							.Select(u => u.UserID)
							.ToListAsync();
						foreach (var item in UserIDs)
						{
							var newPermission = new Permission
							{
								UserID = item,
								FunctionID = update_.FunctionID,
								Status = 1
							};
							_context.Permission.Add(newPermission);
						}
						await _context.SaveChangesAsync();
						returnData.ResponseCode = 1;
						returnData.ResposeMessage = $"Cập nhật thông tin phân quyền thành " +
							$"công cho tất cả người dùng có TypePerson = {update_.TypePerson}!";
						return returnData;
					}
					else
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Cập nhật Permission thất bại";
						return returnData;
					}
				}


				if (update_.TypePerson != null && update_.UserID != null && update_.FunctionID != null && update_.Status != null)
				{
					var permission = await _context.Permission
						.Where(s => s.UserID == update_.UserID && s.FunctionID == update_.FunctionID)
						.FirstOrDefaultAsync();

					if (permission != null)
					{
						permission.Status = update_.Status;
						_context.Permission.Update(permission);
						await _context.SaveChangesAsync();
						returnData.ResponseCode = 1;
						returnData.ResposeMessage = "Cập nhật thông tin phân quyền của người dùng thành công!";
						return returnData;
					}
					else if (permission == null)
					{
						var newPermission = new Permission
						{
							UserID = update_.UserID,
							FunctionID = update_.FunctionID,
							Status = 1
						};
						_context.Permission.Add(newPermission);
						await _context.SaveChangesAsync();
						returnData.ResponseCode = 1;
						returnData.ResposeMessage = "Cập nhật thông tin phân quyền của người dùng thành công!";
						return returnData;
					}
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Cập nhật thông tin phân quyền của người dùng thất bại";
					return returnData;
				}

				returnData.ResponseCode = -1;
				returnData.ResposeMessage = "Thiếu thông tin cần thiết để cập nhật (cung cấp PermissionID hoặc TypePerson với FunctionID và Status)";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in Update_Permission Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
		public async Task<ResponsePermission_Loggin> GetList_SearchPermission(PermissionRequest request)
		{
			var returnData = new ResponsePermission_Loggin();
			try
			{
				if (request.UserID != null)
				{
					var user = await _context.Users
					.Where(s => s.UserID == request.UserID && s.DeleteStatus == 1).FirstOrDefaultAsync();
					if (user == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "UserID không tồn tại.";
						return returnData;
					}
				}

				var parameters = new DynamicParameters();
				parameters.Add("@UserID", request.UserID ?? null);
				var result = await DbConnection.QueryAsync<ResponseData_Permission>("GetList_SearchPermission", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách Permission thành công!";
					returnData.Data_Permission = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy Permission nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetList_SearchPermission Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
		public async Task<ResponsePermission_Loggin> Delete_Permission(PermissionRequest _delete)
		{
			var returnData = new ResponsePermission_Loggin();
			try
			{
				var user = await _context.Users
					.Where(s => s.UserID == _delete.UserID && s.DeleteStatus == 1)
					.FirstOrDefaultAsync();
				if (user != null)
				{
					var permissionsToDelete = _context.Permission
						.Where(p => p.UserID == _delete.UserID)
						.ToList();

					foreach (var per in permissionsToDelete)
					{
						_context.Permission.Remove(per);
					}

					await _context.SaveChangesAsync(); 
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Xóa thành công quyền của User!";
					return returnData;
				}
				returnData.ResponseCode = -1;
				returnData.ResposeMessage = "User không tồn tại!";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in Delete_Permission Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
		public async Task<string> GetListTyperson()
		{

			try
			{
				var typePersons = await _context.Users
					.Select(u => u.TypePerson)  
					.Distinct()                 
					.ToListAsync();
				var jsonResult = JsonSerializer.Serialize(typePersons);
				return jsonResult;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetListTyperson Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
		public async Task<GroupBy_Loggin> GroupByPermission(Update_Permission update_)
		{
			var returnData = new GroupBy_Loggin();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@TypePerson", update_.TypePerson);

				var flatResults = await DbConnection.QueryAsync<FlatPermission>("GroupByPermission", parameters);
				if (flatResults != null && flatResults.Any())
				{
					var grouped = flatResults
						.GroupBy(r => r.Function)
						.Select(g => new GroupBy
						{
							Function = g.Key,
							listFuncitons = g.Select(item => new ListFuncion
							{
								FuncitonID = item.FuncitonID,
								FuncionName = item.FuncionName,
								Status = item.Status
							}).ToList()
						}).ToList();

					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách Permission thành công!";
					returnData.Data_Permission = grouped;
					return returnData;
				}

				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy Permission nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GroupByPermission Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
		public async Task<GroupBy_Loggin> GetPermissionByUserID(Update_Permission userID)
		{
			try
			{
				var returnData = new GroupBy_Loggin();
				try
				{
					var parameters = new DynamicParameters();
					parameters.Add("@UserID", userID.UserID);
					parameters.Add("@TypePerson", userID.TypePerson);

					var flatResults = await DbConnection.QueryAsync<FlatPermission>("GroupByPermissionByUser", parameters);
					if (flatResults != null && flatResults.Any())
					{
						var grouped = flatResults
							.GroupBy(r => r.Function)
							.Select(g => new GroupBy
							{
								Function = g.Key,
								listFuncitons = g.Select(item => new ListFuncion
								{
									FuncitonID = item.FuncitonID,
									FuncionName = item.FuncionName,
									Status = item.Status
								}).ToList()
							}).ToList();

						returnData.ResponseCode = 1;
						returnData.ResposeMessage = "Lấy danh sách Permission thành công!";
						returnData.Data_Permission = grouped; 
						return returnData;
					}

					returnData.ResponseCode = 0;
					returnData.ResposeMessage = "Không tìm thấy Permission nào.";
					return returnData;
				}
				catch (Exception ex)
				{
					throw new Exception($"Error in GroupByPermission Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetPermissionByUserID Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
	}
}
