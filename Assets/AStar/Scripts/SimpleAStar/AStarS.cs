/****************************************************
	文件：SimpleAStar.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/2 21:50:2
	功能：
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using HTUtility;

namespace SimpleAStar
{
    public class AStarS
    {
        /// <summary>
        /// 地图的宽
        /// </summary>
        private int mMapWidth;
        /// <summary>
        /// 地图的高
        /// </summary>
        private int mMapHeight;
        /// <summary>
        /// AStart的地图
        /// </summary>
        private Point[,] mMap;

        public AStarS(int mapWidth, int mapHeight, List<Point> obstacleList)
        {
            mMapWidth = mapWidth;
            mMapHeight = mapHeight;
            InitMap(mMapWidth, mMapHeight, obstacleList);
        }

        /// <summary>
        /// 输出路径
        /// </summary>
        public void DebugPath(List<Point> path)
        {
            foreach (Point item in path)
            {
                Debug.Log(item.X + "--" + item.Y);
            }
        }

        /// <summary>
        /// 初始化地图
        /// </summary>
        private void InitMap(int width, int height, List<Point> obstacleList)
        {
            mMap = new Point[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    mMap[x, y] = new Point(x, y);
                }
            }
            //初始化障碍物
            if (obstacleList == null || obstacleList.Count <= 0) return;
            foreach (Point item in obstacleList)
            {
                mMap[item.X, item.Y].IsObstacle = true;
            }
        }

        /// <summary>
        /// 寻路
        /// </summary>
        public List<Point> FindPath(Point start, Point end)
        {
            List<Point> resultList = new List<Point>();//寻路结果

            List<Point> openList = new List<Point>();//开启列表
            List<Point> closeList = new List<Point>();//关闭列表
            Point startPoint = mMap[start.X, start.Y];
            Point endPoint = mMap[end.X, end.Y];
            //将开始点添加到开启列表
            CalculateF(startPoint, endPoint);
            openList.Add(startPoint);
            //遍历搜索路径
            while (openList.Count > 0)
            {
                //1.寻找开启列表中最小F值的Point
                Point minFPoint = FindMinFOfOpenList(openList);
                openList.Remove(minFPoint);
                closeList.Add(minFPoint);
                //2.获取最小F值周围的Point（非障碍物）
                List<Point> surroundPointList = GetSurroundPoints(minFPoint);
                //3.过滤掉关闭列表中已经存在的Point
                PointsFilter(surroundPointList, closeList);
                //4.遍历符合要求的周围的Point
                foreach (Point surroundPoint in surroundPointList)
                {
                    //计算新路线G值
                    float newG = CalculateG(minFPoint, surroundPoint) + minFPoint.G;
                    //a.在开启列表中已经存在此Point
                    //if (openList.Contains(surroundPoint))
                    if (openList.IndexOf(surroundPoint) > -1)
                    {
                        //若新路线的G值更小，则有意义
                        if (newG < surroundPoint.G)
                        {
                            //Debug.LogFormat("当前Point：（{0}，{1}），G值：{2}；更小G值的Point：（{3}，{4}），旧G值：{5}（{6}，{7}）——新G值：{8}", minFPoint.X, minFPoint.Y, minFPoint.G, surroundPoint.X, surroundPoint.Y, surroundPoint.G, surroundPoint.Parent.X, surroundPoint.Parent.Y, newG);
                            surroundPoint.Parent = minFPoint;//变更父Point
                            surroundPoint.G = newG;//更新G值
                            CalculateF(surroundPoint, endPoint);//更新F值
                        }
                    }
                    //b.此Point不在开启列表中
                    else
                    {
                        openList.Add(surroundPoint);//添加到开启列表中
                        surroundPoint.Parent = minFPoint;//设置父Point
                        surroundPoint.G = newG;//G值
                        CalculateF(surroundPoint, endPoint);//F值
                    }
                }
                //5.如果终点已经在开启列表中，则结束循环
                //if (openList.Contains(endPoint))
                if (openList.IndexOf(endPoint) > -1)
                {
                    resultList = GetParentList(endPoint);
                    break;
                }
            }
            return resultList;
        }

        /// <summary>
        /// 获取指定Point的所有父Point
        /// </summary>
        /// <returns></returns>
        private List<Point> GetParentList(Point point)
        {
            List<Point> res = new List<Point>();
            while (point != null)
            {
                res.Add(point);
                point = point.Parent;
            }
            res.Reverse();
            return res;
        }

        /// <summary>
        /// PointList过滤器
        /// 过滤掉关闭列表中已经存在的Point
        /// </summary>
        private void PointsFilter(List<Point> srcList, List<Point> closeList)
        {
            //移除关闭列表中已经存在的Point
            foreach (Point item in closeList)
            {
                //if (srcList.Contains(item))
                if (srcList.IndexOf(item) > -1)
                //if(srcList.Exists(p=>p==item))//这个效率低
                {
                    srcList.Remove(item);
                }
            }
        }

        /// <summary>
        /// 获取Point周围的Point
        /// 如果移动规则变化，需要改动此函数！！！
        /// 注意：最后的寻路路径结果与此函数中添加周围点的顺序有关
        /// 先添加的会先深度搜索
        /// </summary>
        private List<Point> GetSurroundPoints(Point point)
        {
            List<Point> surroundPointList = new List<Point>();
            Point up = null, down = null, left = null, right = null,
                leftUp = null, leftDown = null, rightUp = null, rightDown = null;
            if (point.Y < mMapHeight - 1)
            {
                up = mMap[point.X, point.Y + 1];
            }
            if (point.Y > 0)
            {
                down = mMap[point.X, point.Y - 1];
            }
            if (point.X > 0)
            {
                left = mMap[point.X - 1, point.Y];
            }
            if (point.X < mMapWidth - 1)
            {
                right = mMap[point.X + 1, point.Y];
            }
            if (left != null && up != null)
            {
                leftUp = mMap[point.X - 1, point.Y + 1];
            }
            if (left != null && down != null)
            {
                leftDown = mMap[point.X - 1, point.Y - 1];
            }
            if (right != null && up != null)
            {
                rightUp = mMap[point.X + 1, point.Y + 1];
            }
            if (right != null && down != null)
            {
                rightDown = mMap[point.X + 1, point.Y - 1];
            }

            if (up != null && up.IsObstacle == false)
            {
                surroundPointList.Add(up);
            }
            if (down != null && down.IsObstacle == false)
            {
                surroundPointList.Add(down);
            }
            if (left != null && left.IsObstacle == false)
            {
                surroundPointList.Add(left);
            }
            if (right != null && right.IsObstacle == false)
            {
                surroundPointList.Add(right);
            }
            if (leftUp != null && leftUp.IsObstacle == false && left.IsObstacle == false && up.IsObstacle == false)
            {
                surroundPointList.Add(leftUp);
            }
            if (leftDown != null && leftDown.IsObstacle == false && left.IsObstacle == false && down.IsObstacle == false)
            {
                surroundPointList.Add(leftDown);
            }
            if (rightUp != null && rightUp.IsObstacle == false && right.IsObstacle == false && up.IsObstacle == false)
            {
                surroundPointList.Add(rightUp);
            }
            if (rightDown != null && rightDown.IsObstacle == false && right.IsObstacle == false && down.IsObstacle == false)
            {
                surroundPointList.Add(rightDown);
            }
            return surroundPointList;
        }

        /// <summary>
        /// 寻找OpenList中最小F值的Point
        /// 为null则不存在最小F值Point
        /// </summary>
        private Point FindMinFOfOpenList(List<Point> openList)
        {
            Point minFPoint = null;
            float minF = float.MaxValue;
            foreach (Point point in openList)
            {
                if (point.F < minF)
                {
                    minF = point.F;
                    minFPoint = point;
                }
            }
            return minFPoint;
        }

        /// <summary>
        /// 计算从当前Point到目标Point的G值
        /// </summary>
        private float CalculateG(Point now, Point target)
        {
            if ((now.X - target.X) != 0 && (now.Y - target.Y) != 0)
            {
                return 1.4f;
            }
            return 1.0f;
            //return (float)Math.Floor(10 * Vector2.Distance(new Vector2(now.X, now.Y), new Vector2(target.X, target.Y))) / 10.0f;
        }

        /// <summary>
        /// 计算当前Point的F值
        /// </summary>
        private void CalculateF(Point now, Point end)
        {
            //1.H值计算：
            float h = Math.Abs(end.X - now.X) + Math.Abs(end.Y - now.Y);
            //2.G值计算：
            float g = 0;
            //开始点
            if (now.Parent == null)
            {
                g = 0;
            }
            //非开始点
            else
            {
                //G = 父Point的G值 + 父Point到当前Point的G值
                g = now.Parent.G + CalculateG(now.Parent, now);
            }
            //3.F值计算F = G + H
            now.G = g;
            now.H = h;
            now.CalculateF();
        }
    }
}
