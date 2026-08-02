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

    private void Start()
    {
        camera = FindAnyObjectByType<CameraManager>();
        input = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();

        
    }

    public enum PlayerState
    {
        Standing,
        Walking,
        Running,
        Jumping,
        Falling,
        WallRunning,
    }
    
}
