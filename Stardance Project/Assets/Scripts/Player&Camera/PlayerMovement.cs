using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rigidbody;
    private Collider collider;
    private PlayerManager player;
    [SerializeField] private float gravity;
    [SerializeField] private float speed;
    [SerializeField] private float acceleration;
    [SerializeField] private float toGroundDis;
    [SerializeField] private LayerMask ground;
    [SerializeField] private float jumpLength;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpHeight;
    private bool holdingJump;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        player = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        Vector3 velocity = rigidbody.linearVelocity;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, toGroundDis * 1.25f, ground) && !holdingJump)
        {
            if (hit.distance < toGroundDis && !player.grounded)
            {
                if (holdingJump)
                {
                    velocity.y -= gravity * Time.deltaTime / 5;
                }
                else
                {
                    velocity.y -= gravity * Time.deltaTime;
                }
                return;
            }
            player.grounded = true;
            velocity.y = 0;
            Vector3 newPositionForSlopes = transform.position;
            newPositionForSlopes.y = hit.point.y + toGroundDis;
            transform.position = newPositionForSlopes;
        }
        else
        {
            StartCoroutine(CoyoteTime());
            if (holdingJump)
            {
                velocity.y -= gravity * Time.deltaTime / 5;
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;
            }

        }
        rigidbody.linearVelocity = velocity;


    }
    public void Movement(Vector2 moveVector)
    {
        Vector3 velocity = moveVector.y * player.camera.transform.forward;
        velocity += moveVector.x * player.camera.transform.right;
        velocity *= speed;
        velocity.y = rigidbody.linearVelocity.y;
        rigidbody.linearVelocity = Vector3.Lerp(rigidbody.linearVelocity, velocity, acceleration * Time.deltaTime);


    }
    public void Jump(bool jumpInput)
    {
        
        
        if (jumpInput && player.grounded) 
        {
            player.grounded = false;
            holdingJump = true;
            StartCoroutine(JumpHold());
            Vector3 velocity = rigidbody.linearVelocity;
            velocity.y = jumpHeight;
            rigidbody.linearVelocity = velocity;
        }
        if (!jumpInput && holdingJump)
        {
            holdingJump = false;
            StopCoroutine(JumpHold());
        }
       
    }
    IEnumerator CoyoteTime()
    {
        yield return new WaitForSeconds(coyoteTime);
        player.grounded = false;

    }

    IEnumerator JumpHold()
    {

        yield return new WaitForSeconds(jumpLength);
        holdingJump = false;

    }
}
