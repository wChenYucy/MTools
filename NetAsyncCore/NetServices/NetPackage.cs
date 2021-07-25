
// ****************************************************
//     文件：NetPackage.cs
//     作者：积极向上小木木
//     邮箱: positivemumu@126.com
//     日期：2021/07/23 15:09
//     功能：信息存储类，用于存储接受到的网络信息。
//          方便处理类进行处理
// *****************************************************

using System;

namespace NetAsyncCore
{
    public class NetPackage
    {
        private int headLength;
        private byte[] headBuffer;
        private int headIndex;

        private int bodyLength;
        private byte[] bodyBuffer;
        private int bodyIndex;
        
        public NetPackage()
        {
            headLength = 4;
            headBuffer = new byte[headLength];
            headIndex = 0;

            bodyLength = 0;
            bodyBuffer = null;
            bodyIndex = 0;
        }

        public int HeadLength { get => headLength; }
        public byte[] HeadBuffer { get => headBuffer; set => headBuffer = value; }
        public int HeadIndex { get => headIndex; set => headIndex = value; }
        public int BodyLength { get => bodyLength; }
        public byte[] BodyBuffer { get => bodyBuffer; set => bodyBuffer = value; }
        public int BodyIndex { get => bodyIndex; set => bodyIndex = value; }

        public void InitBodyBuffer()
        {
            bodyLength = BitConverter.ToInt32(headBuffer, 0);
            bodyBuffer = new byte[bodyLength];
        }


        internal void RestPackage()
        {
            headIndex = 0;
            bodyLength = 0;
            bodyBuffer = null;
            bodyIndex = 0;
        }
    }
}