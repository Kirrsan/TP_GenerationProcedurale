﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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
        ANY = 5
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
    public Text scoreText;

    private int doorValueIfLocked = 0;
    public bool takePointFromPlayer = true;

	private Room _room = null;

    private bool isPrimaryPath = false;

	public void Awake()
	{
		_room = GetComponentInParent<Room>();
        Bounds roomBounds = _room.GetLocalRoomBounds();
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

	public Room GetNextRoom()
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
            case POINT_STATE.ANY:
                return true;
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
                if (closedGo) 
                { 
                    closedGo.SetActive(true); 
                }
                break;
            case STATE.OPEN:
                if (openGo) 
                { 
                    openGo.SetActive(true);
                    scoreText.gameObject.SetActive(false);
                }
                break;
            case STATE.WALL:
                if (wallGo) 
                { 
                    wallGo.SetActive(true);
                    scoreText.gameObject.SetActive(false);
                }
                break;
            case STATE.SECRET:
                if (secretGo) 
                { 
                    secretGo.SetActive(true);
                    scoreText.gameObject.SetActive(false);
                }
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

    public void SetIsPrimaryPath()
    {
        isPrimaryPath = true;
    }

    public bool GetIsPrimarypath()
    {
        return isPrimaryPath;
    }

    public void SetDoorCostIfPrimary(bool hadPrimaryDoorBefore)
    {
        int roomIndex = Room.allRooms.FindIndex(r => r.position == _room.position);
        int index = roomIndex;


        int possibleLoss = 0;
        int possibleGain = 0;

        bool canLoop = true;

        Door[] roomDoorList;

        while(index >= 0 && canLoop)
        {
            possibleLoss += Room.allRooms[index].GetPotentialLoss();
            possibleGain += Room.allRooms[index].GetPotentialPointWin();
            --index;

            if (hadPrimaryDoorBefore)
            {
                roomDoorList = Room.allRooms[index].GetAllDoorInRoom();
                foreach (Door door in roomDoorList)
                {
                    if(door._state == STATE.CLOSED && door.GetIsPrimarypath())
                    {
                        canLoop = false;
                    }
                }
            }
        }
        if(possibleGain - possibleLoss - 1 > 0)
        {
            doorValueIfLocked = possibleGain - possibleLoss - 1;
        }
        else
        {
            doorValueIfLocked = 0;
        }
        _pointState = POINT_STATE.ANY;


        string comparisonCharacter = "";
        switch (_pointState)
        {
            case POINT_STATE.INFERIOR:
                comparisonCharacter = "<";
                break;
            case POINT_STATE.INFERIOR_EQUAL:
                comparisonCharacter = "<=";
                break;
            case POINT_STATE.EQUAL:
                comparisonCharacter = "=";
                break;
            case POINT_STATE.SUPERIOR:
                comparisonCharacter = ">";
                break;
            case POINT_STATE.SUPERIOR_EQUAL:
                comparisonCharacter = ">=";
                break;
        }

        scoreText.text = comparisonCharacter + doorValueIfLocked.ToString();
    }

    public void SetDoorCostIfSecondaryPath(List<DungeonGenerator.RoomNode?> roomNodes)
    {
        int possibleLoss = 0;
        int possibleGain = 0;

        Room currentRoom = GetNextRoom();
        if(currentRoom == null)
        {
            SetState(STATE.WALL);
            return;
        }
        DungeonGenerator.RoomNode OriginalNode = roomNodes.Find(r => r.Value.Position == currentRoom.position).Value;


        possibleLoss += currentRoom.GetPotentialLoss();
        possibleGain += currentRoom.GetPotentialPointWin();
        for (int i = 0; i < OriginalNode.Connections.Count; i++)
        {
            RecursiveNodeCheck(OriginalNode.Connections[i].Value.DestinationRoom, OriginalNode, ref possibleLoss, ref possibleGain);
        }
        if ((possibleGain - possibleLoss) / 2 > 0)
        {
            doorValueIfLocked = (possibleGain - possibleLoss) /2;
        }
        else
        {
            doorValueIfLocked = 0;
        }
        _pointState = POINT_STATE.SUPERIOR_EQUAL;

        string comparisonCharacter = "";
        switch (_pointState)
        {
            case POINT_STATE.INFERIOR:
                comparisonCharacter = "<";
                break;
            case POINT_STATE.INFERIOR_EQUAL:
                comparisonCharacter = "<=";
                break;
            case POINT_STATE.EQUAL:
                comparisonCharacter = "=";
                break;
            case POINT_STATE.SUPERIOR:
                comparisonCharacter = ">";
                break;
            case POINT_STATE.SUPERIOR_EQUAL:
                comparisonCharacter = ">=";
                break;
        }

        scoreText.text = comparisonCharacter + doorValueIfLocked.ToString();
    }

    private void RecursiveNodeCheck(DungeonGenerator.RoomNode node, DungeonGenerator.RoomNode lastNode, ref int possibleLoss, ref int possibleGain)
    {
        Room currentRoom;
        for (int i = 0; i < node.Connections.Count; i++)
        {
            if (node.Connections[i].Value.DestinationRoom.Position == lastNode.Position)
            {
                continue;
            }

            if(node.Connections[i].Value.HasLock)
            {
                continue;
            }

            currentRoom = Room.allRooms.Find(r => r.position == node.Position);
            possibleLoss += currentRoom.GetPotentialLoss();
            possibleGain += currentRoom.GetPotentialPointWin();
            RecursiveNodeCheck(node.Connections[i].Value.DestinationRoom, node, ref possibleLoss, ref possibleGain);
        }
    }

    public int GetDoorValueIfLocked()
    {
        return doorValueIfLocked;
    }

    public void SetDoorValue(int newValue)
    {
        doorValueIfLocked = newValue;
    }

}
