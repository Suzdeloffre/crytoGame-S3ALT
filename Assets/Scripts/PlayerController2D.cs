using Unity.Netcode;
using UnityEngine;

public class PlayerController2D : NetworkBehaviour
{
        public float speed = 5f;
        public ChatSystem chat;
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
        if(Input.GetKey(KeyCode.Q)){
            transform.Translate(-speed * Time.deltaTime, 0, 0);
        }
        if(Input.GetKey(KeyCode.D)){
            transform.Translate(speed * Time.deltaTime, 0, 0);
        }

        if(Input.GetKeyDown(KeyCode.C)){
            Debug.Log($"{chat.GetComponent<Transform>()}");
            chat.sendMessageRpc($"{OwnerClientId} : Hi !");
        }

    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"Spawned on {OwnerClientId}, isOwner={IsOwner}");
        chat = GameObject.FindWithTag("Chat").GetComponent<ChatSystem>();
        //Debug.Log(chat.getChatRpc());
    }
}
