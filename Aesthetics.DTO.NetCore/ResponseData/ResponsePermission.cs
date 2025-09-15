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
		public int? Status { get; set; }
	}

	public class ResponseData_Permission
	{
		public int? PermissionID { get; set; }
		public int? FunctionID { get; set; }
		public string? FunctionName { get; set; }
		public int? UserID { get; set; }
		public string? UserName { get; set; }
		public string? StatusPermission { get; set; }
	}

	public class ResponsePermission_Loggin : ResponseData
	{
		public List<ResponsePermission>? Data { get; set; }
		public List<ResponseData_Permission>? Data_Permission { get; set; }
	}

	public class FlatPermission
	{
		public string? Function { get; set; }
		public int FuncitonID { get; set; }
		public string FuncionName { get; set; }
		public int Status { get; set; }
	}

	public class GroupBy
	{
		public string? Function { get; set; }
		public List<ListFuncion>? listFuncitons { get; set; } 
	}

	public class ListFuncion
	{
		public int FuncitonID { get; set; }
		public string FuncionName { get; set; }
		public int Status { get; set; }
	}

	public class GroupBy_Loggin : ResponseData
	{
		public List<GroupBy> Data_Permission { get; set; }
	}
}
