﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.RequestData
{
    public class InvoiceRequest
    {
		public int? CustomerID { get; set; }
		public int? EmployeeID { get; set; }
		public int? VoucherID { get; set; }
		public string? PaymentMethod { get; set; }
		public List<int>? ProductIDs { get; set; }
		public List<int>? QuantityProduct { get; set; }
		public List<int>? ServicesIDs { get; set; }
		public List<int>? QuantityServices { get; set; }
	}
	public class Delete_Invoice
	{
		public int InvoiceID { get; set; }
	}
	public class GetList_Invoice
	{
		public int? CustomerID { get; set; }
		public int? EmployeeID { get; set; }
		public int? InvoiceID { get; set; }
		public string? InvoiceType { get; set; }
		public string? Status { get; set; }
		public string? PaymentMethod { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}

	public class GetList_InvoiceDetail
	{
		public int? InvoiceID { get; set; }
		public string? InvoiceDetailType { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
	}

	public class UpdateOrderStatus
	{
		public int? InvoiceID { get; set; }
		public string? Status { get; set; }
		public int? InvoiceDetailID { get; set; }
	}
}
