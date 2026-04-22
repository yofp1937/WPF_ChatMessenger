using System.Windows.Controls;
using System.Windows.Input;

namespace ChatMessenger.Client.Views.Tabs.Friends
{
    public partial class FriendListView : UserControl
    {
        public FriendListView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 두 ListBox(로그인한 유저의 Profile, User의 친구 목록) 간의 단일 선택 상태를 유지하기위한 이벤트 핸들러입니다.
        /// </summary>
        /// <remarks>
        /// WPF의 ListBox는 서로 독립적인 컨트롤이므로, 한쪽이 선택될 때 다른 ListBox의 선택을 해제하여<br/>
        /// 전체 화면에서 하나의 항목만 선택된 것처럼 보이게합니다.
        /// </remarks>
        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedListBox = sender as ListBox;

            if (clickedListBox == MyProfileListBox)
            {
                // 내 프로필을 눌렀으니 친구 목록 선택 해제
                FriendsListBox.SelectedIndex = -1;
            }
            else if (clickedListBox == FriendsListBox)
            {
                // 친구 목록을 눌렀으니 내 프로필 선택 해제
                MyProfileListBox.SelectedIndex = -1;
            }
        }
    }
}
