using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    public bool isStartRoom = false;
	public Vector2Int position = Vector2Int.zero;

	private TilemapGroup _tilemapGroup;

	public static List<Room> allRooms = new List<Room>();

    void Awake()
    {
		_tilemapGroup = GetComponentInChildren<TilemapGroup>();
		allRooms.Add(this);
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

	public List<Door> GetAllDoorInRoom()
    {
		List<Door> doors = new List<Door>();
		int childCount = transform.GetChild(0).childCount;
        for (int i = 0; i < childCount; i++)
        {
			if(transform.GetChild(0).GetChild(i).CompareTag("Door"))
			{
				doors.Add(transform.GetChild(0).GetChild(i).GetComponent<Door>());
			}
		}

		return doors;
    }
}
