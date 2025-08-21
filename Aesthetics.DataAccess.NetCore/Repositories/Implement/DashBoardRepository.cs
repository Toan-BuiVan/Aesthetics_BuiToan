using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DataAccess.NetCore.DBContext;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DashBoardResponse;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using BE_102024.DataAces.NetCore.Dapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Implement
{
	public class DashBoardRepository : BaseApplicationService, IDashBoardRepository
	{
		private DB_Context _context;
		private IConfiguration _configuration;
		public DashBoardRepository(DB_Context context,
			IConfiguration configuration, IServiceProvider serviceProvider) : base(serviceProvider)
		{
			_context = context;
			_configuration = configuration;
		}

		public async Task<DashBoardResponseList> GetMonthlyStatistics(DashBoardRequest dashBoard)
		{
			var returnData = new DashBoardResponseList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<DashBoardResponse>("GetMonthlyStatistics", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetMonthlyStatistics Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<DashBoardResponseList> GetMonthlyImportStatistics(DashBoardRequest dashBoard)
		{
			var returnData = new DashBoardResponseList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<DashBoardResponse>("GetMonthlyInputStatistics", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetMonthlyInputStatistics Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<DashBoardProductTop3List> GetTopProductsBySales(DashBoardRequest dashBoard)
		{
			var returnData = new DashBoardProductTop3List();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<DashBoardProductTop3>("GetTopProductsBySales", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetTopProductsBySales Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<DashBoardServicesTop3List> GetTopServicesBySales(DashBoardRequest dashBoard)
		{
			var returnData = new DashBoardServicesTop3List();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<DashBoardServicesTop3>("GetTopServicesBySales", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetTopServicesBySales Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<GetReturningCustomerRateList> GetReturningCustomerRate()
		{
			var returnData = new GetReturningCustomerRateList();
			try
			{
				var parameters = new DynamicParameters();
				var result = await DbConnection.QueryAsync<GetReturningCustomerRate>("GetReturningCustomerRate", parameters);
				if (result!= null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetReturningCustomerRate Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<CountProcessedOrdersList> CountProcessedOrders(DashBoardRequest dashBoard)
		{
			var returnData = new CountProcessedOrdersList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<CountProcessedOrders>(
							"GetInvoiceStatistics",
							parameters,
							commandType: CommandType.StoredProcedure
						);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error CountProcessedOrders Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<GetTotalBookingsList> GetTotalBookings(DashBoardRequest dashBoard)
		{
			var returnData = new GetTotalBookingsList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartMonth", dashBoard.StartDate?.Month ?? null);
				parameters.Add("@EndMonth", dashBoard.EndDate?.Month ?? null);
				var result = await DbConnection.QueryAsync<GetTotalBookings>("GetBookingStatistics", parameters, commandType: CommandType.StoredProcedure);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetTotalBookings Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<Top3EmployeeList> GetTopEmployeesByQuantityProduct(DashBoardRequest dashBoard)
		{
			var returnData = new Top3EmployeeList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<Top3Employee>("GetTopProductEmployees", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetTopEmployeesByQuantityProduct Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<Top3EmployeeList> GetTopEmployeesByQuantityServices(DashBoardRequest dashBoard)
		{
			var returnData = new Top3EmployeeList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<Top3Employee>("GetTopServiceEmployees", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetTopEmployeesByQuantityServices Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<GetMonthlyBookingReportList> GetMonthlyBookingReport(DashBoardRequest dashBoard)
		{
			var returnData = new GetMonthlyBookingReportList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<GetMonthlyBookingReport>("GetMonthlyBookingStatistics", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetMonthlyBookingReport Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<Top3ProductTypesByRevenueList> GetTop3ProductTypesByRevenue(DashBoardRequest dashBoard)
		{
			var returnData = new Top3ProductTypesByRevenueList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<Top3ProductTypesByRevenue>("GetTop3ProductTypesByRevenue", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetTop3ProductTypesByRevenue Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<Top3ClinicsByRevenueList> GetTop3ClinicsByRevenue(DashBoardRequest dashBoard)
		{
			var returnData = new Top3ClinicsByRevenueList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<Top3ClinicsByRevenue>("GetTop3ClinicsByRevenue", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetTop3ClinicsByRevenue Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}

		public async Task<VoucherStatisticsList> GetVoucherStatistics(DashBoardRequest dashBoard)
		{
			var returnData = new VoucherStatisticsList();
			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@StartDate", dashBoard.StartDate ?? null);
				parameters.Add("@EndDate", dashBoard.EndDate ?? null);
				var result = await DbConnection.QueryAsync<VoucherStatistics>("GetVoucherStatistics", parameters);
				if (result != null && result.Any())
				{
					returnData.ResponseCode = 1;
					returnData.ResposeMessage = "Lấy danh sách thống kê thành công!";
					returnData.ListDashBoard = result.ToList();
					return returnData;
				}
				returnData.ResponseCode = 0;
				returnData.ResposeMessage = "Không tìm thấy thống kê nào.";
				return returnData;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error GetVoucherStatistics Message: {ex.Message} | StackTrace: {ex.StackTrace}", ex);
			}
		}
	}
}
