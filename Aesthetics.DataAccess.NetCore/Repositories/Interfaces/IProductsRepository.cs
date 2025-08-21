﻿using Aesthetics.DataAccess.NetCore.CheckConditions.Response;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aesthetics.DataAccess.NetCore.Repositories.Interfaces
{
    public interface IProductsRepository
    {
		//1.Function insert Products
		Task<ResponseProducts_Loggin> Insert_Products(ProductRequest Products_);

		//2.Function update Products
		Task<ResponseProducts_Loggin> Update_Products(Update_Products update_);

		//3.Function delete Products
		Task<ResponseProducts_LogginDelete> Delete_Products(Delete_Products delete_);

		//4.Function Get list & Search Products
		Task<ResponseProducts_LogginGetList> GetList_SearchProducts(GetList_SearchProducts getList_);

		//5. Sort list Product
		Task<ResponseProducts_LogginGetList> GetSortedPagedProducts(SortListProducts sortList_);

		//6.Base Processing Function 64
		Task<string> BaseProcessingFunction64(string? ProductsImage);

		//7.Function get Products by ProductID 
		Task<Products> GetProductsByProductID(int? ProductID);

		//8.Function Export Products list to Excel
		Task<ResponseData> ExportProductsToExcel(ExportProductExcel filePath);

		//9.Function get ProductOfServices by ProductsOfServicesID
		Task<TypeProductsOfServices> GetProductOfServicesByID(int? ProductsOfServicesID);

		//10.Update Quantity Product
		Task UpdateQuantityPro(int productID, int quantity);
	}
}
