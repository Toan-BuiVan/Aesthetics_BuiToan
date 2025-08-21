using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interface
{
	public interface IUserSessionRepository
	{
		Task<int> Insert_Sesion(UserSession session);
		Task<int> Delele_Session(string? token, int? UserID);
		Task<int> DeleleAll_Session(int? UserID);
		Task<DataResponse> GetList_SearchUserSession(UserSessionRequest userSession);
	}
}
