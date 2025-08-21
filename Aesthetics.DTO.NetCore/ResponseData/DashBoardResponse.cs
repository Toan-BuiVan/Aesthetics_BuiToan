using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DashBoardResponse
{
    public class DashBoardResponse
    {
        public int? Month { get; set; }
        public decimal? Revenue { get; set; }
    }

    public class DashBoardResponseList : ResponseData
    {
        public List<DashBoardResponse>? ListDashBoard { get; set; }
    }


    public class DashBoardProductTop3
    {
        public int? ProductID { get; set; }
        public string? ProductName { get; set; }
        public int? TotalSold { get; set; }
    }

    public class DashBoardProductTop3List : ResponseData
    {
        public List<DashBoardProductTop3>? ListDashBoard { get; set; }
    }

    public class DashBoardServicesTop3
    {
        public int? ServiceID { get; set; }
        public string? ServiceName { get; set; }
        public int? TotalSold { get; set; }
    }

    public class DashBoardServicesTop3List : ResponseData
    {
        public List<DashBoardServicesTop3>? ListDashBoard { get; set; }
    }

    public class GetReturningCustomerRate
    {
        public int? RepeatCustomers { get; set; }
        public int? TotalCustomers { get; set; }
        public int? RepeatCustomerPercentage { get; set; }
    }


    public class GetReturningCustomerRateList : ResponseData
    {
        public List<GetReturningCustomerRate>? ListDashBoard { get; set; }
    }

    public class CountProcessedOrders
    {
        public int? TotalCustomersTotalCustomers { get; set; }
        public int? ProcessedInvoices { get; set; }
    }
    public class CountProcessedOrdersList : ResponseData
    {
        public List<CountProcessedOrders>? ListDashBoard { get; set; }
    }

    public class GetTotalBookings
    {
        public int? ProcessedAssignments { get; set; }
        public int? TotalAssignments { get; set; }
    }

    public class GetTotalBookingsList : ResponseData
    {
        public List<GetTotalBookings>? ListDashBoard { get; set; }

    }

    public class Top3Employee
    {
        public int? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public int? TotalSold { get; set; }
    }

    public class Top3EmployeeList : ResponseData
    {
        public List<Top3Employee>? ListDashBoard { get; set; }

    }

    public class GetMonthlyBookingReport
    {
        public int? Month { get; set; }
        public int? CompletedBookings { get; set; } //Số booking hoàn thành
        public int? IncompleteBookings { get; set; } //Số booking chưa hoàn thành

	}
    public class GetMonthlyBookingReportList : ResponseData
    {
        public List<GetMonthlyBookingReport>? ListDashBoard { get; set; }

    }

    public class Top3ProductTypesByRevenue
    {
        public int ProductsOfServicesID { get; set; }
        public string? ProductsOfServicesName { get; set; }
        public decimal? TotalRevenue { get; set; }
    }

    public class Top3ProductTypesByRevenueList : ResponseData
    {
        public List<Top3ProductTypesByRevenue>? ListDashBoard { get; set; }
    }

    public class Top3ClinicsByRevenue
    {
        public int ClinicID { get; set; }
        public string? ClinicName { get; set; }
        public decimal? TotalRevenue { get; set; }
    }

    public class Top3ClinicsByRevenueList : ResponseData
    {
        public List<Top3ClinicsByRevenue>? ListDashBoard { get; set; }
    }

    public class VoucherStatistics
    {
        public int? TotalVouchersIssued { get; set; } //Vouchers đã phát hành
        public int? TotalVouchersUsed { get; set; } //Vouchers đã sử dụng
        public int? TotalVouchersActive { get; set; } //Vouchers còn hiệu lực
    }
    public class VoucherStatisticsList : ResponseData
	{
		public List<VoucherStatistics>? ListDashBoard { get; set; }
	}
}
