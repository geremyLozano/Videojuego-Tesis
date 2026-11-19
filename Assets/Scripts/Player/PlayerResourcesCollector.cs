using UnityEngine;

public class PlayerResourcesCollector : MonoBehaviour
{
    private int money = 0;
    private int meat = 0;
    private int wood = 0;

    private string valueCollision;
    private int validacionTag = 0;
    private int valueToUpdate;

    public AudioSource audioTakeItem;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        valueCollision = collision.gameObject.tag;
        validacionTag = valueCollision == "MoneyBag" || valueCollision == "Meat" || valueCollision == "Wood" ? 1 : 0;
        valueToUpdate = 0;
        if (valueCollision == "MoneyBag")
        {
            money ++;
            valueToUpdate = money;
            Destroy(collision.gameObject);
            audioTakeItem.Play();
        }
        else if (valueCollision == "Meat")
        {
            meat ++; 
            valueToUpdate = meat;
            Destroy(collision.gameObject);
            audioTakeItem.Play();
        }
        else if (valueCollision == "Wood")
        {
            wood ++;
            valueToUpdate = wood;
            Destroy(collision.gameObject);
            audioTakeItem.Play();
        }
        if (validacionTag == 1)
        {
            UIManager.Instance.UpdateMoneyMeetWood(valueCollision, valueToUpdate);
        }
    }

    public void UpdateAllResources()
    {
        UIManager.Instance.UpdateMoneyMeetWood("MoneyBag", money);
        UIManager.Instance.UpdateMoneyMeetWood("Meat", meat);
        UIManager.Instance.UpdateMoneyMeetWood("Wood", wood);
    }
    public int GetMoney() => money;
    public int GetMeet() => meat; 
    public int GetWood() => wood;

    public void SetMoney(int amount) => money = amount;
    public void SetMeet(int amount) => meat = amount;
    public void SetWood(int amount) => wood = amount;
    
}
