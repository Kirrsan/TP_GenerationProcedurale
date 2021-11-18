using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    public bool isStartRoom = false;
	public Vector2Int position = Vector2Int.zero;

	private TilemapGroup _tilemapGroup;
	public enum ROOMDIFFICULTY
    {
		EASY = 0,
		MEDIUM = 1,
		HARD = 2
    }

	public ROOMDIFFICULTY roomDifficulty;

	public static List<Room> allRooms = new List<Room>();

	private int potentialPointToLose = 0;
	private int potentialPointToGain = 0;

    void Awake()
    {
		_tilemapGroup = GetComponentInChildren<TilemapGroup>();
		allRooms.Add(this);
		GetRoomPoints();
	}

	private void OnDestroy()
	{
		allRooms.Remove(this);
	}

	void Start () {
        if(isStartRoom)
        {
            OnEnterRoom();
        }
    }
	
	public void OnEnterRoom()
    {
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        Bounds cameraBounds = GetWorldRoomBounds();
        cameraFollow.SetBounds(cameraBounds);
		Player.Instance.EnterRoom(this);
    }


	public Bounds GetLocalRoomBounds()
    {
		Bounds roomBounds = new Bounds(Vector3.zero, Vector3.zero);
		if (_tilemapGroup == null)
			return roomBounds;

		foreach (STETilemap tilemap in _tilemapGroup.Tilemaps)
		{
			Bounds bounds = tilemap.MapBounds;
			roomBounds.Encapsulate(bounds);
		}
		return roomBounds;
    }

    public Bounds GetWorldRoomBounds()
    {
        Bounds result = GetLocalRoomBounds();
        result.center += transform.position;
        return result;
    }

	public bool Contains(Vector3 position)
	{
		position.z = 0;
		return (GetWorldRoomBounds().Contains(position));
	}

	public Door[] GetAllDoorInRoom()
    {
		return GetComponentsInChildren<Door>();

		/*int childCount = transform.GetChild(0).childCount;
        for (int i = 0; i < childCount; i++)
        {
			if(transform.GetChild(0).GetChild(i).CompareTag("Door"))
			{
				doors.Add(transform.GetChild(0).GetChild(i).GetComponent<Door>());
			}
		}

		return doors;*/
    }

	public void GetRoomPoints()
    {
		GameObject obj = null;

		for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
				obj = transform.GetChild(i).GetChild(j).gameObject;
				if (obj.CompareTag("Enemy"))
				{
					potentialPointToGain += obj.GetComponent<Enemy>().PointToGive;
				}
				else if (obj.CompareTag("PointTrigger"))
				{ 
					PointTrigger trigger = obj.GetComponent<PointTrigger>();
					if(trigger.triggerPointState == PointTrigger.TRIGGER_POINT.ADD)
                    {
						potentialPointToGain += trigger.triggerPointValue;
					}
					else
                    {
						potentialPointToLose += trigger.triggerPointValue;
					}
				}
				else if (obj.CompareTag("PointBlocker"))
				{
					PointBlocker blocker = obj.GetComponent<PointBlocker>();
					if(blocker.takePointFromPlayer)
                    {
						potentialPointToLose += blocker.doorValueIfLocked;
					}
				}
            }
        }
    }

	public int GetPotentialLoss()
    {
		return potentialPointToLose;
    }

	public int GetPotentialPointWin()
    {
		return potentialPointToGain;
    }
}
