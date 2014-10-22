using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEMSPL.Entity
{
    public abstract class TreeNode
    {
        public String Path { get; set; }
        public System.Windows.Visibility Visibility { get; set; }
    }
}
