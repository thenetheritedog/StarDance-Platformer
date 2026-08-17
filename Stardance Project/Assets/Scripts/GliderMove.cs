using Unity.VisualScripting;
using UnityEngine;

public class GliderMove : MonoBehaviour
{
    private PlayerManager player;
    public GameObject spawner;
    [SerializeField] private GameObject instancePrefab;
    [SerializeField] private float baseSpeed;
    public float speed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private Vector3 direction;
    [SerializeField] private float slowDown;
    [SerializeField] private float maxSpeed;
    [SerializeField] private bool gliderCollisionResetFalse;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        speed = baseSpeed;
        player = FindAnyObjectByType<PlayerManager>() as PlayerManager;
        direction = transform.forward * speed;
    }
    public void MoveDirection(Vector3 moveVector)
    {
        Quaternion rotation = Quaternion.AngleAxis(90, player.camera.transform.right);
        moveVector = rotation * moveVector;
        moveVector = Vector3.Lerp(moveVector.normalized, player.camera.transform.forward, 0.5f).normalized;
        
        direction = Vector3.ProjectOnPlane(moveVector, Vector3.up).normalized * moveVector.magnitude * (1f - moveVector.normalized.y) * baseSpeed/2;
        direction.y = -gravity * (1 - moveVector.y/2) * 2;
        speed = Mathf.Lerp(rb.linearVelocity.magnitude, direction.magnitude, turnSpeed * Time.deltaTime);
        direction = Vector3.Lerp(rb.linearVelocity, direction, turnSpeed * Time.deltaTime).normalized * speed;
        transform.forward = direction.normalized;


        player.transform.forward = transform.forward;

        Debug.DrawRay(transform.position, moveVector, Color.blue);
        player.transform.position = transform.position - transform.up;
        
        //rb.linearVelocity = transform.forward * speed;
    }
    private void FixedUpdate()
    {
        if (Time.timeScale == 0) { return; }
        
        rb.linearVelocity = direction;
        if (player.GetComponent<PlayerMovement>().glider != this)
        {
            
            Vector3 moveVector = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            direction = Vector3.ProjectOnPlane(moveVector, Vector3.up).normalized * moveVector.magnitude * (1f - moveVector.normalized.y) * speed;
            direction.y = -gravity * (1 + moveVector.normalized.y);
            speed = Mathf.Lerp(rb.linearVelocity.magnitude, baseSpeed, turnSpeed * Time.deltaTime);
            direction = Vector3.Lerp(rb.linearVelocity, direction, turnSpeed * Time.deltaTime).normalized * speed;
            transform.forward = direction.normalized;



        }
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == 0 && !gliderCollisionResetFalse) 
        {
            FindAnyObjectByType<PlayerManager>().ResetLevel();
        }
        
    }

    public void ResetGlider()
    {

        speed = baseSpeed;
        direction = spawner.transform.forward * speed;
        transform.position = spawner.transform.position;
        transform.forward = spawner.transform.forward; 
    }
}
