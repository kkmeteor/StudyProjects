using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOCStudy
{
    class UserDao : IUserDao
    {
        int IUserDao.Add(int v)
        {
            return v + 1;
        }
    }
}
