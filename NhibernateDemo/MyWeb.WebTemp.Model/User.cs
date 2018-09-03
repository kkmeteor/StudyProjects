using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyWeb.WebTemp.Model
{
    public class User
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public Int32 Id
        {
            get;
            set;
        }

        /// <summary>
        /// 用户名，登录所用的名字
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string NickName
        {
            get;
            set;
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord
        {
            get;
            set;
        }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator
        {
            get;
            set;
        }

        /// <summary>
        /// Email
        /// </summary>
        public string Email
        {
            get;
            set;
        }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string Phone
        {
            get;
            set;
        }

        /// <summary>
        /// 身份证
        /// </summary>
        public string IdentifyId
        {
            get;
            set;
        }

        /// <summary>
        /// 最后一次登录时间
        /// </summary>
        public DateTime LastTimeLogOn
        {
            get;
            set;
        }
    }
}
