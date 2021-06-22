using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
public class PlanManager : MonoBehaviour
{
    internal List<Floor> floors;
    internal List<Floor> secondWingFloors;
    public List<Robot> robots;
    public static PlanManager instance = null;
    private LevelManager levelScript;
    public bool onPause = true;
    private Text topPauseText;
    private Canvas canvas;
    private GameObject infoList;
    private GameObject scrollView;
    private GameObject scrollViewPort;
    private GameObject listContent;
    private GameObject capacityErrMenu;
    private bool capacityErr = false;
    private int errDay;
    public GameObject statusItem;
    public List<GameObject> statusList = new List<GameObject>();
    public GameObject followTarget;
    public WorldBounds worldBounds;
    public Queue<Task> globalTaskQueue = new Queue<Task>();
    public Stack<Storage> accessibleStorage;
    private int globalBoxCount = 0;
    public int storageOnFloor;
    private int maxCapacity;
    public int currentBoxCount = 0;
    public Storage inspectedStorage;
    public List<Box> inspectedBoxes = new List<Box>();
    public List<BoxListButton> boxListContent = new List<BoxListButton>();
    private const float Diviser = 5.0f/6.0f;
    private readonly List<Box> boxesToDispose = new List<Box>();
    private int boxesDisposed = 0;
    private GameObject demandRoot;
    private GameObject demandLegend;
    private GameObject storageMenu;
    private GameObject demandMenu;
    private GameObject shelfLifeMenu;
    private GameObject shelfLifeRoot;
    private GameObject overallMenu;
    private GameObject overallRoot;
    private GameObject overallLegend;
    private GameObject disposeRoot;
    private GameObject disposeLegend;
    [SerializeField]
    private GameObject wedgePrefab;
    [SerializeField]
    private GameObject legendPrefab;
    private GameObject shelfLifeLegend;
    internal List<Queue<Box>> boxesByLife = new List<Queue<Box>>();
    internal List<List<Box>> boxesByChance = new List<List<Box>>();
    private readonly List<StatComponent> shelfLifeData = new List<StatComponent>();
    private readonly List<StatComponent> overallData = new List<StatComponent>();
    private readonly List<StatComponent> disposeData = new List<StatComponent>();
    private readonly List<StatComponent> chanceData = new List<StatComponent>();
    public List<Stack<Storage>> accessibleStorages = new List<Stack<Storage>>();
    public List<bool> reachedEnd = new List<bool>();
    public List<List<ValueAndWeight>> theoryTimestamps = new List<List<ValueAndWeight>>();
    public List<List<ValueAndWeight>> actualTimestamps = new List<List<ValueAndWeight>>();
    private RectTransform graphContainer;
    private RectTransform finishedGraphContainer;
    [SerializeField]
    private Sprite circleSprite;
    [SerializeField]
    private GameObject horizontalDash;
    [SerializeField]
    private GameObject verticalDash;
    [SerializeField]
    private GameObject graphPrefab;
    public bool hasPrinted = false;
    private Text visualisationFinished;
    [SerializeField]
    private GameObject labelXPrefab;
    [SerializeField]
    private GameObject labelYPrefab;
    [SerializeField]
    private GameObject dynamicButton;
    private float maxCalcTime = 0.0f;
    public List<ModelEvent> events = new List<ModelEvent>();
    public List<ModelEvent> pastEvents = new List<ModelEvent>();

    public Queue<Request> globalRequestQueue = new Queue<Request>();
    public List<Request> finishedRequests = new List<Request>();

    public List<Queue<Request>> floorQueues = new List<Queue<Request>>();
    public List<Queue<Request>> floorScanQueues = new List<Queue<Request>>();
    public List<List<Box>> floorBoxes = new List<List<Box>>();
    public List<Box> processingBoxes = new List<Box>();

    private List<GraphData> globalQueueData = new List<GraphData>();
    private List<List<GraphData>> agentGraphData = new List<List<GraphData>>();
    private List<List<GraphData>> floorQueueGraphData = new List<List<GraphData>>();
    private List<List<GraphData>> floorScanQueueGraphData = new List<List<GraphData>>();
    private List<GraphData> finishedRequestsData = new List<GraphData>();
    private List<float> agentWorkTime = new List<float>();
    private GameObject graphMenu;
    private GameObject graphWindow;
    public GameObject finishedGraphWindow;
    private GameObject chartMenu;
    private RectTransform requestStatsContainer;
    private RectTransform agentStatsContainer;
    private RectTransform calcStatsContainer;
    private float maxRequestTime = 0f;
    public float lastTimeStep;
    private List<ModelEvent> futureEventList = new List<ModelEvent>();
    private List<float> requestStats = new List<float>() { 0f, 0f, 0f, 0f, 0f, 0f, 0f};
    private List<GraphData> agentStats = new List<GraphData>();
    private List<GraphData> requestGraphStats = new List<GraphData>();
    private List<GraphData> calcGraphStats = new List<GraphData>();
    public List<float> boxTakingTime = new List<float>();
    private int maxGlobalQueue = 0;
    private readonly List<int> maxAgentRequests = new List<int>();
    private readonly List<int> maxFloorQueue = new List<int>();
    private readonly List<int> maxScanFloorQueue = new List<int>();
    private int maxFinishedRequests = 0;

    [HideInInspector]
    public List<GraphClass> agentGraphs = new List<GraphClass>();
    [HideInInspector]
    public List<GraphClass> floorGraphs = new List<GraphClass>();
    [HideInInspector]
    public List<GraphClass> scanGraphs = new List<GraphClass>();

    public float binarySearchTime = 0.0f;

    [HideInInspector]
    public GameObject floorButtonsContent;
    [HideInInspector]
    public GameObject agentButtonsContent;
    [HideInInspector]
    public GameObject scanButtonsContent;
    public int count1 = 0;
    public int count2 = 0;

    public int skipCheck1 = 0;
    public int skipCheck2 = 0;

    public float maxWorkTime = 0f;
    public int scanCount = 0;
    public int realCount = 1;
    public int duplicateRequestCount = 0;

    public float globalTimer = 0f;

    private GameObject timerContainer;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
        DontDestroyOnLoad(gameObject);
        //UnityEngine.Random.InitState(DateTime.Now.Millisecond);
        Random.InitState((int)DateTime.Now.Ticks);
        levelScript = GetComponent<LevelManager>();
        floors = new List<Floor>();
        secondWingFloors = new List<Floor>();
        robots = new List<Robot>();
        accessibleStorage = new Stack<Storage>();

        if (Settings.instance.roomType == RoomType.SimpleSingleRow || Settings.instance.roomType == RoomType.AdvancedSingleRow)
        {
            storageOnFloor = Settings.instance.shelfLength * Settings.instance.rowCount * Settings.instance.columnCount;
        }
        else
        if (Settings.instance.roomType == RoomType.SimpleDoubleRow || Settings.instance.roomType == RoomType.AdvancedDoubleRow)
        {
            storageOnFloor = Settings.instance.shelfLength * Settings.instance.rowCount * Settings.instance.columnCount * 2;
        }

        maxCapacity = storageOnFloor * 27 * Settings.instance.floorCount * Settings.instance.wingCount;

        Settings.instance.lifeSpan.Sort((p, q) => q.value.CompareTo(p.value));
        Settings.instance.chances.Sort((p, q) => q.value.CompareTo(p.value));

        SortLifeByTypes(Settings.instance.lifeSpan);
        SortByDemand(Settings.instance.chances);

        foreach (ValueAndWeight pair in Settings.instance.lifeSpan)
        {
            Queue<Box> tmp = new Queue<Box>();
            boxesByLife.Add(tmp);
            pair.PrintPair();
        }

        foreach (ValueAndWeight pair in Settings.instance.chances)
        {
            List<Box> tmp = new List<Box>();
            boxesByChance.Add(tmp);
            pair.PrintPair();
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        InitSim();
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        Debug.Log(ts.TotalSeconds);
        maxCalcTime = Mathf.Max(maxCalcTime, (float)ts.TotalSeconds);
        calcGraphStats.Add(new GraphData(calcGraphStats.Count, (float)ts.TotalSeconds));
        stopwatch.Reset();
        SetupUI();

        for (int i = 0; i < robots.Count; ++i) {
            theoryTimestamps.Add(new List<ValueAndWeight>());
            actualTimestamps.Add(new List<ValueAndWeight>());
            agentGraphData.Add(new List<GraphData>());
            agentGraphData[i].Add(new GraphData(0f, 0f));
            agentWorkTime.Add(0f);
            maxAgentRequests.Add(0);
            GameObject button = Instantiate(dynamicButton);
            button.GetComponent<DynamicButton>().buttonID = i;
            button.GetComponent<DynamicButton>().referenceList = agentGraphs;
            button.transform.Find("Text").gameObject.GetComponent<Text>().text =
                "Agent " + i.ToString() + "\nprocessing\nrequests";
            button.transform.SetParent(agentButtonsContent.transform);
        }

        stopwatch.Start();
        if (Settings.instance.boxPlacement == BoxPlacement.Simple)
        {
            BuildStorage();
            //VerySimplePlacing();
        }

        if (Settings.instance.boxPlacement == BoxPlacement.ByDemand)
        {
            SimpleByDemand();
        }

        if (Settings.instance.boxPlacement == BoxPlacement.Corners)
        {
            FillByCorners();
        }
        stopwatch.Stop();
        ts = stopwatch.Elapsed;
        Debug.Log(ts.TotalSeconds);
        maxCalcTime = Mathf.Max(maxCalcTime, (float)ts.TotalSeconds);
        calcGraphStats.Add(new GraphData(calcGraphStats.Count, (float)ts.TotalSeconds));
        stopwatch.Reset();

        /*stopwatch.Start();
        if (Settings.instance.boxDisposal == BoxDisposal.DoDispose)
        {
            DisposeOfBoxes();
        }
        stopwatch.Stop();
        ts = stopwatch.Elapsed;
        Debug.Log(ts.TotalSeconds);
        maxCalcTime = Mathf.Max(maxCalcTime, (float)ts.TotalSeconds);
        calcGraphStats.Add(new GraphData(calcGraphStats.Count, (float)ts.TotalSeconds));
        stopwatch.Reset();*/
        
        calcGraphStats.Add(new GraphData(calcGraphStats.Count, binarySearchTime));
        calcGraphStats[calcGraphStats.Count - 2].yAxis -= binarySearchTime;
        foreach (Queue<Box> q in boxesByLife)
        {
            if (q.Count != 0)
            {
                shelfLifeData.Add(new StatComponent(q.Peek().lifeSpan.ToString(), (float)q.Count / currentBoxCount, q.Count, "Shelf life: " + q.Peek().lifeSpan + "y."));
            }
        }

        foreach (List<Box> list in boxesByChance)
        {
            if (list.Count != 0)
            {      
                chanceData.Add(new StatComponent(list[0].demandChance.ToString(), (float)list.Count / currentBoxCount, list.Count, "Demand chance: " + list[0].demandChance + "%"));
            }
        }

        overallData.Add(new StatComponent("", (float)(maxCapacity - currentBoxCount) / maxCapacity, maxCapacity - currentBoxCount, "Free space:"));
        overallData.Add(new StatComponent("", (float)currentBoxCount / maxCapacity, currentBoxCount, "Space taken:"));

        if (Settings.instance.boxDisposal == BoxDisposal.DoDispose)
        {
            disposeLegend.SetActive(true);
            disposeRoot.SetActive(true);
            disposeData.Add(new StatComponent("", (float)(boxesDisposed) / globalBoxCount, boxesDisposed, "Boxes disposed"));
            disposeData.Add(new StatComponent("", (float)(globalBoxCount - boxesDisposed) / globalBoxCount, globalBoxCount - boxesDisposed, "Boxes remaining"));
            CreatePieChart(disposeData, disposeRoot, disposeLegend);
        }

        CreatePieChart(chanceData, demandRoot, demandLegend);
        CreatePieChart(shelfLifeData, shelfLifeRoot, shelfLifeLegend);
        CreatePieChart(overallData, overallRoot, overallLegend);
        
        globalQueueData.Add(new GraphData(0, 0));
        finishedRequestsData.Add(new GraphData(0, 0));

        for (int i = 0; i < floors.Count; ++i) {
            floorQueues.Add(new Queue<Request>());
            floorScanQueues.Add(new Queue<Request>());
            floorBoxes.Add(new List<Box>());
            floorQueueGraphData.Add(new List<GraphData>());
            floorQueueGraphData[i].Add(new GraphData(0.0f, 0.0f));
            floorScanQueueGraphData.Add(new List<GraphData>());
            floorScanQueueGraphData[i].Add(new GraphData(0.0f, 0.0f));
            maxFloorQueue.Add(0);
            maxScanFloorQueue.Add(0);
            GameObject tmp = Instantiate(dynamicButton);
            tmp.GetComponent<DynamicButton>().buttonID = i;
            tmp.GetComponent<DynamicButton>().referenceList = floorGraphs;
            tmp.transform.Find("Text").gameObject.GetComponent<Text>().text =
                "Floor " + i.ToString() + "\nqueue";
            tmp.transform.SetParent(floorButtonsContent.transform);

            GameObject graphClassItem = Instantiate(graphPrefab);
            graphClassItem.transform.SetParent(graphMenu.transform);
            graphClassItem.GetComponent<GraphClass>().graphID = i;
            floorGraphs.Add(graphClassItem.GetComponent<GraphClass>());

            tmp = Instantiate(dynamicButton);
            tmp.GetComponent<DynamicButton>().buttonID = i;
            tmp.GetComponent<DynamicButton>().referenceList = scanGraphs;
            tmp.transform.Find("Text").gameObject.GetComponent<Text>().text =
                "Floor " + i.ToString() + "\nscan\nqueue";
            tmp.transform.SetParent(scanButtonsContent.transform);
            graphClassItem = Instantiate(graphPrefab);
            graphClassItem.transform.SetParent(graphMenu.transform);
            graphClassItem.GetComponent<GraphClass>().graphID = i;
            scanGraphs.Add(graphClassItem.GetComponent<GraphClass>());
        }

        for (int i = 0; i < robots.Count; ++i)
        {
            GameObject graphClassItem = Instantiate(graphPrefab);
            graphClassItem.transform.SetParent(graphMenu.transform);
            graphClassItem.GetComponent<GraphClass>().graphID = i;
            agentGraphs.Add(graphClassItem.GetComponent<GraphClass>());
        }

        if (capacityErr)
        {
            ShowCapacityErr();
        }
        else
        {
            GenerateListOfEvents();
            if (futureEventList.Count != 0)
            {
                events.Add(futureEventList[0]);
                futureEventList.RemoveAt(0);
            }
            stopwatch.Start();
            if (Settings.instance.simType == SimType.FirstIdle)
            {
                SimpleModeling();
            }
            if (Settings.instance.simType == SimType.Closest)
            {
                FirstClosestModeling();
            }
            stopwatch.Stop();
            ts = stopwatch.Elapsed;
            Debug.Log(ts.TotalSeconds);
            maxCalcTime = Mathf.Max(maxCalcTime, (float)ts.TotalSeconds);
            calcGraphStats.Add(new GraphData(calcGraphStats.Count, (float)ts.TotalSeconds));
            stopwatch.Reset();

            Debug.Log(finishedRequests.Count());

            foreach (Request request in finishedRequests)
            {
                if (request.isADuplicate)
                {
                    duplicateRequestCount++;
                }

                if (!request.isADuplicate)
                {
                    requestStats[2] += request.arrivalTime - request.globalQLeaveTime;
                    realCount++;
                }

                if (request.requestType == RequestType.Scan)
                {
                    requestStats[5] += request.scanQueueLeaveTime - request.processingStart;
                    requestStats[6] += request.processingEnd - request.scanQueueLeaveTime;
                    scanCount++;

                }
                requestStats[3] += request.unpackingQLeaveTime - request.unpackingStart;
                requestStats[4] += request.unpackingEnd - request.unpackingQLeaveTime;
                requestStats[0] += request.globalQLeaveTime - request.creationTime;
            }

            float tmpTime = 0;
            foreach (float f in boxTakingTime)
            {
                tmpTime += f;
            }
            tmpTime = tmpTime / boxTakingTime.Count;
            requestGraphStats.Add(new GraphData(0, requestStats[0] / finishedRequests.Count));
            requestGraphStats.Add(new GraphData(1, tmpTime));
            requestGraphStats.Add(new GraphData(2, requestStats[2] / realCount - tmpTime));
            requestGraphStats.Add(new GraphData(3, requestStats[3] / finishedRequests.Count));
            requestGraphStats.Add(new GraphData(4, requestStats[4] / finishedRequests.Count));
            requestGraphStats.Add(new GraphData(5, requestStats[5] / scanCount));
            requestGraphStats.Add(new GraphData(6, requestStats[6] / scanCount));
            List<string> rerquestLabels = new List<string>
            { "global q",
              "pick up time",
              "travel time",
              "unpacking queue",
              "unpacking",
              "scanning queue",
              "scanning"
            };

            List<string> calcOrder = new List<string>
            {
                "scene building",
                "filling storage",
                "disposing boxes",
                "DES modeling time"
            };


            foreach (GraphData tmp in requestGraphStats)
            {
                if (tmp.yAxis > maxRequestTime)
                {
                    maxRequestTime = tmp.yAxis;
                }
            }

            finishedRequests.Sort((p, q) => p.creationTime.CompareTo(q.creationTime));

            DisplayGraph(globalQueueData, maxGlobalQueue, graphContainer, Color.red);
            DisplayGraph(finishedRequestsData, maxFinishedRequests, finishedGraphContainer, Color.green);

            for (int i = 0; i < floors.Count; ++i)
            {
                DisplayGraph(floorQueueGraphData[i], maxFloorQueue[i], floorGraphs[i].graphContainer, Color.blue);
                DisplayGraph(floorScanQueueGraphData[i], maxScanFloorQueue[i], scanGraphs[i].graphContainer, Color.blue);
            }

            List<string> agentLabels = new List<string>();
            for (int i = 0; i < robots.Count; ++i)
            {
                robots[i].SetFuturetasks();
                DisplayGraph(agentGraphData[i], maxAgentRequests[i], agentGraphs[i].graphContainer, Color.magenta);
                agentStats.Add(new GraphData(i, agentWorkTime[i]));
                if (maxWorkTime < agentWorkTime[i])
                {
                    maxWorkTime = agentWorkTime[i];
                }

                agentLabels.Add("Agent " + i);
            }

            DisplayBarChart(agentStats, maxWorkTime, agentStatsContainer, Color.blue, agentLabels);
            DisplayBarChart(requestGraphStats, maxRequestTime, requestStatsContainer, Color.blue, rerquestLabels);
            DisplayBarChart(calcGraphStats, maxCalcTime, calcStatsContainer, Color.green, calcOrder);
            Debug.Log(globalRequestQueue.Count);

        }

    }
    void ShowCapacityErr()
    {
        capacityErrMenu.SetActive(true);
        capacityErrMenu.transform.Find("MaxCapacity").gameObject.GetComponent<Text>().text = "Maximum capacity: " + maxCapacity;
        capacityErrMenu.transform.Find("ErrorDay").gameObject.GetComponent<Text>().text = "Day of work: " + errDay;
    }

    public void HideAllGraphs()
    {
        foreach (GraphClass graph in instance.floorGraphs)
        {
            graph.gameObject.SetActive(false);
        }

        foreach (GraphClass graph in instance.agentGraphs)
        {
            graph.gameObject.SetActive(false);
        }

        foreach (GraphClass graph in instance.scanGraphs)
        {
            graph.gameObject.SetActive(false);
        }

    }

    void DisplayBarChart(List<GraphData> list, float maxValue, RectTransform graphContainer, Color color, List<string> labels)
    {
        if (list.Count > 0)
        {
            GameObject toolTip = graphContainer.Find("Tooltip").gameObject;
            graphContainer.Find("ErrorMessage").gameObject.SetActive(false);
            int spacesCont = list.Count - 1;
            float xPadding = 10f;
            float yPadding = 10f;
            float spaceLength = 10f;
            float graphHeight = graphContainer.sizeDelta.y - 2 * yPadding;
            float graphWidth = graphContainer.sizeDelta.x - 2 * xPadding;
            float barWidth = (graphWidth - spaceLength * spacesCont) / list.Count;
            for (int i = 0; i < list.Count; ++i)
            {
                float xPosition = xPadding + i * spaceLength + barWidth * ( 1f / 2f + i);
                float yPosition = yPadding + list[i].yAxis / maxValue * graphHeight;
                GameObject barGameObject = CreateBar(graphContainer, new Vector2(xPosition, yPosition), barWidth, color, toolTip,
                    labels[i] + "\n" + list[i].yAxis.ToString());

                RectTransform dashX = Instantiate(verticalDash).GetComponent<RectTransform>();
                dashX.SetParent(graphContainer);
                dashX.gameObject.SetActive(true);
                dashX.anchoredPosition = new Vector2(xPosition, -18.0f);

                RectTransform labelX = Instantiate(labelXPrefab).GetComponent<RectTransform>();
                labelX.SetParent(graphContainer);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPosition, -18.0f);
                labelX.GetComponent<Text>().text = labels[i];
            }

            if (maxValue >= 1)
            {
                int yValueLabelStep;
                float yLabelStep;
                int yStepsCount = 8;
                if ((int)maxValue / yStepsCount == 1)
                {
                    yStepsCount = (int)maxValue;
                }

                if (maxValue < yStepsCount)
                {
                    yStepsCount = (int)maxValue;
                }

                int tmp = (int)maxValue % yStepsCount;
                yValueLabelStep = (int)((maxValue - tmp) / yStepsCount);
                float maxValuePercent = (yStepsCount * yValueLabelStep) / maxValue;
                yLabelStep = maxValuePercent * graphHeight / yStepsCount;
                for (int i = 0; i < yStepsCount + 1; ++i)
                {
                    RectTransform labelY = Instantiate(labelYPrefab).GetComponent<RectTransform>();
                    labelY.SetParent(graphContainer);
                    labelY.gameObject.SetActive(true);
                    labelY.anchoredPosition = new Vector2(-10.0f, yPadding + yLabelStep * i);
                    labelY.GetComponent<Text>().text = (yValueLabelStep * i).ToString();

                    RectTransform dashY = Instantiate(horizontalDash).GetComponent<RectTransform>();
                    dashY.SetParent(graphContainer);
                    dashY.gameObject.SetActive(true);
                    dashY.anchoredPosition = new Vector2(-10.0f, yPadding + yLabelStep * i);
                }
            }
        }
        else
        {
            graphContainer.Find("ErrorMessage").gameObject.SetActive(true);
        }
    }
    void DisplayGraph(List<GraphData> list, int maxValue, RectTransform graphContainer, Color color)
    {
        if (list.Count > 1)
        {

            GameObject toolTip = graphContainer.Find("Tooltip").gameObject;
            graphContainer.Find("ErrorMessage").gameObject.SetActive(false);
            float xPadding = 10f;
            float yPadding = 10f;
            float graphHeight = graphContainer.sizeDelta.y - 2 * yPadding;
            float graphWidth = graphContainer.sizeDelta.x - 2 * xPadding;
            GameObject prevPoint = null;
            for (int i = 0; i < list.Count; ++i)
            {
                float xPosition = xPadding + ((float)list[i].xAxis / (float)list[list.Count - 1].xAxis) * graphWidth;
                float yPosition = yPadding + ((float)list[i].yAxis / maxValue) * graphHeight;
                GameObject point = CreatePoint(new Vector2(xPosition, yPosition), graphContainer, toolTip,
                    list[i].xAxis.ToString() + "\n" + list[i].yAxis.ToString(), color);
                if (prevPoint != null)
                {
                    CreateConnection(prevPoint.GetComponent<RectTransform>().anchoredPosition,
                       point.GetComponent<RectTransform>().anchoredPosition, graphContainer, color);
                }
                prevPoint = point;
            }

            int xStepsCount = 8;
            int xValueLabelStep;
            float xLabelStep;
            if (list[list.Count - 1].xAxis < xStepsCount)
            {
                xStepsCount = (int)list[list.Count - 1].xAxis;
            }
            int xTmp = (int)list[list.Count - 1].xAxis % xStepsCount;
            xValueLabelStep = ((int)list[list.Count - 1].xAxis - xTmp) / xStepsCount;
            float xPercent = (float)((int)list[list.Count - 1].xAxis - xTmp) / (int)list[list.Count - 1].xAxis;
            xLabelStep = xPercent * graphWidth / xStepsCount;

            for (int i = 0; i < xStepsCount + 1; ++i)
            {
                RectTransform labelX = Instantiate(labelXPrefab).GetComponent<RectTransform>();
                labelX.SetParent(graphContainer);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xLabelStep * i + xPadding, -18.0f);
                labelX.GetComponent<Text>().text = (xValueLabelStep * i).ToString();

                RectTransform dashX = Instantiate(verticalDash).GetComponent<RectTransform>();
                dashX.SetParent(graphContainer);
                dashX.gameObject.SetActive(true);
                dashX.anchoredPosition = new Vector2(xLabelStep * i + xPadding, -18.0f);
            }
            int yValueLabelStep;
            float yLabelStep;
            int yStepsCount = 8;
            if (maxValue / yStepsCount == 1)
            {
                yStepsCount = maxValue;
            }

            if (maxValue < yStepsCount)
            {
                yStepsCount = maxValue;
            }

            int tmp = maxValue % yStepsCount;
            yValueLabelStep = (maxValue - tmp) / yStepsCount;
            float maxValuePercent = (float)(maxValue - tmp) / maxValue;
            yLabelStep = maxValuePercent * graphHeight / yStepsCount;

            for (int i = 0; i < yStepsCount + 1; ++i)
            {
                RectTransform labelY = Instantiate(labelYPrefab).GetComponent<RectTransform>();
                labelY.SetParent(graphContainer);
                labelY.gameObject.SetActive(true);
                labelY.anchoredPosition = new Vector2(-10.0f, yPadding + yLabelStep * i);
                labelY.GetComponent<Text>().text = (yValueLabelStep * i).ToString();

                RectTransform dashY = Instantiate(horizontalDash).GetComponent<RectTransform>();
                dashY.SetParent(graphContainer);
                dashY.gameObject.SetActive(true);
                dashY.anchoredPosition = new Vector2(-10.0f, yPadding + yLabelStep * i);
            }
        }
        else
        {
            graphContainer.Find("ErrorMessage").gameObject.SetActive(true);
        }
    }

    public static void ShowToolTip(GameObject toolTip, string toolTipText, Vector2 pos)
    {
        Text toolTipUIText = toolTip.gameObject.transform.Find("Text").GetComponent<Text>();
        toolTip.gameObject.SetActive(true);
        float padding = 4.0f;
        toolTipUIText.text = toolTipText;
        Vector2 size = new Vector2(toolTipUIText.preferredWidth + padding * 2.0f,
            toolTipUIText.preferredHeight + padding * 2.0f);
        toolTip.GetComponent<RectTransform>().anchoredPosition = pos;
        toolTip.transform.Find("Background").GetComponent<RectTransform>().sizeDelta = size;
        toolTip.transform.SetAsLastSibling();
    }

    public static void HideToolTip(GameObject toolTip)
    {
        toolTip.gameObject.SetActive(false);
    }

    GameObject CreateBar(RectTransform graphContainer, Vector2 position, float width, Color color, GameObject toolTip, string originalData)
    {
        GameObject bar = new GameObject("bar", typeof(Image));
        bar.transform.SetParent(graphContainer, false);
        ToolTip toolTipStarter = bar.gameObject.AddComponent<ToolTip>();
        toolTipStarter.toolTip = toolTip;
        toolTipStarter.pos = position;
        toolTipStarter.text = originalData;

        bar.GetComponent<Image>().color = color;
        RectTransform rectTransform = bar.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(position.x, 0);
        rectTransform.sizeDelta = new Vector2(width, position.y);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0.5f, 0.0f);
        return bar;
    }

    GameObject CreatePoint(Vector2 pointPosition, RectTransform graphContainer, GameObject toolTip, string originalData, Color color)
    {
        GameObject point = new GameObject("point", typeof(Image));
        ToolTip toolTipStarter = point.gameObject.AddComponent<ToolTip>();
        toolTipStarter.toolTip = toolTip;
        toolTipStarter.pos = pointPosition;
        toolTipStarter.text = originalData;
        point.transform.SetParent(graphContainer, false);
        point.GetComponent<Image>().sprite = circleSprite;

        point.GetComponent<Image>().color = color;
        RectTransform rectTransform = point.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = pointPosition;
        rectTransform.sizeDelta = new Vector2(2f, 2f);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return point;
    }

    void CreateConnection(Vector2 startPos, Vector2 endPos, RectTransform graphContainer, Color color)
    {
        GameObject connection = new GameObject("connection", typeof(Image));
        connection.transform.SetParent(graphContainer, false);
        connection.GetComponent<Image>().color = color;
        RectTransform rectTransform = connection.GetComponent<RectTransform>();
        Vector2 direction = (endPos - startPos).normalized;
        float dist = Vector2.Distance(startPos, endPos);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(dist, 2f);
        rectTransform.anchoredPosition = startPos + direction * dist * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(direction));
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        return n;
    }

    void ProcessNewRequest(Robot agent, Request request, float timeStamp)
    {
        Floor tmp = request.box.rootStorage.rootFloor;
        Vector2 pos = Robot.FromGlobalToLocal(tmp, request.box.rootStorage.accessPoint);
        int boxesToTake = request.box.CountBoxesToTake();
        agent.processingRequests.Enqueue(request);
        processingBoxes.Add(request.box);
        Task newTask = new Task(new Target(tmp, pos));
        _ = agent.GetTaskTime(newTask, out Target start);
        newTask.startTarget = start;
        request.globalQLeaveTime = events[0].timeStamp;
        if (agent.discreteTaskList.Count != 0)
        {
            newTask.startTime = agent.discreteTaskList.ToList()[agent.discreteTaskList.Count - 1].endTime
                + Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
        }
       else
        {
            newTask.startTime = timeStamp + Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
        }
        request.travelStart = newTask.startTime;
        newTask.BuildActionList(agent.discretePrevFloor, agent.discreteFloorNumber);
        float realTime = agent.GetRealTimeCompletion(newTask, boxesToTake);
        agentWorkTime[agent.idCount] += realTime;
        request.pickUpTime = realTime + newTask.startTime;
        newTask.endTime = request.pickUpTime;
        agent.taskList.Enqueue(newTask);
        if (Settings.instance.simType == SimType.Closest)
        {
            agent.discreteTaskList.Enqueue(newTask);
        }
        agent.discreteState = true;
        agent.discreteIsInOffice = false;
        events.Add(new BoxPickUp(newTask, realTime + newTask.startTime, agent));
    }

    void ProcessLastBoxPickUp(Robot agent)
    {
        Task newTask = new Task(new Target(agent.discreteFloorNumber));
        _ = agent.GetTaskTime(newTask, out Target start);
        newTask.startTarget = start;
        newTask.startTime = events[0].timeStamp +
            Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
        newTask.BuildActionList(agent.discretePrevFloor, agent.discreteFloorNumber);
        float realTime = agent.GetRealTimeCompletion(newTask);
        agentWorkTime[agent.idCount] += realTime;
        newTask.endTime = realTime + newTask.startTime;
        agent.taskList.Enqueue(newTask);
        agent.discreteState = true;
        agent.discreteIsInOffice = false;
        events.Add(new WorkerArrival(newTask, realTime + newTask.startTime, agent));
    }

    List<Robot> CheckAvailibleAgents()
    {
        List<Robot> agentCandidates = new List<Robot>();

        foreach (Robot agent in robots)
        {
            if (agent.processingRequests.Count < Settings.instance.inventoryCap && !agent.isHeadingToOffice)
            {
                agentCandidates.Add(agent);
            }
        }

        return agentCandidates;
    }

    List<Robot> CheckFreeAgents()
    {
        List<Robot> freeAgents = new List<Robot>();

        foreach (Robot agent in robots)
        {
            if (!agent.discreteState && agent.processingRequests.Count < Settings.instance.inventoryCap)
            {
                freeAgents.Add(agent);
            }
        }

        return freeAgents;
    }
    public static bool CheckIfBoxInList(Box box, List<Box>list)
    {
        return list.Any(item => item.boxID == box.boxID);
    }

    bool CheckIfBoxIsOnFloor(Box box)
    {
        bool check = false;
        Parallel.ForEach(floorBoxes, list =>
        {
            if(CheckIfBoxInList(box, list))
            {
                check = true;
            }
        });
        return check;
    }

    public void RemoveBoxFromList(Box box, List<Box> list)
    {
        list.Remove(list.Single(item => item.boxID == box.boxID));
    }

    void ProcessDuplicateRequests()
    {
        while (globalRequestQueue.Count != 0 && (CheckIfBoxInList(globalRequestQueue.Peek().box, processingBoxes) || (CheckIfBoxIsOnFloor(globalRequestQueue.Peek().box))))
        {
            globalRequestQueue.Peek().globalQLeaveTime = events[0].timeStamp;
            globalRequestQueue.Peek().isADuplicate = true;
            if (CheckIfBoxInList(globalRequestQueue.Peek().box, processingBoxes))
            {
                globalRequestQueue.Peek().box.worker.duplicateRequests.Enqueue(globalRequestQueue.Dequeue());
                skipCheck1++;
            }
            else
            if (CheckIfBoxIsOnFloor(globalRequestQueue.Peek().box))
            {
                globalRequestQueue.Peek().arrivalTime = events[0].timeStamp;
                globalRequestQueue.Peek().unpackingStart = events[0].timeStamp +
                        Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
                floorQueues[globalRequestQueue.Peek().box.residingFloor].Enqueue(globalRequestQueue.Peek());
                if (floorQueues[globalRequestQueue.Peek().box.residingFloor].Count == 1)
                {
                    floorQueues[globalRequestQueue.Peek().box.residingFloor].Peek().unpackingQLeaveTime =
                        floorQueues[globalRequestQueue.Peek().box.residingFloor].Peek().unpackingStart;
                    floorQueues[globalRequestQueue.Peek().box.residingFloor].Peek().unpackingEnd =
                        Random.Range(Settings.instance.minTimeToUnpack, Settings.instance.maxTimeToUnpack)
                        + floorQueues[globalRequestQueue.Peek().box.residingFloor].Peek().unpackingStart;

                    QueueRelease qRelease = new QueueRelease(globalRequestQueue.Peek().box.residingFloor,
                        floorQueues[globalRequestQueue.Peek().box.residingFloor].Peek().unpackingEnd);
                    events.Add(qRelease);
                }
                skipCheck2++;
                globalRequestQueue.Dequeue();
            }
        }
    }
    void SimpleModeling()
    {
        while (events.Count != 0)
        {
            if (events[0].GetType() == typeof(RequestArrival))
            {
                globalRequestQueue.Enqueue(((RequestArrival)events[0]).request);

                ProcessDuplicateRequests();

                while (globalRequestQueue.Count != 0 && CheckFreeAgents().Count != 0)
                {
                    if (globalRequestQueue.Count != 0)
                    {
                        List<Robot> freeAgents = CheckFreeAgents();

                        foreach (Robot agent in freeAgents)
                        {
                            if (!agent.discreteState && agent.processingRequests.Count < Settings.instance.inventoryCap)
                            {
                                Request tmpRequest = globalRequestQueue.Peek();
                                Robot worker = robots[agent.idCount];
                                tmpRequest.box.worker = worker;
                                ProcessNewRequest(worker, tmpRequest, events[0].timeStamp);
                                globalRequestQueue.Dequeue();
                                break;
                            }
                        }
                    }
                    ProcessDuplicateRequests();
                }
                ProcessDuplicateRequests();
            }
            else
            if (events[0].GetType() == typeof(BoxPickUp))
            {
                BoxPickUp tmpArrival = (BoxPickUp)events[0];
                Robot worker = tmpArrival.worker;
                worker.discretePrevFloor = tmpArrival.task.target.floor;
                worker.discreteFloorNumber = tmpArrival.task.target.floor.floorNumber;
                worker.discreteState = false;
                worker.discretePos = Robot.FromLocalToGlobal(tmpArrival.task.target.floor, tmpArrival.task.target.point);
                if (globalRequestQueue.Count != 0 && worker.processingRequests.Count < Settings.instance.inventoryCap)
                {
                    ProcessDuplicateRequests();

                    if (globalRequestQueue.Count != 0)
                    {
                        Request tmpRequest = globalRequestQueue.Peek();
                        ProcessNewRequest(worker, tmpRequest, events[0].timeStamp);
                        tmpRequest.box.worker = worker;
                        globalRequestQueue.Dequeue();

                        ProcessDuplicateRequests();
                    }
                    else
                    {
                        ProcessLastBoxPickUp(worker);
                    }
                }
                else
                {
                    ProcessLastBoxPickUp(worker);
                }

                if (futureEventList.Count != 0)
                {
                    events.Add(futureEventList[0]);
                    futureEventList.RemoveAt(0);
                }
            }
            else
            if (events[0].GetType() == typeof(WorkerArrival))
            {
                WorkerArrival finish = (WorkerArrival)events[0];
                Robot worker = finish.worker;
                worker.discreteState = false;
                worker.discreteIsInOffice = true;
                worker.discreteFloorNumber = finish.task.target.floorNumber;
                worker.discretePrevFloor = finish.task.target.floor;

                Queue<Request> qClone = new Queue<Request>();

                while (worker.processingRequests.Count != 0)
                {
                    worker.processingRequests.Peek().arrivalTime = events[0].timeStamp;
                    Request tmp = worker.processingRequests.Dequeue();
                    floorBoxes[worker.discreteFloorNumber].Add(tmp.box);
                    tmp.box.residingFloor = worker.discreteFloorNumber;
                    RemoveBoxFromList(tmp.box, processingBoxes);
                    qClone.Enqueue(tmp);
                }

                while (qClone.Count != 0)
                {
                    worker.processingRequests.Enqueue(qClone.Dequeue());
                }

                while (worker.duplicateRequests.Count != 0)
                {
                    worker.duplicateRequests.Peek().arrivalTime = events[0].timeStamp;
                    worker.processingRequests.Enqueue(worker.duplicateRequests.Dequeue());
                }

                RequestReturn reqReturn = new RequestReturn(worker.processingRequests, worker.discreteFloorNumber, finish.arrivalTime +
                    Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay));
                events.Add(reqReturn);

                ProcessDuplicateRequests();

                if (globalRequestQueue.Count != 0)
                {
                    Request tmpRequest = globalRequestQueue.Peek();
                    ProcessNewRequest(worker, tmpRequest, events[0].timeStamp);
                    tmpRequest.box.worker = worker;
                    globalRequestQueue.Dequeue();

                    ProcessDuplicateRequests();
                }
               
            }
            else
            if (events[0].GetType() == typeof(RequestReturn))
            {
                RequestReturn tmp = (RequestReturn)events[0];

                bool check = false;
                if (floorQueues[tmp.floorNumber].Count == 0)
                {
                    check = true;
                }
                while(tmp.requests.Count != 0)
                {
                    tmp.requests.Peek().unpackingStart = events[0].timeStamp;
                    Request tmpReq = tmp.requests.Dequeue();
                    if (!tmpReq.isADuplicate)
                    {
                        tmpReq.box.residingFloor = tmp.floorNumber;
                    }
                    floorQueues[tmp.floorNumber].Enqueue(tmpReq);
                }
                if (check)
                {
                    floorQueues[tmp.floorNumber].Peek().unpackingQLeaveTime = floorQueues[tmp.floorNumber].Peek().unpackingStart;
                    floorQueues[tmp.floorNumber].Peek().unpackingEnd = Random.Range(Settings.instance.minTimeToUnpack, Settings.instance.maxTimeToUnpack)
                        + floorQueues[tmp.floorNumber].Peek().unpackingStart;
                    QueueRelease qRelease = new QueueRelease(tmp.floorNumber, floorQueues[tmp.floorNumber].Peek().unpackingEnd);
                    events.Add(qRelease);
                }
            }
            else
            if (events[0].GetType() == typeof(QueueRelease))
            {
                QueueRelease tmp = (QueueRelease)events[0];

                if (floorQueues[tmp.floorNumber].Peek().requestType == RequestType.Scan)
                {
                    floorQueues[tmp.floorNumber].Peek().processingStart = events[0].timeStamp +
                        Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
                    floorScanQueues[tmp.floorNumber].Enqueue(floorQueues[tmp.floorNumber].Dequeue());
                    if (floorScanQueues[tmp.floorNumber].Count == 1)
                    {
                        floorScanQueues[tmp.floorNumber].Peek().scanQueueLeaveTime = floorScanQueues[tmp.floorNumber].Peek().processingStart;
                        floorScanQueues[tmp.floorNumber].Peek().processingEnd = Random.Range(Settings.instance.minTimeToScan,
                            Settings.instance.maxTimeToScan) + floorScanQueues[tmp.floorNumber].Peek().scanQueueLeaveTime;
                        ScanRelease qRelease = new ScanRelease(tmp.floorNumber, floorScanQueues[tmp.floorNumber].Peek().processingEnd);
                        events.Add(qRelease);
                    }

                }
                else
                {
                    finishedRequests.Add(floorQueues[tmp.floorNumber].Dequeue());
                }

                if (floorQueues[tmp.floorNumber].Count != 0)
                {
                    floorQueues[tmp.floorNumber].Peek().unpackingQLeaveTime = events[0].timeStamp +
                        Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
                    floorQueues[tmp.floorNumber].Peek().unpackingEnd = floorQueues[tmp.floorNumber].Peek().unpackingQLeaveTime +
                        Random.Range(Settings.instance.minTimeToUnpack, Settings.instance.maxTimeToUnpack);
                    QueueRelease qRelease = new QueueRelease(tmp.floorNumber, floorQueues[tmp.floorNumber].Peek().unpackingEnd);
                    events.Add(qRelease);
                }
            }
            else
            if (events[0].GetType() == typeof(ScanRelease))
            {
                ScanRelease tmp = (ScanRelease)events[0];
                finishedRequests.Add(floorScanQueues[tmp.floorNumber].Dequeue());

                if (floorScanQueues[tmp.floorNumber].Count != 0)
                {
                    floorScanQueues[tmp.floorNumber].Peek().scanQueueLeaveTime = events[0].timeStamp +
                        Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
                    floorScanQueues[tmp.floorNumber].Peek().processingEnd = floorScanQueues[tmp.floorNumber].Peek().scanQueueLeaveTime +
                        Random.Range(Settings.instance.minTimeToScan, Settings.instance.maxTimeToScan);
                    ScanRelease qRelease = new ScanRelease(tmp.floorNumber, floorScanQueues[tmp.floorNumber].Peek().processingEnd);
                    events.Add(qRelease);
                }
            }

            FormGraphData();
            events.RemoveAt(0);
            events.Sort((p, q) => p.timeStamp.CompareTo(q.timeStamp));
        }
    }

    void FirstClosestModeling()
    {
        while (events.Count != 0)
        {
            if (events[0].GetType() == typeof(RequestArrival))
            {
                globalRequestQueue.Enqueue(((RequestArrival)events[0]).request);

                ProcessDuplicateRequests();

                while (globalRequestQueue.Count != 0 && CheckAvailibleAgents().Count != 0)
                {
                    List<Robot> agentCandidates = CheckAvailibleAgents();

                    Request tmpRequest = globalRequestQueue.Peek();
                    Floor tmp = tmpRequest.box.rootStorage.rootFloor;
                    Vector2 pos = Robot.FromGlobalToLocal(tmp, globalRequestQueue.Peek().box.rootStorage.accessPoint);
                    int boxesToTake = tmpRequest.box.CountBoxesToTake();
                    Task newTask = new Task(new Target(tmp, pos));
                    int agentID = 0;
                    float minTime = agentCandidates[0].GetTaskTime(newTask, out Target _);
                    for (int i = 0; i < agentCandidates.Count; ++i)
                    {
                        float timeTmp = agentCandidates[i].GetTaskTime(newTask, out Target _);
                        if (minTime > timeTmp)
                        {
                                minTime = timeTmp;
                                agentID = i;
                        }
                    }
                    Robot worker = agentCandidates[agentID];
                    globalRequestQueue.Peek().box.worker = worker;
                    ProcessNewRequest(worker, tmpRequest, events[0].timeStamp);
                    worker.discretePos = Robot.FromLocalToGlobal(newTask.target.floor, newTask.target.point);
                    worker.discretePrevFloor = newTask.target.floor;
                    worker.discreteFloorNumber = newTask.target.floor.floorNumber;
                    globalRequestQueue.Dequeue();

                    ProcessDuplicateRequests();
                }
            }
            else
            if (events[0].GetType() == typeof(BoxPickUp))
            {
                BoxPickUp tmpArrival = (BoxPickUp)events[0];
                Robot worker = tmpArrival.worker;
                worker.discreteTaskList.Dequeue();
                if (worker.discreteTaskList.Count != 0)
                {
                    worker.discreteState = true;
                }
                else
                {
                    worker.discreteState = false;
                }

                ProcessDuplicateRequests();

                while (globalRequestQueue.Count != 0 && CheckAvailibleAgents().Count != 0)
                {
                    List<Robot> agentCandidates = CheckAvailibleAgents();

                    Request tmpRequest = globalRequestQueue.Peek();
                    Floor tmp = tmpRequest.box.rootStorage.rootFloor;
                    Vector2 pos = Robot.FromGlobalToLocal(tmp, globalRequestQueue.Peek().box.rootStorage.accessPoint);
                    int boxesToTake = tmpRequest.box.CountBoxesToTake();
                    Task newTask = new Task(new Target(tmp, pos));
                    int agentID = 0;
                    float minTime = agentCandidates[0].GetTaskTime(newTask, out Target _);
                    for (int i = 0; i < agentCandidates.Count; ++i)
                    {
                        float timeTmp = agentCandidates[i].GetTaskTime(newTask, out Target _);
                        if (minTime > timeTmp)
                        {
                                minTime = timeTmp;
                                agentID = i;
                        }
                    }
                    Robot workerTmp = agentCandidates[agentID];
                    globalRequestQueue.Peek().box.worker = workerTmp;
                    ProcessNewRequest(workerTmp, tmpRequest, events[0].timeStamp);
                    workerTmp.discretePos = Robot.FromLocalToGlobal(newTask.target.floor, newTask.target.point);
                    workerTmp.discretePrevFloor = newTask.target.floor;
                    workerTmp.discreteFloorNumber = newTask.target.floor.floorNumber;
                    globalRequestQueue.Dequeue();

                    ProcessDuplicateRequests();
                }

                if ((worker.processingRequests.Count == Settings.instance.inventoryCap  || globalRequestQueue.Count == 0) 
                    && worker.discreteTaskList.Count == 0)
                {
                    Task newTask = new Task(new Target(worker.discreteFloorNumber));
                    float time = worker.GetTaskTime(newTask, out Target start);
                    newTask.startTarget = start;
                    newTask.startTime = events[0].timeStamp +
                        Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
                    newTask.BuildActionList(worker.discretePrevFloor, worker.discreteFloorNumber);
                    float realTime = worker.GetRealTimeCompletion(newTask);
                    agentWorkTime[worker.idCount] += realTime;
                    newTask.endTime = realTime + newTask.startTime;
                    worker.taskList.Enqueue(newTask);
                    worker.discreteState = true;
                    worker.discreteIsInOffice = false;
                    worker.discreteTaskList.Enqueue(newTask);
                    worker.isHeadingToOffice = true;
                    events.Add(new WorkerArrival(newTask, realTime + newTask.startTime, worker));
                }
            }
            else
            if (events[0].GetType() == typeof(WorkerArrival))
            {
                WorkerArrival finish = (WorkerArrival)events[0];
                Robot worker = finish.worker;
                worker.discreteState = false;
                worker.discreteIsInOffice = true;
                worker.isHeadingToOffice = false;
                worker.discreteTaskList.Dequeue();
                Queue<Request> qClone = new Queue<Request>();

                while (worker.processingRequests.Count != 0)
                {
                    worker.processingRequests.Peek().arrivalTime = events[0].timeStamp;
                    Request tmp = worker.processingRequests.Dequeue();
                    floorBoxes[worker.discreteFloorNumber].Add(tmp.box);
                    tmp.box.residingFloor = worker.discreteFloorNumber;
                    RemoveBoxFromList(tmp.box, processingBoxes);
                    qClone.Enqueue(tmp);
                }

                while (qClone.Count != 0)
                {
                    worker.processingRequests.Enqueue(qClone.Dequeue());
                }

                while (worker.duplicateRequests.Count != 0)
                {
                    worker.duplicateRequests.Peek().arrivalTime = events[0].timeStamp;
                    worker.processingRequests.Enqueue(worker.duplicateRequests.Dequeue());
                }

                RequestReturn reqReturn = new RequestReturn(worker.processingRequests, worker.discreteFloorNumber, finish.arrivalTime +
                    Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay));
                events.Add(reqReturn);

                ProcessDuplicateRequests();

                while (globalRequestQueue.Count != 0 && CheckAvailibleAgents().Count != 0)
                {
                    List<Robot> agentCandidates = CheckAvailibleAgents();

                    Request tmpRequest = globalRequestQueue.Peek();
                    Floor tmp = tmpRequest.box.rootStorage.rootFloor;
                    Vector2 pos = Robot.FromGlobalToLocal(tmp, globalRequestQueue.Peek().box.rootStorage.accessPoint);
                    int boxesToTake = tmpRequest.box.CountBoxesToTake();
                    Task newTask = new Task(new Target(tmp, pos));
                    int agentID = 0;
                    float minTime = agentCandidates[0].GetTaskTime(newTask, out Target _);
                    for (int i = 0; i < agentCandidates.Count; ++i)
                    {
                        float timeTmp = agentCandidates[i].GetTaskTime(newTask, out Target _);
                        if (minTime > timeTmp)
                        {
                        minTime = timeTmp;
                        agentID = i;
                        }
                    }
                    Robot workerTmp = agentCandidates[agentID];
                    globalRequestQueue.Peek().box.worker = workerTmp;
                    ProcessNewRequest(workerTmp, tmpRequest, events[0].timeStamp);
                    workerTmp.discretePos = Robot.FromLocalToGlobal(newTask.target.floor, newTask.target.point);
                    workerTmp.discretePrevFloor = newTask.target.floor;
                    workerTmp.discreteFloorNumber = newTask.target.floor.floorNumber;
                    globalRequestQueue.Dequeue();

                    ProcessDuplicateRequests();
                }

                if (futureEventList.Count != 0)
                {
                    events.Add(futureEventList[0]);
                    futureEventList.RemoveAt(0);
                }

            }
            else
            if (events[0].GetType() == typeof(RequestReturn))
            {
                RequestReturn tmp = (RequestReturn)events[0];

                bool check = false;
                if (floorQueues[tmp.floorNumber].Count == 0)
                {
                    check = true;
                }
                while (tmp.requests.Count != 0)
                {
                    tmp.requests.Peek().unpackingStart = events[0].timeStamp;
                    Request tmpReq = tmp.requests.Dequeue();
                    if (!tmpReq.isADuplicate)
                    {
                        tmpReq.box.residingFloor = tmp.floorNumber;
                    }
                    floorQueues[tmp.floorNumber].Enqueue(tmpReq);
                }
                if (check)
                {
                    floorQueues[tmp.floorNumber].Peek().unpackingQLeaveTime = floorQueues[tmp.floorNumber].Peek().unpackingStart;
                    floorQueues[tmp.floorNumber].Peek().unpackingEnd = Random.Range(Settings.instance.minTimeToUnpack, Settings.instance.maxTimeToUnpack)
                        + floorQueues[tmp.floorNumber].Peek().unpackingStart;
                    QueueRelease qRelease = new QueueRelease(tmp.floorNumber, floorQueues[tmp.floorNumber].Peek().unpackingEnd);
                    events.Add(qRelease);
                }
            }
            else
            if (events[0].GetType() == typeof(QueueRelease))
            {
                QueueRelease tmp = (QueueRelease)events[0];

                if (floorQueues[tmp.floorNumber].Peek().requestType == RequestType.Scan)
                {
                    floorQueues[tmp.floorNumber].Peek().processingStart = events[0].timeStamp +
                        Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
                    floorScanQueues[tmp.floorNumber].Enqueue(floorQueues[tmp.floorNumber].Dequeue());
                    if (floorScanQueues[tmp.floorNumber].Count == 1)
                    {
                        floorScanQueues[tmp.floorNumber].Peek().scanQueueLeaveTime = floorScanQueues[tmp.floorNumber].Peek().processingStart;
                        floorScanQueues[tmp.floorNumber].Peek().processingEnd = Random.Range(Settings.instance.minTimeToScan,
                            Settings.instance.maxTimeToScan) + floorScanQueues[tmp.floorNumber].Peek().scanQueueLeaveTime;
                        ScanRelease qRelease = new ScanRelease(tmp.floorNumber, floorScanQueues[tmp.floorNumber].Peek().processingEnd);
                        events.Add(qRelease);
                    }

                }
                else
                {
                    finishedRequests.Add(floorQueues[tmp.floorNumber].Dequeue());
                }

                if (floorQueues[tmp.floorNumber].Count != 0)
                {
                    floorQueues[tmp.floorNumber].Peek().unpackingQLeaveTime = events[0].timeStamp +
                        Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
                    floorQueues[tmp.floorNumber].Peek().unpackingEnd = floorQueues[tmp.floorNumber].Peek().unpackingQLeaveTime +
                        Random.Range(Settings.instance.minTimeToUnpack, Settings.instance.maxTimeToUnpack) ;
                    QueueRelease qRelease = new QueueRelease(tmp.floorNumber, floorQueues[tmp.floorNumber].Peek().unpackingEnd);
                    events.Add(qRelease);
                }
            }
            else
            if (events[0].GetType() == typeof(ScanRelease))
            {
                ScanRelease tmp = (ScanRelease)events[0];
                finishedRequests.Add(floorScanQueues[tmp.floorNumber].Dequeue());

                if (floorScanQueues[tmp.floorNumber].Count != 0)
                {
                    floorScanQueues[tmp.floorNumber].Peek().scanQueueLeaveTime = events[0].timeStamp +
                        Random.Range(Settings.instance.minProcessingDelay, Settings.instance.maxProcessingDelay);
                    floorScanQueues[tmp.floorNumber].Peek().processingEnd = floorScanQueues[tmp.floorNumber].Peek().scanQueueLeaveTime +
                        Random.Range(Settings.instance.minTimeToScan, Settings.instance.maxTimeToScan);
                    ScanRelease qRelease = new ScanRelease(tmp.floorNumber, floorScanQueues[tmp.floorNumber].Peek().processingEnd);
                    events.Add(qRelease);
                }
            }

            FormGraphData();
            events.RemoveAt(0);
            events.Sort((p, q) => p.timeStamp.CompareTo(q.timeStamp));
        }
    }

    void FormGraphData()
    {
        for (int i = 0; i < Settings.instance.floorCount; ++i)
        {
            if (floorQueues[i].Count != floorQueueGraphData[i][floorQueueGraphData[i].Count - 1].yAxis)
            {
                floorQueueGraphData[i].Add(new GraphData(events[0].timeStamp, floorQueues[i].Count));
                if (floorQueues[i].Count > maxFloorQueue[i])
                {
                    maxFloorQueue[i] = floorQueues[i].Count;
                }
            }
            if (floorScanQueues[i].Count != floorScanQueueGraphData[i][floorScanQueueGraphData[i].Count - 1].yAxis)
            {
                floorScanQueueGraphData[i].Add(new GraphData(events[0].timeStamp, floorScanQueues[i].Count));
                if (floorScanQueues[i].Count > maxScanFloorQueue[i])
                {
                    maxScanFloorQueue[i] = floorScanQueues[i].Count;
                }
            }

            if (globalRequestQueue.Count != globalQueueData[globalQueueData.Count - 1].yAxis)
            {
                globalQueueData.Add(new GraphData(events[0].timeStamp, globalRequestQueue.Count));
                if (globalRequestQueue.Count > maxGlobalQueue)
                {
                    maxGlobalQueue = globalRequestQueue.Count;
                }
            }

            if (finishedRequests.Count != finishedRequestsData[finishedRequestsData.Count - 1].yAxis)
            {
                finishedRequestsData.Add(new GraphData(events[0].timeStamp, finishedRequests.Count));
                if (finishedRequests.Count > maxFinishedRequests)
                {
                    maxFinishedRequests = finishedRequests.Count;
                }
            }
        }

        for (int i = 0; i < robots.Count; ++i)
        {
            if (robots[i].processingRequests.Count + robots[i].duplicateRequests.Count != agentGraphData[i][agentGraphData[i].Count - 1].yAxis)
            {
                agentGraphData[i].Add(new GraphData(events[0].timeStamp, robots[i].processingRequests.Count
                    + robots[i].duplicateRequests.Count));

                if (robots[i].processingRequests.Count + robots[i].duplicateRequests.Count > maxAgentRequests[i])
                {
                    maxAgentRequests[i] = robots[i].processingRequests.Count + robots[i].duplicateRequests.Count;
                }
            }
        }
    }

    void GenerateListOfEvents()
    {
        int eventCount = (int) (Settings.instance.timeSim * 60 * 60 / (Settings.instance.taskIntencity));
        Debug.Log("Start event count: " + eventCount);
        for (int i = 0; i < eventCount; ++i)
        {
            futureEventList.Add(new RequestArrival(new Request(Random.Range(0.0f, Settings.instance.timeSim * 60 * 60), ChooseARandomBox(), 
                Random.Range(0.0f, 1.0f))));
        }

        futureEventList.Sort((p, q) =>
        {
            return p.timeStamp.CompareTo(q.timeStamp);
        });

    }

    public void Pause()
    {
        instance.onPause = !instance.onPause;
        instance.topPauseText.gameObject.SetActive(instance.onPause);
        //instance.bottomPauseText.gameObject.SetActive(instance.onPause);
    }
    Box ChooseARandomBox()
    {
        float rnd = Random.Range(0f, 100f);
        float tmp = Settings.instance.chances[0].value;
        for (int i = 0; i < Settings.instance.chances.Count; ++i)
        {
            if (rnd <= tmp)
            {
                return instance.boxesByChance[i][Random.Range(0, boxesByChance[i].Count)];
            }
            else
            {
                tmp += Settings.instance.chances[i + 1].value;
            }
        }

        return null;
    }
    void InitSim()
    {
        InputParameters input = new InputParameters(Settings.instance.rowCount, Settings.instance.columnCount, Settings.instance.shelfLength, Settings.instance.floorCount, 
            Settings.instance.wingCount, Settings.instance.officeWidth, Settings.instance.roomType);
 
        levelScript.SetupScene(input);
    }
    void SortLifeByTypes(List<ValueAndWeight> list)
    {
        list[0].shelfLifeType = ShelfLife.Long;
        ValueAndWeight tmp = list[0];
        for (int i = 1; i < list.Count; ++i)
        {
            if (list[i].value == 0)
            {
                list[i].shelfLifeType = ShelfLife.Inf;
            }
            else
            if (tmp.shelfLifeType == ShelfLife.Short)
            {
                list[i].shelfLifeType = ShelfLife.Short;
            }
            else
            if (tmp.value / list[i].value > 5 && tmp.shelfLifeType == ShelfLife.Long)
            {
                list[i].shelfLifeType = ShelfLife.Short;
                tmp = list[i];
            }
            else
            if (list[i].value < tmp.value * Diviser)
            { 
                list[i].shelfLifeType = tmp.shelfLifeType - 1;
                tmp = list[i];
            }
            else
            {
                list[i].shelfLifeType = tmp.shelfLifeType;
            }
        }
    }
    void SortByDemand(List<ValueAndWeight> list)
    {
        list[0].demandType = DemandChance.High;
        ValueAndWeight tmp = list[0];
        for (int i = 1; i < list.Count; ++i)
        {
            if (tmp.demandType == DemandChance.Low)
            {
                list[i].demandType = DemandChance.Low;
            }
            else
            if (tmp.value / list[i].value > 5 && tmp.demandType == DemandChance.High)
            {
                list[i].demandType = DemandChance.Low;
                tmp = list[i];
            }
            else
            if (list[i].value < tmp.value * Diviser)
            {
                list[i].demandType = tmp.demandType - 1;
                tmp = list[i];
            }
            else
            {
                list[i].demandType = tmp.demandType;
            }
        }
    }
    void SetupUI()
    {
        followTarget = null;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        storageMenu = canvas.gameObject.transform.Find("StorageInf").gameObject;
        demandMenu = storageMenu.gameObject.transform.Find("DemandMenu").gameObject;
        shelfLifeMenu = storageMenu.gameObject.transform.Find("ShelfLifeMenu").gameObject;
        shelfLifeRoot = shelfLifeMenu.gameObject.transform.Find("ShelfLifeRoot").gameObject;
        demandRoot = demandMenu.gameObject.transform.Find("DemandRoot").gameObject;
        demandLegend = demandMenu.gameObject.transform.Find("DemandLegend").gameObject;
        shelfLifeLegend = shelfLifeMenu.gameObject.transform.Find("ShelfLifeLegend").gameObject;

        overallMenu = storageMenu.gameObject.transform.Find("OverallMenu").gameObject;
        overallRoot = overallMenu.gameObject.transform.Find("OverallRoot").gameObject;
        overallLegend = overallMenu.gameObject.transform.Find("OverallLegend").gameObject;

        disposeRoot = overallMenu.gameObject.transform.Find("DisposeRoot").gameObject;
        disposeLegend = overallMenu.gameObject.transform.Find("DisposeLegend").gameObject;

        graphMenu = canvas.gameObject.transform.Find("GraphMenu").gameObject;
        graphWindow = graphMenu.gameObject.transform.Find("GlobalQGraph").gameObject;
        graphContainer = graphWindow.gameObject.transform.Find("GraphContainer").GetComponent<RectTransform>();
        finishedGraphWindow = graphMenu.gameObject.transform.Find("FinishedRequests").gameObject;
        finishedGraphContainer = finishedGraphWindow.gameObject.transform.Find("GraphContainer").GetComponent<RectTransform>();

        infoList = canvas.gameObject.transform.Find("StatusList").gameObject;
        scrollView = infoList.gameObject.transform.Find("ScrollList").gameObject;
        scrollViewPort = scrollView.gameObject.transform.Find("ScrollViewPort").gameObject;
        listContent = scrollViewPort.gameObject.transform.Find("Content").gameObject;

        capacityErrMenu = canvas.gameObject.transform.Find("CapacityError").gameObject;

        GameObject graphButtons = graphMenu.gameObject.transform.Find("GraphButtons").gameObject;
        floorButtonsContent = graphButtons.gameObject.transform.Find("FloorButtons").gameObject
            .transform.Find("ScrollViewPort").gameObject.transform.Find("Content").gameObject;
        agentButtonsContent = graphButtons.gameObject.transform.Find("AgentButtons").gameObject
            .transform.Find("ScrollViewPort").gameObject.transform.Find("Content").gameObject;
        scanButtonsContent = graphButtons.gameObject.transform.Find("ScanButtons").gameObject
            .transform.Find("ScrollViewPort").gameObject.transform.Find("Content").gameObject;
        topPauseText = GameObject.Find("PauseTopText").GetComponent<Text>();
        visualisationFinished = canvas.gameObject.transform.Find("EventsFInished").GetComponent<Text>();
        topPauseText.gameObject.SetActive(true);
        visualisationFinished.gameObject.SetActive(false);
        timerContainer = canvas.gameObject.transform.Find("Playback").gameObject.transform.Find("TimerContainer").gameObject;
        chartMenu = canvas.gameObject.transform.Find("ChartsMenu").gameObject;
        agentStatsContainer = chartMenu.gameObject.transform.Find("AgentStats").gameObject.transform.Find("GraphContainer").GetComponent<RectTransform>();
        requestStatsContainer = chartMenu.gameObject.transform.Find("RequestStats").gameObject.transform.Find("GraphContainer").GetComponent<RectTransform>();
        calcStatsContainer = chartMenu.gameObject.transform.Find("CalcStats").gameObject.transform.Find("GraphContainer").GetComponent<RectTransform>();
        PopulateList();
    }
    void CreatePieChart(List<StatComponent> list, GameObject root, GameObject legend)
    {
        List<Wedge> wedges = new List<Wedge>();
        float summ = 0;

        CreateWedges(list, ref summ, wedges, root, legend);

        PlaceWedges(wedges, summ);
    }
    void PlaceWedges(List<Wedge> wedges, float summ)
    {
        float prevRotation = 0;

        for (int i = wedges.Count - 1; i >= 0; --i)
        {
            prevRotation = UpdateWedge(wedges[i], prevRotation, summ);
        }
    }
    float UpdateWedge(Wedge wedge, float prevZ, float summ)
    {
        float zRotation = CalculateRotation(prevZ, wedge.fillAmount, summ);
        prevZ = zRotation;
        Vector3 rotation = wedge.image.transform.eulerAngles;
        rotation.z += zRotation;
        wedge.image.transform.eulerAngles = rotation;

        wedge.image.fillAmount = wedge.fillAmount;
        Text txt = wedge.image.GetComponentInChildren<Text>();
        wedge.image.transform.SetAsFirstSibling();

        if (zRotation >= 180)
        {
            Vector3 rot = txt.transform.localEulerAngles;
            rot.z *= -1;
            txt.transform.localEulerAngles = rot;
        }

        txt.text = (wedge.typeName);
        txt.color = wedge.color;
        return prevZ;
    }
    float CalculateRotation(float prevZ, float fillAmount, float summ)
    {
        return 360 * (fillAmount / summ) + prevZ;
    }
    void CreateWedges(List<StatComponent> list, ref float summ, List<Wedge> wedges, GameObject root, GameObject legend)
    {
        foreach (StatComponent pair in list)
        {
            GameObject wedgeObj = Instantiate(wedgePrefab);
            wedgeObj.transform.SetParent(root.transform);
            wedgeObj.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            Image image = wedgeObj.GetComponent<Image>();

            Color rndColor = new Color(Random.Range(0.0f, 1f), Random.Range(0.0f, 1f), Random.Range(0.0f, 1f));
            image.color = rndColor;
            Wedge wedge = new Wedge(pair.percentage, pair.value.ToString(), image, rndColor);
            summ += pair.percentage;
            wedges.Add(wedge);

            GameObject legendItem = Instantiate(legendPrefab);
            legendItem.transform.SetParent(legend.transform);
            legendItem.GetComponent<Image>().color = rndColor;
            List<Text> tmp = legendItem.GetComponentsInChildren<Text>().ToList();
            tmp[0].text = pair.name;
            tmp[1].text = "Count: " + pair.count.ToString();
        }
    }
    void BuildStorage()
    {

        for (int i = floors.Count - 1; i > -1; --i)
        {
            if (Settings.instance.wingCount == 2)
            {
                secondWingFloors[i].storages.Reverse();
                foreach (Storage storage in secondWingFloors[i].storages)
                {
                    accessibleStorage.Push(storage);
                }
                secondWingFloors[i].storages.Reverse();
            }
            floors[i].storages.Reverse();
            foreach (Storage storage in floors[i].storages)
            {
                accessibleStorage.Push(storage);
            }
            floors[i].storages.Reverse();
        }

        for (int i = 0; i < Settings.instance.yearsSim * 365; ++i)
        {
            List<Box> boxes = GenerateDailyBoxes(i);
            foreach (Box box in boxes)
            {
                if (accessibleStorage.Peek().IsFull())
                {
                    accessibleStorage.Pop();
                }

                if (accessibleStorage.Count == 0)
                {
                    Debug.Log("Out of space");
                    errDay = i;
                    capacityErr = true;
                    break;
                }
                else
                {
                    accessibleStorage.Peek().AddBoxToStorage(box);
                    globalBoxCount++;
                    currentBoxCount++;
                }
            }

            if (accessibleStorage.Count == 0)
            {
                Debug.Log("Space ended!");
                break;
            }

            if (Settings.instance.boxDisposal == BoxDisposal.DoDispose)
            {
                DisposeBoxes(i);
            }

        }

    }

    void VerySimplePlacing()
    {
        for (int i = 0; i < Settings.instance.yearsSim * 365; ++i)
        {
            List<Box> boxes = GenerateDailyBoxes(i);
            foreach (Box box in boxes)
            {
                Storage tmp = FindAvailibleStorage();

                if (tmp == null)
                {
                    Debug.Log("Out of space");
                    errDay = i;
                    capacityErr = true;
                    break;
                }
                else
                {
                    tmp.AddBoxToStorage(box);
                    globalBoxCount++;
                    currentBoxCount++;
                }
            }

            if (capacityErr)
            {
                Debug.Log("Space ended!");
                break;
            }

            if (Settings.instance.boxDisposal == BoxDisposal.DoDispose)
            {
                DisposeBoxes(i);
            }

        }
    }

    Storage FindAvailibleStorage()
    {
        Storage ans = null;
        for (int i = 0; i < floors.Count; ++i)
        {
            bool success = false;
            foreach (Storage tmp in floors[i].storages)
            {
                if (!tmp.IsFull())
                {
                    ans = tmp;
                    success = true;
                    break;
                }
            }

            if (!success && Settings.instance.wingCount == 2)
            {
                foreach (Storage tmp in secondWingFloors[i].storages)
                {
                    if (!tmp.IsFull())
                    {
                        ans = tmp;
                        success = true;
                        break;
                    }
                }
            }

            if (success)
            {
                break;
            }
            
        }

        return ans;
    }

    void SimpleByDemand() 
    { 
        for (int i = 0; i < 3; ++i)
        {
            Stack<Storage> tmp = new Stack<Storage>();
            accessibleStorages.Add(tmp);
        }

        for (int i = floors.Count - 1; i > -1; --i)
        {
            if (Settings.instance.wingCount == 2)
            {
                secondWingFloors[i].storages.Reverse();
                foreach (Storage storage in secondWingFloors[i].storages)
                {
                     Parallel.Invoke(
                        () => accessibleStorages[0].Push(storage),
                        () => accessibleStorages[1].Push(storage),
                        () => accessibleStorages[2].Push(storage));
                }
                secondWingFloors[i].storages.Reverse();
            }

            floors[i].storages.Reverse();

            foreach (Storage storage in floors[i].storages)
            {
                Parallel.Invoke(
                    () => accessibleStorages[0].Push(storage),
                    () => accessibleStorages[1].Push(storage),
                    () => accessibleStorages[2].Push(storage));
            }
            floors[i].storages.Reverse();
        }

        for (int i = 0; i < Settings.instance.yearsSim * 365; ++i)
        {
            List<Box> boxes = GenerateDailyBoxes(i);
            foreach (Box box in boxes)
            {

                if (accessibleStorages[0].Count == 0 && accessibleStorages[1].Count == 0
                && accessibleStorages[2].Count == 0)
                {
                    Debug.Log("Space ended!");
                    break;
                }
                else
                {
                    PutBoxByDemandSimple(box, i);
                }

            }


            if (accessibleStorages[0].Count == 0 && accessibleStorages[1].Count == 0 
                && accessibleStorages[2].Count == 0)
            {
                Debug.Log("Space ended!");
                break;
            }

            if (Settings.instance.boxDisposal == BoxDisposal.DoDispose)
            {
                DisposeBoxes(i);
            }

        }

    }
    public StoragePointer FindNextStoragePointer(StoragePointer pointer)
    {
        if (pointer.storageID == instance.storageOnFloor - 1 && pointer.wing == 1)
        {
            return new StoragePointer(0, 2, pointer.floor);
        }
        else
        if (pointer.storageID == instance.storageOnFloor - 1 && pointer.wing == 2 && pointer.floor < Settings.instance.floorCount - 1)
        {
            return new StoragePointer(0, 1, pointer.floor + 1);
        }
        else
        if (pointer.storageID == instance.storageOnFloor - 1 && pointer.wing == 2 && pointer.floor == Settings.instance.floorCount - 1)
        {
            return null;
        }
        else
        {
            return new StoragePointer(pointer.storageID + 1, pointer.wing, pointer.floor);
        }
    }
    void FillByCorners()
    {
        for (int i = 0; i < 3; ++i)
        {
            Stack<Storage> tmp = new Stack<Storage>();
            accessibleStorages.Add(tmp);
        }

        for (int i = floors.Count - 1; i > -1; --i)
        {
            if (Settings.instance.wingCount == 2)
            {
                secondWingFloors[i].storages.Reverse();
                foreach (Storage storage in secondWingFloors[i].storages)
                {
                    Parallel.Invoke(
                        () => accessibleStorages[0].Push(storage),
                        () => accessibleStorages[1].Push(storage));
                }
                secondWingFloors[i].storages.Reverse();
            }

            floors[i].storages.Reverse();

            foreach (Storage storage in floors[i].storages)
            {
                    Parallel.Invoke(
                    () => accessibleStorages[0].Push(storage),
                    () => accessibleStorages[1].Push(storage));
            }
            floors[i].storages.Reverse();
        }

        for (int i = 0; i < Settings.instance.yearsSim * 365; ++i)
        {
            List<Box> boxes = GenerateDailyBoxes(i);
            boxes.Reverse();
            foreach (Box box in boxes)
            {
                if (accessibleStorages[0].Count == 0 && accessibleStorages[1].Count == 0
                && accessibleStorages[2].Count == 0)
                {
                    Debug.Log("Space ended!");
                    break;
                }
                else
                {
                    PutBoxInCorner(box, i);
                }


            }


            if (accessibleStorages[0].Count == 0 && accessibleStorages[1].Count == 0
                && accessibleStorages[2].Count == 0)
            {
                Debug.Log("Space ended!");
                break;
            }

            if (Settings.instance.boxDisposal == BoxDisposal.DoDispose)
            {
                DisposeBoxes(i);
            }

        }
    }
    void PutBoxInCorner(Box box, int day)
    {
        for (int i = 0; i < accessibleStorages.Count; ++i)
        {
            if (accessibleStorages[i].Count != 0)
            {
                while (!accessibleStorages[i].Peek().CheckStorageInCorners(i))
                {
                    accessibleStorages[i].Pop();
                    if (accessibleStorages[i].Count == 0)
                    {
                        break;
                    }
                }
            }
        }

        if (accessibleStorages[0].Count == 0 && accessibleStorages[1].Count == 0
                && accessibleStorages[2].Count == 0)
        {
            Debug.Log("Out of space");
            errDay = day;
            capacityErr = true;
        }
        else
        {
            if (accessibleStorages[(int)box.demandChanceType - 1].Count != 0)
            {
                accessibleStorages[(int)box.demandChanceType - 1].Peek().BoxToCorner(box, (int)box.demandChanceType - 1);
                globalBoxCount++;
                currentBoxCount++;
            }
            else
            {
                FindAlternativeCorner(box);
                globalBoxCount++;
                currentBoxCount++;
            }
        }

    }
    void FindAlternativeCorner(Box box)
    {
        if ((int)box.demandChanceType - 1 == 2)
        {
            if (accessibleStorages[1].Count != 0)
            {
                accessibleStorages[1].Peek().BoxToCorner(box, 1);
            }
            else
            {
                accessibleStorages[0].Peek().BoxToCorner(box, 0);
            }
        }
        else
        if ((int)box.demandChanceType - 1 == 1)
        {
            if (accessibleStorages[2].Count != 0)
            {
                accessibleStorages[2].Peek().BoxToCorner(box, 2);
            }
            else
            {
                accessibleStorages[0].Peek().BoxToCorner(box, 0);
            }
        }
        else
        {
            if (accessibleStorages[1].Count != 0)
            {
                accessibleStorages[1].Peek().BoxToCorner(box, 1);
            }
            else
            {
                accessibleStorages[2].Peek().BoxToCorner(box, 2);
            }
        }
    }
    void PutBoxByDemandSimple(Box box, int day)
    {
        for (int i = 0; i < accessibleStorages.Count; ++i)
        {
            if (accessibleStorages[i].Count != 0)
            {
                while (accessibleStorages[i].Peek().CheckStorageByStacks(i))
                {
                    accessibleStorages[i].Pop();
                    if (accessibleStorages[i].Count == 0)
                    {
                        break;
                    }
                }
            }
        }

        if (accessibleStorages[0].Count == 0 && accessibleStorages[1].Count == 0
                && accessibleStorages[2].Count == 0)
        {
            Debug.Log("Out of space");
            errDay = day;
            capacityErr = true;
        }
        else
        {
            if (accessibleStorages[(int)box.demandChanceType - 1].Count != 0)
            {
                accessibleStorages[(int)box.demandChanceType - 1].Peek().BoxToStackSimple(box, (int)box.demandChanceType - 1);
                globalBoxCount++;
                currentBoxCount++;
            }
            else
            {
                FindAlternativeStorage(box);
                globalBoxCount++;
                currentBoxCount++;
            }
        }
    }
    void FindAlternativeStorage(Box box)
    {
        if (box.demandChanceType == DemandChance.Low)
        {
            if (accessibleStorages[1].Count != 0)
            {
                accessibleStorages[1].Peek().BoxToStackSimple(box, 1);
            }
            else
            {
                accessibleStorages[2].Peek().BoxToStackSimple(box, 2);
            }
        }

        if (box.demandChanceType == DemandChance.High)
        {
            if (accessibleStorages[1].Count != 0)
            {
                accessibleStorages[1].Peek().BoxToStackSimple(box, 1);
            }
            else
            {
                 accessibleStorages[0].Peek().BoxToStackSimple(box, 0);
            }
        }

        if (box.demandChanceType == DemandChance.Mid)
        {
            if (accessibleStorages[2].Count != 0)
            {
                accessibleStorages[2].Peek().BoxToStackSimple(box, 2);
            }
            else
            {
                accessibleStorages[0].Peek().BoxToStackSimple(box, 0);
            }
        }
    }
    void AllocateStorage(int previousFloor)
    {
        if (Settings.instance.wingCount == 2)
        {
            secondWingFloors[previousFloor+1].storages.Reverse();
            foreach (Storage storage in secondWingFloors[previousFloor+1].storages)
            {
                Parallel.Invoke(
                    () => accessibleStorages[0].Push(storage),
                    () => accessibleStorages[1].Push(storage),
                    () => accessibleStorages[2].Push(storage));
            }
            secondWingFloors[previousFloor+1].storages.Reverse();
        }

        floors[previousFloor+1].storages.Reverse();

        foreach (Storage storage in floors[previousFloor+1].storages)
        {
            Parallel.Invoke(
                () => accessibleStorages[0].Push(storage),
                () => accessibleStorages[1].Push(storage),
                () => accessibleStorages[2].Push(storage));
        }
        floors[previousFloor+1].storages.Reverse();

    }
    void DisposeBoxes(int day)
    {
        foreach (Queue<Box> q in boxesByLife)
        {
            if (q.Count != 0 && q.Peek().lifeSpan != 0)
            {
                while (day - q.Peek().arrivalDay >= q.Peek().lifeSpan * 365)
                {
                    boxesToDispose.Add(q.Dequeue());
                }
            }
        }

        if (boxesToDispose.Count >= Settings.instance.boxDisposeCap)
        {
            foreach(Box box in boxesToDispose)
            {
                    box.Dispose();
                    boxesDisposed++;
                    currentBoxCount--;
            }
            boxesToDispose.Clear();
        }
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    List<Box> GenerateDailyBoxes(int day)
    {
        List<Box> boxes = new List<Box>();
        for (int i = 0; i < Random.Range(Settings.instance.averageLoad - Settings.instance.avgDeviation, 
            Settings.instance.averageLoad + Settings.instance.avgDeviation + 1); ++i)
        {
            Box newBox = new Box(day, globalBoxCount + i);
            boxes.Add(newBox);
        }

        boxes.Sort((p, q) =>
        {
            int result = q.demandChanceType.CompareTo(p.demandChanceType);
            if (result == 0)
            {
                result = p.shelfLifeType.CompareTo(q.shelfLifeType);
            }
            return result;
        });

        return boxes;
    }
    public void AddRobotToList(Robot robot)
    {
        robots.Add(robot);
        robot.idCount = robots.Count - 1;
    }
    private void Update()
    {
#if !UNITY_ANDROID
        if (Input.GetKeyDown("space"))
        {
            onPause = !onPause;
            topPauseText.gameObject.SetActive(onPause);
        }
#endif

        if (!onPause)
        {
            globalTimer += Time.deltaTime;
            lastTimeStep = Time.deltaTime;
            UpdateTimer();
        }

        if (!visualisationFinished.IsActive() && finishedRequestsData.Count != 0
            && globalTimer > finishedRequestsData[finishedRequestsData.Count - 1].xAxis)
        {
            visualisationFinished.gameObject.SetActive(true);
        }

    }

    public void SetNormalSpeed()
    {
        Time.timeScale = 1.0f;
    }
    public void SpeedUpTwice()
    {
        Time.timeScale = 2.0f;
    }
    public void SpeedUpFourTimes()
    {
        Time.timeScale = 4.0f;
    }
    public void SpeedUpEightTimes()
    {
        Time.timeScale = 8.0f;
    }
    void UpdateTimer()
    {
        GameObject mainTimer = timerContainer.gameObject.transform.Find("MainTimer").gameObject;
        GameObject miliSeconds = timerContainer.gameObject.transform.Find("MiliSeconds").gameObject;
        mainTimer.GetComponent<Text>().text = ParseTimer(globalTimer, out string mSeconds);
        miliSeconds.GetComponent<Text>().text = mSeconds;
    }

    public static string ParseTimer(float timer, out string miliSeconds)
    {
        string hours = ((int)timer / 3600).ToString();
        if (hours.Length == 1)
        {
            hours = "0" + hours;
        }

        string minutes = ((int)timer / 60 % 60).ToString();
        if (minutes.Length == 1)
        {
            minutes = "0" + minutes;
        }

        string seconds = ((int)timer % 60).ToString();
        if (seconds.Length == 1)
        {
            seconds = "0" + seconds;
        }

        miliSeconds = (timer - (int)timer).ToString();
        if (miliSeconds.Length == 1)
        {
            miliSeconds = miliSeconds.Remove(0, 1);
        }
        else
        {
            miliSeconds = miliSeconds.Remove(0, 2);
        }
        if (miliSeconds.Length == 0)
        {
            miliSeconds = "00";
        }
        else
        if (miliSeconds.Length == 1)
        {
            miliSeconds = "0" + miliSeconds;
        }
        else
        if (miliSeconds.Length > 2)
        {
            miliSeconds = miliSeconds.Remove(2);
        }
        miliSeconds = "." + miliSeconds;
        return hours + ":" + minutes + ":" + seconds;
    }
    private void PopulateList()
    {
        foreach (Robot robot in robots)
        {
            GameObject scrollItem = Instantiate(statusItem);
            scrollItem.transform.SetParent(listContent.transform, false);
            scrollItem.transform.Find("IDText").gameObject.GetComponent<Text>().text = robot.idCount.ToString();
            statusList.Add(scrollItem);
        }
    }
    public void SetFollowtarget(GameObject gameObject)
    {
        followTarget = gameObject;
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Destroy(Settings.instance.gameObject);
        Destroy(instance.gameObject);
        Settings.instance = null;
        instance = null;
    }
    void DisposeOfBoxes()
    {
        Parallel.ForEach(boxesByChance, list =>
        {
            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (list[i].isDisposed)
                {
                    list.RemoveAt(i);
                }
            }
        });
    }
}
public class WorldBounds
{
    public int left;
    public int right;
    public int up;
    public int down;
    public WorldBounds(int left, int right, int up, int down)
    {
        this.left = left;
        this.right = right;
        this.up = up;
        this.down = down;
    }
}
internal class InputParameters
{
    internal int rows;
    internal int columns;
    internal int shelfLength;
    internal int floorCount;
    internal int wingCount;
    internal int officeWidth;
    internal RoomType roomType;
    public InputParameters() : this(Random.Range(1,10), Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10), RoomType.SimpleSingleRow)
    {

    }
    public InputParameters(int rows, int columns, int shelfLength, int floorCount, RoomType roomType)
    {
        this.rows = rows;
        this.columns = columns;
        this.shelfLength = shelfLength;
        this.floorCount = floorCount;
        wingCount = 0;
        this.roomType = roomType;
    }
    public InputParameters(int rows, int columns, int shelfLength, int floorCount, int wingCount, int officeWidth, RoomType roomType) : this(rows, columns, shelfLength, floorCount, roomType)
    {
        this.wingCount = wingCount;
        this.officeWidth = officeWidth;
    }
}
public class Wedge 
{
    public float fillAmount;
    public string typeName;
    public Image image;
    public Color color;
    public Wedge(float fill, string name, Image image, Color color)
    {
        this.fillAmount = fill;
        this.typeName = name;
        this.image = image;
        this.color = color;
    }
}
public class StatComponent
{
    public string value;
    public float percentage;
    public int count;
    public string name;
    public StatComponent(string value, float percentage, int count, string s)
    {
        this.value = value;
        this.percentage = percentage;
        this.count = count;
        this.name = s;
    }
}

public class StoragePointer
{
    public int storageID;
    public int wing;
    public int floor;
    public StoragePointer(int storageID, int wing, int floor)
    {
        this.storageID = storageID;
        this.wing = wing;
        this.floor = floor;
    }
    public StoragePointer() : this(0, 1, 0)
    {

    }
}

public abstract class ModelEvent
{
    public float timeStamp;

    public ModelEvent(float timeStamp)
    {
        this.timeStamp = timeStamp;
    }
}

public class RequestArrival : ModelEvent
{
    public Request request;

    public RequestArrival(Request request) : base(request.creationTime)
    {
        this.request = request;
        //timeStamp = request.creationTime;
    }
}

public class BoxPickUp : ModelEvent
{
    public Task task;
    public float arrivalTime;
    public Robot worker;

    public BoxPickUp(Task task, float arrivalTime, Robot robot) : base(arrivalTime)
    {
        this.task = task;
        this.arrivalTime = arrivalTime;
        //this.timeStamp = arrivalTime;
        worker = robot;
    }
}

public class QueueRelease : ModelEvent
{
    public int floorNumber;

    public QueueRelease(int floor, float time) : base(time)
    {
        floorNumber = floor;
        //timeStamp = time;
    }
}

public class ScanRelease : ModelEvent
{
    public int floorNumber;

    public ScanRelease(int floor, float time) : base(time)
    {
        floorNumber = floor;
        //timeStamp = time;
    }
}

public class BoxPlaced : ModelEvent
{
    public Task task;
    public Robot worker;
    public BoxPlaced(Task task, Robot worker, float timeStamp) : base(timeStamp)
    {
        this.task = task;
        this.worker = worker;
    }
}

public class WorkerArrival : ModelEvent
{
    public Task task;
    public float arrivalTime;
    public Robot worker;

    public WorkerArrival(Task task, float arrivalTime, Robot robot) : base (arrivalTime)
    {
        this.task = task;
        this.arrivalTime = arrivalTime;
        //this.timeStamp = arrivalTime;
        worker = robot;
    }
}

public class RequestReturn : ModelEvent
{
    public Queue<Request> requests = new Queue<Request>();
    public int floorNumber;
    public RequestReturn(Queue<Request> requests, int floor, float time) : base(time)
    {
        while (requests.Count != 0)
        {
            this.requests.Enqueue(requests.Dequeue());
        }
        floorNumber = floor;
        //timeStamp = time;
    }
}

public class Request
{
    public float creationTime;
    public float travelStart;
    public float pickUpTime;
    public float arrivalTime;
    public float unpackingStart;
    public float unpackingEnd;
    public float processingStart = 0f;
    public float processingEnd = 0f;
    public float scanQueueLeaveTime;
    public float unpackingQLeaveTime = 0f;
    public float globalQLeaveTime = 0f;
    public Box box;
    public RequestType requestType;
    public bool isADuplicate = false;

    public Request(float creationTime, Box box, float diceRoll)
    {
        this.creationTime = creationTime;
        this.box = box;
        if (diceRoll < 0.1f)
        {
            requestType = RequestType.DontScan;
        }
        else
        {
            requestType = RequestType.Scan;
        }
    }
}

public class GraphData
{
    public float xAxis;
    public float yAxis;
    
    public GraphData(float timeStamp, float value)
    {
        xAxis = timeStamp;
        yAxis = value;
    }
}

public enum RequestType
{
    Scan,
    DontScan
}

