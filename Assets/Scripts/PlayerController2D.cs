using Unity.Netcode;
using UnityEngine;

public class PlayerController2D : NetworkBehaviour
{
        public float speed = 3f;
        public ChatSystem chat;
        public bool canMove = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner){
            return;
        }
        if(Input.GetKey(KeyCode.Q)&& canMove){
            transform.Translate(-speed * Time.deltaTime, 0, 0);
        }
        if(Input.GetKey(KeyCode.D)&& canMove){
            transform.Translate(speed * Time.deltaTime, 0, 0);
        }
        if(Input.GetKey(KeyCode.Z)&& canMove){
            transform.Translate(0, speed * Time.deltaTime, 0);
        }
        if(Input.GetKey(KeyCode.S)&& canMove){
            transform.Translate(0, -speed * Time.deltaTime, 0);
        }

        

    }
}
