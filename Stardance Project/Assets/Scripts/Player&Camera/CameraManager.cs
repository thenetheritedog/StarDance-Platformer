using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject pivot;
    public Camera mainCamera;
    private PlayerManager playerManager;
    private float lookAngle;
    private float pivotAngle;
    [SerializeField] private float sensitivity;
    [SerializeField] private float pivotAcceleration;
    private Rigidbody playerRigidbody;
    [SerializeField] private float defaultDistance;
    [SerializeField] private float defaultFov;
    void Start()
    {
        pivot = transform.parent.gameObject;   
        playerManager = FindAnyObjectByType<PlayerManager>();
        playerRigidbody = playerManager.gameObject.GetComponent<Rigidbody>();
        mainCamera = transform.GetChild(0).GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        

        Vector3 newPos = playerManager.transform.position + Vector3.ClampMagnitude( playerRigidbody.linearVelocity, 4f);
        newPos.y = playerManager.transform.position.y;
        

        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFov + playerRigidbody.linearVelocity.magnitude,5f * Time.deltaTime);
        if (!playerManager.grounded)
        {
            newPos.y = Mathf.Lerp(pivot.transform.position.y, newPos.y, pivotAcceleration * Time.deltaTime / 4); ;
        }
        newPos = Vector3.Lerp(pivot.transform.position, newPos, pivotAcceleration * Time.deltaTime);
        newPos.y = Mathf.Clamp(newPos.y, playerManager.transform.position.y -3, playerManager.transform.position.y + 3);
        pivot.transform.position = newPos;
        

        RaycastHit hit;
        Vector3 cameraTransform = Vector3.zero;
        cameraTransform.z = -defaultDistance +1;
        
        if (Physics.Raycast(transform.position, -transform.forward, out hit, defaultDistance, playerManager.defaultLayer))
        {
            cameraTransform.z = -hit.distance +1;
        }
        mainCamera.transform.localPosition = cameraTransform;
    }

    public void MoveCamera(Vector2 look)
    {
        Vector3 pivotTransform = Vector3.zero;
        pivotAngle += look.x * sensitivity;
        pivotTransform.y = pivotAngle;
        pivot.transform.localRotation = Quaternion.Euler(pivotTransform);
        pivotTransform = Vector3.zero;
        lookAngle -= look.y * sensitivity;
        lookAngle = Mathf.Clamp(lookAngle,-35f, 35f);
        pivotTransform.x = lookAngle;
        transform.localRotation = Quaternion.Euler(pivotTransform);
        
    }
}
