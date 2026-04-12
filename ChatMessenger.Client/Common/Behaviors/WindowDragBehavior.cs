/*
 * UI 요소에 드래그 기능을 부여하는 첨부 속성 클래스입니다.
 * MVVM 패턴을 준수하여 XAML 바인딩만으로 DragMove 기능을 제공합니다.
 */
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
namespace ChatMessenger.Client.Common.Behaviors
{
    public static class WindowDragBehavior
    {
        // DragMove를 활성화/비활성화하고 Command를 바인딩할 첨부 속성 정의
        // DependencyProperty: WPF 객체의 속성 값을 정의하는 특별한 속성(데이터 바인딩, 애니메이션, 스타일 등에 사용)
        public static readonly DependencyProperty MouseDownToDragMoveCommandProperty =
            DependencyProperty.RegisterAttached( // RegisterAttached: 첨부 속성(일반 속성과 달리 WPF 요소(Border 등)에 첨부하여 사용할 수 있는 속성)
                "MouseDownToDragMoveCommand", // XAML에서 사용할 속성 이름(Binding으로 매칭)
                typeof(ICommand), // MouseDownToDragMoveCommand의 타입을 ICommand로 설정
                typeof(WindowDragBehavior), // 이 첨부 속성을 등록한 class
                new PropertyMetadata(null, OnMouseDownToDragMoveCommandChanged)); // 속성의 기본값 = null, 값이 변경될때마다 뒤 함수 호출함

        // XAML에서 읽고 쓸 수 있도록 반드시 필요한 Get, Set 메서드
        public static ICommand GetMouseDownToDragMoveCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(MouseDownToDragMoveCommandProperty);
        }
        public static void SetMouseDownToDragMoveCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(MouseDownToDragMoveCommandProperty, value);
        }

        /// <summary>
        /// MouseDownToDragMoveCommand 속성이 설정된 요소에 마우스 이벤트 핸들러를 등록/해제하는 콜백 메서드<br/>
        /// 프로그램이 실행되 View가 화면에 그려질때(MouseDownToDragMoveCommand에 Value가 들어올때) 한번 호출됩니다.
        /// </summary>
        /// <param name="d">MouseDownToDragMoveCommand가 등록된 요소</param>
        private static void OnMouseDownToDragMoveCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement element) return;

            // 기존 이벤트 핸들러 제거
            element.MouseDown -= Element_MouseDown;

            // 새로운 Command가 설정되었다면 이벤트 핸들러 등록
            if (e.NewValue is ICommand command && command != null)
            {
                element.MouseDown += Element_MouseDown;
            }
        }

        /// <summary>
        /// MouseDown 이벤트 핸들러(XAML에서 이벤트가 등록된 요소를 클릭할때 호출되는 함수)
        /// </summary>
        private static void Element_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not UIElement element) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;

            // 실제 클릭된 객체 가져오기
            DependencyObject? originalElement = e.OriginalSource as DependencyObject;
            // originalElement의 상위 객체들을 순회하며 상호작용 가능한 컨트롤이 있는지 검색하고
            // 상호작용 가능한 컨트롤(버튼, 슬라이더 등)이 클릭됐으면 함수 종료
            if (IsInteractiveChild(originalElement)) return;

            // 클릭된 요소에 Binding된 메서드를 가져옴
            ICommand command = GetMouseDownToDragMoveCommand(element);
            // command 실행 가능 여부 확인하고 실행
            if (command != null && command.CanExecute(null))
            {
                command.Execute(null);
                // 창 이동이 시작되면 다른 이벤트가 발생하지않게 처리
                e.Handled = true;
            }
        }

        /// <summary>
        /// 클릭된 요소의 비주얼 트리 상위에서 상호작용 컨트롤(Button, Slider 등)을 찾습니다.
        /// </summary>
        private static bool IsInteractiveChild(DependencyObject? current)
        {
            if (current == null) return false;
            // 현재 탐색중인 요소가 Window창이면 반복문 종료
            while (current != Window.GetWindow(current))
            {
                // TextBlock이나 Path 같은 단순한 시각 요소가 아닌, 상호작용 요소면 return true
                if (current is Button || current is Slider || current is ScrollBar)
                    return true;

                // 상위 요소로 이동
                current = VisualTreeHelper.GetParent(current);
            }
            // 최종적으로 상호작용 요소가 발견되지 않고 Window까지 도달한 경우
            return false;
        }
    }
}