
using UnityEditor;
using UnityEngine;

using UnityEngine.UIElements;

public class UIScript : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    public UIDocument pauseMenu;
    public VisualElement root;
    public Button returnButton;
    public Button quitButton;
    public Slider sensSlider;

    void OnEnable()
    {
        pauseMenu = GetComponent<UIDocument>();
        root = pauseMenu.rootVisualElement;
        playerManager = FindAnyObjectByType<PlayerManager>();
        quitButton = root.Q<Button>("Quit");

        quitButton.clicked += () => Application.Quit();
        
        returnButton = root.Q<Button>("Return");
        returnButton.clicked += playerManager.OpenMenu;
        sensSlider = root.Q<Slider>("Sens");
        sensSlider.value = playerManager.sensitivity;
        sensSlider.RegisterValueChangedCallback(ChangePlayerSens);


    }

    private void ChangePlayerSens(ChangeEvent<float> sens)
    {
        playerManager.sensitivity = sens.newValue;
    }
}


