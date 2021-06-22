using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    private Transform levelHolder;
    public GameObject outerTopWall;
    public GameObject outerBottomWall;
    public GameObject outerRightWall;
    public GameObject outerLeftWall;
    public GameObject outerCornerTopLeftWall;
    public GameObject outerCornerTopRightWall;
    public GameObject outerCornerBottomLeftWall;
    public GameObject outerCornerBottomRightWall;
    public GameObject[] floorTiles;
    public GameObject[] boxTiles;
    public GameObject shelfTile;
    public GameObject robot;
    public GameObject plant;
    public GameObject[] paperTiles;
    public GameObject[] deskTiles;

    void BoardSetup(int rows, int columns, int shelfLength, int floorCount, int wingCount, int officeWidth, RoomType roomType)
    {
        for (int i = 0; i < floorCount; ++i)
        {
            Floor floor = new Floor(rows, columns, shelfLength, roomType)
            {
                floorNumber = i
            };
            floor.floorEntryPoint = new Vector2(0, (floor.RoomHeight - 1) / 2);
            floor.floorExitPoint = new Vector2(0, floor.RoomHeight - 1);
            floor.wing = 1;
            PlanManager.instance.floors.Add(floor);
            if (wingCount == 2)
            {
                Floor symmetricFloor = new Floor(rows, columns, shelfLength, roomType)
                {
                    floorNumber = i
                };
                symmetricFloor.floorEntryPoint = new Vector2(symmetricFloor.RoomWidth - 1, (symmetricFloor.RoomHeight - 1) / 2);
                symmetricFloor.floorExitPoint = new Vector2(symmetricFloor.RoomWidth - 1, symmetricFloor.RoomHeight - 1);
                PlanManager.instance.secondWingFloors.Add(symmetricFloor);
                symmetricFloor.wing = 2;
            }

            for (int y = 0; y < floor.RoomHeight; ++y)
            {
                for (int x = 0; x < floor.RoomWidth; ++x)
                {
                    CreateTile(new Vector2(x, y), floor, i, wingCount, officeWidth);
                }
            }

                SpawnWalls(floor.RoomWidth, floor.RoomHeight, 0, i);
                if (wingCount == 2)
                {
                    SpawnSymmetricWalls(floor.RoomWidth, floor.RoomHeight, 0, i, officeWidth);
                }
        }

        if (roomType == RoomType.SimpleSingleRow)
        {
            BuildOffice(officeWidth, rows * 2 + 1, wingCount, floorCount);
        }
        else
        if (roomType == RoomType.SimpleDoubleRow)
        {
            BuildOffice(officeWidth, rows * 3 + 1, wingCount, floorCount);
        }
        else
        if (roomType == RoomType.AdvancedSingleRow)
        {
            BuildOffice(officeWidth, rows * 2 - 1, wingCount, floorCount);
        }
        else
        if (roomType == RoomType.AdvancedDoubleRow)
        {
            BuildOffice(officeWidth, rows * 3, wingCount, floorCount);
        }

        SetWorldBounds(floorCount, officeWidth, PlanManager.instance.floors[0].RoomWidth, PlanManager.instance.floors[0].RoomHeight);
    }
    void SpawnRandomFloorTile(Vector2 pos)
    {
        GameObject instance = Instantiate(floorTiles[Random.Range(0, floorTiles.Length)], new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        instance.transform.SetParent(levelHolder);
    }

    void SpawnRandomBoxTile(Vector2 pos, int floor, int wing)
    {
        GameObject instance = Instantiate(boxTiles[Random.Range(0, boxTiles.Length)], new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        instance.transform.SetParent(levelHolder);
        Storage newStorage = instance.GetComponent<Storage>();
        newStorage.rootFloorNumber = floor;
        newStorage.wing = wing;
        if (wing == 2)
        {
            newStorage.rootFloor = PlanManager.instance.secondWingFloors[floor];
            PlanManager.instance.secondWingFloors[floor].storages.Add(newStorage);
            newStorage.storageID = PlanManager.instance.secondWingFloors[floor].storages.Count - 1;
        }
        else
        {
            newStorage.rootFloor = PlanManager.instance.floors[floor];
            PlanManager.instance.floors[floor].storages.Add(newStorage);
            newStorage.storageID = PlanManager.instance.floors[floor].storages.Count - 1;
        }

        if (Settings.instance.roomType == RoomType.SimpleSingleRow)
        {
            newStorage.accessPoint = new Vector2(newStorage.transform.position.x, newStorage.transform.position.y - 1);
        }
        else
        if (Settings.instance.roomType == RoomType.AdvancedSingleRow)
        {
            if ((newStorage.storageID  % PlanManager.instance.storageOnFloor) / (Settings.instance.shelfLength * Settings.instance.columnCount) == 0)
            {
                newStorage.accessPoint = new Vector2(newStorage.transform.position.x, newStorage.transform.position.y + 1);
            }
            else
            {
                newStorage.accessPoint = new Vector2(newStorage.transform.position.x, newStorage.transform.position.y - 1);
            }
        }
        else
        if (Settings.instance.roomType == RoomType.SimpleDoubleRow)
        {
            if ((newStorage.storageID % PlanManager.instance.storageOnFloor) / (Settings.instance.shelfLength * Settings.instance.columnCount) % 2 == 0)
            {
                newStorage.accessPoint = new Vector2(newStorage.transform.position.x, newStorage.transform.position.y - 1);
            }
            else
            {
                newStorage.accessPoint = new Vector2(newStorage.transform.position.x, newStorage.transform.position.y + 1);
            }
        }
        else
        if (Settings.instance.roomType == RoomType.AdvancedDoubleRow)
        {
            if ((newStorage.storageID % PlanManager.instance.storageOnFloor) / (Settings.instance.shelfLength * Settings.instance.columnCount) % 2 == 1)
            {
                newStorage.accessPoint = new Vector2(newStorage.transform.position.x, newStorage.transform.position.y - 1);
            }
            else
            {
                newStorage.accessPoint = new Vector2(newStorage.transform.position.x, newStorage.transform.position.y + 1);
            }
        }

    }

    void SpawnWallTile(GameObject wall, Vector2 pos)
    {
        GameObject wallInstance = Instantiate(wall, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        wallInstance.transform.SetParent(levelHolder);
    }

    void SpawnShelfTile(Vector2 pos)
    {
        GameObject instance = Instantiate(shelfTile, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        instance.transform.SetParent(levelHolder);
    }

    void CreateTile(Vector2 pos, Floor floor, int floorNumber, int wingCount, int officeWidth)
    {
        SpawnRandomFloorTile(new Vector2(pos.x, pos.y + (floor.RoomHeight + 2) * floorNumber));
        if (wingCount == 2)
        {
            Vector2 tmp = CoordTransition(pos, officeWidth);
            SpawnRandomFloorTile(new Vector2(tmp.x, tmp.y + (floor.RoomHeight + 2) * floorNumber));
        }

        if (floor.Grid[(int)pos.y][(int)pos.x] == '#')
        {
            SpawnRandomBoxTile(new Vector2(pos.x, pos.y + (floor.RoomHeight + 2) * floorNumber), floorNumber, 1);
            SpawnShelfTile(new Vector2(pos.x, pos.y + (floor.RoomHeight + 2) * floorNumber));

            if (wingCount == 2)
            {
                Vector2 tmp = CoordTransition(pos, officeWidth);
                SpawnRandomBoxTile(new Vector2(tmp.x, tmp.y + (floor.RoomHeight + 2) * floorNumber), floorNumber, 2);
                SpawnShelfTile(new Vector2(tmp.x, tmp.y + (floor.RoomHeight + 2) * floorNumber));
            }
        }
    }

    Vector2 CoordTransition(Vector2 pos, int officeWidth)
    {
        return new Vector2(pos.x - 2 * pos.x - 1 - officeWidth - 4, pos.y);
    }

    void BuildOffice(int officeWidth, int officeHeight, int wingCount, int floorCount)
    {
        for (int j = 0; j < floorCount; ++j)
        {
            for (int x = -2 - officeWidth; x < -2; ++x)
            {
                for (int y = 0; y < officeHeight; ++y)
                {
                    SpawnRandomFloorTile(new Vector2(x, y + j * (officeHeight + 2)));
                }
            }

            for (int i = 0; i < officeHeight; ++i)
            {
                if (i == (officeHeight - 1) / 2)
                {
                    if (wingCount == 2)
                    {
                        SpawnRandomFloorTile(new Vector2(-3 - officeWidth, i + j * (officeHeight + 2)));
                    }
                    else
                    {
                        SpawnWallTile(outerLeftWall, new Vector2(-3 - officeWidth, i + j * (officeHeight + 2)));
                    }
                    SpawnRandomFloorTile(new Vector2(-2, i + j * (officeHeight + 2)));
                }
                else
                {
                    SpawnWallTile(outerLeftWall, new Vector2(-3 - officeWidth, i + j * (officeHeight + 2)));
                    SpawnWallTile(outerRightWall, new Vector2(-2, i + j * (officeHeight + 2)));
                }
            }

            for (int i = -officeWidth - 2; i < -2; ++i)
            {
                SpawnWallTile(outerTopWall, new Vector2(i, -1f + j * (officeHeight + 2)));
                SpawnWallTile(outerBottomWall, new Vector2(i, officeHeight + j * (officeHeight + 2)));
            }

            SpawnWallTile(outerCornerTopLeftWall, new Vector2(-3f - officeWidth, officeHeight + j * (officeHeight + 2)));
            SpawnWallTile(outerCornerBottomRightWall, new Vector2(-2f, -1f + j * (officeHeight + 2)));
            SpawnWallTile(outerCornerTopRightWall, new Vector2(-2f, officeHeight + j * (officeHeight + 2)));
            SpawnWallTile(outerCornerBottomLeftWall, new Vector2(-3f - officeWidth, -1f + j * (officeHeight + 2)));
        }
    }

    internal void SetupScene(InputParameters input)
    {
        int colomns = input.columns;
        int rows = input.rows;
        int floorCount = input.floorCount;
        int shelfLength = input.shelfLength;
        int wingCount = input.wingCount;
        int officeWidth = input.officeWidth;

        levelHolder = new GameObject("Board").transform;
        BoardSetup(rows, colomns, shelfLength, floorCount, wingCount, officeWidth, input.roomType);
        SpawnRobots();
    }

    private void SetWorldBounds(int floorCount, int officeWidth, int floorWidth, int floorHeight)
    {
        PlanManager.instance.worldBounds = new WorldBounds(-2 * floorWidth - officeWidth, 2 * floorWidth, (floorHeight + 2) * (floorCount + 1), -floorHeight);
    }
    public void SpawnWalls(int roomWidth, int roomHeight, int xRoomCount, int yRoomCount)
    {
        for (int i = 0; i < roomWidth; i++)
        {
            SpawnWallTile(outerTopWall, new Vector2(i + (roomWidth + 2) * xRoomCount, -1f + (roomHeight + 2) * yRoomCount));
            SpawnWallTile(outerBottomWall, new Vector2(i + (roomWidth + 2) * xRoomCount, roomHeight + (roomHeight + 2) * yRoomCount));
        }

        for (int i = 0; i < roomHeight; i++)
        {
            if (i == (roomHeight - 1) / 2 && Settings.instance.wingCount != 0)
            {
                SpawnRandomFloorTile(new Vector2(-1f + (roomWidth + 2) * xRoomCount, i + (roomHeight + 2) * yRoomCount));
            }
            else
            {
                SpawnWallTile(outerLeftWall, new Vector2(-1f + (roomWidth + 2) * xRoomCount, i + (roomHeight + 2) * yRoomCount));
            }
            SpawnWallTile(outerRightWall, new Vector2(roomWidth + (roomWidth + 2) * xRoomCount, i + (roomHeight + 2) * yRoomCount));
        }

        SpawnWallTile(outerCornerTopLeftWall, new Vector2(-1f + (roomWidth + 2) * xRoomCount, roomHeight + (roomHeight + 2) * yRoomCount));
        SpawnWallTile(outerCornerBottomRightWall, new Vector2(roomWidth + (roomWidth + 2) * xRoomCount, -1f + (roomHeight + 2) * yRoomCount));
        SpawnWallTile(outerCornerTopRightWall, new Vector2(roomWidth + (roomWidth + 2) * xRoomCount, roomHeight + (roomHeight + 2) * yRoomCount));
        SpawnWallTile(outerCornerBottomLeftWall, new Vector2(-1f + (roomWidth + 2) * xRoomCount, -1f + (roomHeight + 2) * yRoomCount));
    }

    public void SpawnSymmetricWalls(int roomWidth, int roomHeight, int xRoomCount, int yRoomCount, int officeWidth)
    {

        for (int i = 0; i < roomWidth; i++)
        {
            SpawnWallTile(outerTopWall, new Vector2(i + (roomWidth + 2) * xRoomCount - 4f - roomWidth - officeWidth, -1f + (roomHeight + 2) * yRoomCount));
            SpawnWallTile(outerBottomWall, new Vector2(i + (roomWidth + 2) * xRoomCount - 4f - roomWidth - officeWidth, roomHeight + (roomHeight + 2) * yRoomCount));
        }

        for (int i = 0; i < roomHeight; i++)
        {
            if (i == (roomHeight - 1) / 2)
            {
                SpawnRandomFloorTile(new Vector2(roomWidth + (roomWidth + 2) * xRoomCount - 4f - roomWidth - officeWidth, i + (roomHeight + 2) * yRoomCount));
            }
            else
            {
                SpawnWallTile(outerRightWall, new Vector2(roomWidth + (roomWidth + 2) * xRoomCount - 4f - roomWidth - officeWidth, i + (roomHeight + 2) * yRoomCount));

            }
            SpawnWallTile(outerLeftWall, new Vector2(-1f + (roomWidth + 2) * xRoomCount - 4f - roomWidth - officeWidth, i + (roomHeight + 2) * yRoomCount));
        }

        SpawnWallTile(outerCornerTopLeftWall, new Vector2(-1f + (roomWidth + 2) * xRoomCount - 4f - roomWidth - officeWidth, roomHeight + (roomHeight + 2) * yRoomCount));
        SpawnWallTile(outerCornerBottomRightWall, new Vector2(roomWidth + (roomWidth + 2) * xRoomCount - 4f - roomWidth - officeWidth, -1f + (roomHeight + 2) * yRoomCount));
        SpawnWallTile(outerCornerTopRightWall, new Vector2(roomWidth + (roomWidth + 2) * xRoomCount - 4f - roomWidth - officeWidth, roomHeight + (roomHeight + 2) * yRoomCount));
        SpawnWallTile(outerCornerBottomLeftWall, new Vector2(-1f + (roomWidth + 2) * xRoomCount - 4f - roomWidth - officeWidth, -1f + (roomHeight + 2) * yRoomCount));
    }

    public void SpawnRobots()
    {
        for (int i = 0; i < Settings.instance.agentCount; ++i)
        {
            _ = Instantiate(robot, new Vector3(0f, 0f, 0f), Quaternion.identity);
        }

    }
}

internal class Floor
{
    private readonly int rows;
    private readonly int columns;
    private readonly int shelfLength;
    private readonly int roomHeight;
    private readonly int roomWidth;
    private List<List<char>> grid = new List<List<char>>();         //representation of the archive as a grid
    public Vector2 floorEntryPoint;
    public Vector2 floorExitPoint;
    public int floorNumber;
    public List<Storage> storages = new List<Storage>();
    public int wing;
    public Floor(int rows, int columns, int shelfLength, RoomType RoomType)
    {
        this.rows = rows;
        this.columns = columns;
        this.shelfLength = shelfLength;

        if (RoomType == RoomType.SimpleSingleRow)
        {
            roomHeight = rows * 2 + 1;
            roomWidth = 1 + columns * (shelfLength + 1);
            for (int i = 0; i < roomHeight; ++i)
            {
                List<char> tmp = new List<char>();
                for (int j = 0; j < roomWidth; ++j)
                {
                    if (i % 2 == 0)
                    {
                        tmp.Add('.');   //dot counts as a "free" space
                    }
                    else
                    {
                        if (j % (shelfLength + 1) == 0)
                        {
                            tmp.Add('.');
                        }
                        else
                        {
                            tmp.Add('#');  //sharp counts as an obstacle
                        }
                    }
                }
                grid.Add(tmp);
            }
        }
        else
        if (RoomType == RoomType.AdvancedDoubleRow)
        {
            roomHeight = (rows) * 3;
            roomWidth = 1 + columns * (shelfLength + 1);
            for (int i = 0; i < roomHeight; ++i)
            {
                List<char> tmp = new List<char>();
                for (int j = 0; j < roomWidth; ++j)
                {
                    if (i % 3 == 1)
                    {
                        tmp.Add('.');   //dot counts as a "free" space
                    }
                    else
                    {
                        if (j % (shelfLength + 1) == 0)
                        {
                            tmp.Add('.');
                        }
                        else
                        {
                            tmp.Add('#');  //sharp counts as an obstacle
                        }
                    }
                }
                grid.Add(tmp);
            }
        }
        else
        if (RoomType == RoomType.SimpleDoubleRow)
        {
            roomHeight = rows * 3 + 1;
            roomWidth = 1 + columns * (shelfLength + 1);
            for (int i = 0; i < roomHeight; ++i)
            {
                List<char> tmp = new List<char>();
                for (int j = 0; j < roomWidth; ++j)
                {
                    if (i % 3 == 0)
                    {
                        tmp.Add('.');   //dot counts as a "free" space
                    }
                    else
                    {
                        if (j % (shelfLength + 1) == 0)
                        {
                            tmp.Add('.');
                        }
                        else
                        {
                            tmp.Add('#');  //sharp counts as an obstacle
                        }
                    }
                }
                grid.Add(tmp);
            }
        }
        else
        if (RoomType == RoomType.AdvancedSingleRow)
        {
            roomHeight = rows * 2 - 1;
            roomWidth = 1 + columns * (shelfLength + 1);
            for (int i = 0; i < roomHeight; ++i)
            {
                List<char> tmp = new List<char>();
                for (int j = 0; j < roomWidth; ++j)
                {
                    if (i % 2 == 1)
                    {
                        tmp.Add('.');   //dot counts as a "free" space
                    }
                    else
                    {
                        if (j % (shelfLength + 1) == 0)
                        {
                            tmp.Add('.');
                        }
                        else
                        {
                            tmp.Add('#');  //sharp counts as an obstacle
                        }
                    }
                }
                grid.Add(tmp);
            }
        }
    }

    public int Rows => rows;
    public int Columns => columns;
    public int ShelfLength => shelfLength;
    public int RoomHeight => roomHeight;
    public int RoomWidth => roomWidth;

    public List<List<char>> Grid => grid;
}

public enum RoomType
{
    NotSet,
    SimpleSingleRow,
    AdvancedSingleRow,
    SimpleDoubleRow,
    AdvancedDoubleRow
}