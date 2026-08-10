using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private InputManager input;
    public CameraManager camera;
    public Vector3 cameraPlayerPosition;
    public Vector3 cameraPlayerRotation;
    public bool grounded;
    public LayerMask defaultLayer;
    public PlayerState playerState;
    [SerializeField] private Vector3 spawn;
    [SerializeField] private UIDocument pauseMenu;
    public Animator animator;
    
    public float sensitivity;
    private void Start()
    {
        camera = FindAnyObjectByType<CameraManager>();
        input = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        pauseMenu = FindAnyObjectByType<UIDocument>();
        pauseMenu.gameObject.SetActive(false);
        spawn = transform.position;





    }

    private void Update()
    {
        if (Time.timeScale == 0f) 
        {
            
            return; 
        }
        cameraPlayerPosition = transform.position;
        Vector3 baseAngles = camera.pivot.transform.localEulerAngles;
        baseAngles.z = 0f;
        cameraPlayerRotation = Vector3.Lerp(cameraPlayerRotation, baseAngles, Time.deltaTime * 3);
        
}

    public void ResetLevel()
    {
        playerState = PlayerState.Falling;
        grounded = false;
        cameraPlayerPosition = transform.position;
        cameraPlayerRotation = Vector3.zero;
        transform.position = spawn;
        FindAnyObjectByType<GliderMove>().ResetGlider();
        camera.Reset();
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        foreach (var grapple in FindObjectsOfType<GrapplePoint>())
        {
            grapple.ResetGrapple();
        }
    }

    public enum PlayerState
    {
        Standing,
        Walking,
        Running,
        Jumping,
        Falling,
        WallRunning,
        WallSliding,
        WallJumping,
        Grapple,
        Gliding,
    }

    public void OpenMenu()
    {
        pauseMenu.gameObject.SetActive(!pauseMenu.gameObject.activeSelf);
        Time.timeScale = pauseMenu.gameObject.activeSelf ? 0f : 1f;
        if (Time.timeScale == 1f) 
            input.ChangeLockState();
        
    }
    
}
