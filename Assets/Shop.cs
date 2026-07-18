using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
public class Shop : MonoBehaviour
{
    [Serializable]
    public class slot
    {
        public GameObject slotObject;
        public string itemName;
    }
    
    [Serializable]
    public class shelf
    {
        public List<slot> slots;
        public TMP_Text priceText;
    }
    [Serializable]
    public class item
    {
        public string name;
        public int price;
        public Sprite icon;
        public GameObject buyEffect;
    }
    public List<item> items;
    public List<shelf> bottomShelves;
    void Start()
    {
        for (int i = 0; i < bottomShelves.Count; i++)
        {
            shelf s = bottomShelves[i];
            for (int j = 0; j < s.slots.Count; j++)
            {
                slot slot = s.slots[j];
                item it = items.Find(x => x.name == slot.itemName);
                if (it != null)
                {
                    slot.slotObject.GetComponent<Image>().sprite = it.icon;
                    s.priceText.text = it.price.ToString();
                    slot.slotObject.GetComponent<Button>().onClick.AddListener(() => BuyItem(it, slot.slotObject));
                
                }
            }
        }
    }

    public void BuyItem(item it, GameObject slotObject)
    {
        if(CurrencyManager.Instance.Buy(it))
        {
            Destroy(slotObject);
        }
    }
}
