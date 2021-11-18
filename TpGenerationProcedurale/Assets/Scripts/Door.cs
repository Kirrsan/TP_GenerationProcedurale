using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : MonoBehaviour {

    public enum STATE {
        OPEN = 0,
        CLOSED = 1,
        WALL = 2,
        SECRET = 3,
    }

    public enum POINT_STATE
    {
        INFERIOR = 0,
        INFERIOR_EQUAL = 1,
        EQUAL = 2,
        SUPERIOR = 3,
        SUPERIOR_EQUAL = 4,
    }

    public const string PLAYER_NAME = "Player";

    Utils.ORIENTATION _orientation = Utils.ORIENTATION.NONE;
	public Utils.ORIENTATION Orientation { get { return _orientation; } }

	public STATE _state = STATE.OPEN;
    public POINT_STATE _pointState = POINT_STATE.INFERIOR;

    public STATE State { get { return _state; } }
	public GameObject closedGo = null;
    public GameObject openGo = null;
    public GameObject wallGo = null;
    public GameObject secretGo = null;

    public int doorValueIfLocked = 0;
    public bool takePointFromPlayer = true;

	private Room _room = null;

	public void Awake()
	{
		_room = GetComponentInParent<Room>();
        Bounds roomBounds = _room.GetWorldRoomBounds();
        float ratio = roomBounds.size.x / roomBounds.size.y;
        Vector2 dir = transform.position - (_room.transform.position + roomBounds.center);
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y) * ratio)
        {
            _orientation = dir.x > 0 ? Utils.ORIENTATION.EAST : Utils.ORIENTATION.WEST;
        }
        else
        {
            _orientation = dir.y > 0 ? Utils.ORIENTATION.NORTH : Utils.ORIENTATION.SOUTH;
        }
        transform.rotation = Quaternion.Euler(0, 0, -Utils.OrientationToAngle(_orientation));
    }

	public void Start()
    {
		if(closedGo.gameObject.activeSelf)
		{
			SetState(STATE.CLOSED);
		} else if (openGo.gameObject.activeSelf)
		{
			SetState(STATE.OPEN);
		} else if (wallGo.gameObject.activeSelf)
		{
			SetState(STATE.WALL);
		} else if (secretGo.gameObject.activeSelf)
		{
			SetState(STATE.SECRET);
		}
	}

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent != Player.Instance.gameObject.transform)
            return;

        switch (_state) {
            case STATE.CLOSED:
                if (CheckPointsToOpen(Player.Instance.GetPoint()))
                {
                    if(takePointFromPlayer)
                    {
                        Player.Instance.SpendPointsToMin1Hp(doorValueIfLocked);
                    }
                    SetState(STATE.OPEN);
					Room nextRoom = GetNextRoom();
					if(nextRoom)
					{
						Door[] doors = nextRoom.GetComponentsInChildren<Door>(true);
						foreach(Door door in doors)
						{
							if (_orientation == Utils.OppositeOrientation(door.Orientation) && door._state == STATE.CLOSED)
							{
								door.SetState(STATE.OPEN);
							}
						}
					}
				}                
                break;
        }
    }

	private Room GetNextRoom()
	{
		Vector2Int dir = Utils.OrientationToDir(_orientation);
		Room nextRoom = Room.allRooms.Find(x => x.position == _room.position + dir);
		return nextRoom;
	}

    public bool CheckPointsToOpen(int playerPoints)
    {
        switch (_pointState)
        {
            case POINT_STATE.INFERIOR:
                if (playerPoints < doorValueIfLocked)
                {
                    return true;
                }
                break;
            case POINT_STATE.INFERIOR_EQUAL:
                if (playerPoints <= doorValueIfLocked)
                {
                    return true;
                }
                break;
            case POINT_STATE.EQUAL:
                if (playerPoints == doorValueIfLocked)
                {
                    return true;
                }
                break;
            case POINT_STATE.SUPERIOR:
                if (playerPoints > doorValueIfLocked)
                {
                    return true;
                }
                break;
            case POINT_STATE.SUPERIOR_EQUAL:
                if (playerPoints >= doorValueIfLocked)
                {
                    return true;
                }
                break;
            default:
                return false;
        }
        return false;
    }

    public void SetState(STATE state)
    {
        if (closedGo) { closedGo.SetActive(false); }
        if (openGo) { openGo.SetActive(false); }
        if (wallGo) { wallGo.SetActive(false); }
        if (secretGo) { secretGo.SetActive(false); }
        _state = state;
        switch(_state)
        {
            case STATE.CLOSED:
                if (closedGo) { closedGo.SetActive(true); }
                break;
            case STATE.OPEN:
                if (openGo) { openGo.SetActive(true); }
                break;
            case STATE.WALL:
                if (wallGo) { wallGo.SetActive(true); }
                break;
            case STATE.SECRET:
                if (secretGo) { secretGo.SetActive(true); }
                break;
        }
    }

    public void SetState(STATE state, int DoorValue)
    {
        if (closedGo) { closedGo.SetActive(false); }
        if (openGo) { openGo.SetActive(false); }
        if (wallGo) { wallGo.SetActive(false); }
        if (secretGo) { secretGo.SetActive(false); }
        _state = state;
        switch (_state)
        {
            case STATE.CLOSED:
                if (closedGo) 
                { 
                    closedGo.SetActive(true);
                    doorValueIfLocked = DoorValue;
                }
                break;
            case STATE.OPEN:
                if (openGo) { openGo.SetActive(true); }
                break;
            case STATE.WALL:
                if (wallGo) { wallGo.SetActive(true); }
                break;
            case STATE.SECRET:
                if (secretGo) { secretGo.SetActive(true); }
                break;
        }
    }

}
