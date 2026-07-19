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
    [Header("Free Drug Offer")]
    [SerializeField] private Button freeDrugButton;
    [SerializeField] private TMP_Text freeDrugStatusText;

    private bool _freeDrugClaimed;
    private TripController _tripController;

    void OnEnable()
    {
        ResetFreeDrugOffer();
    }

    void OnDisable()
    {
        if (freeDrugButton != null)
        {
            freeDrugButton.onClick.RemoveListener(HandleFreeDrugClicked);
        }
    }

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

        _tripController = FindObjectOfType<TripController>();
        ResetFreeDrugOffer();
    }

    public void BuyItem(item it, GameObject slotObject)
    {
        if (CurrencyManager.Instance.Buy(it))
        {
            Destroy(slotObject);
        }
    }

    private void ResetFreeDrugOffer()
    {
        _freeDrugClaimed = false;

        if (freeDrugButton != null)
        {
            freeDrugButton.onClick.RemoveListener(HandleFreeDrugClicked);
            freeDrugButton.onClick.AddListener(HandleFreeDrugClicked);
            freeDrugButton.interactable = true;
        }

        if (freeDrugStatusText != null)
        {
            freeDrugStatusText.text = "Free";
        }
    }

    private void HandleFreeDrugClicked()
    {
        if (_freeDrugClaimed)
        {
            return;
        }

        IReadOnlyList<DrugRegistry.DrugEntry> entries = DrugRegistry.GetEntries();
        if (entries == null || entries.Count == 0)
        {
            Debug.LogWarning("Shop: no drugs are available in the DrugRegistry.", this);
            return;
        }

        DrugRegistry.DrugEntry selectedEntry = entries[UnityEngine.Random.Range(0, entries.Count)];
        if (selectedEntry == null || selectedEntry.Drug == null)
        {
            Debug.LogWarning("Shop: selected drug entry was empty.", this);
            return;
        }

        if (_tripController == null)
        {
            _tripController = FindObjectOfType<TripController>();
        }

        if (_tripController != null)
        {
            _tripController.ApplyDrug(selectedEntry.Drug);
            Debug.Log($"Applied free drug: {selectedEntry.Name}", this);
        }
        else
        {
            Debug.LogWarning("Shop: no TripController was found, so the free drug could not be applied.", this);
            return;
        }

        _freeDrugClaimed = true;

        if (freeDrugButton != null)
        {
            freeDrugButton.interactable = false;
        }

        if (freeDrugStatusText != null)
        {
            freeDrugStatusText.text = "Taken";
        }
    }
}
