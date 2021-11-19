using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    public static Hud Instance = null;

    public RectTransform heartBar;
    public GameObject heartPrefab;

    public Slider healthSlider;
    public Text healthText;

    private void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        if (Player.Instance == null)
            return;

        if (healthSlider.value == Player.Instance.life) { return; }

        healthSlider.value = Player.Instance.life;
        healthText.text = healthSlider.value.ToString() + "/" + healthSlider.maxValue.ToString();

        //while (heartBar.childCount > 0 && heartBar.childCount > Player.Instance.life)
        //{
        //    AddHearth();
        //}
        //while (Player.Instance.life < heartBar.childCount) {
        //    RemoveHearth();
        //}
    }

    public void SetSliderValuesToPointsValues(int pointValue)
    {
        healthSlider.maxValue = pointValue;
        healthText.text = healthSlider.value.ToString() + "/" + healthSlider.maxValue.ToString();
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
