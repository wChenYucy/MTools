// ****************************************************
// 	文件：MData.cs
// 	作者：积极向上小木木
// 	邮箱：positivemumu@126.com
// 	日期：2020/12/26 14:10:46
// 	功能：网络中传输数据的基类
// *****************************************************
using System;
namespace MNet
{
    [Serializable]
    public abstract class MData
    {
        //初始数据
        private int _sequence;
        private int _commandNumber;
        private int _errorNumber;

        public int Sequence { get => _sequence; set => _sequence = value; }
        public int CommandNumber { get => _commandNumber; set => _commandNumber = value; }
        public int ErrorNumber { get => _errorNumber; set => _errorNumber = value; }
    }
}
