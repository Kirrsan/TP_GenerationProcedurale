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
            Combat,
            BonusPoint,
            Danger,
            Merchant,
            Sudoku,
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
        public enum Orientation
        {
            North = 0,
            East = 1,
            South = 2,
            West = 3,
            NULL = int.MinValue
        }
        public Orientation Direction { get; set; }
        public Room DestinationRoom { get; set; }
        public Room OriginRoom { get; set; }
        public bool hasLock { get; set; }
        public Connection(Orientation direction, Room origin, Room destination, bool lockRoom)
        {
            Direction = direction;
            OriginRoom = origin;
            DestinationRoom = destination;
            hasLock = lockRoom;
        }
    }
    public class DungeonGenerator : MonoBehaviour
    {
        public static DungeonGenerator Instance { get; private set; }
        [Range(3, 2500)] public int nbOfRoomsClamp = 5;
        [Range(0, 9999)] public int difficultyBudget = 10;
        [Header("System")] public int extraTriesClamp = 10;
        public List<GameObject> roomPrefab = new List<GameObject>();
        public Vector2 ROOMSIZE = new Vector2(10f, 10f);
        public RectInt maxLevelSize;
        static List<Room?> rooms = new List<Room?>();
        static List<Connection?> connections = new List<Connection?>();
        static Connection.Orientation RandomOrientation
        {
            get => (Connection.Orientation)Random.Range(0, 4);
        }
        static bool OrientationIsValid(Room roomFrom, Connection.Orientation Orientation)
        {
            Vector2 theoreticalPosition = Vector2.zero;
            switch (Orientation)
            {
                case Connection.Orientation.North:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x, roomFrom.Position.y + Instance.ROOMSIZE.y);
                    }
                    break;
                case Connection.Orientation.South:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x, roomFrom.Position.y - Instance.ROOMSIZE.y);
                    }
                    break;
                case Connection.Orientation.West:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x - Instance.ROOMSIZE.x, roomFrom.Position.y);
                    }
                    break;
                case Connection.Orientation.East:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x + Instance.ROOMSIZE.x, roomFrom.Position.y);
                    }
                    break;
                default:
                    throw new System.NotImplementedException("Orientation not supported");
            }
            return rooms.Find(r => r.Value.Position == theoreticalPosition) == null;
        }
        static Connection.Orientation GenerateValidOrientation(Room roomFrom)
        {
            int safetyNet = 0;
            Connection.Orientation Orientation = DungeonGenerator.RandomOrientation;
            while (!OrientationIsValid(roomFrom, Orientation) && safetyNet < 6)
            {
                if (Orientation == Connection.Orientation.West)
                    Orientation = Connection.Orientation.North;
                else
                    Orientation++;
                safetyNet++;
                continue;
            }
            return Orientation;
        }
        static Room SpawnStartRoom(Vector2 startPos)
        {
            return new Room(startPos, Room.RoomType.Start, 0);
        }
        static int NbOfSpawn(Room.RoomType type)
        {
            return rooms.FindAll(r => r.Value.Type == type).Count;
        }
        static Connection BuildAdjacentRoom(Room roomFrom, Connection.Orientation Orientation, Room.RoomType type = Room.RoomType.Default, bool hasLock = false)
        {
            switch (Orientation)
            {
                case Connection.Orientation.North:
                    {
                        Connection connection = new Connection(Orientation, roomFrom, new Room(new Vector2(roomFrom.Position.x, roomFrom.Position.y + Instance.ROOMSIZE.y), type, 0), hasLock);
                        roomFrom.Connections.Add(connection);
                        return connection;
                    }
                case Connection.Orientation.South:
                    {
                        Connection connection = new Connection(Orientation, roomFrom, new Room(new Vector2(roomFrom.Position.x, roomFrom.Position.y - Instance.ROOMSIZE.y), type, 0), hasLock);
                        roomFrom.Connections.Add(connection);
                        return connection;
                    }
                case Connection.Orientation.West:
                    {
                        Connection connection = new Connection(Orientation, roomFrom, new Room(new Vector2(roomFrom.Position.x - Instance.ROOMSIZE.x, roomFrom.Position.y), type, 0), hasLock);
                        roomFrom.Connections.Add(connection);
                        return connection;
                    }
                case Connection.Orientation.East:
                    {
                        Connection connection = new Connection(Orientation, roomFrom, new Room(new Vector2(roomFrom.Position.x + Instance.ROOMSIZE.x, roomFrom.Position.y), type, 0), hasLock);
                        roomFrom.Connections.Add(connection);
                        return connection;
                    }
                default:
                    throw new System.NotImplementedException("Orientation not supported");
            }
        }
        static bool CheckAdjacentRoom(Room roomToProbe)
        {
            return rooms.Find(r =>
            r.Value.Position == roomToProbe.Position + new Vector2(Instance.ROOMSIZE.x, 0) ||
            r.Value.Position == roomToProbe.Position - new Vector2(Instance.ROOMSIZE.x, 0) ||
            r.Value.Position == roomToProbe.Position + new Vector2(0, Instance.ROOMSIZE.y) ||
            r.Value.Position == roomToProbe.Position - new Vector2(0, Instance.ROOMSIZE.y)) != null;
        }
        static bool CheckAdjacentRoom(Room roomToProbe, Room.RoomType type)
        {
            return rooms.Find(r =>
            (r.Value.Position == roomToProbe.Position + new Vector2(Instance.ROOMSIZE.x, 0) && r.Value.Type == type) ||
            (r.Value.Position == roomToProbe.Position - new Vector2(Instance.ROOMSIZE.x, 0) && r.Value.Type == type) ||
            (r.Value.Position == roomToProbe.Position + new Vector2(0, Instance.ROOMSIZE.y) && r.Value.Type == type) ||
            (r.Value.Position == roomToProbe.Position - new Vector2(0, Instance.ROOMSIZE.y) && r.Value.Type == type)) != null;
        }
        static bool BuildSecondaryPath(ref Room roomStart, ref Room roomTarget, int roomBudget)
        {
            //check if the budget is big enough
            if (//Maximum diagonal distance achievable with the allocated budget
                Vector2.Distance(new Room(Vector2.zero, Room.RoomType.NULL, 0).Position, new Room(Vector2.one * Instance.ROOMSIZE, Room.RoomType.NULL, 0).Position) * roomBudget
                <
                //Diagonal distance between the begining of the room and the target room
                Vector2.Distance(roomStart.Position, roomTarget.Position) || roomBudget <= 0) return false;
            //Begin algo
            int remainingBudget = roomBudget;
            Room? previousRoom;
            Room? latestSpawnedRoom = roomStart;
            bool latestSpawnedRoomIsYAxis = false;
            float yDistance = roomTarget.Position.y - roomStart.Position.y;
            //1 is north, 0 is equal, -1 is south
            int targetYAxis = 0;
            float xDistance = roomTarget.Position.x - roomStart.Position.x;
            //1 is East, 0 is equal, -1 is West
            int targetXAxis = 0;
            int safetyNet = remainingBudget * 2;
            while (remainingBudget > 0 && safetyNet > 0)
            {
                //update relevant values
                safetyNet--;
                previousRoom = latestSpawnedRoom;
                yDistance = roomTarget.Position.y - previousRoom.Value.Position.y;
                xDistance = roomTarget.Position.x - previousRoom.Value.Position.x;
                if (yDistance > 0)
                    targetYAxis = (int)Mathf.Clamp(Mathf.Ceil(yDistance), -1, 1);
                else if (yDistance < 0)
                    targetYAxis = (int)Mathf.Clamp(Mathf.Floor(yDistance), -1, 1);
                if (xDistance > 0)
                    targetXAxis = (int)Mathf.Clamp(Mathf.Ceil(xDistance), -1, 1);
                else if (xDistance < 0)
                    targetXAxis = (int)Mathf.Clamp(Mathf.Floor(xDistance), -1, 1);
                if (CheckAdjacentRoom(latestSpawnedRoom.Value, Room.RoomType.End)) break;
                if (latestSpawnedRoomIsYAxis)
                {
                    switch (targetXAxis)
                    {
                        case 0:
                            continue;
                        case 1:
                            {
                                if(OrientationIsValid(previousRoom.Value, Connection.Orientation.East))
                                {
                                    Connection con = BuildAdjacentRoom(latestSpawnedRoom.Value, Connection.Orientation.East);
                                    connections.Add(con);
                                    rooms.Add(con.DestinationRoom);
                                    latestSpawnedRoom = con.DestinationRoom;
                                }
                                break;
                            }
                        case -1:
                            {
                                if (OrientationIsValid(previousRoom.Value, Connection.Orientation.West))
                                {
                                    Connection con = BuildAdjacentRoom(latestSpawnedRoom.Value, Connection.Orientation.West);
                                    connections.Add(con);
                                    rooms.Add(con.DestinationRoom);
                                    latestSpawnedRoom = con.DestinationRoom;
                                }
                                break;
                            }
                    }
                    latestSpawnedRoomIsYAxis = false;
                    //Spawn room on x axis
                }
                else
                {
                    switch (targetYAxis)
                    {
                        case 0:
                            continue;
                        case 1:
                            {
                                if (OrientationIsValid(previousRoom.Value, Connection.Orientation.North))
                                {
                                    Connection con = BuildAdjacentRoom(latestSpawnedRoom.Value, Connection.Orientation.North);
                                    connections.Add(con);
                                    rooms.Add(con.DestinationRoom);
                                    latestSpawnedRoom = con.DestinationRoom;
                                }
                                break;
                            }
                        case -1:
                            {
                                if (OrientationIsValid(previousRoom.Value, Connection.Orientation.South))
                                {
                                    Connection con = BuildAdjacentRoom(latestSpawnedRoom.Value, Connection.Orientation.South);
                                    connections.Add(con);
                                    rooms.Add(con.DestinationRoom);
                                    latestSpawnedRoom = con.DestinationRoom;
                                }
                                break;
                            }
                    }
                    //Spawn room on y axis
                    latestSpawnedRoomIsYAxis = true;
                }
                remainingBudget--;
            }
            return true;
        }
        static bool GenerateDungeonLoop()
        {
            rooms.Clear();
            connections.Clear();
            Vector2 startPosition = Vector2.zero;
            int roomsSpawned = 0;
            int mainRouteBudget = Instance.nbOfRoomsClamp;
            Connection.Orientation latestOrientation = Connection.Orientation.NULL;
            Room? previousRoom;
            Room? lastestSpawnedRoom = null;
            //PREREQUISITES
            bool startingRoomSpawned = false;
            bool endRoomSpawned = false;

            //Main Route Loop
            while (roomsSpawned < mainRouteBudget)
            {
                //Update relevant values
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
                    latestOrientation = GenerateValidOrientation(previousRoom.Value);
                    Connection con = BuildAdjacentRoom(previousRoom.Value, latestOrientation, Room.RoomType.End);
                    connections.Add(con);
                    lastestSpawnedRoom = con.DestinationRoom;
                    rooms.Add(lastestSpawnedRoom.Value);
                    endRoomSpawned = true;
                    break;
                }
                {
                    //TODO : Track Orientation to have even room distribution on the grid space
                    latestOrientation = GenerateValidOrientation(previousRoom.Value);
                    Connection con = BuildAdjacentRoom(previousRoom.Value, latestOrientation);
                    connections.Add(con);
                    lastestSpawnedRoom = con.DestinationRoom;
                    rooms.Add(lastestSpawnedRoom.Value);
                    roomsSpawned++;
                }
            }

            //TODO : Use AltRoute generation
            //BuildSecondaryPath(ref startRoom, ref endRoom, 50);

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
            int nbOfExtraTries = 0;
            while (nbOfExtraTries < extraTriesClamp && !GenerateDungeonLoop())
                nbOfExtraTries++;
#if UNITY_EDITOR
            Debug.Log("Number of extra tries needed to generate dungeon : " + nbOfExtraTries);
            string mapstring = "Map :" + "\n";
            foreach (Room room in rooms)
            {
                mapstring += "I am a " + room.Type + " room at " + room.Position + ", Difficulty : " + room.Difficulty + "\n";
                GameObject roomGO = Instantiate(roomPrefab[(int)room.Type]);
                roomGO.transform.parent = transform;
                roomGO.transform.position = room.Position;
            }
                
            Debug.Log(mapstring);  
#endif
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
