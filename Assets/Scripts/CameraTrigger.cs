using Cinemachine;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] CinemachineVirtualCamera newCamera;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player"))
        {
            CameraManager.Instance.SwapCamera(newCamera);
        }
    }
}
