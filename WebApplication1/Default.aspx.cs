using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication1.MyService;

namespace WebApplication1
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
         protected void btnClick(object sender, EventArgs e)
         {
             MyService.Service1Client user = new MyService.Service1Client();
             CompositeType type = new CompositeType();
             type.BoolValue = true;
             type.StringValue = this.txtName.Text;
             string result = user.GetDataUsingDataContract(type).StringValue;
             Response.Write(result);
         }
    }
}