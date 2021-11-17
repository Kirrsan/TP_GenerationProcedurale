using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopTile : MonoBehaviour
{
    public enum ITEMTYPE
    {
        ATTACK,
        DEFENSE,
        SPEED
    }

    public ITEMTYPE typeOfItem;

    public GameObject attackObj;
    public GameObject defenseObj;
    public GameObject speedObj;

    public int itemStatMultiplier = 2;
    public int itemCost = 50;

    public Text priceText;

    // Start is called before the first frame update
    void Start()
    {
        switch (typeOfItem)
        {
            case ITEMTYPE.ATTACK:
                attackObj.SetActive(true);
                defenseObj.SetActive(false);
                speedObj.SetActive(false);
                break;
            case ITEMTYPE.DEFENSE:
                attackObj.SetActive(false);
                defenseObj.SetActive(true);
                speedObj.SetActive(false);
                break;
            case ITEMTYPE.SPEED:
                attackObj.SetActive(false);
                defenseObj.SetActive(false);
                speedObj.SetActive(true);
                break;
        }

        priceText.text = itemCost.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Player.Instance == null)
            return;
        if (collision.attachedRigidbody.gameObject != Player.Instance.gameObject)
            return;

        BuyObject();
    }

    private void BuyObject()
    {
        if (!Player.Instance.SpendPointsBlock(itemCost)) return;

        switch (typeOfItem)
        {
            case ITEMTYPE.ATTACK:
                Player.Instance.AddToAttackMultiplier(itemStatMultiplier);
                break;
            case ITEMTYPE.DEFENSE:
                Player.Instance.AddToDefenseMultiplier(itemStatMultiplier);
                break;
            case ITEMTYPE.SPEED:
                Player.Instance.AddToSpeedMultiplier(itemStatMultiplier);
                break;
        }

        Destroy(this.gameObject);
    }
}