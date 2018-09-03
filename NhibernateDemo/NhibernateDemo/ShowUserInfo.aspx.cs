using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NHibernate;
using NHibernate.Cfg;
using MyWeb.WebTemp.Model;

namespace NhibernateDemo
{
    public partial class ShowUserInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Configuration cfg = new Configuration();
            cfg.Configure(Server.MapPath("~") + "Nhibernate.cfg.xml");
            ISessionFactory _SessionFactory = cfg.BuildSessionFactory();//建立Session工厂
            ISession session = _SessionFactory.OpenSession();//打开Session
            int id = int.Parse(this.Request.QueryString["id"].ToString());
            IList<User> users = new List<User>();
            users.Add(session.Get<MyWeb.WebTemp.Model.User>(id));
            this.GridView1.DataSource = users;
            this.GridView1.DataBind();
        }
    }
}
