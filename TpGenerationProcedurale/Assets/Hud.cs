using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    public static Hud Instance = null;

    public RectTransform heartBar;
    public GameObject heartPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        if (Player.Instance == null)
            return;
        while(Player.Instance.life > heartBar.childCount) {
            AddHearth();
        }
        while (Player.Instance.life < heartBar.childCount) {
            RemoveHearth();
        }
    }

    public void AddHearth()
    {
        GameObject heart = GameObject.Instantiate(heartPrefab);
        heart.transform.SetParent(heartBar);
    }

    public void RemoveHearth()
    {
        if (heartBar.childCount == 0)
            return;
        Transform heart = heartBar.GetChild(0);
        heart.SetParent(null);
        GameObject.Destroy(heart.gameObject);
    }
}
