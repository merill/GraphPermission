using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPermissionParser
{
    class GraphPermission
    {
        public string SourceFile { get; set; }
        public string PermissionType { get; set; }
        public string Permission { get; set; }
        public string HttpRequest { get; set; }

    }
}
