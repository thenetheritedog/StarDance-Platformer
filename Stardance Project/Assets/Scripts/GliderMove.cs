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
        
        direction = Vector3.ProjectOnPlane(moveVector, Vector3.up).normalized * moveVector.magnitude * (1f - moveVector.normalized.y) * speed;
        direction.y = -gravity * (1 - moveVector.y) * 2;
        direction = Vector3.Lerp(rb.linearVelocity, direction, turnSpeed * Time.deltaTime);
        transform.forward = direction.normalized;




        Debug.Log(speed);
        Debug.DrawRay(transform.position, moveVector, Color.blue);
        player.transform.position = transform.position - transform.up;
        
        //rb.linearVelocity = transform.forward * speed;
    }
    private void FixedUpdate()
    {
        speed = Mathf.Lerp(speed, baseSpeed, turnSpeed * Time.deltaTime);
        rb.linearVelocity = direction;
        if (player.GetComponent<PlayerMovement>().glider != this)
        {
            Vector3 moveVector = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            direction = Vector3.ProjectOnPlane(moveVector, Vector3.up).normalized * moveVector.magnitude * (1f - moveVector.normalized.y) * speed;
            direction.y = -gravity * (1 + moveVector.normalized.y);
            direction = Vector3.Lerp(rb.linearVelocity, direction, turnSpeed * Time.deltaTime);
            transform.forward = direction.normalized;



        }
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == 0) 
        {
            ResetGlider();
            //FindAnyObjectByType<PlayerManager>().ResetLevel();
        }
        
    }

    public void ResetGlider()
    {
        
        
        GameObject instance = Instantiate(instancePrefab);
        instance.GetComponent<GliderMove>().spawner = spawner;
        instance.transform.position = spawner.transform.position;
        instance.transform.forward = spawner.transform.forward;
        Destroy(gameObject);
    }
}
