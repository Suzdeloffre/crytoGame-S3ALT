using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Netcode;

public class ChatSystem : NetworkBehaviour
{
    public List<string> messages = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void sendMessageRpc(string message){
        Debug.Log("New Chat !");
        messages.Add(message);
    }

    //
}
