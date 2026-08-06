using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private InputManager input;
    public CameraManager camera;
    public bool grounded;
    public LayerMask defaultLayer;
    public PlayerState playerState;
    [SerializeField] private Vector3 spawn;
    private void Start()
    {
        camera = FindAnyObjectByType<CameraManager>();
        input = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        

        
    }

    public void ResetLevel()
    {
        playerState = PlayerState.Falling;
        transform.position = spawn;
        FindAnyObjectByType<GliderMove>().ResetGlider();
    }

    public enum PlayerState
    {
        Standing,
        Walking,
        Running,
        Jumping,
        Falling,
        WallRunning,
        WallJumping,
        Grapple,
        Gliding,
    }
    
}
