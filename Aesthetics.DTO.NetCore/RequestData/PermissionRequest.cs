using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.RequestData
{
    public class PermissionRequest
    {
        public int? UserID { get; set; }
    }

	public class Update_Permission
	{
		public int? PermissionID { get; set; }
		public int? UserID { get; set; }
		public int? FunctionID { get; set; }
		public int? Status { get; set; }
		public string? TypePerson { get; set; }
	}
}
