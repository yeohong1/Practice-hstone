using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//캐릭터 속성
public class Item
{
    public string name;
    public int sttack;
    public int health;
    public Sprite Sprite;
    public float percent;
    
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Object/ItemSO")]//캐릭터 속성 관리를 편하게하기 위함
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
