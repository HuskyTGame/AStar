/****************************************************
	文件：Recorder.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/2 0:21:5
	功能：测试数据记录器
*****************************************************/

using System.Collections.Generic;
using HTUtility;

namespace AStar.Example
{
    public class Recorder : HTSingleton<Recorder>
    {
        private SaveMgr mSaveMgr;
        private Dictionary<int, RunningData> mDataDict;

        public void Init()
        {
            mSaveMgr = SaveMgr.Instance;
            mDataDict = new Dictionary<int, RunningData>();
            HTLogger.Info("Recorder init done.");
        }

        public RunningData GetRunningData(int key)
        {
            RunningData data;
            if (mDataDict.TryGetValue(key, out data) && data != null)
                return data;
            HTLogger.Error("Get RunningData fail.");
            return null;
        }
        public void SetRunningData(RunningData data, int key)
        {
            if (mDataDict.ContainsKey(key) == false)
                mDataDict.Add(key, data);
            else
                mDataDict[key] = data;
        }
        public void SaveRunningData(int key)
        {
            RunningData data;
            if (mDataDict.TryGetValue(key, out data) && data != null)
            {
                mSaveMgr.Save(data, mSaveMgr.PackageSavePath(key + ".json"));
                HTLogger.Debug("Save RunningData done.");
            }
            else
            {
                HTLogger.Debug("Save RunningData fail.");
            }
        }
        public void LoadRunningData(int key)
        {
            RunningData data = mSaveMgr.Load<RunningData>(mSaveMgr.PackageSavePath(key + ".json"));
            if (mDataDict.ContainsKey(key))
                mDataDict[key] = data;
            else
                mDataDict.Add(key, data);
            HTLogger.Debug("Load RunningData done.");
        }
    }
}
