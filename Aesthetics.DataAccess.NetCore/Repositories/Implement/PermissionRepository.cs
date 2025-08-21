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
				var permission = await _context.Permission.Where(s => s.PermissionID == update_.PermissionID).FirstOrDefaultAsync();
				if (permission != null)
				{
					if (update_.UserID != null)
					{
						var userID = await _context.Users
							.Where(s => s.UserID == update_.UserID && s.DeleteStatus == 1).FirstOrDefaultAsync();
						if (update_.IsView != 0 && update_.IsView != 1)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = "IsView không hợp lệ!";
							return returnData;
						}
						if (update_.IsDelete != 0 && update_.IsDelete != 1)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = "IsDelete không hợp lệ!";
							return returnData;
						}
						if (update_.IsInsert != 0 && update_.IsInsert != 1)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = "IsInsert không hợp lệ!";
							return returnData;
						}
						if (update_.IsUpdate != 0 && update_.IsUpdate != 1)
						{
							returnData.ResponseCode = -1;
							returnData.ResposeMessage = "IsUpdate không hợp lệ!";
							return returnData;
						}
						permission.FunctionID = update_.FunctionID;
						 permission.IsView = update_.IsView;
						permission.IsDelete = update_.IsDelete ;
						permission.IsInsert =update_.IsInsert  ;
						permission.IsUpdate = update_.IsUpdate ;
						_context.Permission.Update(permission);
						await _context.SaveChangesAsync();
						returnData.ResponseCode = 1;
						returnData.ResposeMessage = $"Cập nhật thông tin thành công!";
						return returnData;
					}
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "UserID không tồn tại";
					return returnData;
				}
				returnData.ResponseCode = -1;
				returnData.ResposeMessage = "PermissionID không tồn tại";
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
	}
}
