using System;
using System.Collections.Generic;
using System.Timers;

namespace CyTimer
{
    public class CyTimer
    {
        private Action<string> logCallBack;
        private static readonly string taskIDLock = "tidlock";
        private static readonly string timeTaskLock = "timelock";
        private static readonly string frameTaskLock = "framelock";
        private Timer serverTimer;
        private Action<Action<int>, int> taskHandle;

        private int taskID;
        private HashSet<int> taskIDSet;

        private DateTime startUTCTime = new DateTime(1970, 1, 1, 0, 0, 0);
        private double nowTime;
        private List<CyTimeTask> timeTaskList;
        private List<CyTimeTask> tempTimeTaskList;
        private List<int> deleteTimeTaskList;

        private int currentFrame;
        private List<CyFrameTask> frameTaskList;
        private List<CyFrameTask> tempFrameTaskList;
        private List<int> deleteFrameTaskList;

        public CyTimer(Action<string> logAction, int interval = 0, Action<Action<int>, int> handle = null)
        {
            logCallBack = logAction;
            taskID = 0;
            taskIDSet = new HashSet<int>();

            nowTime = GetNowMilliseconds();
            timeTaskList = new List<CyTimeTask>();
            tempTimeTaskList = new List<CyTimeTask>();
            deleteTimeTaskList = new List<int>();

            currentFrame = 0;
            frameTaskList = new List<CyFrameTask>();
            tempFrameTaskList = new List<CyFrameTask>();
            deleteFrameTaskList = new List<int>();

            taskHandle = handle;

            if (interval != 0)
            {
                serverTimer = new Timer(interval) {AutoReset = true};
                serverTimer.Elapsed += (sender, args) => { Tick(); };
            }

            serverTimer?.Start();
            logCallBack?.Invoke("Cytimer Init Done.");
        }

        public void Tick()
        {
            CheckTimeTack();
            RemoveTimeTack();
            CheckFrameTack();
            RemoveFrameTack();
        }

        #region TimeTask

        private void CheckTimeTack()
        {
            if (tempTimeTaskList.Count > 0)
            {
                lock (timeTaskLock)
                {
                    for(int i = 0; i < tempTimeTaskList.Count; i++)
                    {
                        timeTaskList.Add(tempTimeTaskList[i]);
                    }
                }
            }
            tempTimeTaskList.Clear();
            nowTime = GetNowMilliseconds();
            for (int i = 0; i < timeTaskList.Count; i++)
            {
                CyTimeTask timeTask = timeTaskList[i];
                if (nowTime.CompareTo(timeTask.FinishedTime) < 0)
                    continue;
                if (taskHandle != null)
                {
                    taskHandle.Invoke(timeTask.CallBack, timeTask.TaskID);
                }
                else
                {
                    timeTask.CallBack?.Invoke(timeTask.TaskID);
                }
                
                if (timeTask.Count == 1)
                {
                    timeTaskList.RemoveAt(i);
                    taskIDSet.Remove(timeTask.TaskID);
                    i--;
                }
                else
                {
                    timeTask.FinishedTime += timeTask.Delay;
                    if (timeTask.Count != 0)
                        timeTask.Count--;
                }
            }
        }

        private void RemoveTimeTack()
        {
            lock (timeTaskLock)
            {
                bool exist = false;
                lock (taskIDLock)
                {
                    if (taskIDSet.Contains(taskID))
                    {
                        taskIDSet.Remove(taskID);
                    }
                }
                for (int i = 0; i < deleteTimeTaskList.Count; i++)
                {
                    int taskID = deleteTimeTaskList[i];
                    for (int j = 0; j < timeTaskList.Count; j++)
                    {
                        if (taskID == timeTaskList[j].TaskID)
                        {
                            timeTaskList.Remove(timeTaskList[j]);
                            exist = true;
                            break;
                        }
                    }
                    if (!exist)
                    {
                        for (int j = 0; j < tempTimeTaskList.Count; j++)
                        {
                            if (taskID == tempTimeTaskList[j].TaskID)
                            {
                                tempTimeTaskList.Remove(tempTimeTaskList[j]);
                                exist = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public int AddTimeTask(Action<int> callBack, float delay, CyTimerUnit timerUnit = CyTimerUnit.Millisecond,
            int count = 1)
        {
            if (timerUnit != CyTimerUnit.Millisecond)
            {
                switch (timerUnit)
                {
                    case CyTimerUnit.Second:
                        delay = delay * 1000;
                        break;
                    case CyTimerUnit.Minute:
                        delay = delay * 1000 * 60;
                        break;
                    case CyTimerUnit.Hour:
                        delay = delay * 1000 * 60 * 60;
                        break;
                    case CyTimerUnit.Day:
                        delay = delay * 1000 * 60 * 60 * 24;
                        break;
                    default:
                        logCallBack?.Invoke("CyTimerUnit类型错误，进制转换失败！");
                        break;
                }
            }

            int taskID = GetTaskID();
            nowTime = GetNowMilliseconds();
            lock (timeTaskLock)
            {
                tempTimeTaskList.Add(new CyTimeTask(taskID, callBack, nowTime + delay, delay, count >= 0 ? count : 1));
            }
            
            return taskID;
        }

        public void DeleteTimeTask(int taskID)
        {
            lock (timeTaskLock)
            {
                deleteTimeTaskList.Add(taskID);
            }
            
        }

        #endregion

        #region FrameTask

        private void CheckFrameTack()
        {
            currentFrame++;
            if (tempFrameTaskList.Count > 0)
            {
                lock (frameTaskLock)
                {
                    for (int i = 0; i < tempFrameTaskList.Count; i++)
                    {
                        frameTaskList.Add(tempFrameTaskList[i]);
                    }

                    tempFrameTaskList.Clear();
                }
            }
            
            for (int i = 0; i < frameTaskList.Count; i++)
            {
                CyFrameTask frameTask = frameTaskList[i];
                if (currentFrame < frameTask.FinishedFrame)
                    continue;
                if (taskHandle != null)
                {
                    taskHandle.Invoke(frameTask.CallBack, frameTask.TaskID);
                }
                else
                {
                    frameTask.CallBack?.Invoke(frameTask.TaskID);
                }
                if (frameTask.Count == 1)
                {
                    frameTaskList.RemoveAt(i);
                    taskIDSet.Remove(frameTask.TaskID);
                    i--;
                }
                else
                {
                    frameTask.FinishedFrame += frameTask.Delay;
                    if (frameTask.Count != 0)
                        frameTask.Count--;
                }
            }
        }
        
        private void RemoveFrameTack()
        {
            for (int i = 0; i < deleteFrameTaskList.Count; i++)
            {
                int taskID = deleteFrameTaskList[i];
                bool exist = false;
            
                lock (taskIDLock)
                {
                    if (taskIDSet.Contains(taskID))
                    {
                        taskIDSet.Remove(taskID);
                    }
                }
            
                lock (frameTaskLock)
                {
                    for (int j = 0; j < frameTaskList.Count; j++)
                    {
                        if (taskID == frameTaskList[j].TaskID)
                        {
                            frameTaskList.Remove(frameTaskList[j]);
                            exist = true;
                            break;
                        }
                    }

                    if (!exist)
                    {
                        for (int j = 0; j < tempFrameTaskList.Count; j++)
                        {
                            if (taskID == tempFrameTaskList[j].TaskID)
                            {
                                tempFrameTaskList.Remove(tempFrameTaskList[j]);
                                exist = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public int AddFrameTask(Action<int> callBack, int delay, int count = 1)
        {
            int taskID = GetTaskID();
            lock (frameTaskLock)
            {
                tempFrameTaskList.Add(new CyFrameTask(taskID, callBack, currentFrame + delay, delay, count >= 0 ? count : 1));
            }

            return taskID;
        }

        public void DeleteFrameTask(int taskID)
        {
            lock (timeTaskLock)
            {
                deleteFrameTaskList.Add(taskID);
            }
        }

        #endregion
        
        private int GetTaskID()
        {
            lock (taskIDLock)
            {
                taskID++;
                if (taskID == int.MaxValue)
                    taskID = 0;
                while (taskIDSet.Contains(taskID))
                {
                    taskID++;
                }
                
                taskIDSet.Add(taskID);
            }
            return taskID;
        }

        private double GetNowMilliseconds()
        {
            return (DateTime.Now - startUTCTime).TotalMilliseconds;
        }

        public void Reset()
        {
            taskID = 0;
            taskIDSet.Clear();
            timeTaskList.Clear();
            tempTimeTaskList.Clear();
            currentFrame = 0;
            frameTaskList.Clear();
            tempFrameTaskList.Clear();
            serverTimer.Stop();
            serverTimer.Dispose();
            serverTimer = null;
            logCallBack = null;
        }
        
        public int GetYear() {
            return GetLocalDateTime().Year;
        }
        
        public int GetMonth() {
            return GetLocalDateTime().Month;
        }
        
        public int GetDay() {
            return GetLocalDateTime().Day;
        }
        
        public int GetWeek() {
            return (int)GetLocalDateTime().DayOfWeek;
        }
        
        public DateTime GetLocalDateTime() {
            DateTime dt = TimeZone.CurrentTimeZone.ToLocalTime(startUTCTime.AddMilliseconds(nowTime));
            return dt;
        }
        
        public double GetMillisecondsTime() {
            return nowTime;
        }
        
        public string GetLocalTimeStr() {
            DateTime dt = GetLocalDateTime();
            string str = GetTimeStr(dt.Hour) + ":" + GetTimeStr(dt.Minute) + ":" + GetTimeStr(dt.Second);
            return str;
        }
        private string GetTimeStr(int time) {
            if (time < 10) {
                return "0" + time;
            }
            else {
                return time.ToString();
            }
        }
        
        /// <summary>
        /// 根据时间执行的定时数据类
        /// </summary>
        private class CyTimeTask
        {
            public int TaskID { get; set; }
            public double FinishedTime { get; set; }
            public Action<int> CallBack;
            public double Delay { get; set; }
            public int Count { get; set; }

            public CyTimeTask(int taskID,Action<int> callBack, double finishedTime, double delay, int count)
            {
                TaskID = taskID;
                CallBack = callBack;
                FinishedTime = finishedTime;
                Delay = delay;
                Count = count;
            }
        }

    
    
        /// <summary>
        /// 根据帧执行的定时数据类
        /// </summary>
        private class CyFrameTask
        {
            public int TaskID { get; set; }
            public int FinishedFrame { get; set; }
            public Action<int> CallBack;
            public int Delay { get; set; }
            public int Count { get; set; }

            public CyFrameTask(int taskID, Action<int> callBack, int finishedFrame, int delay, int count)
            {
                TaskID = taskID;
                CallBack = callBack;
                FinishedFrame = finishedFrame;
                Delay = delay;
                Count = count;
            }
        }
    }

    /// <summary>
    /// 时间枚举
    /// </summary>
    public enum CyTimerUnit
    {
        Millisecond,
        Second,
        Minute,
        Hour,
        Day
    }
}