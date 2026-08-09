using UnityEngine;
using UnityEngine.InputSystem;

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
    public Animator animator;
    private void Start()
    {
        camera = FindAnyObjectByType<CameraManager>();
        input = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();


        
    }

    private void Update()
    {
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


    
}
