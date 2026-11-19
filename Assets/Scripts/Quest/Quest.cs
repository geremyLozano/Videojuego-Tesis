using UnityEngine;

[System.Serializable] //Atributo que permite que la clase Quest sea serializable y pueda ser mostrada en el Inspector de Unity
public class Quest
{
    public string questName;
    public int moneyRequired;
    public int woodRequired;
    public int meatRequired;

    public bool IsCompleted(int money, int meat, int wood)
    {
        return money >= moneyRequired && meat >= meatRequired && wood >= woodRequired;
    }
}
