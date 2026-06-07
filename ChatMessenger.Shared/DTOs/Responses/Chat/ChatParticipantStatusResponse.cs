using ChatMessenger.Shared.DTOs.Responses.Base;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Shared.DTOs.Responses.Chat
{
    /// <summary>
    /// 채팅방의 참가자 상태가 변경(입장, 퇴장)됐을때 클라이언트에 전송되는 DTO입니다.
    /// </summary>
    /// <remarks>
    /// 시스템 메세지와 함께 현재 방의 인원수, 변경된 유저 정보 등을 포함하여 UI를 갱신할 수 있게합니다.
    /// </remarks>
    public class ChatParticipantStatusResponse : BaseResponse
    {
        /// <summary>
        /// 발생한 입퇴장 시스템 메세지
        /// </summary>
        public ChatMessageResponse Message { get; set; } = null!;
        /// <summary>
        /// 최종 채팅방 인원수
        /// </summary>
        public int CurrentParticipantCount { get; set; }
        /// <summary>
        /// 입장 유저 or 퇴장 유저
        /// </summary>
        public List<FriendResponse> TargetUsers { get; set; } = null!;
        /// <summary>
        /// 누군가 입장 시 true, 퇴장 시 false
        /// </summary>
        public bool IsJoined { get; set; }
    }
}
