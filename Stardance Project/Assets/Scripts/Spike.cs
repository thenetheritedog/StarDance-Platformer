using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private float length;
    [SerializeField] private float width;
    [SerializeField] private GameObject spikePrefab;
    void Start()
    {
        length = GetComponent<Collider>().bounds.size.x;
        width = GetComponent<Collider>().bounds.size.z;
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                GameObject spike = Instantiate(spikePrefab);
                
                spike.transform.parent = transform;
                transform.localRotation = Quaternion.identity;
                spike.transform.localPosition = new Vector3(i - length / 2, 0.5f, j - width / 2);
                spike.transform.localScale = Vector3.one;
            }
        }
        transform.localScale = Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
