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
        playerIds.Clear();
        playerNames.Clear();
        playerDropdown.ClearOptions();

        playerNames.Add("All");
        playerIds.Add(99999);
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            string playerName = "Player " + clientId; // Remplace par le vrai nom si tu as un système de noms
            playerIds.Add(clientId);
            playerNames.Add(playerName);
        }

        playerDropdown.AddOptions(playerNames);
    }

    /// <summary>
    /// Récupère le clientId du joueur sélectionné
    /// </summary>
    public ulong GetSelectedPlayerId()
    {
        int index = playerDropdown.value;
        Debug.Log(index);
        if (index >= 0 && index < playerIds.Count)
            return playerIds[index];

        Debug.LogWarning("Dropdown sélectionné invalide !");
        return 0;
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