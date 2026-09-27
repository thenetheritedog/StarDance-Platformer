
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private string startGame;
    public UIDocument pauseMenu;
    public VisualElement root;
    public Button returnButton;
    public Button quitButton;
    public Slider sensSlider;
    [SerializeField] private AudioSource hoverSFX;
    [SerializeField] private AudioSource pressedSFX;
    [SerializeField] private Animator transition;
    [SerializeField] private bool isWin;

    void OnEnable()
    {
        if (isWin)
        {
            StartCoroutine(LoadWinScreen());
        }
        pauseMenu = GetComponent<UIDocument>();
        root = pauseMenu.rootVisualElement;
        playerManager = FindAnyObjectByType<PlayerManager>();
        quitButton = root.Q<Button>("Quit");

        quitButton.clicked += () => Application.Quit();
        
        returnButton = root.Q<Button>("Return");
        
        if (playerManager != null)
        {
            returnButton.clicked += playerManager.OpenMenu;
            sensSlider = root.Q<Slider>("Sens");

            sensSlider.value = playerManager.sensitivity;
            sensSlider.RegisterValueChangedCallback(ChangePlayerSens);
        }
        else
        {
            returnButton.clicked += () => StartCoroutine(LoadSceneTransition());
        }
        
        quitButton.RegisterCallback<MouseEnterEvent>(evt => PlayerManager.PlaySFXUsingRandom(hoverSFX));
        quitButton.clicked += () => PlayerManager.PlaySFXUsingRandom(pressedSFX);
        returnButton.RegisterCallback<MouseEnterEvent>(evt => PlayerManager.PlaySFXUsingRandom(hoverSFX));
        returnButton.clicked += () => PlayerManager.PlaySFXUsingRandom(pressedSFX);
    }

    private void ChangePlayerSens(ChangeEvent<float> sens)
    {
        playerManager.sensitivity = sens.newValue;
        PlayerManager.PlaySFXUsingRandom(hoverSFX);
    }

    private IEnumerator LoadSceneTransition()
    {
        transition.Play("Transition");
        yield return new WaitForSeconds(0.5f); // Wait for the animation to finish
        SceneManager.LoadScene(startGame);
    }

    private IEnumerator LoadWinScreen()
    {
        yield return new WaitForEndOfFrame();
        transition.Play("Transition", 0, 0.5f);
    }
}


