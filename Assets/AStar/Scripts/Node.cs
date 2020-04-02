/****************************************************
	文件：Node.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/1 23:57:57
	功能：节点
*****************************************************/

using System;
using System.Collections.Generic;

namespace AStar
{
    [Serializable]
    public enum NodeType
    {
        Default,
        /// <summary>
        /// 普通格子
        /// </summary>
        Normal,
    }

    public class Node
    {
        /// <summary>
        /// 经过该节点的路径的总消耗
        /// </summary>
        public int F;
        /// <summary>
        /// 从起点到该节点的消耗
        /// </summary>
        public int G;
        /// <summary>
        /// 从该节点到终点的消耗
        /// </summary>
        public int H;
        public int X;
        public int Y;
        /// <summary>
        /// 是否是连通的（通路，非障碍物）
        /// </summary>
        public bool IsConnected;
        public NodeType NodeType;
        public Node Parent;
        /// <summary>
        /// 邻居节点列表
        /// </summary>
        public List<Node> NeighborList;
        /// <summary>
        /// 邻居节点的G值列表
        /// </summary>
        public List<int> NeighborCostG;
        /// <summary>
        /// 是否在 OpenList 中
        /// </summary>
        public bool InOpenList;
        /// <summary>
        /// 是否在 CloseList 中
        /// </summary>
        public bool InCloseList;

        public Node(int x, int y, NodeType type = NodeType.Normal, bool connected = true)
        {
            X = x;
            Y = y;
            NodeType = type;
            IsConnected = connected;
            F = G = H = 0;
            NeighborList = new List<Node>();
            NeighborCostG = new List<int>();
            InOpenList = InCloseList = false;
        }
    }
}
