using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CyTimer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //Test1();
            Test2();

            
        }

        static void Test1()
        {
            CyTimer timer = new CyTimer(info =>
            {
                Console.WriteLine("Consolelog:" + info);
            });

            timer.AddTimeTask(((tid) =>
            {
                Console.WriteLine("线程ID：{0} , 任务ID：{tid}", Thread.CurrentThread.ManagedThreadId);
            }), 1000, CyTimerUnit.Millisecond, 0);

            while (true)
            {
                timer.Tick();
            }
        }

        static void Test2()
        {
            int id = 0;
            Queue<TaskParma> tasks = new Queue<TaskParma>();
            // CyTimer timer = new CyTimer(null, 50 ,((action, i) =>
            // {
            //     tasks.Enqueue(new TaskParma() {taskID = i, callBack = action});
            // } ));
            CyTimer timer = new CyTimer(null, 50);
            id =timer.AddTimeTask(((tid) =>
            {
                Console.WriteLine("线程ID：{0} , 任务ID：{1}", Thread.CurrentThread.ManagedThreadId, tid);
            }), 1000, CyTimerUnit.Millisecond, 0);
            
            string command = Console.ReadLine();
            if (command == "t")
            {
                timer.DeleteTimeTask(id);
            }
                
            while (true)
            {
                if (tasks.Count > 0)
                {
                    TaskParma tp = tasks.Dequeue();
                    tp.callBack?.Invoke(tp.taskID);
                }
            }
        }
    }

    public class TaskParma
    {
        public int taskID;
        public Action<int> callBack;
    }
}