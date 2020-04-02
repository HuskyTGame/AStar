/****************************************************
	文件：SaveMgr.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/2 0:20:13
	功能：存储 管理器
*****************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HTUtility;

namespace AStar.Example
{
    public class SaveMgr : HTSingleton<SaveMgr>
    {
        private static string SAVE_PATH_PREFIX = Application.dataPath + "/AStar/Resources/MyData";

        public void Init()
        {
            HTLogger.Info("SaveMgr init done.");
        }

        public void Save<T>(T data, string path)
        {
            string json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            HTLogger.Debug("Save map done.");
        }

        public T Load<T>(string path)
        {
            string json = File.ReadAllText(path);
            T data = JsonUtility.FromJson<T>(json);
            if (data == null)
            {
                HTLogger.Debug("Load map fail.");
            }
            HTLogger.Debug("Load map done.");
            return data;
        }

        public string PackageSavePath(string postfix)
        {
            return SAVE_PATH_PREFIX + postfix;
        }
    }
}
