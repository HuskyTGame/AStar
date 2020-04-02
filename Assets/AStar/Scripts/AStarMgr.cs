/****************************************************
	文件：AStarMgr.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/1 23:57:45
	功能：AStar 管理器
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using HTUtility;

namespace AStar
{
    public class AStarMgr : HTSingleton<AStarMgr>
    {
        private MapMgr mMapMgr;
        private List<Node> mOpenList;
        /// <summary>
        /// 关闭列表（HashSet 的 Add 操作效率高）
        /// </summary>
        private HashSet<Node> mCloseSet;
        private List<Node> mRes;
        private Node up, down, left, right, leftUp, leftDown, rightUp, rightDown;

        public void Init()
        {
            mMapMgr = MapMgr.Instance;
            mOpenList = new List<Node>();
            mCloseSet = new HashSet<Node>();
            mRes = new List<Node>();
            HTLogger.Info("AStarMgr init done.");
        }


        #region Public Function
        /// <summary>
        /// AStar寻路
        /// </summary>
        /// <param 起点="startPos"></param>
        /// <param 终点="endPos"></param>
        /// <returns></returns>
        public List<Node> FindPath(Vector2 startPos, Vector2 endPos)
        {
            //1.传入点的合法性检验：
            //边界检验
            if (startPos.x < 0 || startPos.x >= mMapMgr.Width
                || startPos.y < 0 || startPos.y >= mMapMgr.Height
                || endPos.x < 0 || endPos.x >= mMapMgr.Width
                || endPos.y < 0 || endPos.y >= mMapMgr.Height) return null;
            //从 Map 中获取 PointNode
            Node start = mMapMgr.Map[(int)startPos.x, (int)startPos.y];
            Node end = mMapMgr.Map[(int)endPos.x, (int)endPos.y];
            //不可为障碍物
            if (start.IsConnected == false || end.IsConnected == false) return null;
            //2.初始化寻路
            mOpenList.Clear();
            mCloseSet.Clear();
            mRes.Clear();
            //3.将开始点放入关闭列表
            start.Parent = null;
            start.G = 0;
            start.H = 0;
            start.F = start.G + start.H;
            start.InCloseList = true;
            mCloseSet.Add(start);
            while (true)
            {
                //4.寻找周围的点  并放入开启列表中
                FindNearlyNodeToOpenList(start, end);
                //周围无通路
                if (mOpenList.Count == 0)
                {
                    HTLogger.Warning("AStar寻路失败，周围无通路！");
                    //重置 标志位
                    for (int i = 0; i < mMapMgr.Width; i++)
                    {
                        for (int j = 0; j < mMapMgr.Height; j++)
                        {
                            mMapMgr.Map[i, j].InOpenList = false;
                            mMapMgr.Map[i, j].InCloseList = false;
                        }
                    }
                    return null;
                }
                //5.从开启列表中找到 F 最小的（小顶堆的根节点）
                //6.放入关闭列表
                mCloseSet.Add(mOpenList[0]);
                mOpenList[0].InCloseList = true;
                //7.更新StartPoint
                start = mOpenList[0];
                //8.移出开启列表
                RemoveFirst(mOpenList);
                //9.退出循环条件：找到终点
                if (start == end) break;
            }
            //重置 标志位
            for (int i = 0; i < mMapMgr.Width; i++)
            {
                for (int j = 0; j < mMapMgr.Height; j++)
                {
                    mMapMgr.Map[i, j].InOpenList = false;
                    mMapMgr.Map[i, j].InCloseList = false;
                }
            }
            return GetPathRes(end);
        }
        /// <summary>
        /// 预计算 地图中 所有节点的 相邻节点信息（地图数据发生改变的时候才需要 预计算）
        /// </summary>
        public void CalculateNeighborNodes()
        {
            for (int i = 0; i < mMapMgr.Width; i++)
            {
                for (int j = 0; j < mMapMgr.Height; j++)
                {
                    CalculateNeighborNode(mMapMgr.Map[i, j]);
                }
            }
        }
        #endregion


        #region Private Function
        /// <summary>
        /// 寻找当前节点周围的合法节点，并将其加入到 OpenList 中
        /// </summary>
        /// <param 当前节点="parent"></param>
        /// <param 目的地="end"></param>
        private void FindNearlyNodeToOpenList(Node node, Node end)
        {
            //遍历周围的点
            Node neighbor;
            int newG;
            for (int i = 0; i < node.NeighborList.Count; i++)
            {
                neighbor = node.NeighborList[i];
                if (neighbor.InCloseList) continue;
                //计算 新G
                newG = node.G + node.NeighborCostG[i];
                if (neighbor.InOpenList)//开启列表中已经包含此 point
                {
                    if (newG < neighbor.G)//新G 值更小：新路线消耗更小
                    {
                        neighbor.Parent = node;//更新 parent
                        neighbor.G = newG;
                        neighbor.F = neighbor.G + neighbor.H;
                    }
                }
                else
                {
                    //添加到开启列表
                    neighbor.Parent = node;
                    neighbor.G = newG;
                    neighbor.H = Math.Abs(end.X - neighbor.X) + Math.Abs(end.Y - neighbor.Y);
                    neighbor.F = neighbor.G + neighbor.H;
                    Add(mOpenList, neighbor);
                }
            }
        }
        /// <summary>
        /// 计算指定节点的邻居节点
        /// 如果移动规则变化，需要改动此函数！！！
        /// 注意：最后的寻路路径结果与此函数中添加周围点的顺序有关
        /// </summary>
        private void CalculateNeighborNode(Node node)
        {
            node.NeighborCostG.Clear();
            node.NeighborList.Clear();//重新预计算的时候需要清空邻居列表（以防重复添加邻居）
            up = null; down = null; left = null; right = null;
            leftUp = null; leftDown = null; rightUp = null; rightDown = null;
            //合法性检测
            if (node.Y < mMapMgr.Height - 1)
            {
                up = mMapMgr.Map[node.X, node.Y + 1];
            }
            if (node.Y > 0)
            {
                down = mMapMgr.Map[node.X, node.Y - 1];
            }
            if (node.X > 0)
            {
                left = mMapMgr.Map[node.X - 1, node.Y];
            }
            if (node.X < mMapMgr.Width - 1)
            {
                right = mMapMgr.Map[node.X + 1, node.Y];
            }
            if (left != null && up != null)//左上不空
            {
                leftUp = mMapMgr.Map[node.X - 1, node.Y + 1];
            }
            if (left != null && down != null)//左下不空
            {
                leftDown = mMapMgr.Map[node.X - 1, node.Y - 1];
            }
            if (right != null && up != null)//右上不空
            {
                rightUp = mMapMgr.Map[node.X + 1, node.Y + 1];
            }
            if (right != null && down != null)//右下不空
            {
                rightDown = mMapMgr.Map[node.X + 1, node.Y - 1];
            }

            if (up != null && up.IsConnected == true)//上
            {
                node.NeighborList.Add(up);
                node.NeighborCostG.Add(10);
            }
            if (down != null && down.IsConnected == true)//下
            {
                node.NeighborList.Add(down);
                node.NeighborCostG.Add(10);
            }
            if (left != null && left.IsConnected == true)//左
            {
                node.NeighborList.Add(left);
                node.NeighborCostG.Add(10);
            }
            if (right != null && right.IsConnected == true)//右
            {
                node.NeighborList.Add(right);
                node.NeighborCostG.Add(10);
            }
            if (leftUp != null && leftUp.IsConnected == true
                && left.IsConnected == true && up.IsConnected == true)//左上
            {
                node.NeighborList.Add(leftUp);
                node.NeighborCostG.Add(14);
            }
            if (leftDown != null && leftDown.IsConnected == true
                && left.IsConnected == true && down.IsConnected == true)//左下
            {
                node.NeighborList.Add(leftDown);
                node.NeighborCostG.Add(14);
            }
            if (rightUp != null && rightUp.IsConnected == true
                && right.IsConnected == true && up.IsConnected == true)//右上
            {
                node.NeighborList.Add(rightUp);
                node.NeighborCostG.Add(14);
            }
            if (rightDown != null && rightDown.IsConnected == true
                && right.IsConnected == true && down.IsConnected == true)//右下
            {
                node.NeighborList.Add(rightDown);
                node.NeighborCostG.Add(14);
            }
        }
        /// <summary>
        /// 获取路径结果
        /// </summary>
        private List<Node> GetPathRes(Node node)
        {
            while (node != null)
            {
                mRes.Add(node);
                node = node.Parent;
            }
            mRes.Reverse();
            return mRes;
        }
        #endregion


        #region Heap Function
        /// <summary>
        /// 向小顶堆中添加元素
        /// </summary>
        private void Add(List<Node> list, Node node)
        {
            list.Add(node);
            node.InOpenList = true;
            int index = list.Count - 1;
            while (index > 0 && list[index].F < list[(index - 1) / 2].F)//子节点 < 父节点
            {
                Swap(list, (index - 1) / 2, index);//交换父子节点
                index = (index - 1) / 2;//更新 index
            }
        }
        /// <summary>
        /// 删除小顶堆中堆顶元素，然后调整小顶堆
        /// </summary>
        private void RemoveFirst(List<Node> list)
        {
            if (list.Count == 1)
            {
                list.Clear();
                return;
            }
            list[0].InOpenList = false;
            Swap(list, 0, list.Count - 1);//交换首尾元素
            list.RemoveAt(list.Count - 1);//移除尾
            UpdateHeap(list, 0, list.Count);//调整堆
        }
        /// <summary>
        /// 调整（小顶）堆，F 值最小的 Node 在堆顶
        /// </summary>
        /// <param 数据列表="list"></param>
        /// <param 待调整的数据的索引="i"></param>
        /// <param 数据列表长度="len"></param>
        private void UpdateHeap(List<Node> list, int i, int len)
        {
            Node temp = list[i];//记录待 调整 的值
            for (int k = 2 * i + 1; k < len; k = 2 * k + 1)
            {
                if (k + 1 < len)//存在右节点
                {
                    if (list[k + 1].F < list[k].F)//右节点 < 左节点
                    {
                        k += 1;//选择较小的右节点
                    }
                }
                //（左/右）子节点 < 父节点
                if (list[k].F < temp.F)
                {
                    Swap(list, k, i);//交换（要保证父节点一定小于子节点——小顶堆）
                    i = k;//更新 i ，继续下一次循环
                }
                //子节点>父节点
                else
                {
                    break;//跳出循环，此处不需要往深层次遍历的原因在于：在堆排序中是从最后一个非叶子节点开始倒着向前 UpdateHeap
                }
            }
        }
        private void Swap(List<Node> list, int a, int b)
        {
            Node temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }
        #endregion
    }
}
