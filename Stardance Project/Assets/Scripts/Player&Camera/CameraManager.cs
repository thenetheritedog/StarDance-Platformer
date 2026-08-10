using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject pivot;
    public Camera mainCamera;
    private PlayerManager playerManager;
    private float lookAngle;
    private float pivotAngle;

    [SerializeField] private float pivotAcceleration;
    private Rigidbody playerRigidbody;
    [SerializeField] private float defaultDistance;
    [SerializeField] private float defaultFov;
    private ParticleSystem speedLines;
    void Start()
    {
        pivot = transform.parent.gameObject;   
        playerManager = FindAnyObjectByType<PlayerManager>();
        playerRigidbody = playerManager.gameObject.GetComponent<Rigidbody>();
        mainCamera = transform.GetChild(0).GetComponent<Camera>();
        speedLines = mainCamera.transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0) { return; }

        Vector3 newPos = playerManager.cameraPlayerPosition + Vector3.ClampMagnitude( playerRigidbody.linearVelocity, 4f);
        newPos.y = playerManager.cameraPlayerPosition.y;
        

        

        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFov + playerRigidbody.linearVelocity.magnitude,5f * Time.deltaTime);
        mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, defaultFov, 120);

        if (!playerManager.grounded)
        {
            newPos.y = Mathf.Lerp(pivot.transform.position.y, newPos.y, pivotAcceleration * Time.deltaTime / 4); ;
        }
        newPos = Vector3.Lerp(pivot.transform.position, newPos, pivotAcceleration * Time.deltaTime);
        newPos.y = Mathf.Clamp(newPos.y, playerManager.cameraPlayerPosition.y -3, playerManager.cameraPlayerPosition.y + 3);
        pivot.transform.position = newPos;
        

        RaycastHit hit;
        Vector3 cameraTransform = Vector3.zero;
        cameraTransform.z = -defaultDistance + Mathf.Clamp(mainCamera.fieldOfView - defaultFov,0, 120 - defaultFov)/5;
        Vector3 pivotToPlayerVec = pivot.transform.position - playerManager.transform.position;
        if (Physics.Raycast(pivot.transform.position, pivotToPlayerVec.normalized, out hit, pivotToPlayerVec.magnitude, playerManager.defaultLayer))
        {
            pivot.transform.position = playerManager.transform.position + pivotToPlayerVec.normalized * hit.distance;
        }
        if (Physics.Raycast(transform.position, -transform.forward, out hit, -cameraTransform.z, playerManager.defaultLayer))
        {
            cameraTransform.z = -hit.distance +1;
        }
        mainCamera.transform.localPosition = cameraTransform;
    }

    public void MoveCamera(Vector2 look)
    {
        Vector3 pivotTransform = Vector3.zero;
        pivotAngle += look.x * playerManager.sensitivity;
        pivotTransform.y = pivotAngle;
        pivotTransform.z = playerManager.cameraPlayerRotation.z;
        pivot.transform.localRotation = Quaternion.Euler(pivotTransform);
        pivotTransform = Vector3.zero;
        lookAngle -= look.y * playerManager.sensitivity;
        lookAngle = Mathf.Clamp(lookAngle,-35f, 35f);
        pivotTransform.x = lookAngle;
        transform.localRotation = Quaternion.Euler(pivotTransform);
        
    }
    public void Reset()
    {
        transform.localEulerAngles = playerManager.cameraPlayerRotation;
        mainCamera.fieldOfView = defaultFov;
        pivot.transform.position = playerManager.cameraPlayerPosition;
    }
}
