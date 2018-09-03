using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplicationStudy
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn1_Click(object sender, EventArgs e)
        {

        }
        protected string ss()
        {
            return "hello";
        }
        [WebMethod]//标示为web服务方法属性  
        public static string sayhell(string say)//注意函数的修饰符,只能是静态的  
        {
            return say; 
        }
    }
}