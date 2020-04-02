/****************************************************
	文件：RandomMapCreator.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/2 1:23:35
	功能：随机地图生成器
*****************************************************/

using System.Collections.Generic;
using UnityEngine;
using HTUtility;

namespace AStar.Example
{
    public class RandomMapCreator : HTSingleton<RandomMapCreator>
    {
        private MapMgr mMapMgr;

        public void Init()
        {
            mMapMgr = MapMgr.Instance;
            HTLogger.Info("RandomMapCreator init done.");
        }

        /// <summary>
        /// 尝试创建随机 Map
        /// </summary>
        /// <param 障碍物生成概率（0,1）="probability"></param>
        /// <param 起始随机种子>-1="startSeed"></param>
        /// <param 尝试最大次数="times"></param>
        /// <returns></returns>
        public int TryToCreate(float probability, int startSeed, int times)
        {
            Node[,] mapTemp = new Node[mMapMgr.Width, mMapMgr.Height];
            mapTemp = CopyMap();
            int count = 0;
            while (count < times)
            {
                Create(startSeed, probability);
                if (CheckMapIsConnected())
                {
                    HTLogger.Debug("随机地图生成成功");
                    return startSeed;
                }
                else
                    RecoverMap(mapTemp);
                startSeed++;
                count++;
            }
            HTLogger.Debug("随机地图生成失败");
            return -1;
        }
        /// <summary>
        /// 根据随机种子和障碍物生成概率创建 Map
        /// </summary>
        /// <param 随机种子="randomSeed"></param>
        /// <param 障碍物生成概率（0,1）="probability"></param>
        public void Create(int randomSeed, float probability)
        {
            List<Node> obstacleList = GenerateObstacleList(probability, mMapMgr.Width, mMapMgr.Height, randomSeed);
            AddObstacles(obstacleList);
        }
        /// <summary>
        /// 检测地图是连通的
        /// </summary>
        /// <returns></returns>
        public bool CheckMapIsConnected()
        {
            bool isConnected = false;
            mMapMgr.BuildMap();
            List<Node> path = AStarMgr.Instance.FindPath(new Vector2(0, 0), new Vector2(mMapMgr.Width - 1, mMapMgr.Height - 1));
            if (path != null && path.Count > 0)
            {
                isConnected = true;
            }
            return isConnected;
        }
        private void RecoverMap(Node[,] temp)
        {
            for (int i = 0; i < mMapMgr.Width; i++)
            {
                for (int j = 0; j < mMapMgr.Height; j++)
                {
                    Node node = mMapMgr.Map[i, j];
                    Node tem = temp[i, j];
                    CopyNode(tem, ref node);
                }
            }
        }
        private void AddObstacles(List<Node> obstacleList)
        {
            mMapMgr.AddObstacleList(obstacleList);
        }
        /// <summary>
        /// 随机生成障碍物
        /// </summary>
        /// <param 障碍物生成概率（0,1）="probability"></param>
        /// <param 地图宽="width"></param>
        /// <param 地图高="height"></param>
        /// <param 随机种子="seed"></param>
        /// <returns></returns>
        private List<Node> GenerateObstacleList(float probability, int width, int height, int seed)
        {
            if (probability > 1 || probability < 0)
            {
                HTLogger.Error("障碍物生成概率不合法！");
                return null;
            }
            List<Node> res = new List<Node>();
            List<int> randomNumList = GenerateRandomNumList(1, 100, width * height, seed);
            int threshold = Mathf.FloorToInt(probability * 100.0f);
            for (int i = 0, k = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++, k++)
                {
                    if (randomNumList[k] <= threshold)
                    {
                        res.Add(mMapMgr.Map[i, j]);
                    }
                }
            }
            return res;
        }
        /// <summary>
        /// 根据随机种子 生成随机数序列
        /// </summary>
        /// <param 随机区间最小值（闭区间）="min"></param>
        /// <param 随机区间最大值（闭区间）="max"></param>
        /// <param 生成个数="len"></param>
        /// <param 随机种子="randomSeed"></param>
        /// <returns></returns>
        private List<int> GenerateRandomNumList(int min, int max, int len, int randomSeed)
        {
            List<int> res = new List<int>();
            System.Random random = new System.Random(randomSeed);
            for (int i = 0; i < len; i++)
            {
                res.Add(random.Next(min, max + 1));
            }
            return res;
        }

        private Node[,] CopyMap()
        {
            Node[,] temp = new Node[mMapMgr.Width, mMapMgr.Height];
            for (int i = 0; i < mMapMgr.Width; i++)
            {
                for (int j = 0; j < mMapMgr.Height; j++)
                {
                    Node node = mMapMgr.Map[i, j];
                    Node tem = new Node(node.X, node.Y, node.NodeType, node.IsConnected);
                    CopyNode(node, ref tem);
                    temp[i, j] = tem;
                }
            }
            return temp;
        }
        private void CopyNode(Node srcNode, ref Node temp)
        {
            temp.X = srcNode.X;
            temp.Y = srcNode.Y;
            temp.NodeType = srcNode.NodeType;
            temp.IsConnected = srcNode.IsConnected;
            temp.F = srcNode.F;
            temp.G = srcNode.G;
            temp.H = srcNode.H;
            temp.Parent = srcNode.Parent;
            temp.NeighborList = srcNode.NeighborList;
            temp.NeighborCostG = srcNode.NeighborCostG;
            temp.InOpenList = srcNode.InOpenList;
            temp.InCloseList = srcNode.InCloseList;
        }
    }
}