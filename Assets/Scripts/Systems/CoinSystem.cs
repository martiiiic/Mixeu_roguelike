using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSystem : MonoBehaviour
{
    private CoinText coinText;
    public int Coins;
    private AudioSource coinSound;

    private int Coins_;

    public void AddCoins(int CoinsToAdd)
    {
        Debug.Log("Added Coins: " + CoinsToAdd);
        Coins += CoinsToAdd;
        coinSound.Play();
    }
    
    public void RemoveCoins(int CoinsToRemove)
    {
        Coins -= CoinsToRemove;
    }

    private void Start()
    {
        coinSound = GetComponent<AudioSource>();
        Coins_ = Coins;
        coinText = FindAnyObjectByType<CoinText>();
    }

    public void Update()
    {
        if(Coins_ != Coins)
        {
            coinText.updateText(Coins);
            Coins_ = Coins;
        }
    }
}
