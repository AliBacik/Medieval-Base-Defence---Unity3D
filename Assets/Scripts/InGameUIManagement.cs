using TMPro;
using UnityEngine;

public class InGameUIManagement : MonoBehaviour
{
    public static InGameUIManagement Instance;
    //
    public TextMeshProUGUI m_CoinAmount;
    public TextMeshProUGUI m_WoodAmount;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateCoinText(int amount)
    {
        if (m_CoinAmount != null)
        {
            m_CoinAmount.text = amount.ToString();
        }
    }

    public void UpdateWoodText(int amount)
    {
        if (m_WoodAmount != null)
        {
            m_WoodAmount.text = amount.ToString();
        }
    }
   
}
