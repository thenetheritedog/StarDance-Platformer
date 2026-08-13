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
    [SerializeField] private float wallRunSpeed;
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
        if (Time.timeScale == 0) { return; }
        Collider[] spikeChecks = Physics.OverlapCapsule(transform.position + transform.up / 2, transform.position - transform.up / 2, .5f, spikeLayer);

        if (spikeChecks.Length > 0)
        {
            player.ResetLevel();
        }


        Vector3 velocity = rigidbody.linearVelocity;
        if (player.playerState != PlayerManager.PlayerState.Grapple && player.playerState != PlayerManager.PlayerState.Gliding)
        {
            GrappleFind();
        } 
        

        if (Physics.BoxCast(transform.position, (Vector3.one - Vector3.up * 0.85f) / 2.9f, -transform.up, out groundHit, transform.rotation, toGroundDis + Vector3.Project(velocity * Time.fixedDeltaTime, -transform.up).magnitude, player.defaultLayer)
            && velocity.y <= 0 && player.playerState != PlayerManager.PlayerState.Grapple)
        {
            if (player.playerState == PlayerManager.PlayerState.Gliding && groundHit.collider.tag == "FinishLevel")
            {
                player.Win();
            }
            if (Vector3.Angle(Vector3.up, groundHit.normal) < 45)
            {
                if (!player.grounded)
                {
                    float flipped = 0f;
                    if (!player.animator.GetBool("Flip"))
                    {
                        flipped = 0.5f;
                    }
                    player.animator.Play("Run", 0, flipped);
                }
                player.grounded = true;
                gravityPull = 0;
            }


            Vector3 newPositionForSlopes = transform.position;
            newPositionForSlopes.y = groundHit.point.y + 1;

            rigidbody.MovePosition(newPositionForSlopes);
            
        }
        else if (player.grounded && player.playerState != PlayerManager.PlayerState.Falling)
        {
            if (Physics.BoxCast(transform.position, (Vector3.one - Vector3.up * 0.85f) / 2.9f, -transform.up, out groundHit, transform.rotation, (toGroundDis + Vector3.Project(velocity * Time.fixedDeltaTime, -transform.up).magnitude) * 2f, player.defaultLayer))
            {

                if (Vector3.Angle(Vector3.up, groundHit.normal) < 45)
                {
                    player.grounded = true;
                    gravityPull = 0;
                }



                Vector3 newPositionForSlopes = transform.position;
                newPositionForSlopes.y = groundHit.point.y + 1;

                rigidbody.MovePosition(newPositionForSlopes);
            }
            else
            {
                StartCoroutine(CoyoteTime());
                player.grounded = false;
                player.playerState = PlayerManager.PlayerState.Falling;
                gravityPull = rigidbody.linearVelocity.y;
                player.animator.Play("Fall");
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
            case PlayerManager.PlayerState.WallSliding:
                WallSlide();
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
            newAcceleration = acceleration * 10;
            movingState = PlayerManager.PlayerState.Standing;
        }

        if (velocity.magnitude > 0 && !disableMovement)
        {
            transform.forward = Vector3.Lerp(rigidbody.linearVelocity, velocity, newAcceleration * Time.deltaTime).normalized;
        }
        else if (!disableMovement)
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

        }
        else
        {
            smokeEmmisionModule.rateOverTimeMultiplier = 0;

            newAcceleration = decceleration;
            if (player.playerState == PlayerManager.PlayerState.WallJumping)
            {
                newAcceleration = 0;
            }
            velocity = Vector3.Lerp(rigidbody.linearVelocity, velocity, newAcceleration * Time.deltaTime);
            velocity.y = rigidbody.linearVelocity.y;
            float distanceCloser = 2f;
            // check if an object is both over a sphere and at the same y level as player
            if (Physics.OverlapSphere(transform.position + Vector3.ProjectOnPlane(rigidbody.linearVelocity, Vector3.up) * Time.deltaTime, 0.6f, player.defaultLayer).Length > 0 &&
                Physics.OverlapBox(transform.position + Vector3.ProjectOnPlane(rigidbody.linearVelocity, Vector3.up) * Time.deltaTime, new Vector3(0.6f, 0.01f, 0.6f), transform.rotation, player.defaultLayer).Length > 0)
            {

                bool leftWallHit = Physics.Raycast(transform.position, -transform.right, out wallHit, distanceCloser, player.defaultLayer);
                bool rightWallHit = false;
                isWallRunLeft = leftWallHit;
                if (!isWallRunLeft)
                {
                    rightWallHit = Physics.Raycast(transform.position, transform.right, out wallHit, distanceCloser, player.defaultLayer);
                }




                //Find wheather horizontal or forward
                if (leftWallHit || rightWallHit)
                {

                    if (player.playerState != PlayerManager.PlayerState.WallRunning)
                    {
                        player.playerState = PlayerManager.PlayerState.WallRunning;
                        gravityPull = rigidbody.linearVelocity.y;
                        disableMovement = true;
                        player.animator.Play("Wall Run");
                        player.animator.SetBool("Jumping", false);
                        distanceCloser = wallHit.distance;
                    }

                }
                if (Physics.Raycast(transform.position, transform.forward, out wallHit, distanceCloser, player.defaultLayer))
                {
                    if (player.playerState != PlayerManager.PlayerState.WallSliding)
                    {
                        player.playerState = PlayerManager.PlayerState.WallSliding;
                        gravityPull = rigidbody.linearVelocity.y;
                        transform.forward = wallHit.normal;
                        disableMovement = true;
                        player.animator.Play("Wall Slide");
                        player.animator.SetBool("Jumping", false);
                    }
                }
                if (player.playerState == PlayerManager.PlayerState.WallRunning)
                {
                    wallRunSpeed = Vector3.ProjectOnPlane(rigidbody.linearVelocity, Vector3.up).magnitude;
                    WallRun();
                }


            }
            transform.localEulerAngles = Vector3.up * transform.localEulerAngles.y;

        }
        RaycastHit checkingWall;
        if (Physics.SphereCast(transform.position, 0.6f, transform.forward, out checkingWall, 2f, player.defaultLayer))
        {
            Debug.DrawRay(checkingWall.point, checkingWall.normal, Color.blue);
        }
        if (disableMovement)
        {
            return;
        }
        rigidbody.linearVelocity = velocity;
        
        
        
        Debug.DrawRay(transform.position, velocity, Color.red);



    }
    public void Jump(bool jumpInput, Vector2 moveVector)
    {

        if ((player.playerState == PlayerManager.PlayerState.WallRunning || player.playerState == PlayerManager.PlayerState.WallSliding) && jumpInput)
        {
            Vector3 directionOfFoward = player.camera.pivot.transform.forward;
            Vector3 directionOfRight = player.camera.pivot.transform.right;
            directionOfFoward.y = player.transform.forward.y;
            directionOfRight.y = player.transform.right.y;
            Vector3 velocity = moveVector.y * directionOfFoward;
            velocity += moveVector.x * directionOfRight;
            if (Vector3.Angle(velocity, wallHit.normal) > 90)
            {
                velocity = transform.forward;
            }
            velocity += wallHit.normal * wallJumpAway;
            velocity = Vector3.Lerp(Vector3.ProjectOnPlane(rigidbody.linearVelocity, Vector3.up).normalized, velocity.normalized, 0.5f) * (rigidbody.linearVelocity.magnitude + wallJumpAway);
            



            gravityPull = jumpHeight;
            velocity.y = gravityPull;
            player.playerState = PlayerManager.PlayerState.WallJumping;
            jumpAvailable = false;
            player.grounded = false;
            StartCoroutine(JumpHold(wallJumpLength));
            rigidbody.linearVelocity = velocity;
            transform.forward = Vector3.ProjectOnPlane(rigidbody.linearVelocity, Vector3.up).normalized;
            player.animator.SetBool("Flip", isWallRunLeft);
            player.animator.SetBool("Jumping", true);


        }
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
            if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 > 0.5f)
            { player.animator.SetBool("Flip", true); }
            else
            { player.animator.SetBool("Flip", false); }
            player.animator.Play("Jump");


        }
        if (!jumpInput && player.playerState == PlayerManager.PlayerState.Jumping && !disableMovement)
        {
            player.playerState = PlayerManager.PlayerState.Falling;
            StopCoroutine(JumpHold());
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
            player.animator.SetBool("Jumping", true);
        }
        else
        {
            gravityPull -= gravity * Time.deltaTime;
            player.animator.SetBool("Jumping", false);
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
        bool leftWallHit = false;
        bool rightWallHit = false;
        Vector3 oldAngleOfWall = wallHit.normal;
        if (!isWallRunLeft)
        {
            rightWallHit = Physics.Raycast(transform.position, transform.right, out wallHit, (velocity.magnitude + 1), player.defaultLayer);
        }
        else
        {
            leftWallHit = Physics.Raycast(transform.position, -transform.right, out wallHit, (velocity.magnitude + 1), player.defaultLayer);
        }
        if (Vector3.Angle(oldAngleOfWall, wallHit.normal) > 45)
        {
            player.playerState = PlayerManager.PlayerState.Falling;
            player.animator.Play("Fall");
            disableMovement = false;
            return;
        }


        if ((!rightWallHit && !leftWallHit))
        {
            disableMovement = false;
            
            if (!player.grounded)
            {
                gravityPull = 0;
                player.playerState = PlayerManager.PlayerState.Falling;
                player.animator.Play("Fall");
            }


        }
        else
        {
            transform.forward = Vector3.Cross(wallHit.normal, transform.up);
            player.cameraPlayerPosition += -transform.right;
            Vector3 rotationOfCamera = player.cameraPlayerRotation;
            player.animator.SetBool("Flip", true);
            
            if (!isWallRunLeft)
            {
                player.animator.SetBool("Flip", false);
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
        velocity = transform.forward * wallRunSpeed;
        gravityPull -= gravity * Time.deltaTime;
        velocity.y = gravityPull/5;
        rigidbody.linearVelocity = velocity;
 
        

    }
    private void WallSlide()
    {

        
        if (player.grounded)
        {
            player.playerState = PlayerManager.PlayerState.Standing;
            disableMovement = false;
            return;
        }
        else if (!Physics.Raycast(transform.position, -transform.forward, out wallHit, 0.75f, player.defaultLayer))
        {
            player.playerState = PlayerManager.PlayerState.Falling;
            player.animator.Play("Fall");
            disableMovement = false;
            return;
        }
        Vector3 velocity = rigidbody.linearVelocity/5;
        gravityPull -= gravity * Time.deltaTime / 5;
        velocity.y = gravityPull;
        rigidbody.linearVelocity = velocity;
        transform.forward = wallHit.normal;
        

    }
    public void GrappleStart()
    {
        if (player.playerState == PlayerManager.PlayerState.Grapple || player.playerState == PlayerManager.PlayerState.Gliding)
        {
            return;
        }
        if (grapplePoint != null)
        {
            player.playerState = PlayerManager.PlayerState.Grapple;
            disableMovement = true;
            player.grounded = false;
            player.animator.Play("Grapple");
        }
        
    }
    private void Grappling()
    {
        GetComponent<Collider>().enabled = false;
        if (grapplePoint == null) 
        {
            gravityPull = rigidbody.linearVelocity.y - gravity/4;
            return;
        }

        Vector3 targetDirection = grapplePoint.transform.position - transform.position;
        transform.forward = targetDirection;
        rigidbody.linearVelocity = targetDirection.normalized * grappleSpeed ;
        Collider[] checkForPassing = Physics.OverlapSphere(transform.position, 0.5f, grappleLayer, QueryTriggerInteraction.Collide);
        
        if (checkForPassing.Length == 0)
            return;
        GetComponent<Collider>().enabled = true;
        transform.position = checkForPassing[0].transform.position;
        CheckGlider(checkForPassing[0]);
        if (player.playerState == PlayerManager.PlayerState.Gliding)
        {
            disableMovement = false;
            return;
        }
        if (checkForPassing[0] == grapplePoint.GetComponent<Collider>() )
        {
            grapplePoint.GetComponent<Collider>().enabled = false;
            GrapplePoint grappleScript = grapplePoint.GetComponent<GrapplePoint>();
            if (grappleScript.makesYouLookFoward)
            {
                transform.forward = grappleScript.actualForward;
            }
            grappleScript.StartCoroutine(grappleScript.DelayUse());
            grapplePoint = null;
            if (grappleScript.wallRunGrapple)
            {
                player.playerState = PlayerManager.PlayerState.WallRunning;
                player.animator.Play("Wall Run");
                player.animator.SetBool("Jumping", false);
                wallRunSpeed = grappleSpeed;
                rigidbody.linearVelocity = transform.forward * wallRunSpeed;
                bool leftWallHit = Physics.Raycast(transform.position, -transform.right, out wallHit, 0.75f, player.defaultLayer);
                bool rightWallHit = false;
                isWallRunLeft = leftWallHit;
                player.animator.SetBool("Flip", true);
                if (!isWallRunLeft)
                {
                    rightWallHit = Physics.Raycast(transform.position, transform.right, out wallHit, 0.75f, player.defaultLayer);
                    player.animator.SetBool("Flip", false);
                }
                WallRun();
                return;
            }
            disableMovement = false;
            gravityPull = jumpHeight;
            rigidbody.linearVelocity = rigidbody.linearVelocity/2 + Vector3.up * gravityPull;
            player.animator.Play("Jump");
            StartCoroutine(JumpHold(grappleImpactDuration));
            
        }

    }

    private void GrappleFind()
    {
        float distanceAway = Mathf.Infinity;
        
        Collider[] potentialTargets = Physics.OverlapSphere(transform.position, grappleRadius, grappleLayer, QueryTriggerInteraction.Collide);
        foreach (Collider target in potentialTargets)
        {
            Vector3 targetDirection = target.transform.position - transform.position;

            if (Physics.CapsuleCast(transform.position - transform.position.y * transform.up, transform.position + transform.position.y * transform.up, 0.5f, targetDirection, targetDirection.magnitude - 1, player.defaultLayer))
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
            glider.GetComponent<Rigidbody>().linearVelocity += glider.GetComponent<Rigidbody>().linearVelocity.y * Vector3.up;
            player.playerState = PlayerManager.PlayerState.Gliding;
        }
    }

    
}
