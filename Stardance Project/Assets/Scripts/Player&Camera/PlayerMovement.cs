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
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float toGroundDis;

    [SerializeField] private float jumpLength;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpHeight;
    private bool holdingJump;
    private float gravityPull;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        player = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        Vector3 velocity = rigidbody.linearVelocity;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, toGroundDis * 1.25f, player.defaultLayer) && !holdingJump)
        {
            if (hit.distance > toGroundDis && !player.grounded)
            {
                if (holdingJump)
                {
                    gravityPull -= gravity * Time.deltaTime / 5;
                }
                else
                {
                    gravityPull -= gravity * Time.deltaTime;
                }
                return;
            }
            player.grounded = true;
            gravityPull = 0;
            Vector3 newPositionForSlopes = transform.position;
            newPositionForSlopes.y = hit.point.y + toGroundDis;
            transform.position = newPositionForSlopes;
        }
        else
        {
            StartCoroutine(CoyoteTime());
            if (holdingJump)
            {
                gravityPull -= gravity * Time.deltaTime / 5;
            }
            else
            {
                gravityPull -= gravity * Time.deltaTime;
            }

        }
        velocity.y = gravityPull;
        rigidbody.linearVelocity = velocity;


    }
    public void Movement(Vector2 moveVector, bool sprint)
    {
        Vector3 velocity = moveVector.y * player.camera.transform.forward;
        velocity += moveVector.x * player.camera.transform.right;

        if (sprint)
        {
            velocity *= sprintSpeed;
        }
        else
        {
            velocity *= speed;
        }
        velocity.y = rigidbody.linearVelocity.y;
        rigidbody.linearVelocity = Vector3.Lerp(rigidbody.linearVelocity, velocity , acceleration  * Time.deltaTime);


    }
    public void Jump(bool jumpInput)
    {
        
        
        if (jumpInput && player.grounded) 
        {
            player.grounded = false;
            holdingJump = true;
            StartCoroutine(JumpHold());
            Vector3 velocity = rigidbody.linearVelocity;
            gravityPull = jumpHeight;
            velocity.y = gravityPull;
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
