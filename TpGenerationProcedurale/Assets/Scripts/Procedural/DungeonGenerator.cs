using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenerator
{
    public struct Room
    {

        public enum RoomType
        {
            Default,
            Start,
            Secret,
            End,
        }
        public Vector2 Position { get; set; }
        public List<Connection> Connections { get; set; }
        public RoomType Type { get; set; }
        public int Difficulty { get; set; }
        public Room(Vector2 pos, RoomType type, int difficulty)
        {
            Position = pos;
            Type = type;
            Difficulty = difficulty;
            Connections = new List<Connection>();
        }
    }
    public struct Connection
    {
        public Room DestinationRoom { get; set; }
        public bool hasLock { get; set; }
        public Connection(Room destination, bool lockRoom)
        {
            DestinationRoom = destination;
            hasLock = lockRoom;
        }
    }
    public class DungeonGenerator : MonoBehaviour
    {
        public static DungeonGenerator Instance;
        [Range(3, 100)]public int nbOfRoomsClamp = 5;
        [Range(0, 9999)] public int difficultyBudget = 10;
        public RectInt maxLevelSize;
        public static void ConnectRoom(Room roomFrom, Room roomTo)
        {
            roomFrom.Connections.Add(new Connection(roomTo, false));
        }
        public static void InterconnectRooms(Room roomFrom, Room roomTo)
        {
            roomFrom.Connections.Add(new Connection(roomTo,false));
            roomTo.Connections.Add(new Connection(roomFrom,false));
        }
        public static bool GenerateDungeonLoop()
        {
            //TODO : Implement Dungeon Generation
            //return true if dungeon is final
            return true;
        }
        void OnDrawGizmosSelected()
        {
            // Draw a semitransparent blue cube at the transforms position
            Gizmos.color = new Color(0, 1, 0, 0.22f);
            Gizmos.DrawCube(new Vector3(maxLevelSize.position.x, maxLevelSize.position.y, 0), new Vector3(maxLevelSize.size.x, maxLevelSize.size.y, float.Epsilon));
        }
        void Awake()
        {
            if (Instance)
                Destroy(Instance.gameObject);
            Instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            int nbOfExtraTries = 0;
            while (!GenerateDungeonLoop())
            {
                nbOfExtraTries++;
            }
#if UNITY_EDITOR
            Debug.Log("Number of extra tries needed to generate dungeon : " + nbOfExtraTries);
#endif
        }

        // Update is called once per frame
        void Update()
        {

        }

    }

}
