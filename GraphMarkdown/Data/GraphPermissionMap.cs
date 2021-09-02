using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMarkdown.Data
{
    public class GraphPermissionMap
    {
        public GraphPermissionMap(string permissionName)
        {
            PermissionName = permissionName;
        }
        public string PermissionName { get; private set; }

        private List<DocGraphPermission> docGraphPermissions = new List<DocGraphPermission>();

        public List<DocGraphPermission> DocPermissions
        {
            get
            {
                return docGraphPermissions;
            }
            set
            {
                docGraphPermissions = value;
            }
        }
        public Dictionary<string, ApiUri> Uris { get; set; }

        public Oauth2permissionscopes DelegatePermission { get; set; }
        public Approle ApplicationPermission { get; set; }
    }
}
