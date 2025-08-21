using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using BE_102024.DataAces.NetCore.DataOpject.RequestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interface
{
	public interface IUserRepository
	{
		//1.Function Create Account
		Task<ResponseUser_InsertLoggin> CreateAccount(User_CreateAccount account);

		//5.Function Update User By UserID
		Task<ResponseUser_UpdateLoggin> UpdateUser(User_Update user_Update);

		//6.Function Delete User By UserID
		Task<ResponseUser_DeleteLoggin> DeleteUser(User_Delete user_Delete);

		//2. Funciton change password
		Task<ResponseUser_UpdateLoggin> ChangePassword(changePassword password);

		//7.Function Get list User & Search User by UserName or UserID
		Task<ResponseUserData> GetList_SearchUser(GetList_SearchUser getList_);

		//8.Function get User by ReferralCode
		Task<Users> GetUserIdByReferralCode(string referralCode);

		//9.Update AccumulatedPoints by UserID
		Task UpdateAccumulatedPoints(int userId);

		//10.Function tạo ReferralCode & Ktra trùng
		Task<string> GenerateUniqueReferralCode();

		//11.Get User by UserName
		Task<Users> GetUserByUserName(string UserName);

		//12.Get User by UserID
		Task<Users> GetUserByUserID(int? UserID);

		//13.UpdateRatingPoints_Customer
		Task UpdateRatingPoints_Customer(int userID);

		//14.Update SalesPoints
		Task UpdateSalesPoints(int employeeId, decimal money);

		//15. Create Authorization Persson 
		Task CreateAuthorizationPersson(int UserID);
	}
}
