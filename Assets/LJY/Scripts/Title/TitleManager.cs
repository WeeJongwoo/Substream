using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private float _buttonDuration = 2f;
    [SerializeField] private float _transitionDelay = 10f; 

    private Button _startButton;
    private VisualElement _startUI;
    private bool _startGame;
    private VisualElement _titleBackGround;
    private SceneLoader _sceneLoader;

    private void Awake()
    {
        _startGame = false;
        _sceneLoader = this.gameObject.GetComponent<SceneLoader>();

        VisualElement root = this.gameObject.GetComponent<UIDocument>().rootVisualElement;

        _startButton = root.Q<Button>("Start-Button");
        _startUI = root.Q<VisualElement>("image-Container");
        _titleBackGround = root.Q<VisualElement>("BackGround");

        _startButton.RegisterCallback<ClickEvent>(OnStartGame);
    }

    private void Start()
    {
        StartCoroutine(OnBlinkEvent());
        StartCoroutine(OnSwipEvent());
    }

    private void OnStartGame(ClickEvent evt)
    {
        _startGame = true;
        _sceneLoader.StartLoadingScene("LobbyScene");
    }

    private IEnumerator OnBlinkEvent()
    {
        float timer = 0f;
        bool upper = false;

        while (!_startGame)
        {
            if (timer < _buttonDuration
             && upper == false)
            {
                timer += Time.deltaTime;
            }
            else if (timer >= _buttonDuration
             && upper == false)
            {
                _startUI.RemoveFromClassList("start_UI-on");
                upper = true;
            }
            else if (timer > 0
             && upper == true)
            {
                timer -= Time.deltaTime;
            }
            else if (timer <= 0
             && upper == true)
            {
                _startUI.AddToClassList("start_UI-on");
                upper = false;
            }

            yield return null;
        }
    }

    private IEnumerator OnSwipEvent()
    {
        yield return new WaitForSeconds(3f);

        bool isLeft = false;
        while (!_startGame)
        {
            if (isLeft)
            {
                _titleBackGround.RemoveFromClassList("title_backGround-left");
                _titleBackGround.AddToClassList("title_backGround-right");
            }
            else
            {
                _titleBackGround.RemoveFromClassList("title_backGround-right");
                _titleBackGround.AddToClassList("title_backGround-left");
            }

            isLeft = !isLeft;
            yield return new WaitForSeconds(_transitionDelay);
        }
    }
}
