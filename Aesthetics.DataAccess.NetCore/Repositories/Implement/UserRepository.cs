﻿using Aesthetics.DataAccess.NetCore.CheckConditions;
using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using BE_102024.DataAces.NetCore.CheckConditions;
using BE_102024.DataAces.NetCore.Dapper;
using BE_102024.DataAces.NetCore.DataOpject.RequestData;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XAct.Users;
using static System.Collections.Specialized.BitVector32;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class UserRepository : BaseApplicationService, IUserRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		public UserRepository(DB_Context context, IServiceProvider serviceProvider,
			IConfiguration configuration) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<ResponseUser_InsertLoggin> CreateAccount(User_CreateAccount account)
		{
			var returnData = new ResponseUser_InsertLoggin();
			using var transaction = await _context.Database.BeginTransactionAsync();
			var listCarts = new List<Carts_Loggin>();
			var listUser = new List<User_Loggin>();
			try
			{
				// 1. Validate input
				if (!Validation.CheckString(account.UserName) || !Validation.CheckXSSInput(account.UserName))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Tài khoản không hợp lệ || Tài khoản chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (!Validation.CheckString(account.PassWord) || !Validation.CheckXSSInput(account.PassWord))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Mật khẩu không hợp lệ || Mật khẩu chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (!Validation.CheckXSSInput(account.ReferralCode))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Mã giới thiệu chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (await GetUserByUserName(account.UserName) != null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "UserName đã tồn tại, Nhập UserName khác!";
					return returnData;
				}

				// 2. Kiểm tra và mã hóa mật khẩu
				var passWordHash = Security.EncryptPassWord(account.PassWord);

				// 3. Kiểm tra mã giới thiệu nếu có
				Users referrerUser = null;
				if (!string.IsNullOrEmpty(account.ReferralCode))
				{
					referrerUser = await GetUserIdByReferralCode(account.ReferralCode);
					if (referrerUser == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Mã giới thiệu người dùng không tồn tại!";
						return returnData;
					}
				}

				// 4. Tạo mã giới thiệu cho người dùng mới
				var referralCodeUser = await GenerateUniqueReferralCode();

				string typePerson;
				switch (account.TypePersson)
				{
					case "Customer":
						typePerson = "Customer";
						break;
					case "Employee":
						typePerson = "Employee";
						break;
					case "Doctor":
						typePerson = "Doctor";
						break;
					case "Admin":
						typePerson = "Admin";
						break;
					default:
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Loại người dùng không hợp lệ! Phải là 'Customer', 'Employee', 'Doctor' hoặc 'Admin'.";
						return returnData;
				}
				string rankMember = typePerson switch
				{
					"Customer"  => "Default", 
					"Employee" => "Bronze",
					"Doctor" => "Silver",
					"Admin" => "Diamond",
					_ => "Default" 
				};

				// 6. Thêm người dùng mới
				var creation = DateTime.Now;
				var newUser = new Users
				{
					UserName = account.UserName,
					PassWord = passWordHash,
					Creation = creation,
					TypePerson = typePerson,
					AccumulatedPoints = 0,
					DeleteStatus = 1,
					ReferralCode = referralCodeUser,
					RatingPoints = 0,
					SalesPoints = 0,
					RankMember = rankMember
				};
				await _context.Users.AddAsync(newUser);
				await _context.SaveChangesAsync();

				// 7. Ghi log người dùng
				listUser.Add(new User_Loggin
				{
					UserID = newUser.UserID,
					UserName = newUser.UserName,
					PassWord = newUser.PassWord,
					Creation = newUser.Creation,
					TypePerson = newUser.TypePerson,
					AccumulatedPoints = newUser.AccumulatedPoints,
					ReferralCode = newUser.ReferralCode,
					SalesPoints = newUser.SalesPoints,
					DeleteStatus = newUser.DeleteStatus,
					RatingPoints = newUser.RatingPoints,
					RankMember = newUser.RankMember
				});

				// 8. Tạo giỏ hàng cho người dùng mới
				var creationDate = DateTime.Now;
				var cart = new Carts
				{
					UserID = newUser.UserID,
					CreationDate = creationDate,
				};
				await _context.Carts.AddAsync(cart);
				await _context.SaveChangesAsync();

				// 9. Ghi log giỏ hàng
				listCarts.Add(new Carts_Loggin
				{
					CartID = cart.CartID,
					UserID = cart.UserID,
					CreationDate = cart.CreationDate
				});

				// 10. Cập nhật điểm cho người giới thiệu nếu có
				if (referrerUser != null)
				{
					await UpdateAccumulatedPoints(referrerUser.UserID);
				}

				// 11. Commit transaction nếu thành công
				await transaction.CommitAsync();

				// 12. Thêm quyền cho user vừa tạo
				await CreateAuthorizationPersson(newUser.UserID);

				// 13. Trả về kết quả thành công
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = "Tạo Tài Khoản Thành Công!";
				returnData.listUser = listUser;
				returnData.listCarts = listCarts;
				return returnData;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error CreateAccount Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseUser_UpdateLoggin> UpdateUser(User_Update user_Update)
		{
			var returnData = new ResponseUser_UpdateLoggin();
			var listUser = new List<User_Loggin>();
			try
			{
				if (user_Update.UserID <= 0 || user_Update.UserID == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"UserID: {user_Update.UserID} không hợp lệ!";
					return returnData;
				}
				if (await GetUserByUserID(user_Update.UserID) == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"UserID: {user_Update.UserID} không tồn tại!";
					return returnData;
				}

				if (!Validation.CheckXSSInput(user_Update.Email))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Email chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (user_Update.DateBirth > DateTime.Now)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Ngày sinh không hợp lệ!";
					return returnData;
				}

				if (!Validation.CheckXSSInput(user_Update.Sex))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Giới tính chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (!Validation.CheckXSSInput(user_Update.Addres))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Địa chỉ chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (user_Update.Phone != null)
				{
					if (!Validation.CheckNumber(user_Update.Phone))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "Phone không hợp lệ, Phone gồm các số (10-11 số)!";
						return returnData;
					}
				}
				if (!Validation.CheckXSSInput(user_Update.Phone))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Phone chứa kí tự không hợp lệ!";
					return returnData;
				}
				if (user_Update.IDCard != null)
				{
					if (!Validation.CheckNumber(user_Update.IDCard))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "CMND không hợp lệ, CMND gồm các số (10-11 số)!";
						return returnData;
					}
				}
				if (!Validation.CheckXSSInput(user_Update.IDCard))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "CMND chứa kí tự không hợp lệ!";
					return returnData;
				}

				var parameters = new DynamicParameters();
				parameters.Add("@UserID", user_Update.UserID);
				parameters.Add("@Email", user_Update.Email ?? string.Empty);
				parameters.Add("@DateBirth", user_Update.DateBirth);
				parameters.Add("@Sex", user_Update.Sex ?? string.Empty);
				parameters.Add("@Phone", user_Update.Phone ?? string.Empty);
				parameters.Add("@Addres", user_Update.Addres ?? string.Empty);
				parameters.Add("@IDCard", user_Update.IDCard ?? string.Empty);
				var result = await DbConnection.ExecuteAsync("UpdateUser_ByUserID", parameters);
				listUser.Add(new User_Loggin
				{
					UserID = user_Update.UserID,
					Email = user_Update.Email ?? null,
					DateBirth = user_Update.DateBirth ?? null,
					Sex = user_Update.Sex ?? null,
					Phone = user_Update.Phone ?? null,
					Addres = user_Update.Addres ?? null,
					IDCard = user_Update.IDCard ?? null
				});
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = $"Cập nhật User có mã: {user_Update.UserID} thành công!";
				returnData.listUser = listUser;
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error UpdateUser Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseUser_UpdateLoggin> ChangePassword(changePassword password)
		{
			var returnData = new ResponseUser_UpdateLoggin();
			var listUser = new List<User_Loggin>();
			try
			{
				var user = await GetUserByUserID(password.UserID);
				if (password.UserID <= 0 || password.UserID == null)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"UserID: {password.UserID} không hợp lệ!";
					return returnData;
				}
				if (user == null) 
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"UserID: {password.UserID} không tồn tại!";
					return returnData;
				}

				if (!Validation.CheckXSSInput(password.passWord))
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = "Mật Khẩu chứa kí tự không hợp lệ!";
					return returnData;
				}
				var passWordHash = Security.EncryptPassWord(password.passWord);
				user.PassWord = passWordHash;
				_context.Users.Update(user);
				await _context.SaveChangesAsync();
				listUser.Add(new User_Loggin
				{
					PassWord = user.PassWord
				});
				returnData.ResponseCode = 1;
				returnData.ResposeMessage = $"Đổi mật khẩu thành công!";
				returnData.listUser = listUser;
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error ChangePassword Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseUser_DeleteLoggin> DeleteUser(User_Delete user_Delete)
		{
			var returnData = new ResponseUser_DeleteLoggin();
			var listUser = new List<User_Loggin>();
			var listCarts = new List<Carts_Loggin>();
			var listClinicStaff = new List<Clinic_Staff_Loggin>();
			var listWallets = new List<Wallets_Loggin>();
			var listPermission = new List<Permission_Loggin>();
			var listUserSession = new List<UserSession_Loggin>();

			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				if (user_Delete.UserID <= 0)
				{
					returnData.ResponseCode = -1;
					returnData.ResposeMessage = $"User: {user_Delete.UserID} không hợp lệ!";
					return returnData;
				}
				var user = await _context.Users
					.Include(s => s.Clinic_Staff)
					.Include(s => s.Wallets)
					.Include(s => s.UserSession)
					.Include(s => s.Permissions)
					.Include(s => s.Carts)
					.AsSplitQuery()
					.FirstOrDefaultAsync(v => v.UserID == user_Delete.UserID);
				if (user != null)
				{
					//1.Xóa User nếu tìm thấy
					user.DeleteStatus = 0;
					listUser.Add(new User_Loggin
					{
						UserID = user.UserID,
						UserName = user.UserName,
						PassWord = user.PassWord,
						Email = user.Email ?? null,
						DateBirth = user.DateBirth ?? null,
						Sex = user.Sex ?? null,
						Creation = user.Creation,
						Phone = user.Phone ?? null,
						Addres = user.Addres ?? null,
						IDCard = user.IDCard ?? null,
						TypePerson = user.TypePerson,
						AccumulatedPoints = user.AccumulatedPoints,
						ReferralCode = user.ReferralCode,
						RefeshToken = user.RefeshToken ?? null,
						DeleteStatus = user.DeleteStatus,
						TokenExprired = user.TokenExprired ?? null,
						RatingPoints = user.RatingPoints,
						RankMember = user.RankMember
					});

					//2. Xóa các bản ghi ở Clinic_Staff nếu liên quan đến UserID
					var clinic_staff = user.Clinic_Staff
						.Where(s => s.UserID == user.UserID).ToList();
					if (clinic_staff != null)
					{
						foreach (var user_clinic_staff in clinic_staff)
						{
							_context.Clinic_Staff.Remove(user_clinic_staff);
							await _context.SaveChangesAsync();
							listClinicStaff.Add(new Clinic_Staff_Loggin
							{
								ClinicStaffID = user_clinic_staff.ClinicStaffID,
								ClinicID = user_clinic_staff.ClinicID,
								UserID = user_clinic_staff.UserID,
							});
						}
					}

					//3. Xóa các bản ghi ở Wallets nếu liên quan đến UserID
					if (user.Wallets.Any())
					{
						_context.Wallets.RemoveRange(user.Wallets);
						foreach (var wallet in user.Wallets)
						{
							listWallets.Add(new Wallets_Loggin
							{
								WalletsID = wallet.WalletsID,
								UserID = wallet.UserID,
								VoucherID = wallet.VoucherID
							});
						}
					}


					//4. Xóa các bản ghi ở UserSession nếu liên quan đến UserID
					var userSession = user.UserSession
						.Where(s => s.UserID == user.UserID).ToList();
					if (userSession != null)
					{
						foreach (var user_userSession in userSession)
						{
							user_userSession.DeleteStatus = 0;
							listUserSession.Add(new UserSession_Loggin
							{
								UserSessionID = user_userSession.UserSessionID,
								UserID = user_userSession.UserID,
								Token = user_userSession.Token,
								DeviceName = user_userSession.DeviceName,
								Ip = user_userSession.Ip,
								CreateTime = user_userSession.CreateTime,
								DeleteStatus = user_userSession.DeleteStatus
							});
						}
					}

					//5. Xóa các bản ghi ở Permissions nếu liên quan đến UserID
					if (user.Permissions.Any())
					{
						_context.Permission.RemoveRange(user.Permissions);
						foreach (var permission in user.Permissions)
						{
							listPermission.Add(new Permission_Loggin
							{
								PermissionID = permission.PermissionID,
								UserID = permission.UserID,
								FunctionID = permission.FunctionID,
								IsView = permission.IsView,
								IsInsert = permission.IsInsert,
								IsUpdate = permission.IsUpdate,
								IsDelete = permission.IsDelete
							});
						}
					}

					//6. Xóa các bản ghi ở Carts nếu liên quan đến UserID
					if (user.Carts != null)
					{
						_context.Carts.RemoveRange(user.Carts);
						listCarts.Add(new Carts_Loggin
						{
							CartID = user.Carts.CartID,
							UserID = user.Carts.UserID,
							CreationDate = user.Carts.CreationDate
						});
					}

					//Commit transaction nếu thành công
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = $"Delete User: {user_Delete.UserID} thành công!";
					returnData.listUser = listUser;
					returnData.listCarts = listCarts;
					returnData.listClinicStaff = listClinicStaff;
					returnData.listWallets = listWallets;
					returnData.listPermission = listPermission;
					returnData.listUserSession = listUserSession;
					return returnData;
				}
				returnData.ResponseCode = -1;
				returnData.ResposeMessage = $"Không tìm thấy User: {user_Delete.UserID}!";
				return returnData;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception($"Error DeleteUser Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<ResponseUserData> GetList_SearchUser(GetList_SearchUser getList_)
		{
			var returnData = new ResponseUserData();
			var listUser = new List<ResponseUser>();
			try
			{
				if (getList_.UserID != null)
				{
					if (getList_.UserID <= 0)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $":User  {getList_.UserID} không hợp lệ!";
						return returnData;
					}
					if (await GetUserByUserID(getList_.UserID) == null)
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = $"Danh sách không tồn tại User có mã: {getList_.UserID}!";
						return returnData;
					}
				}

				if (getList_.UserName != null)
				{
					if (!Validation.CheckXSSInput(getList_.UserName))
					{
						returnData.ResponseCode = -1;
						returnData.ResposeMessage = "UserName chứa kí tự không hợp lệ!";
						return returnData;
					}
				}

				var parameters = new DynamicParameters();
				parameters.Add("@UserID", getList_.UserID ?? null);
				parameters.Add("@UserName", getList_.UserName ?? null);
				var result = await DbConnection.QueryAsync<ResponseUser>("GetList_SearchUser ", parameters);

				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách người dùng thành công!";
					returnData.Data = result.ToList();
					return returnData;
				}
				else
				{
					returnData.ResponseCode = 0;
					returnData.ResposeMessage = "Không tìm thấy người dùng nào.";
					return returnData;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetList_SearchUser Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task UpdateRatingPoints_Customer(int userID)
		{
			try
			{
				var customer = await _context.Users.FindAsync(userID);
				if (customer == null)
				{
					throw new Exception($"Không tìm thấy người dùng có mã: {userID}");
				}
				customer.RatingPoints += 5;
				await _context.SaveChangesAsync();
				string newRank = customer.RatingPoints switch
				{
					>= 150 => "Diamond",
					>= 100 => "Gold",
					>= 45 => "Silver",
					>= 20 => "Bronze",
					_ => "Default"
				};
				if (customer.RankMember != newRank)
				{
					customer.RankMember = newRank;
					await _context.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Lỗi khi cập nhật điểm cho người dùng {userID}: {ex.Message}");
			}
		}

		public async Task UpdateSalesPoints(int employeeId, decimal money)
		{
			try
			{
				var employee = await _context.Users.FirstOrDefaultAsync(e => e.UserID == employeeId);
				if (employee == null)
				{
					throw new Exception("Nhân viên không tồn tại.");
				}

				decimal pointsEarned = money / 100000;
				employee.SalesPoints = (employee.SalesPoints ?? 0) + pointsEarned;

				var result = await _context.SaveChangesAsync();
				Console.WriteLine($"Rows affected: {result}");
			}
			catch (Exception ex)
			{
				throw new Exception($"Lỗi khi cập nhật điểm bán hàng: {ex.Message}");
			}
		}

		public async Task<string> GenerateUniqueReferralCode()
		{
			string referralCode;
			bool exists;
			Random random = new Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			do
			{
				referralCode = new string(chars.OrderBy(x => random.Next()).Take(5).ToArray());
				exists = await _context.Users.AnyAsync(s => s.ReferralCode == referralCode);
			} while (exists);

			return referralCode;
		}

		public async Task UpdateAccumulatedPoints(int userId)
		{
			try
			{
				var user = await _context.Users.FindAsync(userId);
				if (user != null)
				{
					user.AccumulatedPoints += 10;
					await _context.SaveChangesAsync();
				}
			}
			catch
			{
				throw new Exception($"Không tìm thấy người dùng có mã: {userId}");
			}
		}

		public async Task<Users> GetUserByUserName(string UserName)
		{
			return await _context.Users.Where(s => s.UserName == UserName).FirstOrDefaultAsync();
		}

		public async Task<Users> GetUserIdByReferralCode(string referralCode)
		{
			return await _context.Users.Where(s => s.ReferralCode == referralCode && s.DeleteStatus == 1).FirstOrDefaultAsync();
		}

		public async Task<Users> GetUserByUserID(int? UserID)
		{
			return await _context.Users.Where(s => s.UserID == UserID && s.DeleteStatus == 1).FirstOrDefaultAsync();
		}

		public async Task CreateAuthorizationPersson(int userId)
		{
			try
			{
				var user = await _context.Users
					.FirstOrDefaultAsync(u => u.DeleteStatus == 1 && u.UserID == userId);
				if (user != null)
				{
					if (user.TypePerson == "Admin")
					{
						var parameters = new DynamicParameters();
						parameters.Add("@UserID", userId);
						await DbConnection.ExecuteAsync("CreateAuthorize_Admin", parameters);
					}
					else if (user.TypePerson == "Employee")
					{
						var parameters = new DynamicParameters();
						parameters.Add("@UserID", userId);
						await DbConnection.ExecuteAsync("CreateAuthorize_Employee", parameters);
					}
					else if (user.TypePerson == "Doctor")
					{
						var parameters = new DynamicParameters();
						parameters.Add("@UserID", userId);
						await DbConnection.ExecuteAsync("CreateAuthorize_Doctor", parameters);
					}
					else
					{
						var parameters = new DynamicParameters();
						parameters.Add("@UserID", userId);
						await DbConnection.ExecuteAsync("CreateAuthorize_Customer", parameters);
					}
				}
				else
				{
					throw new Exception($"Không tìm thấy người dùng có mã: {userId}");
				}
			}
			catch (Exception ex)
			{

				Console.WriteLine($"Error in CreateAuthorizationPersson: {ex.Message}");
				throw new Exception($"Error in CreateAuthorizationPersson: {ex.Message}", ex);
			}
		}

		
	}
}
