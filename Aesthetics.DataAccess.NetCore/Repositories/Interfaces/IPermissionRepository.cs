using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponsePermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interfaces
{
    public interface IPermissionRepository
    {
        Task<ResponsePermission_Loggin> GetList_SearchPermission(PermissionRequest request);
        Task<ResponsePermission_Loggin> Update_Permission(Update_Permission update_);
        Task<ResponsePermission_Loggin> Delete_Permission(PermissionRequest _delete);
        Task<string> GetListTyperson();
        Task<GroupBy_Loggin> GroupByPermission(Update_Permission update_);
        Task<GroupBy_Loggin> GetPermissionByUserID(Update_Permission userID);
    }
}
