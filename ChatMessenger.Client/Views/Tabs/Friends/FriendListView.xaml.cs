using System.Windows;
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
            ListBox? clickedListBox = sender as ListBox;
            if (clickedListBox == null) return;

            // 1. 현재 클릭한 Element의 객체를 반환받습니다.(글자를 클릭했으면 TextBlock, 이미지를 클릭했으면 Image)
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            // 2. Element가 ListBoxItem이 될때까지 부모 트리로 올라가서 확인합니다.
            while(dep != null && dep is not ListBoxItem)
            {
                dep = System.Windows.Media.VisualTreeHelper.GetParent(dep);
            }
            // 3. 찾아낸 Element가 ListBoxItem일 경우에만 동작합니다.
            if(dep is ListBoxItem clickedItem)
            {
                if (clickedListBox == MyProfileListBox)
                {
                    // 내 프로필을 눌렀으니 친구 목록 선택 해제
                    FriendsListBox.SelectedIndex = -1;
                    MyProfileListBox.SelectedItem = clickedItem;
                }
                else if (clickedListBox == FriendsListBox)
                {
                    // 친구 목록을 눌렀으니 내 프로필 선택 해제
                    MyProfileListBox.SelectedIndex = -1;
                    FriendsListBox.SelectedItem = clickedItem.DataContext;
                }
            }
        }
    }
}
