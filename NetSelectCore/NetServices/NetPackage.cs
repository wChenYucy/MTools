
// ****************************************************
//     文件：NetPackage.cs
//     作者：积极向上小木木
//     邮箱: positivemumu@126.com
//     日期：2021/07/24 14:24
//     功能：
// *****************************************************

using System;

namespace NetSelectCore
{
    public class NetPackage
    {
        public const int DEFAULT_SIZE = 1024;

        private int m_InitSize = 0;
        public byte[] Bytes;
        public int ReadIndex = 0;
        public int WriteIndex = 0;

        private int Capacity = 0;
        
        public int Remain
        {
            get { return Capacity - WriteIndex; }
        }
        
        public int Length
        {
            get { return WriteIndex - ReadIndex; }
        }
        
        public NetPackage() 
        {
            Bytes = new byte[DEFAULT_SIZE];
            Capacity = DEFAULT_SIZE;
            m_InitSize = DEFAULT_SIZE;
            ReadIndex = 0;
            WriteIndex = 0;
        }

        public void CheckAndMoveBytes()
        {
            if (Length < 8)
            {
                MoveBytes();
            }
        }

        public void MoveBytes()
        {
            if (ReadIndex < 0)
                return;
            Array.Copy(Bytes,ReadIndex,Bytes,0,Length);
            WriteIndex = Length;
            ReadIndex = 0;
        }

        public void Resize(int size)
        {
            if (ReadIndex < 0 || size < Length || size < m_InitSize)
            {
                return;
            }

            int n = 1024;
            while (n < size) 
            {
                n *= 2;
            }

            Capacity = n;
            byte[] newBytes = new byte[Capacity];
            Array.Copy(Bytes,ReadIndex,newBytes,0,Length);
            Bytes = newBytes;
            WriteIndex = Length;
            ReadIndex = 0;
        }
    }
}