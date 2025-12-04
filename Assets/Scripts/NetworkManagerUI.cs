using Unity.Netcode;
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
        NetworkManager.Singleton.StartClient();
        HideConnectionPanel();
    }

    void HideConnectionPanel()
    {
        if (connectionPanel != null)
            connectionPanel.SetActive(false);
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