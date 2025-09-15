using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.RequestData
{
    public class ChatBoxRequest
    {
        public string? searchText { get; set; }
        public int? UserId { get; set; }
	}
}
