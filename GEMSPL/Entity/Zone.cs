using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEMSPL.Entity
{
    public class Zone :TreeNode
    {
        public String Name { get; set; }
        public List<Area> Areas { get; set; }

        public Zone(String path, System.Windows.Visibility visibility)
        {
            base.Path = path;
            base.Visibility = visibility;
            Areas = new List<Area>();
            Name = path.Substring(path.LastIndexOf("\\") + 1);
            foreach (String s in Directory.GetDirectories(path))
            {
               
                Areas.Add(new Area(s,visibility));
            }
        }

    }
}
