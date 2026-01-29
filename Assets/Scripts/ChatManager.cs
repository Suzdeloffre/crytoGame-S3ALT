using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatPanel;
    public Transform chatView;
    public TMP_InputField textChatInput;
    public Button sendButton;
    public Button toggleChatButton;

    [Header("Dropdown Manager")]
    public DropDownManager dropDownManager; // Référence au script séparé

    [Header("Message Prefab")]
    public GameObject messagePrefab;

    [Header("Settings")]
    public int maxMessages = 50;

    private NetworkChatManager networkManager;
    private bool isChatOpen = false;
    private GameObject localPlayer;

    void Start()
    {
        networkManager = FindFirstObjectByType<NetworkChatManager>();

        // Cache le chat au démarrage
        if (chatPanel != null)
            chatPanel.SetActive(false);

        if (sendButton != null)
            sendButton.onClick.AddListener(SendMessage);

        if (textChatInput != null)
            textChatInput.onSubmit.AddListener(delegate { SendMessage(); });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
            ToggleChat();
    }

    public void ToggleChat()
    {
        isChatOpen = !isChatOpen;
        chatPanel.SetActive(isChatOpen);
        GameObject localPlayer = GameObject.FindWithTag("LocalPlayer");
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("LocalPlayer"))
        {
            if (player.GetComponent<NetworkObject>().IsOwner)
            {
                player.GetComponent<PlayerController2D>().canMove = isChatOpen ? false : true;
                break;
            }
        }
        {
            textChatInput.Select();
            textChatInput.ActivateInputField();
        }
    }

   public void SendMessage()
{
    if (textChatInput == null || string.IsNullOrEmpty(textChatInput.text))
        return;

    string message = textChatInput.text;
    textChatInput.text = "";
    textChatInput.ActivateInputField();

    if (networkManager != null)
    {
        if (dropDownManager != null && dropDownManager.GetSelectedPlayerId() != 99999)
        {
            // Envoi crypté à un joueur spécifique
            ulong targetId = dropDownManager.GetSelectedPlayerId();
            networkManager.SendEncryptedMessageToPlayer(targetId, message);
            DisplayMessage("Moi", message, true);
        }
        else
        {
            // Pas de sélection → envoi à tous en clair
            networkManager.SendChatMessage(message);
            DisplayMessage("Moi", message, false);
        }
    }
    else
    {
        Debug.LogWarning("NetworkChatManager non trouvé !");
        DisplayMessage("Moi", message, false);
    }
}


    public void DisplayMessage(string senderName, string message, bool isEncrypted)
    {
        if (chatView == null || messagePrefab == null)
            return;

        GameObject newMessage = Instantiate(messagePrefab, chatView);
        TMP_Text messageText = newMessage.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            string encryptedTag = isEncrypted ? " -Crypted " : "";
            messageText.text = $"<b>{senderName}</b>: {message}{encryptedTag}";
        }

        if (chatView.childCount > maxMessages)
            Destroy(chatView.GetChild(0).gameObject);

        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = chatView.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void DisplaySystemMessage(string message) { DisplayMessage("SYSTÈME", message, false); }
    
}