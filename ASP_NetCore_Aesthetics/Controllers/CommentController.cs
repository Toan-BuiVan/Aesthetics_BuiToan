﻿using Aesthetics.DataAccess.NetCore.Repositories.Implement;
using Aesthetics.DataAccess.NetCore.Repositories.Interface;
using Aesthetics.DataAccess.NetCore.Repositories.Interfaces;
using Aesthetics.DTO.NetCore.DataObject.Model;
using Aesthetics.DTO.NetCore.RequestData;
using Aesthetics.DTO.NetCore.ResponesComment;
using Aesthetics.DTO.NetCore.Response;
using ASP_NetCore_Aesthetics.Filter;
using ASP_NetCore_Aesthetics.Services.IoggerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace ASP_NetCore_Aesthetics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
		private ICommentRepository _commentRepository;
		private readonly IDistributedCache _cache;
		private readonly ILoggerManager _loggerManager;
		public CommentController(ICommentRepository commentRepository, IDistributedCache cache,
			ILoggerManager loggerManager)
		{
			_commentRepository = commentRepository;
			_cache = cache;
			_loggerManager = loggerManager;
		}

		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Insert_Comment")]
		[HttpPost("Insert_Comment")]
		public async Task<IActionResult> Insert_Comment(CommnetRequest _comment)
		{
			try
			{
				//1. Insert Comment
				var responseData = await _commentRepository.Insert_Comment(_comment);
				//2. Lưu log data Insert Comment
				_loggerManager.LogInfo("Insert Comment data: " + JsonConvert.SerializeObject(responseData.comment_Loggins));
				//if (responseData.ResponseCode == 1)
				//{
				//	var cacheKey = "GetClinic_Caching";
				//	await _cache.RemoveAsync(cacheKey);
				//}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Insert Comment} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Update_Comment")]
		[HttpPost("Update_Comment")]
		public async Task<IActionResult> Update_Comment(Update_Comment _comment)
		{
			try
			{
				//1. Update Comment
				var responseData = await _commentRepository.Update_Comment(_comment);
				//2. Lưu log data Update Comment
				_loggerManager.LogInfo("Update Comment data: " + JsonConvert.SerializeObject(responseData.comment_Loggins));
				//if (responseData.ResponseCode == 1)
				//{
				//	var cacheKey = "GetClinic_Caching";
				//	await _cache.RemoveAsync(cacheKey);
				//}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Update Comment} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}


		[ServiceFilter(typeof(Filter_CheckToken))]
		[Filter_Authorization("Delete_Comment")]
		[HttpDelete("Delete_Comment")]
		public async Task<IActionResult> Delete_Comment(Delete_Comment _comment)
		{
			try
			{
				//1. Delete Comment
				var responseData = await _commentRepository.Delete_Comment(_comment);
				//2. Lưu log data Delete Comment
				_loggerManager.LogInfo("Delete Comment data: " + JsonConvert.SerializeObject(responseData.comment_Loggins));
				//if (responseData.ResponseCode == 1)
				//{
				//	var cacheKey = "GetClinic_Caching";
				//	await _cache.RemoveAsync(cacheKey);
				//}
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error Delete Comment} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}

		//[Filter_Authorization("GetList_SearchComment")]
		[HttpPost("GetList_SearchComment")]
		public async Task<IActionResult> GetList_SearchComment(GetList_SearchCommnet _searchComment)
		{
			try
			{
				var responseData = await _commentRepository.GetList_SearchComment(_searchComment);
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				_loggerManager.LogError("{Error GetList_SearchComment} Message: " + ex.Message +
					"|" + "Stack Trace: " + ex.StackTrace);
				return Ok(ex.Message);
			}
		}
	}
}
