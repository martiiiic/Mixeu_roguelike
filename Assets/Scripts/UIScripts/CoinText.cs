using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinText : MonoBehaviour
{
    private TMPro.TextMeshProUGUI CoinText_;

    private void Start()
    {
        CoinText_ = GetComponent<TMPro.TextMeshProUGUI>();
    }
    public void updateText(int currentCoins)
    {
        CoinText_.text = ("x" + currentCoins);
    }
}
