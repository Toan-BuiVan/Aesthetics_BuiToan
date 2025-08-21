using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.RequestData
{
    public class UserSessionRequest
    {
		public int? UserID { get; set; }
		public string? UserName { get; set; }
	}
}
