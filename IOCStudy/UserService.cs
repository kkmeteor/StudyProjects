using ClassLibrary;
using System;

namespace IOCStudy
{
    internal class UserService
    {
        private static UserService instance;
        internal IUserDao usrDao;
        internal ISpeak speaker;

        private UserService()
        {
            usrDao = CastleContainer.CreateInstance().Resolve<IUserDao>();
            speaker = CastleContainer.CreateInstance().Resolve<ISpeak>();
        }
        public static UserService Instance()
        {
            if (instance == null)
                instance = new UserService();
            return instance;

        }

        internal int Add(int v)
        {
            return usrDao.Add(v);
        }
        internal string Fly()
        {
            return speaker.Speak();
        }
    }
}