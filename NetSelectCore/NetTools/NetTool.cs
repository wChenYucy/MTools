// ****************************************************
//     文件：NetTool.cs
//     作者：积极向上小木木
//     邮箱: positivemumu@126.com
//     日期：2021/07/24 14:35
//     功能：工具类，可以为字节数组添加长度头信息、
//          序列化、反序列化、压缩、解压传输信息等。
// *****************************************************

using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using DetailLogTool;
using ProtoBuf;

namespace NetSelectCore
{
    public class NetTool
    {
        internal static byte[] PackNetData<T>(T data) where T : NetData
        {
            return AddHeadInfo(Serialize(data));
        }

        internal static byte[] AddHeadInfo(byte[] data)
        {
            int len = data.Length;
            byte[] result = new byte[len + 4];
            byte[] head = BitConverter.GetBytes(len);
            head.CopyTo(result, 0);
            data.CopyTo(result, 4);
            return result;
        }

        internal static byte[] Serialize<T>(T data) where T : NetData
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    Serializer.Serialize(ms,data);
                    ms.Seek(0, SeekOrigin.Begin);
                    return Compession(ms.ToArray());
                }
                catch (SerializationException e)
                {
                    DetailLog.Error("Failed to serialize. Log: " + e.Message);
                    return null;
                }
            }
        }

        internal static T Deserialize<T>(byte[] data,int offset,int count) where T : NetData
        {
            byte[] realData = new byte[count];
            Array.Copy(data,offset,realData,0,count);
            using (MemoryStream ms = new MemoryStream(Decompession(realData)))
            {
                try
                {
                    T result = Serializer.NonGeneric.Deserialize(typeof(T), ms) as T;
                    return result;
                }
                catch (SerializationException e)
                {
                    DetailLog.Error("Failed to deserialize. Log: " + e.Message);
                    return null;
                }
            }
        }

        private static byte[] Compession(byte[] data)
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

        private static byte[] Decompession(byte[] data)
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
    }
}