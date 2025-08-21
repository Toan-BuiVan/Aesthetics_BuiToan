using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.DataObject.LogginModel
{
	public class Booking_AssignmentLoggin
	{
		public int? AssignmentID { get; set; }
		public int? BookingID { get; set; }
		public int? ClinicID { get; set; }
		public int? ProductsOfServicesID { get; set; }
		public string? UserName { get; set; }
		public int ServiceID { get; set; }
		public string? ServiceName { get; set; }
		public int? NumberOrder { get; set; }
		public DateTime? AssignedDate { get; set; }
		public int? Status { get; set; }
		public int? DeleteStatus { get; set; }
		public int? QuantityServices { get; set; }
		public decimal? PriceService { get; set; }
		public int? PaymentStatus { get; set; }
	}
}
