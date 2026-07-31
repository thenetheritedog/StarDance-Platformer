using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject pivot;
    private PlayerManager playerManager;
    private float lookAngle;
    private float pivotAngle;
    [SerializeField] private float sensitivity;
    [SerializeField] private float lookAcceleration;
    [SerializeField] private float pivotAcceleration;
    private Rigidbody playerRigidbody;
    void Start()
    {
        pivot = transform.parent.gameObject;   
        playerManager = FindAnyObjectByType<PlayerManager>();
        playerRigidbody = playerManager.gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        

        Vector3 newPos = playerManager.transform.position + playerRigidbody.linearVelocity;

        if (!playerManager.grounded)
        {
            newPos.y = pivot.transform.position.y;
        }


        pivot.transform.position = Vector3.Lerp(pivot.transform.position, newPos, pivotAcceleration * Time.deltaTime);
    }

    public void MoveCamera(Vector2 look)
    {
        Debug.Log(look);
        Vector3 pivotTransform = Vector3.zero;
        pivotAngle -= look.x * sensitivity;
        pivotTransform.y = pivotAngle;
        pivot.transform.localRotation = Quaternion.Euler(pivotTransform);
        pivotTransform = Vector3.zero;
        lookAngle -= look.y * sensitivity;
        lookAngle = Mathf.Clamp(lookAngle,-35f, 35f);
        pivotTransform.x = lookAngle;
        transform.localRotation = Quaternion.Euler(pivotTransform);

    }
}
