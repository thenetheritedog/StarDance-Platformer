using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rigidbody;
    private Collider collider;
    private PlayerManager player;
    [SerializeField] private float gravity;
    [SerializeField] private float speed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float toGroundDis;
    [SerializeField] private float slopeSpeed;

    [SerializeField] private float jumpLength;
    [SerializeField] private float coyoteTime;
    private bool jumpAvailable;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float wallRunJumpAway;
    [SerializeField] private bool isWallRunLeft;
    [SerializeField] private bool disableMovement;
    private RaycastHit wallHit;
    private RaycastHit groundHit;

    [SerializeField] private float gravityPull;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        player = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        Vector3 velocity = rigidbody.linearVelocity;



        if (Physics.BoxCast(transform.position, (Vector3.one - Vector3.up * 0.99f) / 2.9f, -transform.up, out groundHit, transform.rotation, toGroundDis, player.defaultLayer))
        {
            player.grounded = true;
            //Vector3 newPositionForSlopes = transform.position;
            //newPositionForSlopes.y = groundHit.point.y + toGroundDis;
            //transform.position = newPositionForSlopes;
        }
        else if (player.grounded && player.playerState != PlayerManager.PlayerState.Falling)
        {
            if (Physics.BoxCast(transform.position, (Vector3.one - Vector3.up * 0.99f) / 2.9f, -transform.up, out groundHit, transform.rotation, toGroundDis * 1.25f, player.defaultLayer))
            {
                player.grounded = true;
                //Vector3 newPositionForSlopes = transform.position;
                //newPositionForSlopes.y = groundHit.point.y + toGroundDis;
                //transform.position = newPositionForSlopes;
            }
            else
            {
                StartCoroutine(CoyoteTime());
                player.grounded = false;
                player.playerState = PlayerManager.PlayerState.Falling;
                gravityPull = rigidbody.linearVelocity.y;
            }
        }
        switch (player.playerState)
        {
            case PlayerManager.PlayerState.Falling:
            case PlayerManager.PlayerState.Jumping:
                GravityNormal(player.playerState == PlayerManager.PlayerState.Jumping);
                velocity.y = gravityPull;
                rigidbody.linearVelocity = velocity;
                break;
            case PlayerManager.PlayerState.WallRunning:
                WallRun();
                break;
            default:
                player.grounded = true;
                jumpAvailable = true;
                disableMovement = false;
                gravityPull = 0;
                break;

        }





    }
    public void Movement(Vector2 moveVector, bool sprint)
    {
        if (disableMovement)
        {
            return;
        }

        Vector3 velocity = moveVector.y * player.camera.transform.forward;
        velocity += moveVector.x * player.camera.transform.right;
        PlayerManager.PlayerState movingState;
        if (sprint)
        {
            velocity *= sprintSpeed;
            movingState = PlayerManager.PlayerState.Running;
        }
        else if (moveVector.magnitude > 0)
        {
            velocity *= speed;
            movingState = PlayerManager.PlayerState.Walking;
        }
        else
        {
            movingState = PlayerManager.PlayerState.Standing;
        }
        if (true)
        {
            velocity.x += groundHit.normal.x * slopeSpeed;
            velocity.z += groundHit.normal.z * slopeSpeed;
        }
        velocity = Vector3.Lerp(rigidbody.linearVelocity, velocity, acceleration * Time.deltaTime);

        if (player.grounded)
        {
            player.playerState = movingState;
            
            velocity = Vector3.ProjectOnPlane(velocity, groundHit.normal);
            if (Vector3.Angle(transform.up, groundHit.normal) < 45)
            {
                transform.forward = velocity.normalized;
            }
            
            Debug.DrawRay(transform.position, groundHit.normal, Color.yellow);
            Debug.DrawRay(transform.position, Vector3.Project(Vector3.forward * gravity, groundHit.normal), Color.red);

        }
        else
        {
            velocity.y = rigidbody.linearVelocity.y;
            if (Physics.Raycast(transform.position, transform.right, out wallHit, .75f, player.defaultLayer) || Physics.Raycast(transform.position, -transform.right, out wallHit, .75f, player.defaultLayer))
            {
                player.playerState = PlayerManager.PlayerState.WallRunning;
                disableMovement = true;
            }
            transform.localEulerAngles = Vector3.up * transform.localEulerAngles.y;

        }
        rigidbody.linearVelocity = velocity;
        
        
        Debug.DrawRay(transform.position, velocity, Color.red);



    }
    public void Jump(bool jumpInput)
    {
        
        
        if (jumpInput && jumpAvailable) 
        {
            player.grounded = false;
            jumpAvailable = false;
            player.playerState = PlayerManager.PlayerState.Jumping;
            StartCoroutine(JumpHold());
            Vector3 velocity = rigidbody.linearVelocity;
            gravityPull = jumpHeight + rigidbody.linearVelocity.y * Time.deltaTime;
            velocity.y = gravityPull;
            rigidbody.linearVelocity = velocity;
            
        }
        if (!jumpInput && player.playerState == PlayerManager.PlayerState.Jumping)
        {
            player.playerState = PlayerManager.PlayerState.Falling;
            StopCoroutine(JumpHold());
        }
        if (player.playerState == PlayerManager.PlayerState.WallRunning && jumpInput)
        {
            Vector3 velocity = rigidbody.linearVelocity;
            if (isWallRunLeft)
            {
                velocity += transform.right * wallRunJumpAway;
            }
            else
            {
                velocity += -transform.right * wallRunJumpAway;
            }
            gravityPull = jumpHeight * 2/3;
            player.playerState = PlayerManager.PlayerState.Jumping;
            disableMovement = true;
            StartCoroutine(JumpHold());
            rigidbody.linearVelocity = velocity;

        }
       
    }
    IEnumerator CoyoteTime()
    {
        yield return new WaitForSeconds(coyoteTime);
        jumpAvailable = false;

    }

    IEnumerator JumpHold()
    {

        yield return new WaitForSeconds(jumpLength);
        disableMovement = false;
        player.playerState = PlayerManager.PlayerState.Falling;

    }

    private void GravityNormal(bool holdingJump)
    {
        
        if (holdingJump)
        {
            gravityPull -= gravity * Time.deltaTime / 5;
        }
        else
        {
            gravityPull -= gravity * Time.deltaTime;

        }

      
    }
    private void WallRun()
    {
        if (player.grounded == true)
        {
            disableMovement = false;
            return;
        }
        Vector3 velocity = rigidbody.linearVelocity;
        bool leftWallHit = Physics.Raycast(transform.position, -transform.right, out wallHit, .75f, player.defaultLayer);
        bool rightWallHit = false;
        isWallRunLeft = leftWallHit;
        if (!isWallRunLeft)
        {
            rightWallHit = Physics.Raycast(transform.position, transform.right, out wallHit, .75f, player.defaultLayer);
        }
        
        
        
        if (!rightWallHit && !leftWallHit)
        {
            Jump(true);
            Debug.Log("No Wall");
            return;

        }
        transform.forward = Vector3.Cross(wallHit.normal, transform.up);
        if (!isWallRunLeft)
        {
            Vector3 otherway = transform.localEulerAngles;
            otherway.y -= 180;
            transform.localEulerAngles = otherway;
        }
        
        velocity.y = 0f;
        velocity = transform.forward * velocity.magnitude;
        gravityPull -= gravity * Time.deltaTime;
        velocity.y = gravityPull/5;
        rigidbody.linearVelocity = velocity;
        

    }
}
