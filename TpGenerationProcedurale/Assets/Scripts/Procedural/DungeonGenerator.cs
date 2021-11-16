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
            NULL
        }
        public Vector2 Position { get; set; }
        public List<Connection> Connections { get; set; }
        public RoomType Type { get; set; }
        public int Difficulty { get; set; }
        public Room(RoomType type = RoomType.NULL)
        {
            Position = new Vector2(float.Epsilon, float.Epsilon);
            Type = RoomType.NULL;
            Difficulty = int.MinValue;
            Connections = null;
        }
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
        public enum Direction
        {
            North = 0,
            East = 1,
            South = 2,
            West = 3,
            NULL = int.MinValue
        }
        public Room DestinationRoom { get; set; }
        public bool hasLock { get; set; }
        public Connection(Direction direction, Room destination, bool lockRoom)
        {
            DestinationRoom = destination;
            hasLock = lockRoom;
        }
    }
    public class DungeonGenerator : MonoBehaviour
    {
        public static DungeonGenerator Instance;
        [Range(3, 100)] public int nbOfRoomsClamp = 5;
        [Range(0, 9999)] public int difficultyBudget = 10;
        [Header("System")] public int extraTriesClamp = 10;
        public Vector2 ROOMSIZE = new Vector2(100f, 100f);
        public RectInt maxLevelSize;
        static List<Room?> rooms = new List<Room?>();
        static List<Connection?> connections = new List<Connection?>();

        static Connection.Direction RandomDirection
        {
            get => (Connection.Direction)Random.Range(0, 4);
        }
        static bool DirectionIsValid(Room roomFrom, Connection.Direction direction)
        {
            Vector2 theoreticalPosition = Vector2.zero;
            switch (direction)
            {
                case Connection.Direction.North:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x, roomFrom.Position.y + Instance.ROOMSIZE.y);
                    }
                    break;
                case Connection.Direction.South:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x, roomFrom.Position.y - Instance.ROOMSIZE.y);
                    }
                    break;
                case Connection.Direction.West:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x - Instance.ROOMSIZE.x, roomFrom.Position.y);
                    }
                    break;
                case Connection.Direction.East:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x + Instance.ROOMSIZE.x, roomFrom.Position.y);
                    }
                    break;
                default:
                    throw new System.NotImplementedException("Direction not supported");
            }
            return rooms.Find(r => r.Value.Position == theoreticalPosition) == null;
        }
        static Connection.Direction GenerateValidDirection(Room roomFrom)
        {
            int safetyNet = 0;
            Connection.Direction direction = DungeonGenerator.RandomDirection;
            while(!DirectionIsValid(roomFrom, direction) && safetyNet < 6)
            {
                if(direction == Connection.Direction.West)
                    direction = Connection.Direction.North;
                else
                    direction++;
                safetyNet++;
                continue;
            }
            return direction;
        }
        static Room SpawnStartRoom(Vector2 startPos)
        {
            return new Room(startPos, Room.RoomType.Start, 0);
        }
        static Connection BuildAdjacentRoom(Room roomFrom, Connection.Direction direction, Room.RoomType type = Room.RoomType.Default, bool hasLock = false)
        {
            switch (direction)
            {
                case Connection.Direction.North:
                    {

                        Connection connection = new Connection(direction, new Room(new Vector2(roomFrom.Position.x, roomFrom.Position.y + Instance.ROOMSIZE.y), type, 0), hasLock);
                        roomFrom.Connections.Add(connection);
                        return connection;
                    }
                case Connection.Direction.South:
                    {
                        Connection connection = new Connection(direction, new Room(new Vector2(roomFrom.Position.x, roomFrom.Position.y - Instance.ROOMSIZE.y), type, 0), hasLock);
                        roomFrom.Connections.Add(connection);
                        return connection;
                    }
                case Connection.Direction.West:
                    {
                        Connection connection = new Connection(direction, new Room(new Vector2(roomFrom.Position.x - Instance.ROOMSIZE.x, roomFrom.Position.y), type, 0), hasLock);
                        roomFrom.Connections.Add(connection);
                        return connection;
                    }
                case Connection.Direction.East:
                    {
                        Connection connection = new Connection(direction, new Room(new Vector2(roomFrom.Position.x + Instance.ROOMSIZE.x, roomFrom.Position.y), type, 0), hasLock);
                        roomFrom.Connections.Add(connection);
                        return connection;
                    }
                default:
                    throw new System.NotImplementedException("Direction not supported");
            }
        }
        public static bool GenerateDungeonLoop()
        {
            rooms.Clear();
            connections.Clear();
            Vector2 startPosition = Vector2.zero;
            int roomsSpawned = 0;
            Connection.Direction latestDirection = Connection.Direction.NULL;
            Room? previousRoom;
            Room? lastestSpawnedRoom = null;
            //PREREQUISITES
            bool startingRoomSpawned = false;
            bool endRoomSpawned = false;

            //TODO : Implement Dungeon Generation
            while (roomsSpawned < Instance.nbOfRoomsClamp)
            {
                previousRoom = lastestSpawnedRoom;

                if (!startingRoomSpawned)
                {
                    lastestSpawnedRoom = SpawnStartRoom(startPosition);
                    rooms.Add(lastestSpawnedRoom);
                    startingRoomSpawned = true;
                    roomsSpawned++;
                    continue;
                }
                if (roomsSpawned + 1 == Instance.nbOfRoomsClamp)
                {
                    latestDirection = GenerateValidDirection(previousRoom.Value);
                    Connection con = BuildAdjacentRoom(previousRoom.Value, latestDirection, Room.RoomType.End);
                    connections.Add(con);
                    lastestSpawnedRoom = con.DestinationRoom;
                    rooms.Add(lastestSpawnedRoom.Value);
                    endRoomSpawned = true;
                    break;
                }
                {
                    //TODO : Track Direction to have even room distribution on the grid space
                    latestDirection = GenerateValidDirection(previousRoom.Value);
                    Connection con = BuildAdjacentRoom(previousRoom.Value, latestDirection);
                    connections.Add(con);
                    lastestSpawnedRoom = con.DestinationRoom;
                    rooms.Add(lastestSpawnedRoom.Value);
                    roomsSpawned++;
                }
            }
            //return true if all prerequisites are filled
            if (startingRoomSpawned && endRoomSpawned)
                return true;
            else
                return false;
            throw new System.NotImplementedException("You did the oopsie");
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
            for(int i = 0; i < 5 ; i++)
            {
                int nbOfExtraTries = 0;
                while (nbOfExtraTries < extraTriesClamp && !GenerateDungeonLoop())
                    nbOfExtraTries++;
#if UNITY_EDITOR
                Debug.Log("Number of extra tries needed to generate dungeon : " + nbOfExtraTries);
                string mapstring = "Map :" + "\n";
                foreach (Room room in rooms)
                    mapstring += "I am a " + room.Type + " room at " + room.Position + ", Difficulty : " + room.Difficulty + "\n";
                Debug.Log(mapstring);
#endif
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

    }

}
