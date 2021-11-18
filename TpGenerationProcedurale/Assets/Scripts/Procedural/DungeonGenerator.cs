using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenerator
{
    public struct RoomNode
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
        public Vector2Int Position { get; set; }
        public List<ConnectionNode?> Connections { get; set; }
        public RoomType Type { get; set; }
        public int Difficulty { get; set; }
        public bool IsPrimary { get; set; }
        public RoomNode(RoomType type = RoomType.NULL)
        {
            Position = new Vector2Int(int.MinValue, int.MinValue);
            Type = RoomType.NULL;
            Difficulty = int.MinValue;
            Connections = null;
            IsPrimary = false;
        }
        public RoomNode(Vector2Int pos, RoomType type, int difficulty, bool isPrimary = false)
        {
            Position = pos;
            Type = type;
            Difficulty = difficulty;
            Connections = new List<ConnectionNode?>();
            IsPrimary = isPrimary;
        }
    }
    public struct ConnectionNode
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
        public RoomNode DestinationRoom { get; set; }
        public RoomNode OriginRoom { get; set; }
        public int OpeningCost { get; set; }
        public bool HasLock { get; set; }
        public bool IsSecret { get; set; }
        public ConnectionNode(Orientation direction, RoomNode origin, RoomNode destination, bool locked, int openingCost, bool secretRoom = false)
        {
            Direction = direction;
            OriginRoom = origin;
            DestinationRoom = destination;
            HasLock = locked;
            IsSecret = secretRoom;
            OpeningCost = openingCost;
        }
        public ConnectionNode(Orientation direction, RoomNode origin, RoomNode destination, bool secretRoom = false)
        {
            Direction = direction;
            OriginRoom = origin;
            DestinationRoom = destination;
            HasLock = false;
            IsSecret = secretRoom;
            OpeningCost = 0;
        }

        
    }
    public class DungeonGenerator : MonoBehaviour
    {
        public static DungeonGenerator Instance { get; private set; }
        [Range(3, 2500)] public int nbOfRoomsClamp = 5;
        [Range(0, 9999)] public int difficultyBudget = 10;
        [Header("System")] public int extraTriesClamp = 10;
        public List<GameObject> roomPrefab = new List<GameObject>();
        public Vector2Int ROOMSIZE = new Vector2Int(11, 9);
        public RectInt maxLevelSize;
        public static List<RoomNode?> rooms = new List<RoomNode?>();
        public static List<ConnectionNode?> connections = new List<ConnectionNode?>();
        static ConnectionNode.Orientation RandomOrientation
        {
            get => (ConnectionNode.Orientation)Random.Range(0, 4);
        }
        static bool OrientationIsValid(RoomNode roomFrom, ConnectionNode.Orientation Orientation)
        {
            Vector2 theoreticalPosition = Vector2.zero;
            switch (Orientation)
            {
                case ConnectionNode.Orientation.North:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x, roomFrom.Position.y + Instance.ROOMSIZE.y);
                    }
                    break;
                case ConnectionNode.Orientation.South:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x, roomFrom.Position.y - Instance.ROOMSIZE.y);
                    }
                    break;
                case ConnectionNode.Orientation.West:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x - Instance.ROOMSIZE.x, roomFrom.Position.y);
                    }
                    break;
                case ConnectionNode.Orientation.East:
                    {
                        theoreticalPosition = new Vector2(roomFrom.Position.x + Instance.ROOMSIZE.x, roomFrom.Position.y);
                    }
                    break;
                default:
                    throw new System.NotImplementedException("Orientation not supported");
            }
            return rooms.Find(r => r.Value.Position == theoreticalPosition) == null;
        }
        static ConnectionNode.Orientation GenerateValidOrientation(RoomNode roomFrom)
        {
            int safetyNet = 0;
            ConnectionNode.Orientation Orientation = DungeonGenerator.RandomOrientation;
            while (!OrientationIsValid(roomFrom, Orientation) && safetyNet < 6)
            {
                if (Orientation == ConnectionNode.Orientation.West)
                    Orientation = ConnectionNode.Orientation.North;
                else
                    Orientation++;
                safetyNet++;
                continue;
            }
            return Orientation;
        }
        static RoomNode SpawnStartRoom(Vector2Int startPos)
        {
            return new RoomNode(startPos, RoomNode.RoomType.Start, 0, true);
        }
        static int NbOfSpawn(RoomNode.RoomType type)
        {
            return rooms.FindAll(r => r.Value.Type == type).Count;
        }
        static ConnectionNode BuildAdjacentRoom(RoomNode roomFrom, ConnectionNode.Orientation Orientation, RoomNode.RoomType type = RoomNode.RoomType.Default, bool hasLock = false, bool isPrimary = false, int cost = 0)
        {
            switch (Orientation)
            {
                case ConnectionNode.Orientation.North:
                    {
                        ConnectionNode connection = new ConnectionNode(Orientation,
                            roomFrom,
                            new RoomNode(new Vector2Int(roomFrom.Position.x, roomFrom.Position.y + Instance.ROOMSIZE.y), type, 0, isPrimary),
                            hasLock,
                            cost,
                            type == RoomNode.RoomType.Secret
                            );
                        roomFrom.Connections.Add(connection);
                        connection.DestinationRoom.Connections.Add(new ConnectionNode(ConnectionNode.Orientation.South, connection.DestinationRoom, roomFrom, hasLock, cost, type == RoomNode.RoomType.Secret));
                        return connection;
                    }
                case ConnectionNode.Orientation.South:
                    {
                        ConnectionNode connection = new ConnectionNode(Orientation, roomFrom, new RoomNode(new Vector2Int(roomFrom.Position.x, roomFrom.Position.y - Instance.ROOMSIZE.y), type, 0, isPrimary), hasLock,
                            cost,
                            type == RoomNode.RoomType.Secret
                            );
                        roomFrom.Connections.Add(connection);
                        connection.DestinationRoom.Connections.Add(new ConnectionNode(ConnectionNode.Orientation.North, connection.DestinationRoom, roomFrom, hasLock, cost, type == RoomNode.RoomType.Secret));
                        return connection;
                    }
                case ConnectionNode.Orientation.West:
                    {
                        ConnectionNode connection = new ConnectionNode(Orientation, roomFrom, new RoomNode(new Vector2Int(roomFrom.Position.x - Instance.ROOMSIZE.x, roomFrom.Position.y), type, 0, isPrimary), hasLock,
                            cost,
                            type == RoomNode.RoomType.Secret
                            );
                        roomFrom.Connections.Add(connection);
                        connection.DestinationRoom.Connections.Add(new ConnectionNode(ConnectionNode.Orientation.East, connection.DestinationRoom, roomFrom, hasLock, cost, type == RoomNode.RoomType.Secret));
                        return connection;
                    }
                case ConnectionNode.Orientation.East:
                    {
                        ConnectionNode connection = new ConnectionNode(Orientation, roomFrom, new RoomNode(new Vector2Int(roomFrom.Position.x + Instance.ROOMSIZE.x, roomFrom.Position.y), type, 0, isPrimary), hasLock,
                            cost,
                            type == RoomNode.RoomType.Secret
                            );
                        roomFrom.Connections.Add(connection);
                        connection.DestinationRoom.Connections.Add(new ConnectionNode(ConnectionNode.Orientation.West, connection.DestinationRoom, roomFrom, hasLock, cost, type == RoomNode.RoomType.Secret));
                        return connection;
                    }
                default:
                    throw new System.NotImplementedException("Orientation not supported");
            }
        }
        static bool CheckAdjacentRoom(RoomNode roomToProbe)
        {
            return rooms.Find(r =>
            r.Value.Position == roomToProbe.Position + new Vector2(Instance.ROOMSIZE.x, 0) ||
            r.Value.Position == roomToProbe.Position - new Vector2(Instance.ROOMSIZE.x, 0) ||
            r.Value.Position == roomToProbe.Position + new Vector2(0, Instance.ROOMSIZE.y) ||
            r.Value.Position == roomToProbe.Position - new Vector2(0, Instance.ROOMSIZE.y)) != null;
        }
        static bool CheckAdjacentRoom(RoomNode roomToProbe, RoomNode.RoomType type)
        {
            return rooms.Find(r =>
            (r.Value.Position == roomToProbe.Position + new Vector2(Instance.ROOMSIZE.x, 0) && r.Value.Type == type) ||
            (r.Value.Position == roomToProbe.Position - new Vector2(Instance.ROOMSIZE.x, 0) && r.Value.Type == type) ||
            (r.Value.Position == roomToProbe.Position + new Vector2(0, Instance.ROOMSIZE.y) && r.Value.Type == type) ||
            (r.Value.Position == roomToProbe.Position - new Vector2(0, Instance.ROOMSIZE.y) && r.Value.Type == type)) != null;
        }
        static bool BuildPrimaryPath(Vector2Int startPos, int roomBudget)
        {
            int roomsSpawned = 0;
            ConnectionNode.Orientation latestOrientation = ConnectionNode.Orientation.NULL;
            RoomNode? previousRoom;
            RoomNode? lastestSpawnedRoom = null;
            //PREREQUISITES
            bool startingRoomSpawned = false;
            bool endRoomSpawned = false;

            //Main Route Loop
            while (roomsSpawned < roomBudget)
            {
                //Update relevant values
                previousRoom = lastestSpawnedRoom;

                if (!startingRoomSpawned)
                {
                    lastestSpawnedRoom = SpawnStartRoom(startPos);
                    rooms.Add(lastestSpawnedRoom);
                    startingRoomSpawned = true;
                    roomsSpawned++;
                    continue;
                }
                if (roomsSpawned + 1 == Instance.nbOfRoomsClamp)
                {
                    latestOrientation = GenerateValidOrientation(previousRoom.Value);
                    ConnectionNode con = BuildAdjacentRoom(previousRoom.Value, latestOrientation, RoomNode.RoomType.End, false, true);
                    connections.Add(con);
                    lastestSpawnedRoom = con.DestinationRoom;
                    rooms.Add(lastestSpawnedRoom.Value);
                    endRoomSpawned = true;
                    break;
                }
                {
                    //TODO : Track Orientation to have even room distribution on the grid space
                    latestOrientation = GenerateValidOrientation(previousRoom.Value);
                    ConnectionNode con = BuildAdjacentRoom(previousRoom.Value, latestOrientation, RoomNode.RoomType.Combat, true, true);
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
        }
        static bool BuildSecondaryPath(ref RoomNode roomStart, ref RoomNode roomTarget, int roomBudget)
        {
            //check if the budget is big enough
            if (//Maximum diagonal distance achievable with the allocated budget
                Vector2.Distance(new RoomNode(Vector2Int.zero, RoomNode.RoomType.NULL, 0).Position, new RoomNode(Vector2Int.one * Instance.ROOMSIZE, RoomNode.RoomType.NULL, 0).Position) * roomBudget
                <
                //Diagonal distance between the begining of the room and the target room
                Vector2.Distance(roomStart.Position, roomTarget.Position) || roomBudget <= 0) return false;
            //Begin algo
            int remainingBudget = roomBudget;
            RoomNode? previousRoom;
            RoomNode? latestSpawnedRoom = roomStart;
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
                if (CheckAdjacentRoom(latestSpawnedRoom.Value, RoomNode.RoomType.End)) break;
                if (latestSpawnedRoomIsYAxis)
                {
                    switch (targetXAxis)
                    {
                        case 0:
                            continue;
                        case 1:
                            {
                                if (OrientationIsValid(previousRoom.Value, ConnectionNode.Orientation.East))
                                {
                                    ConnectionNode con = BuildAdjacentRoom(latestSpawnedRoom.Value, ConnectionNode.Orientation.East);
                                    connections.Add(con);
                                    rooms.Add(con.DestinationRoom);
                                    latestSpawnedRoom = con.DestinationRoom;
                                }
                                break;
                            }
                        case -1:
                            {
                                if (OrientationIsValid(previousRoom.Value, ConnectionNode.Orientation.West))
                                {
                                    ConnectionNode con = BuildAdjacentRoom(latestSpawnedRoom.Value, ConnectionNode.Orientation.West);
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
                                if (OrientationIsValid(previousRoom.Value, ConnectionNode.Orientation.North))
                                {
                                    ConnectionNode con = BuildAdjacentRoom(latestSpawnedRoom.Value, ConnectionNode.Orientation.North);
                                    connections.Add(con);
                                    rooms.Add(con.DestinationRoom);
                                    latestSpawnedRoom = con.DestinationRoom;
                                }
                                break;
                            }
                        case -1:
                            {
                                if (OrientationIsValid(previousRoom.Value, ConnectionNode.Orientation.South))
                                {
                                    ConnectionNode con = BuildAdjacentRoom(latestSpawnedRoom.Value, ConnectionNode.Orientation.South);
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
            return BuildPrimaryPath(Vector2Int.zero, Instance.nbOfRoomsClamp);
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
            foreach (RoomNode room in rooms)
            {
                mapstring += "I am a " + room.Type + " room at " + room.Position + ", Difficulty : " + room.Difficulty + "\n";
                GameObject roomGO = Instantiate(roomPrefab[(int)room.Type]);
                roomGO.GetComponent<Room>().position = new Vector2Int(room.Position.x, room.Position.y);
                //TODO : Do the Doors
                Room roomComponent = roomGO.GetComponent<Room>();
                foreach (Door door in roomComponent.GetAllDoorInRoom())
                {
                    ConnectionNode? connectionNode = null;
                    switch (door.Orientation)
                    {
                        case Utils.ORIENTATION.NORTH:
                            {
                                if(room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.North).HasValue)
                                connectionNode = room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.North).Value;
                                break;
                            }

                        case Utils.ORIENTATION.SOUTH:
                            {
                                if (room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.South).HasValue)
                                    connectionNode = room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.South).Value;
                                break;
                            }

                        case Utils.ORIENTATION.WEST:
                            {
                                if (room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.West).HasValue)
                                    connectionNode = room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.West).Value;
                                break;
                            }

                        case Utils.ORIENTATION.EAST:
                            {
                                if (room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.East).HasValue)
                                    connectionNode = room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.East).Value;
                                break;
                            }
                    }
                    if (connectionNode != null)
                    {
                        if (connectionNode.Value.IsSecret)
                            door.SetState(Door.STATE.SECRET);
                        else if (connectionNode.Value.HasLock)
                            door.SetState(Door.STATE.CLOSED);
                        else
                            door.SetState(Door.STATE.OPEN);
                    }
                    else
                        door.SetState(Door.STATE.WALL);
                }
                roomGO.transform.parent = transform;
                roomGO.transform.position = new Vector2(room.Position.x, room.Position.y);
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
