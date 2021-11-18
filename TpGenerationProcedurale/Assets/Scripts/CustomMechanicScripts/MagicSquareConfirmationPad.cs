using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSquareConfirmationPad : MonoBehaviour
{

    public MagicSquareGeneration magicSquareScript;
    public Room currentRoom;

    private int _pointToHaveToConfirmSquare = -1;
    private bool _hasOpenedDoor = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Player.Instance == null)
            return;
        if (collision.attachedRigidbody.gameObject != Player.Instance.gameObject)
            return;

        if(_pointToHaveToConfirmSquare == -1)
        {
            _pointToHaveToConfirmSquare = magicSquareScript.GetPointsToHave();
        }

        if (_pointToHaveToConfirmSquare == Player.Instance.GetPoint())
        {
            //secret room opening
            Door[] doorList = currentRoom.GetAllDoorInRoom();
            foreach (var door in doorList)
            {
                if(door.State == Door.STATE.SECRET)
                {
                    door.SetState(Door.STATE.OPEN);
                    _hasOpenedDoor = true;
                }
            }

        }
    }
}
