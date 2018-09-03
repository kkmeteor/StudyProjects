using System;

namespace TestProject
{
    [Serializable]
    public class User
    {
        public User(string name, bool male)
        {
            this.name = name;
            this.male = male;
        }
        string name = "";
        bool male = true;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public bool Male
        {
            get { return male; }
            set { male = value; }
        }

    }
}