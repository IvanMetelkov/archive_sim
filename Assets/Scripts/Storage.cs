using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using System;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Diagnostics;

public class Storage : MonoBehaviour
{
    public static int MaxBoxCount = 27;
    public int boxCount;
    public Shelf topShelf;
    public Shelf midShelf;
    public Shelf bottomShelf;
    public int storageID;
    internal SpriteRenderer spriteRenderer;
    internal Floor rootFloor;
    public int rootFloorNumber;
    public int wing;
    public Vector2 accessPoint;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCount = 0;
        topShelf = new Shelf(ShelfLocation.Top);
        midShelf = new Shelf(ShelfLocation.Mid);
        bottomShelf = new Shelf(ShelfLocation.Bottom);
    }

    public Vector2 GetLocalPosition()
    {
        if (wing == 1)
        {
            return new Vector2(accessPoint.x, accessPoint.y - rootFloorNumber * (rootFloor.RoomHeight + 2));
        }
        else
        {
            return new Vector2(accessPoint.x + 4 + Settings.instance.officeWidth + rootFloor.RoomWidth,
                accessPoint.y - rootFloorNumber * (rootFloor.RoomHeight + 2));
        }
    }

    public bool IsFull()
    {
        if (boxCount >= MaxBoxCount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetValues(out int realBoxCount)
    {
        realBoxCount = bottomShelf.boxStacks[0].Count + bottomShelf.boxStacks[1].Count + bottomShelf.boxStacks[2].Count +
            midShelf.boxStacks[0].Count + midShelf.boxStacks[1].Count + midShelf.boxStacks[2].Count +
            topShelf.boxStacks[0].Count + topShelf.boxStacks[1].Count + topShelf.boxStacks[2].Count;
        return this.boxCount;
    }

    public void AddBoxToStorage(Box box)
    {
        box.rootStorage = this;
        if (!bottomShelf.IsFull())
        {
            bottomShelf.AddBoxToShelf(box);
        }
        else
        {
            if (!midShelf.IsFull())
            {
                midShelf.AddBoxToShelf(box);
            }
            else
            {
                topShelf.AddBoxToShelf(box);
            }
        }
        boxCount++;
        spriteRenderer.enabled = true;
    }
    public void BoxToCorner(Box box, int cornerType)
    {
        box.rootStorage = this;
        List<bool> tmp = new List<bool>();

        for (int i = 0; i < 3; ++i)
        {
            tmp.Add(CheckStorageInCorners(i));
        }

        if (bottomShelf.CheckShelfCorners(cornerType))
        {
            FindCorner(box, bottomShelf, cornerType);
        }
        else
        {
            if (midShelf.CheckShelfCorners(cornerType))
            {
                FindCorner(box, midShelf, cornerType);
            }
            else
            {
                FindCorner(box, topShelf, cornerType);
            }
        }

        for (int i = 0; i < 3; ++i)
        {
            if (i != cornerType)
            {
                if (tmp[i] == false && CheckStorageInCorners(i))
                {
                    PlanManager.instance.accessibleStorages[i].Push(this);
                }
            }
        }

        box.cornerID = cornerType;
        boxCount++;
        spriteRenderer.enabled = true;

    }
    void FindCorner(Box box, Shelf shelf, int cornerType)
    {
        if (cornerType == 2)
        {
            if (shelf.boxStacks[1].Count == 2)
            {
                BoxToStack(box, shelf, shelf.boxStacks[1], 1);
            }
            else
            {
                if (shelf.boxStacks[2].Count > 0 && shelf.boxStacks[2].Count < 3)
                {
                    BoxToStack(box, shelf, shelf.boxStacks[2], 2);
                }
            }
        }
        else
        if (cornerType == 1)
        {
            if (shelf.boxStacks[1].Count == 1)
            {
                BoxToStack(box, shelf, shelf.boxStacks[1], 1);
            }
            else
            {
                if (shelf.boxStacks[2].Count == 0)
                {
                    BoxToStack(box, shelf, shelf.boxStacks[2], 2);
                }
                else
                {
                    if (shelf.boxStacks[0].Count == 2)
                    {
                        BoxToStack(box, shelf, shelf.boxStacks[0], 0);
                    }
                }
            }
        }
        else
        {
            if (shelf.boxStacks[0].Count >= 0 && shelf.boxStacks[0].Count < 2)
            {
                BoxToStack(box, shelf, shelf.boxStacks[0], 0);
            }
            else
            {
                if (shelf.boxStacks[1].Count == 0)
                {
                    BoxToStack(box, shelf, shelf.boxStacks[1], 1);
                }
            }
        }
    }

    void BoxToStack(Box box, Shelf shelf, Stack<Box> stack, int stackID)
    {
        box.rootShelf = shelf;
        stack.Push(box);
        shelf.shelfBoxCount++;
        box.rootStack = stack;
        box.stackID = stackID;
    }

    public void BoxToStackSimple(Box box, int stack)
    {
        box.rootStorage = this;
        if (bottomShelf.boxStacks[stack].Count < 3)
        {
            box.rootShelf = bottomShelf;
            bottomShelf.boxStacks[stack].Push(box);
            bottomShelf.shelfBoxCount++;
            box.rootStack = bottomShelf.boxStacks[stack];
            box.stackID = stack;
        }
        else
        if (midShelf.boxStacks[stack].Count < 3)
        {
            box.rootShelf = midShelf;
            midShelf.boxStacks[stack].Push(box);
            midShelf.shelfBoxCount++;
            box.rootStack = midShelf.boxStacks[stack];
            box.stackID = stack;
        }
        else
        {
            if (topShelf.boxStacks[stack].Count < 3)
            {
                box.rootShelf = topShelf;
                topShelf.boxStacks[stack].Push(box);
                topShelf.shelfBoxCount++;
                box.rootStack = topShelf.boxStacks[stack];
                box.stackID = stack;
            }
        }

        boxCount++;
        spriteRenderer.enabled = true;
    }

    public bool CheckStorageByStacks(int i)
    {
        if (topShelf.boxStacks[i].Count == 3 && midShelf.boxStacks[i].Count == 3 && bottomShelf.boxStacks[i].Count == 3)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public bool CheckStorageInCorners(int type)
    {
        int count = 0;
        if (topShelf.CheckShelfCorners(type))
        {
            count++;
        }
        if (midShelf.CheckShelfCorners(type))
        {
            count++;
        }
        if (bottomShelf.CheckShelfCorners(type))
        {
            count++;
        }

        if (count == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private void OnMouseDown()
    {
        if (!PlanManager.IsPointerOverUIObject())
        {
            Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            GameObject boxMenu = canvas.gameObject.transform.Find("BoxMenu").gameObject;

            if (!boxMenu.activeSelf || PlanManager.instance.inspectedStorage == null || PlanManager.instance.inspectedStorage.storageID != this.storageID)
            {
                PlanManager.instance.inspectedStorage = this;
                PlanManager.instance.inspectedBoxes.Clear();
                boxMenu.SetActive(true);
                boxMenu.GetComponent<BoxMenu>().HideUI();
                FillBoxList(topShelf);
                FillBoxList(midShelf);
                FillBoxList(bottomShelf);

                if (boxCount == 0)
                {
                    boxMenu.transform.Find("StandEmpty").gameObject.SetActive(true);
                }
                else
                {
                    boxMenu.transform.Find("StandEmpty").gameObject.SetActive(false);
                }

                boxMenu.GetComponent<BoxMenu>().ClearLists();
                boxMenu.GetComponent<BoxMenu>().PopulateLists();
                boxMenu.GetComponent<BoxMenu>().SetUIText(this);
            }
        }
    }

    private void FillBoxList(Shelf shelf)
    {
        List<Box> frontList = MakeBoxList(shelf.boxStacks[2]);
        List<Box> middleList = MakeBoxList(shelf.boxStacks[1]);
        List<Box> backList = MakeBoxList(shelf.boxStacks[0]);

        for (int i = 0; i < 3; ++i)
        {
            PlanManager.instance.inspectedBoxes.Add(frontList[i]);
            PlanManager.instance.inspectedBoxes.Add(middleList[i]);
            PlanManager.instance.inspectedBoxes.Add(backList[i]);
        }
    }
    private List<Box> MakeBoxList(Stack<Box> boxes)
    {
        List<Box> ans = boxes.ToList();
        while (ans.Count < 3)
        {
            ans.Insert(0, new Box());
        }
        return ans;
    }
}

public class Shelf
{
    public ShelfLocation location;
    public int shelfBoxCount;
    public static int ShelfMaxBoxCount = 9;
    public List<Stack<Box>> boxStacks = new List<Stack<Box>>();


    public Shelf(ShelfLocation location)
    {
        for (int i = 0; i < 3; ++i)
        {
            Stack<Box> tmp = new Stack<Box>();
            boxStacks.Add(tmp);
        }
        this.location = location;
        shelfBoxCount = 0;
    }

    public bool IsFull()
    {
        if (shelfBoxCount == Shelf.ShelfMaxBoxCount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckShelfCorners(int type)
    {
        if (type == 2)
        {
            if ((boxStacks[2].Count > 0 && boxStacks[2].Count < 3) || boxStacks[1].Count == 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        if (type == 1)
        {
            if (boxStacks[0].Count == 2 || boxStacks[1].Count == 1 || boxStacks[2].Count == 0)
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
            if ((boxStacks[0].Count >= 0 && boxStacks[0].Count < 2) || boxStacks[1].Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public void AddBoxToShelf(Box box)
    {
        box.rootShelf = this;
        if (boxStacks[0].Count < 3)
        {
            box.rootStack = boxStacks[0];
            boxStacks[0].Push(box);
            box.stackID = 0;
        }
        else
        {
            if (boxStacks[1].Count < 3)
            {
                box.rootStack = boxStacks[1];
                boxStacks[1].Push(box);
                box.stackID = 1;
            }
            else
            {
                box.rootStack = boxStacks[2];
                boxStacks[2].Push(box);
                box.stackID = 2;
            }
        }
        shelfBoxCount++;
    }
}

public class Box
{
    public ShelfLife shelfLifeType;
    public DemandChance demandChanceType;
    public int boxID;
    public float lifeSpan;
    public float demandChance;
    public bool isBlank;
    public int arrivalDay;
    public Storage rootStorage;
    public Shelf rootShelf;
    public Stack<Box> rootStack;
    public int stackID;
    public int cornerID;
    public bool isDisposed = false;
    public Robot worker = null;
    public int residingFloor = -1;
    public List<Box> referenceList = new List<Box>();
    public Box(int day, int ID)
    {

        float rnd = Random.Range(0f, 100f);
        float tmp = Settings.instance.lifeSpan[0].weight;
        for (int i = 0; i < Settings.instance.lifeSpan.Count; ++i)
        {
            if (rnd <= tmp)
            {
                lifeSpan = Settings.instance.lifeSpan[i].value;
                shelfLifeType = Settings.instance.lifeSpan[i].shelfLifeType;
                PlanManager.instance.boxesByLife[i].Enqueue(this);
                break;
            }
            else
            {
                tmp += Settings.instance.lifeSpan[i+1].weight;
            }
        }

        rnd = Random.Range(0f, 100f);
        tmp = Settings.instance.chances[0].weight;
        for (int i = 0; i < Settings.instance.chances.Count; ++i)
        {
            if (rnd <= tmp)
            {
                demandChance = Settings.instance.chances[i].value;
                demandChanceType = Settings.instance.chances[i].demandType;
                PlanManager.instance.boxesByChance[i].Add(this);
                referenceList = PlanManager.instance.boxesByChance[i];
                break;
            }
            else
            {
                tmp += Settings.instance.chances[i + 1].weight;
            }
        }

        boxID = ID;
        isBlank = false;
        arrivalDay = day;
    }

    public Box()
    {
        isBlank = true;
    }

    public int CountBoxesToTake()
    {
        int count = 1;
        Stack<Box> tmp = new Stack<Box>();
        while (rootStack.Peek().boxID != boxID)
        {
            tmp.Push(rootStack.Pop());
            count++;
        }
        int ans = 0;
        while(tmp.Count != 0)
        {
            rootStack.Push(tmp.Pop());
        }
        for (int i = 2; i >= stackID; --i)
        {
            if (i != stackID)
            {
                if (rootShelf.boxStacks[i].Count > rootStack.Count - count)
                {
                    ans += rootShelf.boxStacks[i].Count - (rootStack.Count - count);
                }
            }
        }
        return ans + count;
    }

    public void Dispose()
    {
        Stack<Box> tmp = new Stack<Box>();

        List<bool> check = new List<bool>();
        if (Settings.instance.boxPlacement == BoxPlacement.Corners)
        {
            for (int i = 0; i < 3; ++i)
            {
                check.Add(rootStorage.CheckStorageInCorners(i));
            }
        }

        while (rootStack.Peek().boxID != this.boxID)
        {
            tmp.Push(rootStack.Pop());
        }
        rootStack.Pop();
        while (tmp.Count != 0)
        {
            rootStack.Push(tmp.Pop());
        }
        if (Settings.instance.boxPlacement == BoxPlacement.Simple)
        {
            if (rootStorage.boxCount == 27)
            {
                PlanManager.instance.accessibleStorage.Push(rootStorage);
            }
        }

        if (Settings.instance.boxPlacement == BoxPlacement.ByDemand)
        {
            if (rootStorage.topShelf.boxStacks[stackID].Count + rootStorage.midShelf.boxStacks[stackID].Count + rootStorage.bottomShelf.boxStacks[stackID].Count == 8)
            {
                PlanManager.instance.accessibleStorages[stackID].Push(rootStorage);
            }
            
        }

        if (Settings.instance.boxPlacement == BoxPlacement.Corners)
        {
            for (int i = 0; i <  3; ++i)
            {
                if (check[i] == false && rootStorage.CheckStorageInCorners(i))
                {
                    PlanManager.instance.accessibleStorages[i].Push(rootStorage);
                }
            }
        }

        if (Settings.instance.boxDisposal == BoxDisposal.DoDispose)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            int index = referenceList.BinarySearch(this, new BoxComparer());
            referenceList.RemoveAt(index);
            st.Stop();
            TimeSpan ts = st.Elapsed;
            PlanManager.instance.binarySearchTime += (float)ts.TotalSeconds;
            st.Reset();
        }

        rootStorage.boxCount--;
        rootShelf.shelfBoxCount--;
        isDisposed = true;

        if (rootStorage.boxCount == 0)
        {
            rootStorage.spriteRenderer.enabled = false;
        }
    }
}

public class BoxComparer : IComparer<Box>
{
    public int Compare(Box a, Box b)
    {
        if (a.boxID > b.boxID)
        {
            return 1;
        }
        else
        if (a.boxID < b.boxID)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}
public enum ShelfLife 
{ 
    NotSet,
    Short,
    Average,
    Long,
    Inf
}

public enum DemandChance
{
    NotSet,
    Low,
    Mid,
    High
}

public enum ShelfLocation
{
    Top,
    Mid,
    Bottom
}
