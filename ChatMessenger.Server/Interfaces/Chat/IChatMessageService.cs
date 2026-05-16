using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Server.Common.Interfaces.Chats
{
    public interface IChatMessageService
    {
        /// <summary>
        /// Db에 메세지를 등록합니다.
        /// </summary>
        /// <param name="userEmail">메세지 작성자의 Email</param>
        /// <param name="request">메세지 정보가 담긴 Request</param>
        /// <returns></returns>
        Task<ChatMessage?> AddMessageAsnyc(string? userEmail, SendMessageRequest request);
    }
}
