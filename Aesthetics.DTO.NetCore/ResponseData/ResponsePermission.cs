using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.DataObject.LogginModel;
using Aesthetics.DTO.NetCore.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DTO.NetCore.ResponsePermission
{
    public class ResponsePermission
    {
		public int? PermissionID { get; set; }
		public int UserID { get; set; }
		public int FunctionID { get; set; }
		public int? IsView { get; set; }
		public int? IsInsert { get; set; }
		public int? IsUpdate { get; set; }
		public int? IsDelete { get; set; }
	}

	public class ResponseData_Permission
	{
		public int? PermissionID { get; set; }
		public int? FunctionID { get; set; }
		public string? FunctionName { get; set; }
		public int? UserID { get; set; }
		public string? UserName { get; set; }
		public string? ViewPermission { get; set; }    
		public string? InsertPermission { get; set; }  
		public string? UpdatePermission { get; set; }  
		public string? DeletePermission { get; set; }  
	}

	public class ResponsePermission_Loggin : ResponseData
	{
		public List<ResponsePermission>? Data { get; set; }
		public List<ResponseData_Permission>? Data_Permission { get; set; }
	}
}
