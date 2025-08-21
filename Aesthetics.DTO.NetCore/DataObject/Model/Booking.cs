﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.Model
{
	public class Booking
	{
		[Key]
		public int BookingID { get; set; }
		public int UserID { get; set; }
		public DateTime? BookingCreation { get; set; }
		public int? Status { get; set; }
		public int? DeleteStatus { get; set; }
		public int? PaymentStatus { get; set; }
		public Users Users { get; set; }
		public ICollection<BookingAssignment> Booking_Assignment { get; set; }
	}
}
