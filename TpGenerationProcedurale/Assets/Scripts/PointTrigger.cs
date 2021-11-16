using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointTrigger : MonoBehaviour
{

	public enum TRIGGER_POINT
	{
		ADD,
		REMOVE,
	}

	public GameObject addSprite;
	public GameObject removeSprite;

	public TRIGGER_POINT triggerPointState = TRIGGER_POINT.REMOVE;
	public int triggerPointValue;

	public bool isUnique = true;

	
    private void Start()
    {
        switch(triggerPointState)
        {
			case TRIGGER_POINT.ADD:
				addSprite.SetActive(true);
				break;
			case TRIGGER_POINT.REMOVE:
				removeSprite.SetActive(true);
				break;
        }
    }


    private void OnTriggerStay2D(Collider2D collision)
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
