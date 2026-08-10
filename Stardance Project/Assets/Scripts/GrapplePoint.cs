using System.Collections;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    private Camera mainCamera;
    private Collider colliderOfGrapple;
    private RectTransform rectTransform;
    public Vector3 actualForward;
    public bool makesYouLookFoward;
    public bool wallRunGrapple;

    void Start()
    {
        mainCamera = FindAnyObjectByType<Camera>();
        actualForward = transform.forward;
        
        rectTransform = GetComponent<RectTransform>();
        colliderOfGrapple = GetComponent<Collider>();

    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = mainCamera.transform.forward;
        if (colliderOfGrapple.enabled)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one, Time.deltaTime * 5);
        }
        else
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one/3, Time.deltaTime * 5);
        }
    }

    public IEnumerator DelayUse()
    {
        yield return new WaitForSeconds(4f);
        colliderOfGrapple .enabled = true;
    }

    public void ResetGrapple()
    {
        colliderOfGrapple.enabled = true;
    }
}
