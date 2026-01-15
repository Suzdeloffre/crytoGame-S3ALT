using UnityEngine;
using UnityEngine.UI;
using TMPro; // Utilisez TMPro si vous avez TextMeshPro, sinon utilisez Text

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject chatPanel;           // Le panel qui contient tout le chat
    public Transform chatView;             // Le Grid Layout où s'affichent les messages
    public TMP_InputField textChatInput;   // L'input field pour taper
    public Button sendButton;              // Bouton pour envoyer (optionnel)
    public Button toggleChatButton;        // Le bouton qui ouvre/ferme le chat
    
    [Header("Message Prefab")]
    public GameObject messagePrefab;       // Prefab pour afficher un message
    
    [Header("Settings")]
    public int maxMessages = 50;           // Nombre max de messages affichés
    
    private NetworkChatManager networkManager;
    private bool isChatOpen = false;

    void Start()
    {
        networkManager = FindFirstObjectByType<NetworkChatManager>();
        
        // Cache le chat au démarrage
        if (chatPanel != null)
            chatPanel.SetActive(false);
        
        // Configure les boutons
        //if (toggleChatButton != null)
        //    toggleChatButton.onClick.AddListener(ToggleChat);
        // Button already configured in ChatToggleButton.cs causing the chat to
        // open and close instantly
        
        if (sendButton != null)
            sendButton.onClick.AddListener(SendMessage);
        
        // Permet d'envoyer avec la touche Entrée
        if (textChatInput != null)
        {
            textChatInput.onSubmit.AddListener(delegate { SendMessage(); });
        }
    }

    void Update()
    {
        // Ouvre/ferme le chat avec la touche T (ou autre)
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            ToggleChat();
        }
    }

    public void ToggleChat()
    {
        isChatOpen = !isChatOpen;
        chatPanel.SetActive(isChatOpen);
        
        // Focus sur l'input quand on ouvre le chat
        if (isChatOpen && textChatInput != null)
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
        textChatInput.text = ""; // Vide l'input
        textChatInput.ActivateInputField(); // Re-focus sur l'input
        
        // Envoie via le réseau
        if (networkManager != null)
        {
            networkManager.SendChatMessage(message);
        }
        else
        {
            Debug.LogWarning("NetworkChatManager non trouvé !");
            // En mode test sans réseau
            DisplayMessage("Moi", message, false);
        }
    }

    // Affiche un message dans le chat
    public void DisplayMessage(string senderName, string message, bool isEncrypted)
    {
        if (chatView == null || messagePrefab == null)
        {
            Debug.LogWarning("ChatView ou MessagePrefab non assigné !");
            return;
        }
        
        // Crée une nouvelle instance du message
        GameObject newMessage = Instantiate(messagePrefab, chatView);
        
        // Configure le texte du message
        TMP_Text messageText = newMessage.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            string encryptedTag = isEncrypted ? " 🔒" : "";
            messageText.text = $"<b>{senderName}</b>: {message}{encryptedTag}";
        }
        
        // Limite le nombre de messages affichés
        if (chatView.childCount > maxMessages)
        {
            Destroy(chatView.GetChild(0).gameObject);
        }
        
        // Scroll automatique vers le bas (si vous avez un ScrollRect)
        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = chatView.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // Pour afficher des messages système
    public void DisplaySystemMessage(string message)
    {
        DisplayMessage("SYSTÈME", message, false);
    }
}