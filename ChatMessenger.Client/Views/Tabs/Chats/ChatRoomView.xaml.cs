using System.Windows.Controls;
using System.Windows.Threading;

namespace ChatMessenger.Client.Views.Tabs.Chats
{
    public partial class ChatRoomView : UserControl
    {
        public ChatRoomView()
        {
            InitializeComponent();

            // 데이터 컨텍스트(ViewModel)가 변경될 때 (다른 방으로 전환 시)
            this.DataContextChanged += (s, e) =>
            {
                if (e.NewValue != null)
                {
                    // 방 정보를 불러온 후 UI가 그려질 시간을 벌어줌
                    ScrollToBottom();
                }
            };
        }
        /// <summary>
        /// UI 렌더링 우선순위 이후에 스크롤을 최하단으로 내립니다.
        /// </summary>
        private void ScrollToBottom()
        {
            // DispatcherPriority.Background를 사용하면 
            // UI 레이아웃 작업이 모두 끝난 뒤(메시지들이 다 그려진 뒤) 실행됩니다.
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            }));
        }
    }
}
