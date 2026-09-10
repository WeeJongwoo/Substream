using UnityEngine.UIElements;

namespace Utils
{
    /// <summary>
    /// VisualElement의 페이드 인앤 아웃을 제어함
    /// </summary>
    public static class PopupController
    {
        /// <summary>
        /// UI Toolkit 팝업 페이드 인 애니메이션
        /// </summary>
        /// <param name="element">애니메이션을 적용할 VisualElement</param>
        /// <param name="delayMs">USS 트랜지션 적용을 위한 딜레이 (기본값 10ms)</param>
        public static void ShowPopupFade(this VisualElement element, long delayMs = 50)
        {
            if (element == null) return;

            if (!element.ClassListContains("popup-fade-base")) {
                element.AddToClassList("popup-fade-base");
            }

            element.AddToClassList("popup-no-transition");
            element.style.display = DisplayStyle.Flex;

            element.schedule.Execute(() => {
                element.RemoveFromClassList("popup-no-transition");
                element.AddToClassList("popup-visible");
            }).StartingIn(delayMs);
        }

        /// <summary>
        /// UI Toolkit 팝업 페이드 아웃 애니메이션
        /// </summary>
        /// <param name="element">애니메이션을 적용할 VisualElement</param>
        /// <param name="durationMs">디스플레이를 끄기 전 대기할 USS 트랜지션 시간 (기본값 300ms)</param>
        public static void HidePopupFade(this VisualElement element, long durationMs = 300)
        {
            if (element == null) return;

            element.RemoveFromClassList("popup-visible");

            element.schedule.Execute(() => {
                // 중간에 다시 호출되어 visible 클래스가 붙었다면 숨기지 않음
                if (!element.ClassListContains("popup-visible")) {
                    element.style.display = DisplayStyle.None;
                }
            }).StartingIn(durationMs);
        }
    }
}