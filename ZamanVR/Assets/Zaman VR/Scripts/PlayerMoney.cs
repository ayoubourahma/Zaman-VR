using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class PlayerMoney : MonoBehaviour
{
    [Header("Money Settings")]
    public int startingMoney = 0;
    
    [Header("UI Display")]
    public TMP_Text moneyText;
    
    [Header("Events")]
    public UnityEvent<int> onMoneyChanged;
    public UnityEvent<int> onMoneyAdded;
    public UnityEvent<int> onMoneyRemoved;
    
    private int currentMoney;

    void Start()
    {
        currentMoney = startingMoney;
        UpdateMoneyDisplay();
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        
        currentMoney += amount;
        UpdateMoneyDisplay();
        
        onMoneyAdded?.Invoke(amount);
        onMoneyChanged?.Invoke(currentMoney);
    }

    public void RemoveMoney(int amount)
    {
        if (amount <= 0) return;
        
        currentMoney -= amount;
        
        if (currentMoney < 0)
            currentMoney = 0;
        
        UpdateMoneyDisplay();
        
        onMoneyRemoved?.Invoke(amount);
        onMoneyChanged?.Invoke(currentMoney);
    }

    public void SetMoney(int amount)
    {
        currentMoney = amount;
        UpdateMoneyDisplay();
        onMoneyChanged?.Invoke(currentMoney);
    }

    public int GetMoney()
    {
        return currentMoney;
    }

    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = currentMoney.ToString();
        }
    }
}