using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NHibernate.Cfg;
using NHibernate;
using MyWeb.WebTemp.Model;

namespace NhibernateDemo
{
    public partial class _Default : System.Web.UI.Page
    {
        public IList<User> cUsers = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            Configuration cfg = new Configuration();
            cfg.Configure(Server.MapPath("~") + "Nhibernate.cfg.xml");
            ISessionFactory _SessionFactory = cfg.BuildSessionFactory();//建立Session工厂
            ISession session = _SessionFactory.OpenSession();//打开Session
            //User user = session.Get<User>(1) as User;
           


           MyWeb.WebTemp.Model.User myUser = new User {IdentifyId="122222",Email="22222",CreateTime=DateTime.Now, LastTimeLogOn = DateTime.Now, Name= "ml",NickName="malun",PassWord="123",Phone="nihao" };
           session.Save(myUser);
           session.Flush();
            //session.Delete(User);

            IList<User> users = null;
            IQuery query = session.CreateQuery("from User u where u.Id>=:id");
            query.SetInt32("id", 2);
            users = query.List<User>();
            cUsers = users;
            //this.GridView1.DataSource = users;
            //this.GridView1.DataBind();
        }


    }
}
