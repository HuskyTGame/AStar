/****************************************************
	文件：Point.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/2 21:50:16
	功能：
*****************************************************/

namespace SimpleAStar
{
    public class Point
    {
        /// <summary>
        /// 父Point
        /// </summary>
        public Point Parent { get; set; }
        public float F { get; set; }
        public float G { get; set; }
        public float H { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        /// <summary>
        /// 是否为障碍物
        /// </summary>
        public bool IsObstacle { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
            Parent = null;
            IsObstacle = false;
        }

        public void CalculateF()
        {
            F = G + H;
        }
    }
}
