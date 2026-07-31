using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    private PlayerManager playerManager;
    private PlayerMovement playerMovement;
    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        playerMovement = GetComponent<PlayerMovement>();
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Movement();
        CameraMovement();
        Jump();
    }

    private void Movement()
    {
        playerMovement.Movement(moveAction.ReadValue<Vector2>());
    }

    private void CameraMovement()
    {

        playerManager.camera.MoveCamera(lookAction.ReadValue<Vector2>());
    }

    private void Jump()
    {
        playerMovement.Jump(jumpAction.IsPressed());
     
    }
}
