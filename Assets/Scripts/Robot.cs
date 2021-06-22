using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Robot : MovingObject
{
    public List<List<char>> grid;
    public int idCount;
    private Queue<Vector2> currentPathway;
    public Queue<Task> taskList;
    public Queue<Task> doneTasks;
    public Action currentAction = null;
    public Task currentTask;
    public int currentFloor;
    internal Floor curFloor = null;
    public float workStartTime = 0f;
    public string instructions;
    public bool isBusy = false;
    public bool isIdleInOffice = true;
    public int doneTaskCount = 0;

    public GameObject description;
    private GameObject descriptionInstance;

    private GameObject finishedMenu;
    private GameObject finScrollView;
    private GameObject finScrollViewPort;
    private GameObject finishedListContent;

    public List<GameObject> futureTasks;
    private List<GameObject> finishedTasks;
    private GameObject futureMenu;
    private GameObject futureScrollView;
    private GameObject futureScrollViewPort;
    public GameObject futureListContent;

    private GameObject currMenu;

    private List<DetectedCollision> collisions;

    private GameObject errField;
    private GameObject timeField;
    private GameObject descField;
    public float ownTimer = 0;

    private GameObject collisionMenu;
    private GameObject collisionScrollView;
    private GameObject collisionScrollViewPort;
    private GameObject collisionListContent;

    public GameObject taskItem;

    public int discreteFloorNumber = 0;
    internal Floor discretePrevFloor = null;
    public Vector2 discretePos;
    public Task discreteCurrentTask = null;
    public Queue<Task> discreteTaskList = new Queue<Task>();
    public bool discreteState = false;
    public bool discreteIsInOffice = true;
    public Queue<Request> processingRequests = new Queue<Request>();
    public Queue<Request> duplicateRequests = new Queue<Request>();
    public bool isHeadingToOffice = false;



    protected override void Awake()
    {
        InitQueues();
        GenerateStartingPosition();
        grid = PlanManager.instance.floors[currentFloor].Grid;
        finishedTasks = new List<GameObject>();
        futureTasks = new List<GameObject>();
        collisions = new List<DetectedCollision>();
        PlanManager.instance.AddRobotToList(this);
        InstantiateCanvasUI();
        base.Awake();
    }

    private void Update()
    {
        if (PlanManager.instance == null || PlanManager.instance.onPause)
        {

        }
        else
        {
            if (currentTask == null && currentAction == null && taskList.Count == 0 || currentTask == null && taskList.Count != 0 
                && PlanManager.instance.globalTimer < taskList.Peek().startTime)
            {
                UpdateStatus(idCount, "idle", "");
                isBusy = false;
                descField.GetComponent<Text>().text = "";
                timeField.GetComponent<Text>().text = "";
                errField.SetActive(true);
                return;
            }
            else
            {
                if (currentTask == null && taskList.Count != 0 && currentAction == null && 
                    PlanManager.instance.globalTimer >= taskList.Peek().startTime)
                {
                    boxCollider.enabled = true;
                    spriteRenderer.enabled = true;
                    currentTask = taskList.Dequeue();
                    isBusy = true;

                    RemoveTaskFromView(futureListContent);
                    currentTask.currentTime = currentTask.startTime;
                    descField.GetComponent<Text>().text = currentTask.description;
                    string taskStartTime = PlanManager.ParseTimer(currentTask.startTime, out string miliSeconds);
                    timeField.GetComponent<Text>().text = taskStartTime + miliSeconds;
                    errField.SetActive(false);
                }

                if (currentTask != null)
                {
                    HandleTask();
                }
            }
        }
    }

    public void SetFuturetasks()
    {
        List<Task> tmp = taskList.ToList();

        foreach (Task task in tmp)
        {
            AddTaskToView(task, futureListContent, futureTasks, task.startTime);
        }
    }

    public void InstantiateCanvasUI()
    {
        GameObject toInstantiate = description;
        RectTransform objectRectTransform = description.GetComponent<RectTransform>();
        descriptionInstance = Instantiate(toInstantiate, new Vector3(transform.position.x, transform.position.y - 0.7f - objectRectTransform.rect.height * 0.02f / 2.0f , transform.position.z), Quaternion.identity) as GameObject;
        descriptionInstance.transform.SetParent(this.transform);

        descriptionInstance.transform.Find("RobotID").GetComponent<Text>().text = "This robot ID is: " + idCount;

        finishedMenu = descriptionInstance.transform.Find("FinishedTaskMenu").gameObject;
        finScrollView = finishedMenu.transform.Find("FinishedTaskList").gameObject;
        finScrollViewPort = finScrollView.transform.Find("ScrollViewPort").gameObject;
        finishedListContent = finScrollViewPort.transform.Find("Content").gameObject;

        futureMenu = descriptionInstance.transform.Find("FutureTasksMenu").gameObject;
        futureScrollView = futureMenu.transform.Find("FutureTaskList").gameObject;
        futureScrollViewPort = futureScrollView.transform.Find("ScrollViewPort").gameObject;
        futureListContent = futureScrollViewPort.transform.Find("Content").gameObject;

        currMenu = descriptionInstance.transform.Find("CurrentTaskMenu").gameObject;
        errField = currMenu.transform.Find("ErrField").gameObject;
        timeField = currMenu.transform.Find("TimeField").gameObject;
        descField = currMenu.transform.Find("DescField").gameObject;

        collisionMenu = descriptionInstance.transform.Find("CollisionsMenu").gameObject;
        collisionScrollView = collisionMenu.transform.Find("CollisionList").gameObject;
        collisionScrollViewPort = collisionScrollView.transform.Find("ScrollViewPort").gameObject;
        collisionListContent = collisionScrollViewPort.transform.Find("Content").gameObject;
    }

    internal static Vector2 FromLocalToGlobal(Floor floor, Vector2 pos)
    {
        if (floor.wing == 1)
        {
            return new Vector2(pos.x, pos.y + floor.floorNumber * (floor.RoomHeight + 2));
        }
        else
        {
            return new Vector2(pos.x - 4 - Settings.instance.officeWidth - floor.RoomWidth, pos.y + floor.floorNumber * (floor.RoomHeight + 2));
        }
    }

    internal static Vector2 FromGlobalToLocal(Floor floor, Vector2 pos)
    {
        if (floor.wing == 1)
        {
            return new Vector2(pos.x, pos.y - floor.floorNumber * (floor.RoomHeight + 2));
        }
        else
        {
            return new Vector2(pos.x + 4 + Settings.instance.officeWidth + floor.RoomWidth, pos.y - floor.floorNumber * (floor.RoomHeight + 2));
        }
    }

    public void InitQueues()
    {
        currentPathway = new Queue<Vector2>();
        taskList = new Queue<Task>();
        doneTasks = new Queue<Task>();
    }

    public void GenerateStartingPosition()
    {
        currentFloor = 0;
        curFloor = null;
        Vector3 startPos;
        startPos.x = -Settings.instance.officeWidth / 2f - 2;
        startPos.y = PlanManager.instance.floors[0].RoomHeight / 2;
        startPos.z = transform.position.z;
        transform.position = startPos;
    }

    public void HandleTask()
    {
        if (currentAction == null && currentTask.actions.Count == 0)
        {
            AddTaskToView(currentTask, finishedListContent, finishedTasks, currentTask.endTime);
            doneTaskCount++;
            currentTask = null;
            PlanManager.instance.actualTimestamps[idCount].Add(new ValueAndWeight(idCount, PlanManager.instance.globalTimer));
        }
        else
        {
            if (currentAction == null && currentTask.actions.Count != 0)
            {
                currentAction = currentTask.actions.Dequeue();
                UpdateStatus(idCount, "busy", currentAction.description);
            }

            if (currentAction.GetType() == typeof(MoveAction))
            {
                if (currentPathway.Count == 0)
                {
                    MoveAction tmp = (MoveAction)currentAction;
                    currentPathway = tmp.path;
                }
                currentAction.currentTime += PlanManager.instance.lastTimeStep;

                if (Mathf.Abs(transform.position.x - currentPathway.Peek().x) < float.Epsilon && Mathf.Abs(transform.position.y 
                    - currentPathway.Peek().y) < float.Epsilon)
                {
                    transform.position = currentPathway.Peek();
                    currentPathway.Dequeue();
                    if (currentPathway.Count == 0)
                    {
                        currentAction = null;
                    }
                }
                else
                { 
                    transform.position = Vector2.MoveTowards(transform.position, currentPathway.Peek(), moveSpeed * PlanManager.instance.lastTimeStep);
                }
            }
            else
            if (currentAction.GetType() == typeof(TakeBox))
            {
                TakeBox tmp = (TakeBox)currentAction;
                tmp.currentTime += PlanManager.instance.lastTimeStep;
                if (tmp.currentTime < tmp.timeToComplete)
                {

                }
                else
                {
                    currentAction = null;
                }
            }
            else
            if (currentAction.GetType() == typeof(MoveFloors))
            { 
                MoveFloors tmp = (MoveFloors)currentAction;
                tmp.currentTime += PlanManager.instance.lastTimeStep;
                currentFloor = tmp.endFloor.floorNumber;
                curFloor = tmp.endFloor;

                transform.position = FromLocalToGlobal(tmp.endFloor, tmp.endFloor.floorExitPoint);

                boxCollider.enabled = false;
                Color afkColor = spriteRenderer.color;
                afkColor.a = 0.3f;
                spriteRenderer.color = afkColor;

                if (tmp.currentTime < tmp.timeToComplete)
                {

                }
                else
                {
                    boxCollider.enabled = true;
                    afkColor.a = 1f;
                    spriteRenderer.color = afkColor;
                    currentAction = null;
                }
            }
            else
            if (currentAction.GetType() == typeof(LeaveOffice))
            {
                isIdleInOffice = false;
                LeaveOffice tmp = (LeaveOffice)currentAction;
                tmp.currentTime += PlanManager.instance.lastTimeStep;
                curFloor = tmp.endFloor;
                currentFloor = tmp.endFloor.floorNumber;

                transform.position = FromLocalToGlobal(tmp.endFloor, tmp.endFloor.floorEntryPoint);

                boxCollider.enabled = false;
                Color afkColor = spriteRenderer.color;
                afkColor.a = 0.3f;
                spriteRenderer.color = afkColor;

                if (tmp.currentTime < tmp.timeToComplete)
                {

                }
                else
                {
                    boxCollider.enabled = true;
                    afkColor.a = 1f;
                    spriteRenderer.color = afkColor;
                    currentAction = null;
                }
            }
            else
            if (currentAction.GetType() == typeof(EnterOffice))
            {
                isIdleInOffice = true;
                isBusy = false;
                EnterOffice tmp = (EnterOffice)currentAction;
                tmp.currentTime += PlanManager.instance.lastTimeStep;
                curFloor = null;
                currentFloor = tmp.endFloor;

                transform.position =  new Vector2(-Settings.instance.officeWidth / 2 - 2, PlanManager.instance.floors[0].RoomHeight / 2 + currentFloor * 
                    (PlanManager.instance.floors[0].RoomHeight + 2));
                boxCollider.enabled = false;
                Color afkColor = spriteRenderer.color;
                afkColor.a = 0.3f;
                spriteRenderer.color = afkColor;

                if (tmp.currentTime < tmp.timeToComplete)
                {

                }
                else
                {
                    currentAction = null;
                }
            }
            else
            if (currentAction.GetType() == typeof(CrossOffice))
            {
                CrossOffice tmp = (CrossOffice)currentAction;
                tmp.currentTime += PlanManager.instance.lastTimeStep;
                curFloor = tmp.endFloor;

                transform.position = FromLocalToGlobal(tmp.endFloor, tmp.endFloor.floorEntryPoint);

                boxCollider.enabled = false;
                Color afkColor = spriteRenderer.color;
                afkColor.a = 0.3f;
                spriteRenderer.color = afkColor;

                if (tmp.currentTime < tmp.timeToComplete)
                {

                }
                else
                {
                    boxCollider.enabled = true;
                    afkColor.a = 1f;
                    spriteRenderer.color = afkColor;
                    currentAction = null;
                }
            }
        }
    }

    public void AddTaskToView(Task task, GameObject content, List<GameObject> list, float timeStamp)
    {
        GameObject scrollItem = Instantiate(taskItem) as GameObject;
        scrollItem.transform.SetParent(content.transform, false);
        string taskStart = PlanManager.ParseTimer(timeStamp, out string miliSeconds);
        scrollItem.transform.Find("TaskID").gameObject.GetComponent<Text>().text = taskStart + miliSeconds;
        scrollItem.transform.Find("Description").gameObject.GetComponent<Text>().text = task.description;
        list.Add(scrollItem);
    }
    
    private void RemoveTaskFromView(GameObject content)
    {
        futureTasks.RemoveAt(0);
        GameObject oldTaskInstance = content.transform.GetChild(0).gameObject;
        Destroy(oldTaskInstance);
    }

    public int BFS(Target start, Target end, out Queue<Vector2> path)
    {
        int steps = 0;

        Vector2 thisUnitPosition = start.point;
        Vector2 targetPosition = end.point;

        Queue<Vector2> queue = new Queue<Vector2>();
        Vector2 blank = new Vector2(-1, -1);
        int nodesLeft = 1;
        int nodesNextGen = 0;

        bool finished = false;

        List<List<bool>> visited = new List<List<bool>>();
        List<List<Vector2>> parents = new List<List<Vector2>>();


        for (int i = 0; i < start.floor.RoomHeight; ++i)
        {
            List<bool> tmp = new List<bool>();
            List<Vector2> tmp2 = new List<Vector2>();
            for (int j = 0; j < start.floor.RoomWidth; ++j)
            {
                tmp.Add(false);
                tmp2.Add(blank);
            }
            visited.Add(tmp);
            parents.Add(tmp2);
        }

        queue.Enqueue(thisUnitPosition);
        visited[(int)thisUnitPosition.y][(int)thisUnitPosition.x] = true;

        while (queue.Count > 0)
        {
            Vector2 tempPoint = queue.Dequeue();
            if (tempPoint.y == targetPosition.y && tempPoint.x == targetPosition.x)
            {
                finished = true;
                break;
            }
            FindNeighbours(tempPoint, queue, ref visited, ref nodesNextGen, ref parents);
            nodesLeft--;

            if (nodesLeft == 0)
            {
                nodesLeft = nodesNextGen;
                nodesNextGen = 0;
                steps++;
            }
        }

        path = GetPath(targetPosition, parents, end.floor);
        return steps;
    }

    public void FindNeighbours(Vector2 point, Queue<Vector2> queue, ref List<List<bool>> visited, ref int nodesNextGen, ref List<List<Vector2>> parents)
    {

        Vector2 up = new Vector2(0, 1);
        Vector2 down = new Vector2(0, -1);
        Vector2 right = new Vector2(1, 0);
        Vector2 left = new Vector2(-1, 0);
        List<Vector2> directions = new List<Vector2>
        {
            up,
            down,
            right,
            left
        };

        foreach (Vector2 direction in directions)
        {
            Vector2 tmp = point + direction;

            if (tmp.x < 0 || tmp.y < 0 || tmp.x >= PlanManager.instance.floors[currentFloor].RoomWidth || 
                tmp.y >= PlanManager.instance.floors[currentFloor].RoomHeight)
            {
                continue;
            }

            if (visited[(int)tmp.y][(int)tmp.x] || grid[(int)tmp.y][(int)tmp.x] == '#')
            {
                continue;
            }

            parents[(int)tmp.y][(int)tmp.x] = point;

            queue.Enqueue(tmp);
            visited[(int)tmp.y][(int)tmp.x] = true;
            nodesNextGen++;
        }

    }

    internal Queue<Vector2> GetPath(Vector2 target, List<List<Vector2>> parents, Floor currentFloor)
    {
        List<Vector2> path = new List<Vector2>();
        while (target.x != -1 && target.y != -1)
        {
            path.Add(target);
            target = parents[(int)target.y][(int)target.x];
        }
        path.Reverse();
        Queue<Vector2> qPath = new Queue<Vector2>();
        foreach (Vector2 point in path)
        {
            Vector2 tmp = FromLocalToGlobal(currentFloor, point);
            qPath.Enqueue(tmp);
        }
        return qPath;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Robot"))
        {
            DetectedCollision collision = new DetectedCollision(PlanManager.instance.globalTimer, transform.position, col.gameObject);
            collisions.Add(collision);

            GameObject scrollItem = Instantiate(taskItem);
            scrollItem.transform.SetParent(collisionListContent.transform, false);
            string timeStamp = PlanManager.ParseTimer(PlanManager.instance.globalTimer, out string miliSeconds);
            scrollItem.transform.Find("TaskID").gameObject.GetComponent<Text>().text = timeStamp + miliSeconds;
            scrollItem.transform.Find("Description").gameObject.GetComponent<Text>().text = transform.position.ToString();
        }
    }

    private void OnMouseDown()
    {
        if (!PlanManager.IsPointerOverUIObject())
        {
            descriptionInstance.SetActive(true);
        }
    }

    public void UpdateStatus(int id, string state, string description)
    {
        PlanManager.instance.statusList[id].transform.Find("StateText").gameObject.GetComponent<Text>().text = state;
        PlanManager.instance.statusList[id].transform.Find("DescText").gameObject.GetComponent<Text>().text = description;
    }

    internal float GetTaskTime(Task task, out Target startPos)
    {
        float ans = 0f;
        Vector2 startPosition;
        Target start;
        if (discreteState)
        {
            if (discreteTaskList.Count != 0)
            {
                List<Task> tmp = discreteTaskList.ToList();
                if (tmp[tmp.Count - 1].target.floor == null)
                {
                    task.BuildActionList(null, tmp[tmp.Count - 1].target.floorNumber);
                    LeaveOffice tmpLeave = new LeaveOffice(tmp[tmp.Count - 1].target.floorNumber, tmp[tmp.Count - 1].target.floor);
                    start = new Target(tmpLeave.endFloor, tmpLeave.endFloor.floorEntryPoint);
                    startPos = new Target(tmpLeave.endFloor, tmpLeave.endFloor.floorEntryPoint);
                }
                else
                {
                    task.BuildActionList(tmp[tmp.Count - 1].target.floor, tmp[tmp.Count - 1].target.floor.floorNumber);
                    startPosition = tmp[tmp.Count - 1].target.point;
                    start = new Target(tmp[tmp.Count - 1].target.floor, startPosition);
                    startPos = new Target(tmp[tmp.Count - 1].target.floor, startPosition);
                }
            }
            else
            {
                if (discreteCurrentTask != null && discreteCurrentTask.target.floor == null)
                {
                    task.BuildActionList(null, discreteCurrentTask.target.floorNumber);
                    LeaveOffice tmpLeave = new LeaveOffice(discreteCurrentTask.target.floorNumber, discreteCurrentTask.target.floor);
                    start = new Target(tmpLeave.endFloor, tmpLeave.endFloor.floorEntryPoint);
                    startPos = new Target(tmpLeave.endFloor, tmpLeave.endFloor.floorEntryPoint);
                }
                else
                {
                    task.BuildActionList(discreteCurrentTask.target.floor, discreteCurrentTask.target.floorNumber);
                    startPosition = discreteCurrentTask.target.point;
                    start = new Target(discreteCurrentTask.target.floor, startPosition);
                    startPos = new Target(discreteCurrentTask.target.floor, startPosition);
                }
            }
        }
        else
        {
            if (discreteIsInOffice)
            {
                task.BuildActionList(null, discreteFloorNumber);
                LeaveOffice tmp = new LeaveOffice(discreteFloorNumber, task.target.floor);
                start = new Target(tmp.endFloor, tmp.endFloor.floorEntryPoint);
                startPos = new Target(tmp.endFloor, tmp.endFloor.floorEntryPoint);
            }
            else
            {
                task.BuildActionList(discretePrevFloor, discreteFloorNumber);
                startPosition = FromGlobalToLocal(discretePrevFloor, discretePos);
                start = new Target(discretePrevFloor, startPosition);
                startPos = new Target(discretePrevFloor, startPosition);
            }
        }

        while (task.actions.Count != 0)
        {
            Action tmp = task.actions.Dequeue();
            if (tmp.GetType() == typeof(MoveAction))
            {
                int steps;
                MoveAction tmpType = (MoveAction)tmp;
                steps = BFS(start, tmpType.point, out Queue<Vector2> _);
                ans += (float)steps / moveSpeed;

                start = new Target(tmpType.point.floor, tmpType.point.point);
            }
            else
            if (tmp.GetType() == typeof(MoveFloors))
            {
                MoveFloors tmpType = (MoveFloors)tmp;
                start = new Target(tmpType.endFloor, tmpType.endFloor.floorExitPoint);

                ans += Mathf.Abs(tmpType.startFloor - tmpType.endFloor.floorNumber) * Settings.instance.minTimeToMoveFloors;
            }
            else
            if (tmp.GetType() == typeof(LeaveOffice))
            {
                LeaveOffice tmpType = (LeaveOffice)tmp;
                start = new Target(tmpType.endFloor, tmpType.endFloor.floorEntryPoint);

                ans += Settings.instance.minTimeToLeave;
            }
            else
            if (tmp.GetType() == typeof(CrossOffice))
            {
                CrossOffice tmpType = (CrossOffice)tmp;
                start = new Target(tmpType.endFloor, tmpType.endFloor.floorEntryPoint);
                ans += Settings.instance.minTimeToCross;

            }
            if (tmp.GetType() == typeof(EnterOffice))
            {
                EnterOffice tmpType = (EnterOffice)tmp;
                start = new Target(tmpType.endFloor);
                ans += Settings.instance.minTimeToLeave;
            }
            else
            if (tmp.GetType() == typeof(TakeBox))
            {

            }
            else
            if (tmp.GetType() == typeof(CrossAndMove))
            {
                CrossAndMove tmpType = (CrossAndMove)tmp;

                int crossSteps = BFS(new Target(start.floor, start.point), new Target(start.floor, start.floor.floorEntryPoint), out Queue<Vector2> _);
                int moveSteps = BFS(new Target(start.floor, start.point), new Target(start.floor, start.floor.floorExitPoint), out Queue<Vector2> _);

                Floor interFloor;
                Queue<Action> qClone = new Queue<Action>();
                
                while (task.actions.Count != 0)
                {
                    qClone.Enqueue(task.actions.Dequeue());
                }

                if (moveSteps < crossSteps)
                {
                    task.actions.Enqueue(new MoveAction(new Target(start.floor, start.floor.floorExitPoint)));
                    if (tmpType.endWing == 2)
                    {
                        interFloor = PlanManager.instance.floors[tmpType.endFloor.floorNumber];
                    }
                    else
                    {
                        interFloor = PlanManager.instance.secondWingFloors[tmpType.endFloor.floorNumber];
                    }
                    task.actions.Enqueue(new MoveFloors(start.floor.floorNumber, interFloor));
                    task.actions.Enqueue(new MoveAction(new Target(interFloor, interFloor.floorEntryPoint)));
                    task.actions.Enqueue(new CrossOffice(tmpType.endWing, tmpType.endFloor));
                    task.actions.Enqueue(new MoveAction(new Target(tmpType.endFloor, tmpType.target.point)));
                }
                else
                {
                    task.actions.Enqueue(new MoveAction(new Target(start.floor, start.floor.floorEntryPoint)));
                    if (tmpType.endWing == 2)
                    {
                        interFloor = PlanManager.instance.secondWingFloors[start.floor.floorNumber];
                    }
                    else
                    {
                        interFloor = PlanManager.instance.floors[start.floor.floorNumber];
                    }
                    task.actions.Enqueue(new CrossOffice(tmpType.endWing, start.floor));
                    task.actions.Enqueue(new MoveAction(new Target(interFloor, interFloor.floorExitPoint)));
                    task.actions.Enqueue(new MoveFloors(interFloor.floorNumber, tmpType.endFloor));
                    task.actions.Enqueue(new MoveAction(new Target(tmpType.endFloor, tmpType.target.point)));
                }

                while (qClone.Count != 0)
                {
                    task.actions.Enqueue(qClone.Dequeue());
                }

            }
        }

        return ans;
    }

    internal float GetRealTimeCompletion(Task task, int boxesToTake = 0)
    {
        float ans = 0;
        Target tmpTarget = task.startTarget;
        Queue<Action> actionQClone = new Queue<Action>();

        while (task.actions.Count != 0)
        {
            Action tmp = task.actions.Dequeue();
            if (tmp.GetType() == typeof(MoveAction))
            {
                int steps;
                MoveAction tmpType = (MoveAction)tmp;
                steps = BFS(tmpTarget, tmpType.point, out Queue<Vector2> path);
                ans += (float)steps / MovingObject.moveSpeed;
                tmpTarget = new Target(tmpType.point.floor, tmpType.point.point);
                tmpType.path = path;
                actionQClone.Enqueue(tmp);
            }
            else
            if (tmp.GetType() == typeof(MoveFloors))
            {
                MoveFloors tmpType = (MoveFloors)tmp;
                tmpTarget = new Target(tmpType.endFloor, tmpType.endFloor.floorExitPoint);
                for (int i = 0; i < Mathf.Abs(tmpType.startFloor - tmpType.endFloor.floorNumber); ++i)
                {
                    tmp.timeToComplete += Random.Range(Settings.instance.minTimeToMoveFloors, Settings.instance.maxTimeToMoveFloors);
                }
                ans += tmp.timeToComplete;
                actionQClone.Enqueue(tmp);
            }
            else
            if (tmp.GetType() == typeof(LeaveOffice))
            {
                LeaveOffice tmpType = (LeaveOffice)tmp;
                tmpTarget = new Target(tmpType.endFloor, tmpType.endFloor.floorEntryPoint);
                tmp.timeToComplete = Random.Range(Settings.instance.minTimeToLeave, Settings.instance.maxTimeToLeave);
                ans += tmp.timeToComplete;
                actionQClone.Enqueue(tmp);
            }
            else
            if (tmp.GetType() == typeof(CrossOffice))
            {
                CrossOffice tmpType = (CrossOffice)tmp;
                tmpTarget = new Target(tmpType.endFloor, tmpType.endFloor.floorEntryPoint);
                if (processingRequests.Count > 1)
                {
                    Queue<Request> qClone = new Queue<Request>();
                    List<Box> originalBoxes = new List<Box>();

                    while (processingRequests.Count > 1)
                    {
                        processingRequests.Peek().arrivalTime = ans + task.startTime;
                        processingRequests.Peek().box.residingFloor = tmpType.endFloor.floorNumber;
                        PlanManager.instance.RemoveBoxFromList(processingRequests.Peek().box, PlanManager.instance.processingBoxes);
                        originalBoxes.Add(processingRequests.Peek().box);
                        PlanManager.instance.floorBoxes[tmpType.endFloor.floorNumber].Add(processingRequests.Peek().box);
                        qClone.Enqueue(processingRequests.Dequeue());
                    }

                    Request lastAddedRequest = processingRequests.Dequeue();

                    while (qClone.Count != 0)
                    {
                        processingRequests.Enqueue(qClone.Dequeue());
                    }

                    while (duplicateRequests.Count != 0)
                    {
                        if (PlanManager.CheckIfBoxInList(duplicateRequests.Peek().box, originalBoxes))
                        {
                            duplicateRequests.Peek().arrivalTime = ans + task.startTime;
                            processingRequests.Enqueue(duplicateRequests.Dequeue());
                        }
                        else
                        {
                            qClone.Enqueue(duplicateRequests.Dequeue());
                        }
                    }

                    while (qClone.Count != 0)
                    {
                        duplicateRequests.Enqueue(qClone.Dequeue());
                    }

                    RequestReturn reqReturn = new RequestReturn(processingRequests, tmpType.endFloor.floorNumber, task.startTime + ans);
                    processingRequests.Enqueue(lastAddedRequest);
                    PlanManager.instance.events.Add(reqReturn);
                }
                tmp.timeToComplete = Random.Range(Settings.instance.minTimeToCross, Settings.instance.maxTimeToCross);
                ans += tmp.timeToComplete;
                actionQClone.Enqueue(tmp);
            }
            else
            if (tmp.GetType() == typeof(EnterOffice))
            {
                EnterOffice tmpType = (EnterOffice)tmp;
                tmp.timeToComplete = Random.Range(Settings.instance.minTimeToLeave, Settings.instance.maxTimeToLeave);
                tmpTarget = new Target(tmpType.endFloor);
                ans += tmp.timeToComplete;
                if (Settings.instance.simType == SimType.Closest)
                {
                    this.discreteIsInOffice = true;
                    this.discreteState = false;
                    discretePrevFloor = null;
                    discreteFloorNumber = tmpType.endFloor;
                }

                actionQClone.Enqueue(tmp);
            }
            else
            if (tmp.GetType() == typeof(CrossAndMove))
            {
                CrossAndMove tmpType = (CrossAndMove)tmp;

                int crossSteps = BFS(new Target(tmpTarget.floor, tmpTarget.point), new Target(tmpTarget.floor, tmpTarget.floor.floorEntryPoint), out Queue<Vector2> _);
                int moveSteps = BFS(new Target(tmpTarget.floor, tmpTarget.point), new Target(tmpTarget.floor, tmpTarget.floor.floorExitPoint), out Queue<Vector2> _);

                Floor interFloor;
                Queue<Action> qClone = new Queue<Action>();

                while (task.actions.Count != 0)
                {
                    qClone.Enqueue(task.actions.Dequeue());
                }

                if (moveSteps < crossSteps)
                {
                    task.actions.Enqueue(new MoveAction(new Target(tmpTarget.floor, tmpTarget.floor.floorExitPoint)));
                    if (tmpType.endWing == 2)
                    {
                        interFloor = PlanManager.instance.floors[tmpType.endFloor.floorNumber];
                    }
                    else
                    {
                        interFloor = PlanManager.instance.secondWingFloors[tmpType.endFloor.floorNumber];
                    }
                    task.actions.Enqueue(new MoveFloors(tmpTarget.floor.floorNumber, interFloor));
                    task.actions.Enqueue(new MoveAction(new Target(interFloor, interFloor.floorEntryPoint)));
                    task.actions.Enqueue(new CrossOffice(tmpType.endWing, tmpType.endFloor));
                    task.actions.Enqueue(new MoveAction(new Target(tmpType.endFloor, tmpType.target.point)));
                }
                else
                {
                    task.actions.Enqueue(new MoveAction(new Target(tmpTarget.floor, tmpTarget.floor.floorEntryPoint)));
                    if (tmpType.endWing == 2)
                    {
                        interFloor = PlanManager.instance.secondWingFloors[tmpTarget.floor.floorNumber];
                    }
                    else
                    {
                        interFloor = PlanManager.instance.floors[tmpTarget.floor.floorNumber];
                    }
                    task.actions.Enqueue(new CrossOffice(tmpType.endWing, tmpTarget.floor));
                    task.actions.Enqueue(new MoveAction(new Target(interFloor, interFloor.floorExitPoint)));
                    task.actions.Enqueue(new MoveFloors(interFloor.floorNumber, tmpType.endFloor));
                    task.actions.Enqueue(new MoveAction(new Target(tmpType.endFloor, tmpType.target.point)));
                }

                while (qClone.Count != 0)
                {
                    task.actions.Enqueue(qClone.Dequeue());
                }

            }
        }

        while (actionQClone.Count != 0)
        {
            task.actions.Enqueue(actionQClone.Dequeue());
        }

        if (boxesToTake != 0)
        {
            TakeBox takingBox = new TakeBox(boxesToTake);
            ans += takingBox.timeToComplete;
            PlanManager.instance.boxTakingTime.Add(takingBox.timeToComplete);
            task.actions.Enqueue(takingBox);
        }

        return ans;
    }

}

public class Target
{
    public int floorNumber;
    public Vector2 point;
    internal Floor floor;

    public Target(int floorNumber)
    {
       this.floorNumber = floorNumber;
       floor = null;
    }

    internal Target(Floor floor, Vector2 point)
    {
        this.floor = floor;
        this.point = point;
        floorNumber = floor.floorNumber;
    }
}

public abstract class Action
{
    public float startTime = 0f;
    public float timeToComplete = 0f;
    public float currentTime = 0f;
    public string description;

    public Action(string description)
    {
        this.description = description;
    }
}

public class TakeBox : Action
{
    public TakeBox(int boxesToTake) : base("Picking up the box")
    {
        for (int i = 0; i < boxesToTake; ++i)
        {
            timeToComplete += Random.Range(Settings.instance.minTimeToTake, Settings.instance.maxTimeToTake);
        }

        for (int i = 0; i < boxesToTake - 1; ++i)
        {
            timeToComplete += Random.Range(Settings.instance.minTimeToPlace, Settings.instance.maxTimeToPlace);
        }

        //description = "Picking up the box";
    }
}

public class PlaceBox : Action
{
    public PlaceBox(int boxesToMove) : base("Placing a box")
    {
        for (int i = 0; i < boxesToMove; ++i)
        {
            timeToComplete += Random.Range(Settings.instance.minTimeToTake, Settings.instance.maxTimeToTake);
        }

        for (int i = 0; i < boxesToMove + 1; ++i)
        {
            timeToComplete += Random.Range(Settings.instance.minTimeToPlace, Settings.instance.maxTimeToPlace);
        }
    }
}
public class MoveAction : Action
{
    public Target point;
    public Queue<Vector2> path = new Queue<Vector2>();
    public MoveAction(Target point) : base("Moving to: " + point.point.x + ';' + point.point.y + " on F" + point.floorNumber + " " + point.floor.wing)
    {
        this.point = point;
        //description = "Moving to: " + point.point.x + ';' + point.point.y + " on F" + point.floorNumber + " " + point.floor.wing; 
    }
}

public class LeaveOffice : Action
{
    internal int startFloor;
    internal Floor endFloor;
    internal LeaveOffice (int startFloor, Floor endFloor) : base("Leaving office F" + startFloor)
    {
        this.startFloor = startFloor;
        if (endFloor.wing == 2)
        {
            this.endFloor = PlanManager.instance.secondWingFloors[startFloor];
        }
        else
        {
            this.endFloor = PlanManager.instance.floors[startFloor];
        }

        //description = "Leaving office F" + startFloor;
    }
}

public class EnterOffice : Action
{
    public int startFloor;
    public int endFloor;

    public EnterOffice(int endFloor) : base("Entering office F" + endFloor)
    {
        this.endFloor = endFloor;
        //description = "Entering office F" + endFloor;
    }
}

public class MoveFloors : Action
{
    public int startFloor;
    internal Floor endFloor;

    internal MoveFloors(int startFloor, Floor endFloor) : base("Going from F" + startFloor + " to F" + endFloor.floorNumber)
    {
        this.startFloor = startFloor;
        this.endFloor = endFloor;
        //description = "Going from F" + startFloor + " to F" + endFloor.floorNumber;
    }
}

public class CrossOffice : Action
{
    public int endWing;
    internal Floor endFloor;
    internal CrossOffice(int endWing, Floor startFloor) : base("Crossing office on F" + startFloor.floorNumber)
    {
        this.endWing = endWing;
        if (endWing == 2)
        {
            endFloor = PlanManager.instance.secondWingFloors[startFloor.floorNumber];
        }
        else
        {
            endFloor = PlanManager.instance.floors[startFloor.floorNumber];
        }
        //description = "Crossing office on F" + startFloor.floorNumber;
    }
}

public class CrossAndMove : Action
{
    internal Floor startFloor;
    internal Floor endFloor;
    public int endWing;
    public Target target;

    internal CrossAndMove(Floor start, Floor end, int endWing, Target target) : base(string.Empty)
    {
        this.startFloor = start;
        this.endFloor = end;
        this.endWing = endWing;
        this.target = target;
    }
}

public class DetectedCollision
{
    public float timeOfDetection;
    public Vector2 position;
    public GameObject collisionTarget;

    public DetectedCollision (float time, Vector2 position, GameObject gameObject)
    {
        timeOfDetection = time;
        this.position = position;
        collisionTarget = gameObject;
    }
}

public class Task
{
    public Target target;
    public Queue<Action> actions = new Queue<Action>();
    public string description;
    public float startTime = 0;
    public float endTime = 0;
    public float currentTime = 0;
    public Target startTarget;

    public Task(Target target)
    {
        if (target.floor != null)
        {
            description = "Take a box";
        }
        else
        {
            description = "Go to office on F: " + target.floorNumber;
        }
        this.target = target;
    }

    internal void BuildActionList(Floor startFloor, int startFloorNumber)
    {
        if (target.floor == null)
        {
            actions.Enqueue(new MoveAction(new Target(startFloor, startFloor.floorEntryPoint)));
            actions.Enqueue(new EnterOffice(startFloorNumber));
        }
        else
        {

            if (startFloor == null)
            {
                actions.Enqueue(new LeaveOffice(startFloorNumber, target.floor));
                if (target.floor.wing == 1)
                {
                    startFloor = PlanManager.instance.floors[startFloorNumber];
                }
                else
                {
                    startFloor = PlanManager.instance.secondWingFloors[startFloorNumber];
                }
            }

            if (startFloor.floorNumber != target.floor.floorNumber && startFloor.wing != target.floor.wing)
            {
                actions.Enqueue(new CrossAndMove(startFloor, target.floor, target.floor.wing, target));
            }
            else
            if (startFloor.floorNumber != target.floor.floorNumber)
            {
                actions.Enqueue(new MoveAction(new Target(startFloor, startFloor.floorExitPoint)));
                actions.Enqueue(new MoveFloors(startFloor.floorNumber, target.floor));
                actions.Enqueue(new MoveAction(target));
            }
            else
            if (startFloor.wing != target.floor.wing)
            {
                actions.Enqueue(new MoveAction(new Target(startFloor, startFloor.floorEntryPoint)));
                actions.Enqueue(new CrossOffice(target.floor.wing, startFloor));
                actions.Enqueue(new MoveAction(new Target(target.floor, target.point)));
            }
            else
            {
                actions.Enqueue(new MoveAction(target));
            }
        }
    }
}