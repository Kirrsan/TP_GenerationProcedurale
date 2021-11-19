using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointTrigger : MonoBehaviour
{

	public enum TRIGGER_POINT
	{
		ADD,
		REMOVE,
	}

	public GameObject addSprite;
	public GameObject removeSprite;
	public Text scoreText;

	public TRIGGER_POINT triggerPointState = TRIGGER_POINT.REMOVE;
	public int triggerPointValue;

	public bool isUnique = true;

	
    private void Start()
    {
		string comparisonCharacter = "";
		switch (triggerPointState)
        {
			case TRIGGER_POINT.ADD:
				addSprite.SetActive(true);
				comparisonCharacter = "+";
				break;
			case TRIGGER_POINT.REMOVE:
				removeSprite.SetActive(true);
				comparisonCharacter = "-";
				break;
        }

		scoreText.text = comparisonCharacter + triggerPointValue.ToString();

	}


    private void OnTriggerEnter2D(Collider2D collision)
	{
		if (Player.Instance == null)
			return;
		if(collision.attachedRigidbody.gameObject != Player.Instance.gameObject)
			return;

		if(triggerPointState == TRIGGER_POINT.REMOVE)
        {
			Player.Instance.SpendPoints(triggerPointValue);
        }
		else if(triggerPointState == TRIGGER_POINT.ADD)
        {
			Player.Instance.AddPoints(triggerPointValue);
		}

		if(isUnique)
        {
			Destroy(gameObject);
        }
	}

}
