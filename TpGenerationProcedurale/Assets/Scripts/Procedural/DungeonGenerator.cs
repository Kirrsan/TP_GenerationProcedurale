using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenerator
{
    public struct RoomNode
    {
        public enum RoomType
        {
            NULL = int.MinValue,
            Start = 0,
            Classic,
            Trap,
            Danger,
            Safe,
            Merchant,
            Secret,
            End,

        }
        public enum RoomDifficulty
        {
            NULL = int.MinValue,
            Easy = 0,
            Medium = 1,
            Hard = 2
        }
        public Vector2Int Position { get; set; }
        public List<ConnectionNode?> Connections { get; set; }
        public RoomType Type { get; set; }
        public RoomDifficulty Difficulty { get; set; }
        public bool IsPrimary { get; set; }
        public RoomNode(RoomType type = RoomType.NULL)
        {
            Position = new Vector2Int(int.MinValue, int.MinValue);
            Type = RoomType.NULL;
            Difficulty = RoomDifficulty.NULL;
            Connections = null;
            IsPrimary = false;
        }
        public RoomNode(Vector2Int pos, RoomType type, RoomDifficulty difficulty, bool isPrimary = false)
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

        [Range(3, 100)] public int nbOfRoomsClampFirstSecondaryPath = 5;
        [Range(0, 9999)] public int difficultyBudgetFirstSecondaryPath = 10;

        [Range(3, 100)] public int nbOfRoomsClampSecondSecondaryPath = 5;
        [Range(0, 9999)] public int difficultyBudgetSecondSecondaryPath = 10;


        [Header("System")] public int extraTriesClamp = 10;
        public List<GameObject> prefabStart = new List<GameObject>();
        public GameObject prefabEnd;
        public List<GameObject> prefabEasyClassic = new List<GameObject>();
        public List<GameObject> prefabMediumClassic = new List<GameObject>();
        public List<GameObject> prefabHardClassic = new List<GameObject>();
        public List<GameObject> prefabEasyTrap = new List<GameObject>();
        public List<GameObject> prefabMediumTrap = new List<GameObject>();
        public List<GameObject> prefabHardTrap = new List<GameObject>();
        public List<GameObject> prefabEasyDanger = new List<GameObject>();
        public List<GameObject> prefabMediumDanger = new List<GameObject>();
        public List<GameObject> prefabHardDanger = new List<GameObject>();
        public List<GameObject> prefabEasySafe = new List<GameObject>();
        public List<GameObject> prefabMediumSafe = new List<GameObject>();
        public List<GameObject> prefabHardSafe = new List<GameObject>();
        public List<GameObject> prefabEasyShop = new List<GameObject>();
        public List<GameObject> prefabMediumShop = new List<GameObject>();
        public List<GameObject> prefabHardShop = new List<GameObject>();
        public List<GameObject> prefabEasySecret = new List<GameObject>();
        public List<GameObject> prefabMediumSecret = new List<GameObject>();
        public List<GameObject> prefabHardSecret = new List<GameObject>();





        public List<GameObject> roomPrefab = new List<GameObject>();
        public Vector2Int ROOMSIZE = new Vector2Int(11, 9);
        public RectInt maxLevelSize;
        public static List<RoomNode?> rooms = new List<RoomNode?>();
        public static List<ConnectionNode?> connections = new List<ConnectionNode?>();

        private static int firstDoorIndex;
        private static int secondDoorIndex;

        private static bool startingSequenceSpawned = false;
        private static bool endRoomSpawned = false;

        static ConnectionNode.Orientation RandomOrientation
        {
            get => (ConnectionNode.Orientation)Random.Range(0, 4);
        }
        static float RandomFloat
        {
            get => Random.Range(0.0f, 1.0f);
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

            if (safetyNet >= 6)
            {
                return ConnectionNode.Orientation.NULL;
            }

            return Orientation;
        }
        static RoomNode SpawnStartRoom(Vector2Int startPos)
        {
            return new RoomNode(startPos, RoomNode.RoomType.Start, 0, true);
        }
        static RoomNode SpawnEndRoom(Vector2Int endPos)
        {
            return new RoomNode(endPos, RoomNode.RoomType.End, 0, true);
        }
        static int NbOfSpawn(RoomNode.RoomType type)
        {
            return rooms.FindAll(r => r.Value.Type == type).Count;
        }
        static ConnectionNode BuildAdjacentRoom(RoomNode roomFrom, ConnectionNode.Orientation Orientation, RoomNode.RoomType type = RoomNode.RoomType.Classic, RoomNode.RoomDifficulty difficulty = RoomNode.RoomDifficulty.Easy, bool isPrimary = false, bool hasLock = false, int cost = 0, bool pathIsSecret = false)
        {
            switch (Orientation)
            {
                case ConnectionNode.Orientation.North:
                    {
                        ConnectionNode connection = new ConnectionNode(Orientation,
                            roomFrom,
                            new RoomNode(new Vector2Int(roomFrom.Position.x, roomFrom.Position.y + Instance.ROOMSIZE.y), type, difficulty, isPrimary),
                            hasLock,
                            cost,
                            pathIsSecret
                            );
                        roomFrom.Connections.Add(connection);
                        connection.DestinationRoom.Connections.Add(new ConnectionNode(ConnectionNode.Orientation.South, connection.DestinationRoom, roomFrom, hasLock, cost, pathIsSecret));
                        return connection;
                    }
                case ConnectionNode.Orientation.South:
                    {
                        ConnectionNode connection = new ConnectionNode(Orientation, roomFrom, new RoomNode(new Vector2Int(roomFrom.Position.x, roomFrom.Position.y - Instance.ROOMSIZE.y), type, difficulty, isPrimary), hasLock,
                            cost,
                            pathIsSecret
                            );
                        roomFrom.Connections.Add(connection);
                        connection.DestinationRoom.Connections.Add(new ConnectionNode(ConnectionNode.Orientation.North, connection.DestinationRoom, roomFrom, hasLock, cost, pathIsSecret));
                        return connection;
                    }
                case ConnectionNode.Orientation.West:
                    {
                        ConnectionNode connection = new ConnectionNode(Orientation, roomFrom, new RoomNode(new Vector2Int(roomFrom.Position.x - Instance.ROOMSIZE.x, roomFrom.Position.y), type, difficulty, isPrimary), hasLock,
                            cost,
                            pathIsSecret
                            );
                        roomFrom.Connections.Add(connection);
                        connection.DestinationRoom.Connections.Add(new ConnectionNode(ConnectionNode.Orientation.East, connection.DestinationRoom, roomFrom, hasLock, cost, pathIsSecret));
                        return connection;
                    }
                case ConnectionNode.Orientation.East:
                    {
                        ConnectionNode connection = new ConnectionNode(Orientation, roomFrom, new RoomNode(new Vector2Int(roomFrom.Position.x + Instance.ROOMSIZE.x, roomFrom.Position.y), type, difficulty, isPrimary), hasLock,
                            cost,
                            pathIsSecret
                            );
                        roomFrom.Connections.Add(connection);
                        connection.DestinationRoom.Connections.Add(new ConnectionNode(ConnectionNode.Orientation.West, connection.DestinationRoom, roomFrom, hasLock, cost, pathIsSecret));
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
        static bool BuildPrimaryPath(Vector2Int startPos, int roomBudget, bool isFirstPath = false)
        {
            int roomsSpawned = 0;
            int roomPerDifficulty = roomBudget / 3;
            //Room type weights
            float weightClassic = 0.2f;
            float weightTrap = 0.2f;
            float weightDanger = 0.2f;
            float weightSafe = 0.2f;
            float weightShop = 0.2f;
            //Room difficulty
            float weightEasy = 0.8f;
            float weightMedium = 0.15f;
            float weighhtHard = 0.05f;
            ConnectionNode.Orientation latestOrientation = ConnectionNode.Orientation.NULL;
            RoomNode? previousRoom;
            RoomNode? latestSpawnedRoom = null;
            RoomNode.RoomType selectedRoomType = RoomNode.RoomType.Classic;
            RoomNode.RoomDifficulty selectedDifficulty = RoomNode.RoomDifficulty.Easy;
            //PREREQUISITES


            //Main Route Loop
            while (roomsSpawned < roomBudget)
            {
                //Update relevant values
                previousRoom = latestSpawnedRoom;
                float sampleRoomType = RandomFloat;
                float sampleDifficulty = RandomFloat;

                if (roomsSpawned > (1 / 3) * roomBudget)
                {
                    weightEasy = 0.15f;
                    weightMedium = 0.70f;
                    weighhtHard = 0.15f;
                }
                if (roomsSpawned > (2 / 3) * roomBudget)
                {
                    weightEasy = 0.05f;
                    weightMedium = 0.15f;
                    weighhtHard = 0.80f;
                }

                if (sampleRoomType > weightClassic)
                    if (sampleDifficulty > weightClassic + weightTrap)
                        if (sampleDifficulty > weightClassic + weightTrap + weightDanger)
                            if (sampleDifficulty > weightClassic + weightTrap + weightDanger + weightSafe)
                                selectedRoomType = RoomNode.RoomType.Merchant;
                            else selectedRoomType = RoomNode.RoomType.Safe;
                        else selectedRoomType = RoomNode.RoomType.Danger;
                    else selectedRoomType = RoomNode.RoomType.Trap;
                else selectedRoomType = RoomNode.RoomType.Classic;

                if (sampleDifficulty > weightEasy)
                    if (sampleDifficulty > weightEasy + weightMedium)
                        selectedDifficulty = RoomNode.RoomDifficulty.Hard;
                    else selectedDifficulty = RoomNode.RoomDifficulty.Medium;
                else selectedDifficulty = RoomNode.RoomDifficulty.Easy;

                switch (selectedRoomType)
                {
                    case RoomNode.RoomType.Merchant:
                        {
                            weightShop -= 0.1f;
                            if (weightShop < 0) weightShop = 0;
                            weightDanger += 0.025f;
                            weightSafe += 0.025f;
                            weightTrap += 0.025f;
                            weightClassic += 0.025f;
                        }
                        break;
                    case RoomNode.RoomType.Safe:
                        {
                            weightSafe -= 0.1f;
                            if (weightSafe < 0) weightSafe = 0;
                            weightDanger += 0.025f;
                            weightShop += 0.025f;
                            weightTrap += 0.025f;
                            weightClassic += 0.025f;
                        }
                        break;
                    case RoomNode.RoomType.Danger:
                        {
                            weightDanger -= 0.1f;
                            if (weightDanger < 0) weightDanger = 0;
                            weightShop += 0.025f;
                            weightSafe += 0.025f;
                            weightTrap += 0.025f;
                            weightClassic += 0.025f;
                        }
                        break;
                    case RoomNode.RoomType.Trap:
                        {
                            weightTrap -= 0.1f;
                            if (weightTrap < 0) weightTrap = 0;
                            weightDanger += 0.025f;
                            weightSafe += 0.025f;
                            weightShop += 0.025f;
                            weightClassic += 0.025f;
                        }
                        break;
                    case RoomNode.RoomType.Classic:
                        {
                            weightClassic -= 0.1f;
                            if (weightClassic < 0) weightClassic = 0;
                            weightDanger += 0.025f;
                            weightSafe += 0.025f;
                            weightTrap += 0.025f;
                            weightShop += 0.025f;
                        }
                        break;
                }

                if (!startingSequenceSpawned)
                {
                    //Spawn start room
                    latestSpawnedRoom = SpawnStartRoom(startPos);
                    rooms.Add(latestSpawnedRoom);
                    roomsSpawned++;

                    //Spawn first combat room
                    latestSpawnedRoom = BuildAdjacentRoom(latestSpawnedRoom.Value, ConnectionNode.Orientation.East, RoomNode.RoomType.Start, RoomNode.RoomDifficulty.Medium, isFirstPath).DestinationRoom;
                    rooms.Add(latestSpawnedRoom);
                    roomsSpawned++;

                    latestSpawnedRoom = BuildAdjacentRoom(latestSpawnedRoom.Value, ConnectionNode.Orientation.East, RoomNode.RoomType.Start, RoomNode.RoomDifficulty.Hard, isFirstPath, true, 5).DestinationRoom;
                    rooms.Add(latestSpawnedRoom);
                    roomsSpawned++;
                    startingSequenceSpawned = true;

                    continue;
                }
                else if (!previousRoom.HasValue)
                {
                    previousRoom = rooms.Find(r => r.Value.Position == startPos);
                }

                if (!endRoomSpawned && roomsSpawned + 1 == roomBudget)
                {
                    latestOrientation = GenerateValidOrientation(previousRoom.Value);
                    if (latestOrientation == ConnectionNode.Orientation.NULL)
                    {
                        return false;
                    }
                    ConnectionNode con = BuildAdjacentRoom(previousRoom.Value, latestOrientation, RoomNode.RoomType.End, RoomNode.RoomDifficulty.Hard, isFirstPath, true, 0);
                    connections.Add(con);
                    latestSpawnedRoom = con.DestinationRoom;
                    rooms.Add(latestSpawnedRoom.Value);
                    endRoomSpawned = true;
                    break;
                }
                {
                    //TODO : Track Orientation to have even room distribution on the grid space
                    latestOrientation = GenerateValidOrientation(previousRoom.Value);
                    if (latestOrientation == ConnectionNode.Orientation.NULL)
                        return false;

                    ConnectionNode con = BuildAdjacentRoom(previousRoom.Value, latestOrientation, selectedRoomType, selectedDifficulty, isFirstPath);
                    connections.Add(con);
                    latestSpawnedRoom = con.DestinationRoom;
                    rooms.Add(latestSpawnedRoom.Value);
                    roomsSpawned++;
                }
            }

            //TODO : Use AltRoute generation
            //BuildSecondaryPath(ref startRoom, ref endRoom, 50);

            //return true if all prerequisites are filled
            if (startingSequenceSpawned && endRoomSpawned)
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

            startingSequenceSpawned = false;
            endRoomSpawned = false;

            if (BuildPrimaryPath(Vector2Int.zero, Instance.nbOfRoomsClamp, true))
            {
                RoomNode secondaryPathStart = rooms[3].Value;

                if (BuildPrimaryPath(secondaryPathStart.Position, Instance.nbOfRoomsClampFirstSecondaryPath))
                {
                    RoomNode lastroom = rooms[rooms.Count - 1].Value;
                    lastroom.Type = RoomNode.RoomType.Secret;
                    rooms[rooms.Count - 1] = lastroom;
                    rooms.Add(BuildAdjacentRoom(lastroom, GenerateValidOrientation(lastroom), RoomNode.RoomType.Merchant, RoomNode.RoomDifficulty.Easy, false, false, 0, true).DestinationRoom);
                    secondaryPathStart = rooms[7].Value;
                    if (BuildPrimaryPath(secondaryPathStart.Position, Instance.nbOfRoomsClampSecondSecondaryPath))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }


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
            Debug.Log("Number of extra tries needed to generate dungeon : " + nbOfExtraTries);
            string mapstring = "Map :" + "\n";
            bool hasHadPrimaryDoor = false;

            RoomNode lastPrimaryNodeWithDoor = new RoomNode();
            int lastDoorScoreToPass = 0;
            int roomIndex = 0;

            int scoreToThisPath = 0;
            bool hasGottenToFirstSecondaryPath = false;
            bool hasGottenToSecondSecondaryPath = false;

            foreach (RoomNode room in rooms)
            {
                mapstring += "I am a " + room.Type + " room at " + room.Position + ", I have connections : ";
                foreach (ConnectionNode connection in room.Connections)
                    mapstring += connection.Direction + " ,";
                mapstring += "\n";
                GameObject roomGO = null;
                switch (room.Type)
                {
                    case RoomNode.RoomType.Start:
                        {
                            switch (room.Difficulty)
                            {
                                case RoomNode.RoomDifficulty.Easy:
                                    roomGO = Instantiate(prefabStart[0]);
                                    break;
                                case RoomNode.RoomDifficulty.Medium:
                                    roomGO = Instantiate(prefabStart[1]);
                                    break;
                                case RoomNode.RoomDifficulty.Hard:
                                    roomGO = Instantiate(prefabStart[2]);
                                    break;
                            }
                        }
                        break;
                    case RoomNode.RoomType.Classic:
                        {
                            switch (room.Difficulty)
                            {
                                case RoomNode.RoomDifficulty.Easy:
                                    roomGO = Instantiate(prefabEasyClassic[Random.Range(0, prefabEasyClassic.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Medium:
                                    roomGO = Instantiate(prefabMediumClassic[Random.Range(0, prefabMediumClassic.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Hard:
                                    roomGO = Instantiate(prefabHardClassic[Random.Range(0, prefabEasyClassic.Count - 1)]);
                                    break;
                            }
                        }
                        break;
                    case RoomNode.RoomType.Trap:
                        {
                            switch (room.Difficulty)
                            {
                                case RoomNode.RoomDifficulty.Easy:
                                    roomGO = Instantiate(prefabEasyTrap[Random.Range(0, prefabEasyTrap.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Medium:
                                    roomGO = Instantiate(prefabMediumTrap[Random.Range(0, prefabMediumTrap.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Hard:
                                    roomGO = Instantiate(prefabHardTrap[Random.Range(0, prefabEasyTrap.Count - 1)]);
                                    break;
                            }
                        }
                        break;
                    case RoomNode.RoomType.Danger:
                        {
                            switch (room.Difficulty)
                            {
                                case RoomNode.RoomDifficulty.Easy:
                                    roomGO = Instantiate(prefabEasyDanger[Random.Range(0, prefabEasyDanger.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Medium:
                                    roomGO = Instantiate(prefabMediumDanger[Random.Range(0, prefabMediumDanger.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Hard:
                                    roomGO = Instantiate(prefabHardDanger[Random.Range(0, prefabEasyDanger.Count - 1)]);
                                    break;
                            }
                        }
                        break;
                    case RoomNode.RoomType.Safe:
                        {
                            switch (room.Difficulty)
                            {
                                case RoomNode.RoomDifficulty.Easy:
                                    roomGO = Instantiate(prefabEasySafe[Random.Range(0, prefabEasySafe.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Medium:
                                    roomGO = Instantiate(prefabMediumSafe[Random.Range(0, prefabMediumSafe.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Hard:
                                    roomGO = Instantiate(prefabHardSafe[Random.Range(0, prefabHardSafe.Count - 1)]);
                                    break;
                            }
                        }
                        break;
                    case RoomNode.RoomType.Merchant:
                        {
                            switch (room.Difficulty)
                            {
                                case RoomNode.RoomDifficulty.Easy:
                                    roomGO = Instantiate(prefabEasyShop[Random.Range(0, prefabEasyShop.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Medium:
                                    roomGO = Instantiate(prefabMediumShop[Random.Range(0, prefabMediumShop.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Hard:
                                    roomGO = Instantiate(prefabHardShop[Random.Range(0, prefabEasyShop.Count - 1)]);
                                    break;
                            }
                        }
                        break;
                    case RoomNode.RoomType.Secret:
                        {
                            switch (room.Difficulty)
                            {
                                case RoomNode.RoomDifficulty.Easy:
                                    roomGO = Instantiate(prefabEasySecret[Random.Range(0, prefabEasySecret.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Medium:
                                    roomGO = Instantiate(prefabMediumSecret[Random.Range(0, prefabMediumSecret.Count - 1)]);
                                    break;
                                case RoomNode.RoomDifficulty.Hard:
                                    roomGO = Instantiate(prefabHardSecret[Random.Range(0, prefabEasySecret.Count - 1)]);
                                    break;
                            }
                        }
                        break;
                    case RoomNode.RoomType.End:
                        {
                            roomGO = Instantiate(prefabEnd);
                        }
                        break;
                }
                if (roomGO && roomGO.TryGetComponent(out Room broom))
                {
                    broom.position = room.Position;
                    //TODO : Do the Doors
                    Room roomComponent = broom;
                    int i = 0;
                    foreach (Door door in roomComponent.GetAllDoorInRoom())
                    {
                        ConnectionNode? connectionNode = null;
                        switch (i)
                        {
                            case 0:
                                {
                                    if (room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.North).HasValue)
                                        connectionNode = room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.North).Value;
                                    break;
                                }

                            case 2:
                                {
                                    if (room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.South).HasValue)
                                        connectionNode = room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.South).Value;
                                    break;
                                }

                            case 3:
                                {
                                    if (room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.West).HasValue)
                                        connectionNode = room.Connections.Find(c => c.Value.Direction == ConnectionNode.Orientation.West).Value;
                                    break;
                                }

                            case 1:
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
                            {
                                if (room.IsPrimary && connectionNode.Value.DestinationRoom.IsPrimary
                                    && lastPrimaryNodeWithDoor.Position != connectionNode.Value.DestinationRoom.Position)
                                {
                                    door.SetIsPrimaryPath();
                                    door.SetDoorCostIfPrimary(hasHadPrimaryDoor);
                                    lastDoorScoreToPass = door.GetDoorValueIfLocked();

                                    if (!hasHadPrimaryDoor)
                                    {
                                        firstDoorIndex = roomIndex;
                                        hasHadPrimaryDoor = true;
                                    }
                                    else
                                    {
                                        secondDoorIndex = roomIndex;
                                    }
                                    lastPrimaryNodeWithDoor = room;
                                }
                                //add to path score the cost of the previous door
                                else if (room.IsPrimary && connectionNode.Value.DestinationRoom.IsPrimary
                                    && lastPrimaryNodeWithDoor.Position == connectionNode.Value.DestinationRoom.Position)
                                {
                                    scoreToThisPath -= lastDoorScoreToPass;
                                    door.scoreText.gameObject.SetActive(false);
                                }
                                else if (!room.IsPrimary
                                    && lastPrimaryNodeWithDoor.Position != connectionNode.Value.DestinationRoom.Position)
                                {
                                    door.SetDoorCostIfSecondaryPath(rooms);
                                    lastDoorScoreToPass = door.GetDoorValueIfLocked();
                                    lastPrimaryNodeWithDoor = room;
                                }
                                else if (!room.IsPrimary && lastPrimaryNodeWithDoor.Position == connectionNode.Value.DestinationRoom.Position)
                                {
                                    scoreToThisPath -= lastDoorScoreToPass;
                                }
                                door.SetState(Door.STATE.CLOSED);
                            }
                            else
                            {
                                door.SetState(Door.STATE.OPEN);
                            }
                        }
                        else
                            door.SetState(Door.STATE.WALL);
                        i++;
                    }
                    roomGO.transform.parent = transform;
                    roomGO.transform.position = new Vector2(room.Position.x, room.Position.y);
                }
                roomGO.transform.parent = transform;
                roomGO.transform.position = new Vector2(room.Position.x, room.Position.y);

                Room currentRoom = roomGO.GetComponent<Room>();

                if (room.IsPrimary)
                {
                    scoreToThisPath += currentRoom.GetPotentialPointWin() - currentRoom.GetPotentialLoss();
                    currentRoom.SetPotentialPointWithShortestPathToRoom(scoreToThisPath);
                }
                else
                {
                    if (roomIndex > nbOfRoomsClamp && roomIndex <= nbOfRoomsClamp + nbOfRoomsClampFirstSecondaryPath)
                    {
                        //firstpath originates from room[3]
                        if (!hasGottenToFirstSecondaryPath)
                        {
                            hasGottenToFirstSecondaryPath = true;
                            scoreToThisPath = Room.allRooms[3].GetPotentialPointWithShortestPathToRoom();
                        }

                        scoreToThisPath += currentRoom.GetPotentialPointWin() - currentRoom.GetPotentialLoss();
                        currentRoom.SetPotentialPointWithShortestPathToRoom(scoreToThisPath);
                    }
                    else if (roomIndex > nbOfRoomsClamp + nbOfRoomsClampFirstSecondaryPath)
                    {
                        //firstpath originates from room[7]
                        if (!hasGottenToSecondSecondaryPath)
                        {
                            hasGottenToSecondSecondaryPath = true;
                            scoreToThisPath = Room.allRooms[7].GetPotentialPointWithShortestPathToRoom();
                        }

                        scoreToThisPath += currentRoom.GetPotentialPointWin() - currentRoom.GetPotentialLoss();
                        currentRoom.SetPotentialPointWithShortestPathToRoom(scoreToThisPath);
                    }
                }
                ++roomIndex;
            }

            scoreToThisPath = Room.allRooms[nbOfRoomsClamp + nbOfRoomsClampFirstSecondaryPath - 1].GetPotentialPointWithShortestPathToRoom();
            for (int i = 4; i < 8; i++)
            {
                scoreToThisPath += Room.allRooms[i].GetPotentialPointWin() - Room.allRooms[i].GetPotentialLoss();
                Room.allRooms[i].SetPotentialPointWithShortestPathToRoom(scoreToThisPath);
            }

            scoreToThisPath = Room.allRooms[nbOfRoomsClamp + nbOfRoomsClampFirstSecondaryPath + nbOfRoomsClampSecondSecondaryPath - 1].GetPotentialPointWithShortestPathToRoom();
            for (int i = 8; i < nbOfRoomsClamp - 1; i++)
            {
                scoreToThisPath += Room.allRooms[i].GetPotentialPointWin() - Room.allRooms[i].GetPotentialLoss();
                Room.allRooms[i].SetPotentialPointWithShortestPathToRoom(scoreToThisPath);
            }
            Debug.Log(mapstring);
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
