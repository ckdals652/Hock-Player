using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class WorldSpaceHPUICanves : MonoBehaviourPun
{
    public Queue<GameObject> HpWindows = new Queue<GameObject>();

    private int hpWindowCount = 6;

    [SerializeField] private GameObject hpPrefab;

    private Transform playerHps;
    private string playerHpWindowsText = "PlayerHpWindows";

    private void Awake()
    {
        playerHps = transform.Find(playerHpWindowsText);
    }

    private void Start()
    {
        for (int i = 0; i < hpWindowCount; i++)
        {
            AddHpUI();
        }
    }

    private void AddHpUI()
    {
        var hp = Instantiate(hpPrefab, playerHps);
        HpWindows.Enqueue(hp);
        hp.SetActive(false);
    }
    
    public GameObject GetHpSlot()
    {
        if (HpWindows.Count <= 0)
        {
            AddHpUI();
        }
        return HpWindows.Dequeue();
    }
}