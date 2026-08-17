using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;
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
    [SerializeField] private int levelFolder;
    [SerializeField] private int currentLevelInFolder;
    [SerializeField] private GameObject levelOn;

    public float sensitivity;
    private void Start()
    {
        camera = FindAnyObjectByType<CameraManager>();
        input = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        pauseMenu = FindAnyObjectByType<UIDocument>();
        pauseMenu.gameObject.SetActive(false);
        spawn = transform.position;
        levelOn = Instantiate(Resources.Load<GameObject>($"Levels/{levelFolder}/{currentLevelInFolder}"));
        Application.targetFrameRate = 60;





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
        transform.rotation = Quaternion.identity;
        playerState = PlayerState.Falling;
        GetComponent<Rigidbody>().MovePosition(spawn);
        transform.position = spawn;
        grounded = false;
        cameraPlayerPosition = transform.position;
        cameraPlayerRotation = Vector3.zero;
        playerMovement.gravityPull = 0;
        playerMovement.glider = null;
        playerMovement.disableMovement = false;
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

    public void Win()
    {
        ResetLevel();
        currentLevelInFolder++;
        if (currentLevelInFolder > Resources.LoadAll($"Levels/{levelFolder}", typeof(GameObject)).Length)
        {
            currentLevelInFolder = 1;
            levelFolder += 1;
        }

        Destroy(levelOn);
        levelOn = Instantiate(Resources.Load<GameObject>($"Levels/{levelFolder}/{currentLevelInFolder}"));
        
    }

    public void ChangeLevelDebug(bool level,bool folder, bool test)
    {
        
        if (folder)
        {
            levelFolder++;
            currentLevelInFolder = 1;
            
        }
        else if (test)
        {
            ResetLevel();
            Destroy(levelOn);
            currentLevelInFolder = 1;
            levelFolder = 1;
            levelOn = Instantiate(Resources.Load<GameObject>("Levels/TestLevel/Level"));
            
            return;
        }
        else if (level)
        {
            currentLevelInFolder++;
            if (currentLevelInFolder > Resources.LoadAll($"Levels/{levelFolder}", typeof(GameObject)).Length)
            {
                currentLevelInFolder = 1;
                levelFolder += 1;
            }

        }
        else 
            return;
        ResetLevel();
        Destroy(levelOn);
        if (levelFolder > 2)
        {
            levelFolder = 1;
        }
        levelOn = Instantiate(Resources.Load<GameObject>($"Levels/{levelFolder}/{currentLevelInFolder}"));
    }
    
}
