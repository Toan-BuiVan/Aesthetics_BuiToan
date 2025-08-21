﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model
{
    public class Invoice
    {
        [Key]
        public int InvoiceID { get; set; }
        public int? CustomerID { get; set; }
        public int? EmployeeID { get; set; }
		public int? VoucherID { get; set; }
        public string? Code { get; set; }
        public double? DiscountValue { get; set; }
		public decimal? TotalMoney { get; set; }
		public DateTime? DateCreated { get; set; }
        public string? Status { get; set; }
        public int? DeleteStatus { get; set; }
        public string? Type { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentMethod { get; set; }
		public Users Users { get; set; }
        public ICollection<InvoiceDetail> InvoiceDetails { get; set; }
        public Vouchers? Voucher { get; set; }
    }
}
