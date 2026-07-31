using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private InputManager input;
    public CameraManager camera;
    public bool grounded;

    private void Start()
    {
        camera = FindAnyObjectByType<CameraManager>();
        input = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
    }


}
