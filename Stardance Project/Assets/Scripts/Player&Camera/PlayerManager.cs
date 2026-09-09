using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private Rigidbody rb;
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
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private GameObject playerModel;
    [SerializeField] GliderMove glider;


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
        Time.fixedDeltaTime = 1f / 60f;
        glider = FindAnyObjectByType<GliderMove>();
        rb = GetComponent<Rigidbody>();




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
        cameraPlayerRotation = Vector3.Lerp(cameraPlayerRotation, baseAngles, Time.unscaledDeltaTime * 3);
        
}

    public IEnumerator ResetLevel()
    {
        Time.timeScale = 0;
        deathParticles.Play();
        playerModel.SetActive(false);
        playerMovement.lockOn.transform.localScale = Vector3.zero;
        if (glider != null)
        {
            glider.ResetGlider();
        }
        else         
        {
            glider = FindAnyObjectByType<GliderMove>();
            glider.ResetGlider();
        }
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1;
        playerModel.SetActive(true);
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
        
        camera.Reset();
        rb.linearVelocity = Vector3.zero;

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
        
        currentLevelInFolder++;
        if (currentLevelInFolder > Resources.LoadAll($"Levels/{levelFolder}", typeof(GameObject)).Length)
        {
            currentLevelInFolder = 1;
            levelFolder += 1;
        }

        Destroy(levelOn);
        levelOn = Instantiate(Resources.Load<GameObject>($"Levels/{levelFolder}/{currentLevelInFolder}"));
        glider = FindAnyObjectByType<GliderMove>();
        StartCoroutine(ResetLevel());

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
            StartCoroutine(ResetLevel());
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
        StartCoroutine(ResetLevel());
        Destroy(levelOn);
        if (levelFolder > 2)
        {
            levelFolder = 1;
        }
        levelOn = Instantiate(Resources.Load<GameObject>($"Levels/{levelFolder}/{currentLevelInFolder}"));
    }
    
}
