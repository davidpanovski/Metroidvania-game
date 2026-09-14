using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private float followspeed = 0.1f;
    [SerializeField] private Vector3 offset;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, PlayerController.Instance.transform.position + offset, followspeed);
        //dodavame offset bidejki ako e samo position kamerata kje bide zalepena na igrachot a nie sakame da go gledame od daleku
        //vo late update za da e po smooth
    }
    void Update()
    {
       }
}
