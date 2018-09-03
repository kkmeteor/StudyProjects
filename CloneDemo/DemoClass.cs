using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace CloneDemo
{
    [Serializable]
    class DemoClass
    {
        internal int i = 0;
        internal int[] ints = { 8, 9, 10 };
        /// <summary>
        /// 浅表克隆，值类型复制，引用类型使用原始的
        /// </summary>
        /// <returns></returns>
        public DemoClass Clone1()
        {
            return this.MemberwiseClone() as DemoClass;
        }
        /// <summary>
        /// 深克隆，值类型引用类型都复制
        /// </summary>
        /// <returns></returns>
        public DemoClass Clone2()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, this);
            ms.Position = 0;
            return formatter.Deserialize(ms) as DemoClass;
        }
    }
}
