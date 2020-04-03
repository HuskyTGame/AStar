/****************************************************
	文件：MapMgr.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/1 23:59:45
	功能：Map 管理器
*****************************************************/

using System;
using UnityEngine;
using HTUtility;
using System.Collections.Generic;

namespace AStar
{
    public class MapMgr : HTSingleton<MapMgr>
    {
        private int mWidth;
        private int mHeight;
        private Node[,] mMap;

        /// <summary>
        /// 地图宽
        /// </summary>
        public int Width { get => mWidth; }
        /// <summary>
        /// 地图高
        /// </summary>
        public int Height { get => mHeight; }
        /// <summary>
        /// 地图 Node
        /// </summary>
        public Node[,] Map { get => mMap; }


        #region 生命周期
        public void Init(int width, int height)
        {
            mWidth = width;
            mHeight = height;
            mMap = new Node[mWidth, mHeight];
            for (int i = 0; i < mWidth; i++)
            {
                for (int j = 0; j < mHeight; j++)
                {
                    mMap[i, j] = new Node(i, j, NodeType.Normal);
                }
            }
            HTLogger.Info("MapMgr init done.");
        }
        public void Reset()
        {
            mMap = new Node[mWidth, mHeight];
            for (int i = 0; i < mWidth; i++)
            {
                for (int j = 0; j < mHeight; j++)
                {
                    mMap[i, j] = new Node(i, j, NodeType.Normal, true);
                }
            }
            //HTLogger.Debug("Map reset done.");
        }
        #endregion


        /// <summary>
        /// 构建 地图（寻路前的 节点信息 预计算）
        /// </summary>
        public void BuildMap()
        {
            //地图 节点 相关信息 预计算
            AStarMgr.Instance.CalculateNeighborNodes();
            //HTLogger.Debug("Map build done.");
        }
        public void SetMap(Node[,] map)
        {
            mMap = map;
        }

        #region 操纵地图节点的Function
        public Node GetNode(int x, int y)
        {
            if (mMap == null)
                HTLogger.Error("MapMgr 尚未初始化，GetPointNode失败！");
            return mMap[x, y];
        }
        public MapMgr ChangeNodeTypes(List<Node> nodeList, NodeType type)
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                ChangeNodeType(nodeList[i], type);
            }
            return this;
        }
        public MapMgr ChangeNodeType(Node node, NodeType type)
        {
            (mMap[node.X, node.Y]).NodeType = type;
            return this;
        }
        public MapMgr AddObstacleList(List<Node> pointNodeList)
        {
            for (int i = 0; i < pointNodeList.Count; i++)
            {
                AddObstacle(pointNodeList[i]);
            }
            return this;
        }
        public MapMgr AddObstacle(Node pointNode)
        {
            (mMap[pointNode.X, pointNode.Y]).IsConnected = false;
            return this;
        }
        #endregion
    }
}
