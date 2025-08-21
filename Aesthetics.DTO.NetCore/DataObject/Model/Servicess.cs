﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model
{
    public class Servicess
    {
        [Key]
        public int ServiceID { get; set; }
        public int ProductsOfServicesID { get; set; }
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public string? ServiceImage { get; set; }
        public decimal? PriceService { get; set; }
        public int? DeleteStatus { get; set; }
        public TypeProductsOfServices TypeProductsOfServices { get; set; }
		public ICollection<InvoiceDetail> InvoiceDetail { get; set; } 
		public ICollection<Comments> Comments { get; set; }
		public ICollection<BookingAssignment> Booking_Assignment { get; set; }
	}
}
