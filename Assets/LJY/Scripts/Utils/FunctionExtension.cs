// UI Toolkit의 확장자를 작성하는 스크립트
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils
{
    /// <summary>
    /// VisualElement에 원하는 Sprite의 삽입, 위치 조정, 크기 조정을 제어함
    /// </summary>
    public static class SpriteController
    {
        /// <summary>
        /// 삽입될 이미지와 맞춤형 위치/크기를 적용함
        /// </summary>
        /// <param name="visualElement">사용 Sprite가 삽입될 공간</param>
        /// <param name="sprite">사용 이미지</param>
        /// <param name="offsetX">좌우 이동 %값 (50 입력 시 50% 이동)</param>
        /// <param name="offsetY">상하 이동 %값 (50 입력 시 50% 이동)</param>
        /// <param name="scale">확대/축소 배율</param>
        public static void SetImage(this VisualElement visualElement, Sprite sprite, float offsetX = 0, float offsetY = 0, float scale = 1)
        {
            if (visualElement == null) return;
            if (sprite == null) {
                visualElement.style.backgroundImage = new StyleBackground(StyleKeyword.None);
            }
            else {
                visualElement.style.backgroundImage = new StyleBackground(sprite);
            }
            visualElement.style.translate = new StyleTranslate(new Translate(Length.Percent(offsetX), Length.Percent(offsetY), 0));
            visualElement.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1f)));
        }
    }

    /// <summary>
    /// 생성된 VisualElement를 제어함
    /// </summary>
    public static class VisualElementController
    {
        /// <summary>
        /// 생성된 팝업 패널 VisualTree를 화면 전체 사이즈에 맞도록 수정
        /// </summary>
        /// <param name="root">UXML 파일 상의 Root Element</param>
        public static void SetAbsolutePosition(this VisualElement root)
        {
            root.style.position = Position.Absolute;
            root.style.top = 0;
            root.style.bottom = 0;
            root.style.left = 0;
            root.style.right = 0;
            root.style.flexGrow = 1;
        }
    }
}