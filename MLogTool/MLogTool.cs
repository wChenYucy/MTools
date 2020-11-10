// ****************************************************
// 	文件：MLogTool.cs
// 	作者：积极向上小木木
// 	邮箱：PositiveMumu@126.com
// 	日期：2020/11/10 20:37:39
// 	功能：日志输出工具类
// *****************************************************

using System;

namespace MTools.MLogTool
{
    public class MLogTool
    {
        //是否显示日志信息
        private static bool _showLog = true;

        //日志回调事件
        private static Action<string, MLogLevels> _callback;

        //是否显示日志信息（只读）
        public static bool ShowLog { get => _showLog; }

        /// <summary>
        /// 设置是否显示日志
        /// </summary>
        /// <param name="state">日志显示状态</param>
        public static void SetLogState(bool state = true)
        {
            _showLog = state;
        }
        /// <summary>
        /// 注册日志回调事件
        /// </summary>
        /// <param name="method">日志回调事件，包含string和MLogLevels类的方法</param>
        public static void RegistPrintLogMethod(Action<string, MLogLevels> method)
        {
            _callback += method;
        }
        /// <summary>
        /// 注销日志回调事件
        /// </summary>
        /// <param name="method">日志回调事件，包含string和MLogLevels类的方法</param>
        public static void LogOffPrintLogMethod(Action<string, MLogLevels> method)
        {
            _callback -= method;
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="log">日志信息</param>
        /// <param name="logLevel">日志等级</param>
        public static void PrintLog(string log, MLogLevels logLevel = MLogLevels.Log)
        {
            if (!_showLog)
                return;
            log = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + " >> " + logLevel.ToString() + " : " + log;
            if (_callback != null)
            {
                _callback(log, logLevel);
            }
            else
            {
                if (logLevel == MLogLevels.Log)
                {
                    Console.WriteLine(log);
                }
                else if (logLevel == MLogLevels.Information)
                {
                    Console.WriteLine("//--------------------Info--------------------//");
                }
                else if (logLevel == MLogLevels.Warning)
                {
                    Console.WriteLine("//--------------------Warn--------------------//");
                }
                else if (logLevel == MLogLevels.Error)
                {
                    Console.WriteLine("//--------------------Error--------------------//");
                }
                else
                {
                    Console.WriteLine("//--------------------Error--------------------//");
                    Console.WriteLine(log + " >> Unknow Log Type\n");
                }
                Console.WriteLine(log);
            }
        }
    }
}
