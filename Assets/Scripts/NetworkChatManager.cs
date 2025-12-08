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
        
        if (rsaEncryption == null)
        {
            rsaEncryption = gameObject.AddComponent<RSA>(); // ← Changé
        }
        
        // Affiche les clés générées
        if (IsClient)
        {
            rsaEncryption.ShowKeys();
            SharePublicKeyServerRpc(NetworkManager.Singleton.LocalClientId, rsaEncryption.PublicKeyString);
        }
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
        playerPublicKeys[clientId] = publicKey;
        Debug.Log($"Clé publique reçue du joueur {clientId} : {publicKey}");
        
        // Broadcast à tous les clients
        UpdatePublicKeyClientRpc(clientId, publicKey);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdatePublicKeyClientRpc(ulong clientId, string publicKey)
    {
        if (!playerPublicKeys.ContainsKey(clientId))
        {
            playerPublicKeys[clientId] = publicKey;
            Debug.Log($"Clé publique du joueur {clientId} ajoutée : {publicKey}");
            
            if (chatManager != null)
            {
                chatManager.DisplaySystemMessage($"Joueur {clientId} a rejoint le chat (clé reçue)");
            }
        }
    }

    // Méthode pour envoyer un message chiffré à un joueur spécifique
    public void SendEncryptedMessageToPlayer(ulong targetPlayerId, string message)
    {
        if (!playerPublicKeys.ContainsKey(targetPlayerId))
        {
            Debug.LogWarning($"Clé publique du joueur {targetPlayerId} non trouvée");
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
}