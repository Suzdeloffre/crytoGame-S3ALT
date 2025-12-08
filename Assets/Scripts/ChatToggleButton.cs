using UnityEngine;
using UnityEngine.UI;

// Attachez ce script directement sur votre bouton ChatBox
public class ChatToggleButton : MonoBehaviour
{
    private ChatManager chatManager;
    private Button button;

    void Start()
    {
        // Trouve le ChatManager dans la scène
        chatManager = FindFirstObjectByType<ChatManager>();
        
        // Récupère le composant Button
        button = GetComponent<Button>();
        
        if (button != null && chatManager != null)
        {
            // Connecte le clic à la fonction ToggleChat
            button.onClick.AddListener(OnButtonClick);
            Debug.Log("Bouton ChatBox connecté au ChatManager");
        }
        else
        {
            Debug.LogError("Button ou ChatManager non trouvé !");
        }
    }

    void OnButtonClick()
    {
        if (chatManager != null)
        {
            chatManager.ToggleChat();
        }
    }
}