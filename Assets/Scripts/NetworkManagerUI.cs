using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interface simple pour démarrer le jeu en mode Host, Server ou Client
/// </summary>
public class NetworkManagerUI : MonoBehaviour
{
    [Header("Boutons UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    
    [Header("Panneau de connexion")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] public GameObject chatPanel;

    [Header("Réseau du chat")]
    [SerializeField] public NetworkChatManager netWorkChatManager;

    void Awake()
    {
        // Assigner les fonctions aux boutons
        if (hostButton != null)
            hostButton.onClick.AddListener(StartHost);
            
        if (serverButton != null)
            serverButton.onClick.AddListener(StartServer);
            
        if (clientButton != null)
            clientButton.onClick.AddListener(StartClient);
    }

    void StartHost()
    {
        Debug.Log("Démarrage en mode Host (Serveur + Client)");
        NetworkManager.Singleton.StartHost();
        netWorkChatManager.share();
        HideConnectionPanel();
    }

    void StartServer()
    {
        Debug.Log("Démarrage en mode Serveur uniquement");
        NetworkManager.Singleton.StartServer();
        HideConnectionPanel();
    }

    void StartClient()
    {
        Debug.Log("Démarrage en mode Client");

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager absent !");
            return;
        }

        // Start le client
        NetworkManager.Singleton.StartClient();

        // Cache le panel
        HideConnectionPanel();

    }

    void HideConnectionPanel()
    {
        if (connectionPanel != null)
            connectionPanel.SetActive(false);
        //also show ChatPanel
        if (chatPanel != null)
            chatPanel.SetActive(true);
    }

    void OnDestroy()
    {
        // Nettoyer les listeners
        if (hostButton != null)
            hostButton.onClick.RemoveListener(StartHost);
            
        if (serverButton != null)
            serverButton.onClick.RemoveListener(StartServer);
            
        if (clientButton != null)
            clientButton.onClick.RemoveListener(StartClient);
    }
}