using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction grappleAction;
    InputAction glideAction;
    InputAction menuAction;
    InputAction nextLevelAction;
    InputAction nextFolderAction;
    InputAction testLevelAction;
    private PlayerManager playerManager;
    private PlayerMovement playerMovement;
    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        playerMovement = GetComponent<PlayerMovement>();
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        grappleAction = InputSystem.actions.FindAction("Attack");
        glideAction = InputSystem.actions.FindAction("GlideTest");
        menuAction = InputSystem.actions.FindAction("Menu");
        nextLevelAction = InputSystem.actions.FindAction("NextLevel");
        nextFolderAction = InputSystem.actions.FindAction("NextFolder");
        testLevelAction = InputSystem.actions.FindAction("TestLevel");
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Menu();
        if (Time.timeScale == 0) 
            return;
        Movement();
        CameraMovement();
        Jump();
        Grapple();
        Glider();
        playerManager.ChangeLevelDebug(nextLevelAction.WasPressedThisFrame(), nextFolderAction.WasPressedThisFrame(), testLevelAction.WasPressedThisFrame());
    }

    private void Movement()
    {
        playerMovement.Movement(moveAction.ReadValue<Vector2>(), sprintAction.IsPressed());
    }

    private void CameraMovement()
    {

        playerManager.camera.MoveCamera(lookAction.ReadValue<Vector2>());
    }

    private void Jump()
    {
        playerMovement.Jump(jumpAction.IsPressed(), moveAction.ReadValue<Vector2>());

    }
    private void Grapple()
    {
        if (grappleAction.IsPressed())
        {
            playerMovement.GrappleStart();
        }
    }
    private void Glider()
    {
        if (glideAction.IsPressed())
        {
            playerManager.ResetLevel();
        }
    }
    private void Menu()
    {

        if (menuAction.WasPressedThisFrame())
        {
            playerManager.OpenMenu();
            Cursor.lockState = Time.timeScale == 0 ? CursorLockMode.None : CursorLockMode.Locked;
        }

    }

    public void ChangeLockState()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
}
