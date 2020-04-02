/****************************************************
	文件：CameraController.cs
	作者：HuskyT
	邮箱：1005240602@qq.com
	日期：2020/4/2 19:57:19
	功能：摄像机控制
*****************************************************/

using UnityEngine;

namespace AStar.Example
{
    public class CameraController : MonoBehaviour
    {
        private const float MATH_CONST_1 = 1.8f;

        public void Setup(int width, int height)
        {
            transform.position = new Vector3(width / 2.0f, height * MATH_CONST_1 / 2.0f, height / 2.0f);
        }
    }
}
