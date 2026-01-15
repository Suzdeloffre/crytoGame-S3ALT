using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class NetworkChatManager : NetworkBehaviour
{
    private ChatManager chatManager;
    private RSA rsaEncryption; // ← Changé de RSAEncryption à RSA
    
    // Dictionnaire pour stocker les clés publiques des joueurs
    private Dictionary<ulong, string> playerPublicKeys = new Dictionary<ulong, string>();
    
    void Start()
    {

        chatManager = FindFirstObjectByType<ChatManager>();
        rsaEncryption = GetComponent<RSA>(); // ← Changé de RSAEncryption à RSA

        NetworkManager.OnClientConnectedCallback += OnClientConnected;
        
        if (rsaEncryption == null)
        {
            rsaEncryption = gameObject.AddComponent<RSA>(); // ← Changé
        }
        
        
    }

    public void share()
    {        
        Debug.Log("share called, IsClient: " + NetworkManager.IsClient);
        if (NetworkManager.IsClient)
        {
            rsaEncryption.ShowKeys();
            SharePublicKeyServerRpc(NetworkManager.Singleton.LocalClientId, rsaEncryption.PublicKeyString);
            
            // Demande toutes les clés existantes si on n'est pas le host
            if (!NetworkManager.Singleton.IsHost)
            {
                RequestAllKeysServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        Debug.Log("Client local connecté et prêt !");

        // Vérifie que le NetworkObject est spawné avant de partager la clé
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            share();
        }
        else
        {
            // Sinon, attendre que le NetworkObject soit spawné
            StartCoroutine(WaitForSpawnAndShare());
        }

        NetworkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    private System.Collections.IEnumerator WaitForSpawnAndShare()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        while (netObj == null || !netObj.IsSpawned)
            yield return null;

        share();
    }
    // Envoie un message chat
    public void SendChatMessage(string message)
    {
        if (!IsClient) return;
        
        string playerName = "Joueur " + NetworkManager.Singleton.LocalClientId;
        
        // Pour un chat global, on chiffre avec la clé publique du serveur
        // (Dans votre cas, vous pouvez adapter pour chiffrer pour chaque joueur)
        string encryptedMessage = message; // On garde en clair pour démo
        
        // Si vous voulez chiffrer pour le serveur uniquement :
        // if (playerPublicKeys.ContainsKey(0)) // 0 = serveur
        // {
        //     encryptedMessage = rsaEncryption.Encrypt(message, playerPublicKeys[0]);
        // }
        
        SendMessageServerRpc(playerName, message, NetworkManager.Singleton.LocalClientId);
    }

    // Le client envoie un message au serveur
    [Rpc(SendTo.Server)]
    void SendMessageServerRpc(string senderName, string message, ulong senderId)
    {
        // Option 1 : Message en clair (pour chat global)
        ReceiveMessageClientRpc(senderName, message, senderId, false);
        
        // Option 2 : Si vous voulez chiffrer pour tous les joueurs
        // Vous devriez alors chiffrer le message pour chaque clé publique
    }

    // Tous les clients reçoivent le message
    [Rpc(SendTo.ClientsAndHost)]
    void ReceiveMessageClientRpc(string senderName, string message, ulong senderId, bool isEncrypted)
    {
        string displayMessage = message;
        
        if (chatManager != null)
        {
            chatManager.DisplayMessage(senderName, displayMessage, isEncrypted);
        }
    }

    // Partage la clé publique RSA
    [Rpc(SendTo.Server)]
    void SharePublicKeyServerRpc(ulong clientId, string publicKey)
    {
        Debug.Log($"[Server] Clé reçue de {clientId}: {publicKey}");
        
        if (!playerPublicKeys.ContainsKey(clientId))
        {
            playerPublicKeys[clientId] = publicKey;
        }
        
        // Broadcast à tous les clients
        UpdatePublicKeyClientRpc(clientId, publicKey);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdatePublicKeyClientRpc(ulong clientId, string publicKey)
    {
        if (!playerPublicKeys.ContainsKey(clientId))
        {
            playerPublicKeys[clientId] = publicKey;
        }
        else
        {
            Debug.LogWarning($"Clé du joueur {clientId} déjà présente");
        }
        Debug.Log($"Clé publique du joueur {clientId} ajoutée : {publicKey}");
            
        if (chatManager != null)
        {
            chatManager.DisplaySystemMessage($"Joueur {clientId} a rejoint le chat (clé reçue)");
        }
    }

    // Méthode pour envoyer un message chiffré à un joueur spécifique
    public void SendEncryptedMessageToPlayer(ulong targetPlayerId, string message)
    {
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Tentative envoi crypté à {targetPlayerId}");
        Debug.Log($"Clés disponibles: {string.Join(", ", playerPublicKeys.Keys)}");
        
        if (!playerPublicKeys.ContainsKey(targetPlayerId))
        {
            Debug.LogWarning($"Clé publique du joueur {targetPlayerId} non trouvée");
            if (chatManager != null)
            {
                chatManager.DisplaySystemMessage($"Erreur: Clé du joueur {targetPlayerId} manquante");
            }
            return;
        }
        
        // Chiffre avec la clé publique du destinataire
        string encryptedMessage = rsaEncryption.Encrypt(message, playerPublicKeys[targetPlayerId]);
        
        string senderName = "Joueur " + NetworkManager.Singleton.LocalClientId;
        SendPrivateMessageServerRpc(senderName, encryptedMessage, targetPlayerId, NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    void SendPrivateMessageServerRpc(string senderName, string encryptedMessage, ulong targetId, ulong senderId)
    {
        // Envoie uniquement au destinataire et à l'expéditeur
        ReceivePrivateMessageClientRpc(senderName, encryptedMessage, senderId, targetId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ReceivePrivateMessageClientRpc(string senderName, string encryptedMessage, ulong senderId, ulong targetId)
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        
        // Seuls l'expéditeur et le destinataire voient le message
        if (localId == senderId || localId == targetId)
        {
            string decryptedMessage = rsaEncryption.Decrypt(encryptedMessage);
            
            if (chatManager != null)
            {
                string displayName = localId == senderId ? senderName + " (à Joueur " + targetId + ")" : senderName + " (privé)";
                chatManager.DisplayMessage(displayName, decryptedMessage, true);
            }
        }
    }
   // Demande toutes les clés existantes au serveur
    [Rpc(SendTo.Server)]
    void RequestAllKeysServerRpc(ulong requestingClientId)
    {
        Debug.Log($"[Server] Client {requestingClientId} demande toutes les clés");
        
        // Envoie toutes les clés existantes au client qui demande
        foreach (var kvp in playerPublicKeys)
        {
            SendSingleKeyClientRpc(kvp.Key, kvp.Value, requestingClientId);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    void SendSingleKeyClientRpc(ulong playerId, string publicKey, ulong targetClientId)
    {
        // Seul le client ciblé traite ce message
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;
            
        if (!playerPublicKeys.ContainsKey(playerId))
        {
            playerPublicKeys[playerId] = publicKey;
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Clé du joueur {playerId} reçue : {publicKey}");
        }
    }
}