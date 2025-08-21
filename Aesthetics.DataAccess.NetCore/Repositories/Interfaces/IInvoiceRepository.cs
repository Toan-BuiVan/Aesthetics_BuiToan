using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponseInvoice_Loggin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interfaces
{
	public interface IInvoiceRepository
	{
		//1. Insert Invoice
		Task<ResponseInvoice_Loggin> Insert_Invoice(InvoiceRequest insert_);

		//2. Delete Invoice
		Task<ResponseInvoice_Loggin> Delete_Invoice(Delete_Invoice delete_);

		//3. Get list search Invoice
		Task<ResponseGetListInvoice> GetList_SearchInvoice(GetList_Invoice getList_);
		Task<ResponseGetListInvoice> GetList_SearchInvoicee(GetList_Invoice getList_);

		//14. Cập nhật hóa đơn khi thanh toán bằng phương thức Nhận Hàng Thanh Toán
		Task<ResponseData> ProcessDirectPayment(Delete_Invoice delete_);

		//15. Cập nhật hóa đơn nhận hàng thành công khi thanh toán bằng phương thức Nhận Hàng Thanh Toán
		Task<ResponseData> ConfirmCodPaymentSuccess(Delete_Invoice delete_);

		//4. Get list search InvoiceDetail
		Task<ResponseGetListInvoiceDetail> GetList_SearchInvoiceDetail(GetList_InvoiceDetail getList_);

		//5. Get Invoice by InvoiceID 
		Task<Invoice> GetInvoiceByInvoiceID(int InvoiceID);

		//6. Update status Invoice
		Task UpdateStatusInvoice(int InvoiceID);

		//7. Update status InvoiceDetail 
		Task UpdateStatusInvoiceDetail(int InvoiceID);

		//8. Get List InvoiceDetail by InvoiceID 
		Task<List<InvoiceDetail>> InvoiceDetailByInvoiceID(int invoiceID);

		//9. Update status Invoice đã hủy
		Task UpdateStatusInvoiceFail(int InvoiceID);

		//10. Update status InvoiceDetail đã hủy
		Task UpdateStatusInvoiceDetailFail(int InvoiceID);

		//11. Cập nhật trạng thái đơn hàng
		Task<ResponseInvoice_Loggin> UpdateOrderStatus(UpdateOrderStatus orderStatus);

		//12. Tự động cập nhật trạng thái đơn hàng khi mua online
		Task AutoUpdateOrderStatus(int InvoiceID);

		//13. Tự động cập nhật trạng thái đơn hàng khi mua tại của hàng
		Task AutoUpdateOrderStatusEmployee(int InvoiceID);

		public Task<List<Products>> GetProductsByProductIDs(List<int> productIds);

		public Task<List<Servicess>> GetServicessByServicesIDs(List<int> serviceIds);
		
	}
}
