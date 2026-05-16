namespace ChatMessenger.Client.Common.Messages.Tab.Friend
{
    /// <summary>
    /// FriendDetail에서 친구를 검색했을때 FriendList의 SelectedFriend를 초기화시키는 메세지<br/>
    /// (test 친구의 프로필을 띄워뒀다가 친구를 검색한다음 다시 test 친구의 프로필을 누르면 아무 변화가없어서 초기화 해야함)
    /// </summary>
    public record SelectedFriendResetMessage();
}
