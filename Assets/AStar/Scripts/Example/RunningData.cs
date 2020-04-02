/****************************************************
	文件：RunningData.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/2 0:21:34
	功能：测试数据（运行数据）
*****************************************************/

using System;

namespace AStar.Example
{
    [Serializable]
    public class RunningData
    {
        public int Id;
        public string Info;
        public int MapWidth;
        public int MapHeight;
        public int MapNodeCount;
        public int RandomSeed;
        public int LoopCount;
        public float RunningTime;
        public int PathCost;
    }
}
