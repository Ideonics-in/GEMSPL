using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEMSPL.Entity
{
    public class ULB : TreeNode
    {
        public string Name { get; set; }
        public List<Zone> Zones { get; set; }

        public ULB(String path,System.Windows.Visibility visibility)
        {
            base.Path = path;
            base.Visibility = visibility;
            Zones = new List<Zone>();
            Name = path.Substring(path.LastIndexOf("\\") + 1);
            foreach (String s in Directory.GetDirectories(path))
            {
                Zones.Add(new Zone(s,visibility));
            }
        }
    }
}
