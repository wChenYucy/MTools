// ****************************************************
// 	文件：MPackage.cs
// 	作者：积极向上小木木
// 	邮箱：positivemumu@126.com
// 	日期：2020/12/26 14:11:38
// 	功能：数据包类，用于接收发送的数据。
// *****************************************************
using System;
namespace MNet
{
    public class MPackage
    {
        private int _headLength;
        private byte[] _headBuffer;
        private int _headIndex;

        private int _bodyLength;
        private byte[] _bodyBuffer;
        private int _bodyIndex;



        public MPackage()
        {
            _headLength = 4;
            _headBuffer = new byte[_headLength];
            _headIndex = 0;

            _bodyLength = 0;
            _bodyBuffer = null;
            _bodyIndex = 0;
        }

        public int HeadLength { get => _headLength; }
        public byte[] HeadBuffer { get => _headBuffer; set => _headBuffer = value; }
        public int HeadIndex { get => _headIndex; set => _headIndex = value; }
        public int BodyLength { get => _bodyLength; }
        public byte[] BodyBuffer { get => _bodyBuffer; set => _bodyBuffer = value; }
        public int BodyIndex { get => _bodyIndex; set => _bodyIndex = value; }

        public void InitBodyBuffer()
        {
            _bodyLength = BitConverter.ToInt32(_headBuffer, 0);
            _bodyBuffer = new byte[_bodyLength];
        }


        public void RestPackage()
        {
            _headIndex = 0;
            _bodyLength = 0;
            _bodyBuffer = null;
            _bodyIndex = 0;
        }
    }
}
