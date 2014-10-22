using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace GEMSPL.Entity
{
    public class User
    {
        public String Name { get; set; }
        public String Password { get; set; }

    }

    [DataContract]
    public class Users
    {
        // need a parameterless constructor for serialization
        public Users()
        {
            UserDictionary = new Dictionary<string, string>();
        }

        
        [DataMember]
        public Dictionary<string, string> UserDictionary { get; set; }
    }

}
