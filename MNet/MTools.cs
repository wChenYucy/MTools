// ****************************************************
// 	文件：MTools.cs
// 	作者：积极向上小木木
// 	邮箱：positivemumu@126.com
// 	日期：2020/12/26 14:17:37
// 	功能：工具类
// *****************************************************
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MNet
{
    public class MTools
    {
        public static byte[] PackNetData<T>(T data) where T : MData
        {
            return AddHeadInfo(Serialize(data));
        }

        public static byte[] AddHeadInfo(byte[] data)
        {
            int len = data.Length;
            byte[] result = new byte[len + 4];
            byte[] head = BitConverter.GetBytes(len);
            head.CopyTo(result, 0);
            data.CopyTo(result, 4);
            return result;
        }

        public static byte[] Serialize<T>(T data) where T : MData
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, data);
                    ms.Seek(0, SeekOrigin.Begin);
                    //return ms.ToArray();
                    return Compess(ms.ToArray());
                }
                catch (SerializationException e)
                {
                    PrintLog("Failed to serialize. Log: " + e.Message, MLogLevels.Error);
                    return null;
                }
            }
        }

        public static T Deserialize<T>(byte[] data) where T : MData
        {
            using (MemoryStream ms = new MemoryStream(Decompess(data)))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    T result = bf.Deserialize(ms) as T;
                    return result;
                }
                catch (SerializationException e)
                {
                    PrintLog("Failed to deserialize. Log: " + e.Message, MLogLevels.Error);
                    return null;
                }
            }
        }

        public static byte[] Compess(byte[] data)
        {
            using (MemoryStream input = new MemoryStream())
            {
                using (GZipStream gzs = new GZipStream(input, CompressionMode.Compress, true))
                {
                    gzs.Write(data, 0, data.Length);

                    gzs.Close();
                    return input.ToArray();
                }
            }
        }

        public static byte[] Decompess(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(input, CompressionMode.Decompress))
                    {
                        byte[] bytes = new byte[1024];
                        int len = 0;
                        while ((len = gzs.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            output.Write(bytes, 0, len);
                        }
                        //gzs.CopyTo(output);
                        gzs.Close();
                        return output.ToArray();

                    }
                }
            }
        }

        #region Log
        public static bool showLog = true;
        public static Action<string, int> logCallBack = null;
        public static void PrintLog(string log, MLogLevels mLogLevel = MLogLevels.Log)
        {
            if (!showLog)
            {
                return;
            }
            else
            {
                log = DateTime.Now.ToLongTimeString() + " >> " + log;
                if (logCallBack != null)
                {
                    logCallBack(log, (int)mLogLevel);
                }
                else
                {
                    if (mLogLevel == MLogLevels.Log)
                    {
                        Console.WriteLine(log);
                    }
                    else if (mLogLevel == MLogLevels.Warning)
                    {
                        //Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("//--------------------Warn--------------------//");
                        Console.WriteLine(log);
                        //Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else if (mLogLevel == MLogLevels.Error)
                    {
                        //Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("//--------------------Error--------------------//");
                        Console.WriteLine(log);
                        //Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else if (mLogLevel == MLogLevels.Information)
                    {
                        //Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("//--------------------Info--------------------//");
                        Console.WriteLine(log);
                        //Console.ForegroundColor = ConsoleColor.Gray;
                    }
                    else
                    {
                        //Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("//--------------------Error--------------------//");
                        Console.WriteLine(log + " >> Unknow Log Type\n");
                        //Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
            }
        }
        #endregion
    }
}
