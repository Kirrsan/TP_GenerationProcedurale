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
