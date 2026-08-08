using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rigidbody;

    private PlayerManager player;
    public GliderMove glider;
    [SerializeField] private float gravity;
    [SerializeField] private float speed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float decceleration;
    [SerializeField] private float toGroundDis;
    [SerializeField] private float slopeSpeed;
    [SerializeField] private ParticleSystem runSmokeParticles;
    private Vector3 oldVelocity;
    


    [SerializeField] private float jumpLength;
    [SerializeField] private float coyoteTime;
    private bool jumpAvailable;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float wallJumpAway;
    [SerializeField] private float wallJumpLength;
    [SerializeField] private bool isWallRunLeft;
    [SerializeField] private bool disableMovement;
    private RaycastHit wallHit;
    private RaycastHit groundHit;
    [SerializeField] private GameObject grapplePoint;

    [SerializeField] private float gravityPull;
    [SerializeField] private float grappleRadius;
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LayerMask gliderLayer;
    [SerializeField] private LayerMask spikeLayer;
    [SerializeField] private float grappleSpeed;
    [SerializeField] private float grappleImpactDuration;
    private Vector3 originalGrappleDistance;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        player = GetComponent<PlayerManager>();
        runSmokeParticles = GetComponentInChildren<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        Collider[] spikeChecks = Physics.OverlapCapsule(transform.position + transform.up/2 , transform.position - transform.up/2 , .5f, spikeLayer);

        if (spikeChecks.Length > 0) 
        {
            player.ResetLevel();
        }

        
        Vector3 velocity = rigidbody.linearVelocity;
        if (player.playerState != PlayerManager.PlayerState.Grapple)
        {
            GrappleFind();
        } 
        

        if (Physics.BoxCast(transform.position, (Vector3.one - Vector3.up * 0.85f) / 2.9f, -transform.up, out groundHit, transform.rotation, toGroundDis + Vector3.Project(velocity * Time.fixedDeltaTime, -transform.up).magnitude, player.defaultLayer) && velocity.y <= 0)
        {
            if (Vector3.Angle(Vector3.up, groundHit.normal) < 45)
            {
                player.grounded = true;
                gravityPull = 0;
            }


            Vector3 newPositionForSlopes = transform.position;
            newPositionForSlopes.y = groundHit.point.y + 1;
            transform.position = newPositionForSlopes;
        }
        else if (player.grounded && player.playerState != PlayerManager.PlayerState.Falling)
        {
            if (Physics.BoxCast(transform.position, (Vector3.one - Vector3.up * 0.85f) / 2.9f, -transform.up, out groundHit, transform.rotation, (toGroundDis + Vector3.Project(velocity * Time.fixedDeltaTime, -transform.up).magnitude) * 1.25f, player.defaultLayer))
            {

                if (Vector3.Angle(Vector3.up, groundHit.normal) < 45)
                {
                    player.grounded = true;
                    gravityPull = 0;
                }



                Vector3 newPositionForSlopes = transform.position;
                newPositionForSlopes.y = groundHit.point.y + 1;
                transform.position = newPositionForSlopes;
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
            case PlayerManager.PlayerState.WallJumping:
                GravityNormal(player.playerState != PlayerManager.PlayerState.Falling);
                velocity.y = gravityPull;
                rigidbody.linearVelocity = velocity;
                break;
            case PlayerManager.PlayerState.WallRunning:
                WallRun();
                break;
            case PlayerManager.PlayerState.Grapple:
                Grappling();
                break;
            case PlayerManager.PlayerState.Gliding:
                if (glider != null)
                {
                    rigidbody.linearVelocity = glider.GetComponent<Rigidbody>().linearVelocity;
                }
                jumpAvailable = true;
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
        var smokeEmmisionModule = runSmokeParticles.emission;
        Vector3 directionOfFoward = player.camera.pivot.transform.forward;
        Vector3 directionOfRight = player.camera.pivot.transform.right;
        directionOfFoward.y = player.transform.forward.y;
        directionOfRight.y = player.transform.right.y;
        Vector3 velocity = moveVector.y * directionOfFoward;
        velocity += moveVector.x * directionOfRight;
        PlayerManager.PlayerState movingState;
        float newAcceleration = acceleration;
        if (player.playerState == PlayerManager.PlayerState.Gliding && glider != null && !player.grounded)
        {
            glider.MoveDirection(velocity);
            return;
        }
        

        if (sprint && moveVector.magnitude > 0)
        {
            velocity *= sprintSpeed;
            if (velocity.magnitude < rigidbody.linearVelocity.magnitude && speed < rigidbody.linearVelocity.magnitude)
            {
                newAcceleration = decceleration;
                
            }
            movingState = PlayerManager.PlayerState.Running;
        }
        else if (moveVector.magnitude > 0)
        {
            velocity *= speed;
            newAcceleration = acceleration * 10;
            movingState = PlayerManager.PlayerState.Walking;
        }
        else
        {
            newAcceleration = acceleration*10;
            movingState = PlayerManager.PlayerState.Standing;
        }

        if (velocity.magnitude > 0 && !disableMovement)
        {
            transform.forward = velocity.normalized;
        }
        else
        {
            transform.forward = transform.forward;
        }


        if (player.grounded)
        {
            
            player.playerState = movingState;
            if (disableMovement)
            {
                return;
            }
            if (Mathf.Abs(Vector3.SignedAngle(velocity, rigidbody.linearVelocity, transform.up)) > 120)
            {
                rigidbody.linearVelocity = Vector3.Project(rigidbody.linearVelocity, transform.up);
            }
            velocity.x += groundHit.normal.x * slopeSpeed;
            velocity.z += groundHit.normal.z * slopeSpeed;
            velocity = Vector3.Lerp(rigidbody.linearVelocity, velocity, newAcceleration * Time.deltaTime);
            velocity = Vector3.ProjectOnPlane(velocity, groundHit.normal);



            smokeEmmisionModule.rateOverTimeMultiplier = velocity.magnitude * 6;
            
            player.animator.SetFloat("Speed", velocity.magnitude + 1);


            Debug.DrawRay(transform.position, groundHit.normal, Color.yellow);
            Debug.DrawRay(transform.position, Vector3.Project(Vector3.forward * gravity, groundHit.normal), Color.red);

        }
        else
        {
            smokeEmmisionModule.rateOverTimeMultiplier = 0;
            if (disableMovement)
            {
                return;
            }
            newAcceleration = decceleration;
            velocity = Vector3.Lerp(rigidbody.linearVelocity, velocity, newAcceleration * Time.deltaTime);
            velocity.y = rigidbody.linearVelocity.y;
            if ((Physics.Raycast(transform.position, transform.right, out wallHit, .75f, player.defaultLayer)
                || Physics.Raycast(transform.position, -transform.right, out wallHit, .75f, player.defaultLayer)) && player.playerState 
                != PlayerManager.PlayerState.WallJumping)
            {
                player.playerState = PlayerManager.PlayerState.WallRunning;
                gravityPull = jumpHeight;
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
            if (glider != null) 
            {
                rigidbody.linearVelocity = glider.GetComponent<Rigidbody>().linearVelocity;
                Vector3 changeGliderVelocity = rigidbody.linearVelocity;
                changeGliderVelocity.y = -jumpHeight * 12;
                glider.GetComponent<Rigidbody>().linearVelocity = changeGliderVelocity;
                glider = null;
            }
            jumpAvailable = false;
            player.playerState = PlayerManager.PlayerState.Jumping;
            StartCoroutine(JumpHold(jumpLength));
            Vector3 velocity = rigidbody.linearVelocity;
            gravityPull = jumpHeight;
            velocity.y += gravityPull;
            rigidbody.linearVelocity = velocity;
            
        }
        if (!jumpInput && player.playerState == PlayerManager.PlayerState.Jumping && !disableMovement)
        {
            player.playerState = PlayerManager.PlayerState.Falling;
            StopCoroutine(JumpHold());
        }
        if (player.playerState == PlayerManager.PlayerState.WallRunning && jumpInput)
        {
            Vector3 velocity = rigidbody.linearVelocity;
            velocity += wallHit.normal * wallJumpAway;
            gravityPull = jumpHeight;
            player.playerState = PlayerManager.PlayerState.WallJumping;
            disableMovement = true;
            jumpAvailable = false;
            StartCoroutine(JumpHold(wallJumpLength));
            rigidbody.linearVelocity = velocity;
            transform.forward = velocity.normalized - velocity.normalized.y * Vector3.up;

        }
       
    }
    IEnumerator CoyoteTime()
    {
        yield return new WaitForSeconds(coyoteTime);
        jumpAvailable = false;

    }

    IEnumerator JumpHold(float time = 0)
    {

        yield return new WaitForSeconds(time);
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
        bool leftWallHit = Physics.Raycast(transform.position, -transform.right, out wallHit, .75f * velocity.magnitude, player.defaultLayer);
        bool rightWallHit = false;
        isWallRunLeft = leftWallHit;
        if (!isWallRunLeft)
        {
            rightWallHit = Physics.Raycast(transform.position, transform.right, out wallHit, .75f * velocity.magnitude, player.defaultLayer);
        }



        if (!rightWallHit && !leftWallHit)
        {
            disableMovement = false;
            if (!player.grounded)
            {
                gravityPull = 0;
                player.playerState = PlayerManager.PlayerState.Falling;
            }


        }
        else
        {
            transform.forward = Vector3.Cross(wallHit.normal, transform.up);
            player.cameraPlayerPosition += -transform.right;
            Vector3 rotationOfCamera = player.cameraPlayerRotation;
            
            
            if (!isWallRunLeft)
            {
                Vector3 otherway = transform.localEulerAngles;
                otherway.y -= 180;
                transform.localEulerAngles = otherway;
                rotationOfCamera.z = Mathf.Lerp(rotationOfCamera.z, -15, Time.deltaTime * 5);
            }
            else
            {
                rotationOfCamera.z = Mathf.Lerp(rotationOfCamera.z, 15, Time.deltaTime * 5);
            }
            player.cameraPlayerRotation = rotationOfCamera;
        }
        
        
        velocity.y = 0f;
        velocity = transform.forward * velocity.magnitude;
        gravityPull -= gravity * Time.deltaTime;
        velocity.y = gravityPull/5;
        rigidbody.linearVelocity = velocity;
 
        

    }

    public void GrappleStart()
    {
        if (player.playerState == PlayerManager.PlayerState.Grapple)
        {
            return;
        }
        if (grapplePoint != null)
        {
            player.playerState = PlayerManager.PlayerState.Grapple;
            disableMovement = true;
            oldVelocity = rigidbody.linearVelocity/2;
        }
        
    }
    private void Grappling()
    {
        
        if (grapplePoint == null) 
        {
            gravityPull = rigidbody.linearVelocity.y - gravity/4;
            return;
        }
        Vector3 targetDirection = grapplePoint.transform.position - transform.position;
        rigidbody.linearVelocity = targetDirection.normalized * (grappleSpeed + oldVelocity.magnitude);
        Collider[] checkForPassing = Physics.OverlapSphere(transform.position, 1f, grappleLayer, QueryTriggerInteraction.Collide);
        if (checkForPassing.Length == 0)
            return;
        CheckGlider(checkForPassing[0]);
        if (player.playerState == PlayerManager.PlayerState.Gliding)
        {
            disableMovement = false;
            return;
        }
        if (checkForPassing[0] == grapplePoint.GetComponent<Collider>() )
        {
            transform.forward = grapplePoint.GetComponent<GrapplePoint>().actualForward;
            disableMovement = false;
            StartCoroutine(JumpHold(grappleImpactDuration));
            grapplePoint = null;
        }
        oldVelocity = Vector3.Lerp(oldVelocity, Vector3.zero, Time.deltaTime);
    }

    private void GrappleFind()
    {
        float distanceAway = Mathf.Infinity;
        
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, grappleRadius, grappleLayer, QueryTriggerInteraction.Collide);
        foreach (Collider target in potentialTargets)
        {
            Vector3 targetDirection = target.transform.position - transform.position;
            
            if (Physics.CapsuleCast(transform.position - transform.position.y * transform.up, transform.position + transform.position.y * transform.up, 0.9f, targetDirection, targetDirection.magnitude - 1, player.defaultLayer))
            {
                return;
            }
            if (targetDirection.magnitude > distanceAway)
            {
                return;
            }
            if (Vector3.Angle(targetDirection, player.camera.mainCamera.transform.forward) > 45)
            {
                return;
            }
            distanceAway = targetDirection.magnitude;
            grapplePoint = target.gameObject;
            Debug.Log("GrappleFound");


        }
        if (potentialTargets.Length == 0)
        {
            grapplePoint = null;
        }

        
    }

    public void CheckGlider(Collider collision)
    {
        if (collision.gameObject.GetComponent<GliderMove>() != null)
        {
            glider = collision.gameObject.GetComponent<GliderMove>();
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            rb.linearVelocity = rigidbody.linearVelocity;
            glider.speed = rigidbody.linearVelocity.magnitude;
            glider.transform.forward = transform.forward;
            player.playerState = PlayerManager.PlayerState.Gliding;
        }
    }

    
}
