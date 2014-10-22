using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GEMSPL.Entity
{
    public class Area :TreeNode
    {
        public String Name { get; set; }
        public List<Module> Modules { get; set; }
        public Schedule Schedule { get; set; }

        public Area(String path, System.Windows.Visibility visibility)
        {
            base.Path = path;
            base.Visibility = visibility;
            Modules = new List<Module>();
            Name = path.Substring(path.LastIndexOf("\\") + 1);
            XmlSerializer serializer = new XmlSerializer(typeof(Schedule));

            StreamReader stream = new StreamReader(path + "\\Schedule");
            Schedule = (Schedule)serializer.Deserialize(stream);
            stream.Close();


           
            foreach (String s in Directory.GetDirectories(path))
            {

                Modules.Add(new Module(s,Schedule,visibility));
            }
        }

    }
}
