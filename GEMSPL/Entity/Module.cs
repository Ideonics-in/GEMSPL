using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GEMSPL.Entity
{
    [Serializable]
    public class Module : TreeNode
    {
        [XmlIgnore]
        public String Location { get; set; }

        public ModuleDetails Details { get; set; }

        [XmlIgnore]
        public Schedule Schedule { get; set; }

        public Module()
        {
            Details = new ModuleDetails();
        }

        public Module(String path, Schedule sch, System.Windows.Visibility visibility)
        {
            base.Path = path;
            base.Visibility = visibility;
            Schedule = sch;
            FileStream fs;
            XmlSerializer serializer = new XmlSerializer(typeof(ModuleDetails));
            
            Location = path.Substring(path.LastIndexOf("\\") + 1);

            

            
            StreamReader stream = new StreamReader(path + "\\Details");
            Details = (ModuleDetails)serializer.Deserialize(stream);
            stream.Close();

        }

    }

    [Serializable]
    public class ModuleDetails
    {
        public String No { get; set; }
        public String RR { get; set; }
        public String SIM { get; set; }
        public String Load { get; set; }
        public String Image { get; set; }

        public ModuleDetails()
        {
            No = RR = SIM = Load = Image = String.Empty;
        }

    }
}
