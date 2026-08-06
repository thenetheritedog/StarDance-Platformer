using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    private Camera mainCamera;
    public Vector3 actualForward;
    void Start()
    {
        mainCamera = FindAnyObjectByType<Camera>();
        actualForward = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = mainCamera.transform.forward;
    }
}
