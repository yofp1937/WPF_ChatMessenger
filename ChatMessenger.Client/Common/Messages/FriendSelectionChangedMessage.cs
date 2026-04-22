/*
 * Messenger를 통해 FriendList에서 FriendDetail로 데이터를 전송할때 사용하는 메세지
 */
using ChatMessenger.Shared.DTOs.Responses;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ChatMessenger.Client.Common.Messages
{
    public class FriendSelectionChangedMessage : ValueChangedMessage<FriendResponse>
    {
        /// <summary>
        /// FriendListViewModel에서 선택된 친구가 변경되면 해당 친구의 데이터를 FriendDetailViewModel로 넘겨주기위해 사용하는 Message
        /// </summary>
        /// <param name="value">변경된 친구의 데이터</param>
        public FriendSelectionChangedMessage(FriendResponse value) : base(value) { }
    }
}
