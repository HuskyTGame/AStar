/****************************************************
	文件：Util.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/1 13:58:44
	功能：工具类
*****************************************************/

namespace AStar.Tools
{
    public static class Util
    {
        /// <summary>
        /// 字符串数字 转为 int数字
        /// </summary>
        public static int IsNumeric(string strNum)
        {
            int i;
            if (strNum != null && System.Text.RegularExpressions.Regex.IsMatch(strNum, @"^-?\d+$"))
                i = int.Parse(strNum);
            else
                i = -1;
            return i;
        }
    }
}