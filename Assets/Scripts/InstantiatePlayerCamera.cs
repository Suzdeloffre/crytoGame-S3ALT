using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class InstantiatePlayerCamera : NetworkBehaviour
{
    public Camera mainCamera;
    public Camera playerCamera;

    public GameObject cameraPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!IsOwner){
            return;
        }
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        if (mainCamera) mainCamera.enabled = false;
        playerCamera = Instantiate(cameraPrefab, transform.position, Quaternion.identity).GetComponent<Camera>();
        playerCamera.enabled = true;
        playerCamera.GetComponent<CameraMove>().player = gameObject;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
