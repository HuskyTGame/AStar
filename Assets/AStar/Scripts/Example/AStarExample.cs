/****************************************************
	文件：AStarExample.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/1 23:56:55
	功能：AStar 例子
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using HTUtility;
using System.Collections;

namespace AStar.Example
{
    public class AStarExample : MonoBehaviour
    {
        private const string NODE_PREFAB_PATH = "Prefabs/Node";
        private const string TIMER_KEY_ASTAR_PF = "AStarPathFinding";
        private const string TIMER_KEY_CREATE_MAP = "CreateRandomMap";
        private const string SAVE_KEY_DATA_ID = "NewDataId000";

        public int MapWidth;
        public int MapHeight;
        public GameObject NodePrefab;
        public float AnimTime;
        [Tooltip("障碍物生成概率（0,1）")]
        public float Probability;
        [Tooltip("初始随机种子（>-1）")]
        public int RandomSeed;
        [Tooltip("地图生成最大尝试次数")]
        public int MaxTimes;
        [Tooltip("寻路循环次数（测试使用）")]
        public int LoopTimes;
        public Color NormalColor;
        public Color StartColor;
        public Color EndColor;
        public Color ObstacleColor;
        public Color PathColor;
        public Color SearchColor;
        public CameraController CameraCtr;

        private MapMgr mMapMgr;
        private AStarMgr mAStarMgr;
        private RandomMapCreator mRandomMapCreator;
        private Recorder mRecorder;
        private TimerServiceForTest mTimer;
        private int mCurrentSeed;
        private Transform[,] mNodeTransArray;
        private List<Node> mPath;
        private float mFindPathTime;
        private int mFindPathCost;
        private int mDataId;

        public int NewDataId
        {
            get
            {
                mDataId++;
                PlayerPrefs.SetInt(SAVE_KEY_DATA_ID, mDataId);
                return mDataId;
            }
        }

        public void SetColor(Transform target, Color c)
        {
            target.GetComponent<MeshRenderer>().material.color = c;
        }
        public void SetColor(Node target, Color c)
        {
            GetTrans(target).GetComponent<MeshRenderer>().material.color = c;
        }
        public Transform GetTrans(Node point)
        {
            if (mNodeTransArray == null || mNodeTransArray.Length <= 0) return null;
            return mNodeTransArray[point.X, point.Y];
        }

        private void Init()
        {
            CustomLoggerService.Instance.Init();
            mTimer = TimerServiceForTest.Instance;
            mTimer.Init();
            SaveMgr.Instance.Init();
            mMapMgr = MapMgr.Instance;
            mMapMgr.Init(MapWidth, MapHeight);
            mAStarMgr = AStarMgr.Instance;
            mAStarMgr.Init();
            mRandomMapCreator = RandomMapCreator.Instance;
            mRandomMapCreator.Init();
            mRecorder = Recorder.Instance;
            mRecorder.Init();
            mTimer.Subscribe(TIMER_KEY_CREATE_MAP);
            mTimer.Subscribe(TIMER_KEY_ASTAR_PF);
            InitDataId();
            HTLogger.Info("All init done.");
        }
        private void Start()
        {
            Init();
            CameraCtr.Setup(MapWidth, MapHeight);
            GenerateRandomMap();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ClickPathFindingBtn(LoopTimes);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                PathFindingAnimation();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SaveRunningData();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                SimpleAStarPathFinding();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                PathFindingAnimationForSimple();
            }
        }

        private void GenerateRandomMap()
        {
            mTimer.Begin(TIMER_KEY_CREATE_MAP);
            mCurrentSeed = RandomMapCreator.Instance.TryToCreate(Probability, RandomSeed, MaxTimes);
            mTimer.End(TIMER_KEY_CREATE_MAP);
            HTLogger.Debug(string.Format("生成随机地图耗时：{0}毫秒", mTimer.GetSumTime(TIMER_KEY_CREATE_MAP)));
            HTLogger.Debug("随机种子：" + mCurrentSeed);
            SpawnMap();
        }
        private void ClickPathFindingBtn(int times)
        {
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(mMapMgr.Width - 1, mMapMgr.Height - 1);
            mMapMgr.BuildMap();
            mTimer.Begin(TIMER_KEY_ASTAR_PF);
            for (int i = 0; i < times; i++)
            {
                mAStarMgr.FindPath(start, end);
            }
            mTimer.End(TIMER_KEY_ASTAR_PF);
            mPath = mAStarMgr.FindPath(start, end);
            mFindPathTime = mTimer.GetSumTime(TIMER_KEY_ASTAR_PF);
            mFindPathCost = CalculatePathConsume(mPath);
            HTLogger.Debug(string.Format("寻路 {0} 次：{1}毫秒", times, mFindPathTime));
            HTLogger.Debug("路径消耗：" + mFindPathCost);
        }
        private void SaveRunningData()
        {
            int newId = NewDataId;
            RunningData data = new RunningData
            {
                Id = newId,
                Info = "AStart 运行时的数据",
                MapWidth = MapWidth,
                MapHeight = MapHeight,
                MapNodeCount = MapWidth * MapHeight,
                RandomSeed = mCurrentSeed,
                LoopCount = LoopTimes,
                RunningTime = mFindPathTime,
                PathCost = mFindPathCost,
            };
            mRecorder.SetRunningData(data, newId);
            mRecorder.SaveRunningData(newId);
        }
        /// <summary>
        /// 计算路径消耗
        /// </summary>
        private int CalculatePathConsume(List<Node> path)
        {
            int consume = 0;
            Node pre = null, now = null;
            foreach (Node point in path)
            {
                now = point;
                if (pre == null)
                {
                    pre = point;
                    continue;
                }
                if ((now.X - pre.X) != 0 && (now.Y - pre.Y) != 0)
                    consume += 14;
                else
                    consume += 10;
                pre = point;
            }
            return consume;
        }
        public void PathFindingAnimation()
        {
            mPath.RemoveAt(mPath.Count - 1);
            mPath.RemoveAt(0);
            StartCoroutine(PathFindingAnim(mPath, AnimTime, PathColor));
        }
        private IEnumerator PathFindingAnim(List<Node> path, float time, Color pathColor)
        {
            for (int i = 0; i < path.Count; i++)
            {
                yield return new WaitForSeconds(time);
                Node point = path[i];
                Transform nodeTrans = mNodeTransArray[point.X, point.Y];
                SetColor(nodeTrans, pathColor);
            }
        }
        private void RefreshColor()
        {
            for (int i = 0; i < mMapMgr.Width; i++)
            {
                for (int j = 0; j < mMapMgr.Height; j++)
                {
                    Node node = mMapMgr.Map[i, j];
                    if (node.IsConnected)
                    {
                        SetColor(node, NormalColor);
                    }
                    else
                    {
                        SetColor(node, ObstacleColor);
                    }
                }
            }
            SetColor(mMapMgr.Map[0, 0], StartColor);
            SetColor(mMapMgr.Map[mMapMgr.Width - 1, mMapMgr.Height - 1], EndColor);
        }
        private void SpawnMap()
        {
            mNodeTransArray = new Transform[mMapMgr.Width, mMapMgr.Height];
            for (int i = 0; i < mMapMgr.Width; i++)
            {
                for (int j = 0; j < mMapMgr.Height; j++)
                {
                    Node node = mMapMgr.Map[i, j];
                    mNodeTransArray[i, j] = SpawnNode(node).transform;
                    if (node.IsConnected)
                    {
                        SetColor(node, NormalColor);
                    }
                    else
                    {
                        SetColor(node, ObstacleColor);
                    }
                }
            }
            SetColor(mMapMgr.Map[0, 0], StartColor);
            SetColor(mMapMgr.Map[mMapMgr.Width - 1, mMapMgr.Height - 1], EndColor);
            HTLogger.Debug("实例化地图成功");
        }
        private GameObject SpawnNode(Node point)
        {
            GameObject go = LoadResource(NODE_PREFAB_PATH);
            return Instantiate(go, new Vector3(point.X, 0, point.Y), Quaternion.identity);
        }
        private GameObject LoadResource(string path)
        {
            return Resources.Load<GameObject>(path);
        }
        private void InitDataId()
        {
            mDataId = PlayerPrefs.GetInt(SAVE_KEY_DATA_ID, 0);
        }


        #region 未优化的 AStar
        private List<SimpleAStar.Point> mSimplePath = new List<SimpleAStar.Point>();

        private void SimpleAStarPathFinding()
        {
            List<SimpleAStar.Point> obstacleList = new List<SimpleAStar.Point>();
            for (int i = 0; i < MapWidth; i++)
            {
                for (int j = 0; j < MapHeight; j++)
                {
                    if (mMapMgr.Map[i, j].IsConnected == false)
                    {
                        obstacleList.Add(new SimpleAStar.Point(i, j));
                    }
                }
            }
            SimpleAStar.AStarS aStars = new SimpleAStar.AStarS(MapWidth, MapHeight, obstacleList);
            SimpleAStar.Point start = new SimpleAStar.Point(0, 0);
            SimpleAStar.Point end = new SimpleAStar.Point(MapWidth - 1, MapHeight - 1);
            mTimer.Begin(TIMER_KEY_ASTAR_PF);
            for (int i = 0; i < LoopTimes; i++)
            {
                aStars.FindPath(start, end);
            }
            mTimer.End(TIMER_KEY_ASTAR_PF);
            mSimplePath = aStars.FindPath(start, end);
            List<Node> pathNodeList = new List<Node>();
            for (int i = 0; i < mSimplePath.Count; i++)
            {
                SimpleAStar.Point point = mSimplePath[i];
                pathNodeList.Add(new Node(point.X, point.Y));
            }
            int cost = CalculatePathConsume(pathNodeList);
            float time = mTimer.GetSumTime(TIMER_KEY_ASTAR_PF);
            HTLogger.Debug(string.Format("（简易 AStar）寻路 {0} 次：{1}毫秒", LoopTimes, time));
            HTLogger.Debug("（简易 AStar）路径消耗：" + cost);
        }
        public void PathFindingAnimationForSimple()
        {
            mSimplePath.RemoveAt(mSimplePath.Count - 1);
            mSimplePath.RemoveAt(0);
            StartCoroutine(PathFindingAnimForSimple(mSimplePath, AnimTime, SearchColor));
        }
        private IEnumerator PathFindingAnimForSimple(List<SimpleAStar.Point> path, float time, Color pathColor)
        {
            for (int i = 0; i < path.Count; i++)
            {
                yield return new WaitForSeconds(time);
                SimpleAStar.Point point = path[i];
                Transform nodeTrans = mNodeTransArray[point.X, point.Y];
                SetColor(nodeTrans, pathColor);
            }
        }
        #endregion
    }
}
