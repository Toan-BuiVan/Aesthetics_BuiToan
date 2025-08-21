using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using BE_102024.DataAces.NetCore.CheckConditions;
using BE_102024.DataAces.NetCore.Dapper;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class UserSessionRepository : BaseApplicationService, IUserSessionRepository
	{
		private DB_Context _context;
		private IUserRepository _userRepository;
		public UserSessionRepository(DB_Context context,
			IServiceProvider serviceProvider, IUserRepository userRepository) : base(serviceProvider)
		{
			_context = context;
			_userRepository = userRepository;
		}

		public async Task<int> DeleleAll_Session(int? UserID)
		{
			var userSession_Loggins = new List<UserSession_Loggin>();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@UserId", UserID);
				userSession_Loggins.Add(new UserSession_Loggin
				{
					UserID = UserID,
				});
				return await DbConnection.ExecuteAsync("UpdateSatusDeleteAll_UserSession", parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error DeleleAll_Session Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<int> Delele_Session(string? token, int? UserID)
		{
			var userSession_Loggins = new List<UserSession_Loggin>();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@UserID", UserID);
				parameters.Add("@Token", token);
				userSession_Loggins.Add(new UserSession_Loggin
				{
					UserID = UserID,
					Token = token
				});
				return await DbConnection.ExecuteAsync("UpdateSatusDelete_UserSession", parameters);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error Delele_Session Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<DataResponse> GetList_SearchUserSession(UserSessionRequest userSession)
		{
			var responseData = new DataResponse();
			try
			{
				if (userSession.UserID <= 0)
				{
					responseData.ResponseCode = -1;
					responseData.ResposeMessage = "Dữ liệu đầu vào UserID không hợp lệ!";
					return responseData;
				}
				if (userSession.UserID != null)
				{
					if (await _userRepository.GetUserByUserID(userSession.UserID) == null)
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = $"Danh sách không tồn tại UserID có mã: {userSession.UserID}!";
						return responseData;
					}
				}
				if (userSession.UserName != null)
				{
					if (!Validation.CheckXSSInput((userSession.UserName)))
					{
						responseData.ResponseCode = -1;
						responseData.ResposeMessage = "UserName chứa kí tự không hợp lệ!";
						return responseData;
					}
				}
				var parameters = new DynamicParameters();
				parameters.Add("@UserID", userSession.UserID ?? null);
				parameters.Add("@UserName", userSession.UserName ?? null);
				var result = await DbConnection.QueryAsync<Data>("GetList_SearchUserSession", parameters);
				if (result != null && result.Any())
				{
					responseData.ResponseCode = 1;
					responseData.ResposeMessage = "Lấy danh sách UserSession thành công!";
					responseData.Data = result.ToList();
					return responseData;
				}
				responseData.ResponseCode = 0;
				responseData.ResposeMessage = "Không tìm thấy UserSession nào.";
				return responseData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetList_SearchUserSession Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<int> Insert_Sesion(UserSession session)
		{
			var userSession_Loggins = new List<UserSession_Loggin>();
			try
			{
				_context.UserSession.Add(session);
				userSession_Loggins.Add(new UserSession_Loggin
				{
					UserSessionID = session.UserSessionID,
					UserID = session.UserID,
					Token = session.Token,
					DeviceName = session.DeviceName,
					Ip = session.Ip,
					CreateTime = session.CreateTime,
					DeleteStatus = 1
				});
				return _context.SaveChanges();
			}
			catch (Exception ex)
			{
				throw new Exception($"Error Insert_Sesion Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
	}
}
