using Aesthetics.DTO.NetCore.DashBoardResponse;
using Aesthetics.DTO.NetCore.RequestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interfaces
{
	public interface IDashBoardRepository
	{
		//1. Doanh thu theo quý (ngày bắt đầu - ngày kết thúc) - Hóa đơn bán
		public Task<DashBoardResponseList> GetMonthlyStatistics(DashBoardRequest dashBoard);

		//2. Doanh chi theo quý (ngày bắt đầu - ngày kết thúc) - Hóa đơn nhập
		public Task<DashBoardResponseList> GetMonthlyImportStatistics(DashBoardRequest dashBoard);

		//3. Lấy top 3 sản phẩm bán chạy
		public Task<DashBoardProductTop3List> GetTopProductsBySales(DashBoardRequest dashBoard);

		//4. Lấy top 3 dịch vụ được ưa chuộng nhất cửa hàng
		public Task<DashBoardServicesTop3List> GetTopServicesBySales(DashBoardRequest dashBoard);

		//5. Tính % khách hàng quay lại mua hàng
		public Task<GetReturningCustomerRateList> GetReturningCustomerRate();

		//6. Lấy số hóa đơn đã xử li / tổng số hóa đơn
		public Task<CountProcessedOrdersList> CountProcessedOrders(DashBoardRequest dashBoard);

		//7. Tính tổng số đặt lịch đã sử lý / Tổng số đặt lịch (ngày bắt đầu - ngày kết thúc)
		public Task<GetTotalBookingsList> GetTotalBookings(DashBoardRequest dashBoard);

		//8. Top 3 nhân viên bán nhiều sản phẩm nhất
		public Task<Top3EmployeeList> GetTopEmployeesByQuantityProduct(DashBoardRequest dashBoard);

		//9. Top 3 nhân viên bán nhiều dịch vụ nhất
		public Task<Top3EmployeeList> GetTopEmployeesByQuantityServices(DashBoardRequest dashBoard);

		//10. Tính tổng số booking của mỗi tháng(ngày bắt đầu - ngày kết thúc)
		public Task<GetMonthlyBookingReportList> GetMonthlyBookingReport(DashBoardRequest dashBoard);

		//11. Top 3 Loại Sản Phẩm có doanh thu lớn nhất cửa hàng
		public Task<Top3ProductTypesByRevenueList> GetTop3ProductTypesByRevenue(DashBoardRequest dashBoard);

		//12. Top 3 phòng khám có doanh thu lớn nhất
		public Task<Top3ClinicsByRevenueList> GetTop3ClinicsByRevenue(DashBoardRequest dashBoard);

		//13. Lấy vouchers đã phát hành, còn hiệu lực, đã dùng 
		public Task<VoucherStatisticsList> GetVoucherStatistics(DashBoardRequest dashBoard);
	}
}
