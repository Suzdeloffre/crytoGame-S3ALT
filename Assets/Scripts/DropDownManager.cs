using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using Unity.VisualScripting;

public class DropDownManager : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public TMP_Dropdown playerDropdown;

    private List<ulong> playerIds = new List<ulong>();
    private List<string> playerNames = new List<string>();

    void Start()
    {

        if (playerDropdown == null)
        {
            Debug.LogError("Dropdown non assigné !");
            return;
        }
        
        // Initialise la liste au démarrage
        UpdatePlayerList();
        
        // On peut aussi écouter la sélection si besoin
        playerDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
}

    /// <summary>
    /// IPointerClickHandler : appelé quand on clique sur le dropdown pour le dérouler
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        UpdatePlayerList();
    }

    /// <summary>
    /// Met à jour la liste des joueurs et le dropdown
    /// </summary>
   public void UpdatePlayerList()
    {
        // Sauvegarde la sélection actuelle
        int currentSelection = playerDropdown.value;
        
        playerIds.Clear();
        playerNames.Clear();
        playerDropdown.ClearOptions();
        
        playerNames.Add("All");
        playerIds.Add(99999);
        
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            string playerName = "Player " + clientId;
            playerIds.Add(clientId);
            playerNames.Add(playerName);
        }
        
        playerDropdown.AddOptions(playerNames);
        
        // Restaure la sélection si elle est toujours valide
        if (currentSelection < playerNames.Count)
        {
            playerDropdown.value = currentSelection;
        }
    }

    /// <summary>
    /// Récupère le clientId du joueur sélectionné
    /// </summary>
    public ulong GetSelectedPlayerId()
    {
        // S'assure que les listes sont à jour
        if (playerIds.Count == 0)
        {
            UpdatePlayerList();
        }
        
        int index = playerDropdown.value;
        Debug.Log($"Index dropdown: {index}, Nombre d'IDs: {playerIds.Count}");
        
        if (index >= 0 && index < playerIds.Count)
            return playerIds[index];
        
        Debug.LogWarning("Dropdown sélectionné invalide !");
        return 99999; // Retourne "All" par défaut au lieu de 0
    }

    /// <summary>
    /// Optionnel : quand on change la sélection
    /// </summary>
    private void OnDropdownValueChanged(int index)
    {
        if (index >= 0 && index < playerIds.Count)
        {
            Debug.Log($"Joueur sélectionné: {playerNames[index]} (ID: {playerIds[index]})");
        }
    }
}