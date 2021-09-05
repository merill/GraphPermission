using System.Collections.Generic;
using System.Linq;

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

        public string PermissionType
        {
            get
            {
                return
                    (DelegatePermission != null && ApplicationPermission != null) ? "Application + Delegate" :
                    (ApplicationPermission != null) ? "Application" :
                    (DelegatePermission != null) ? "Delegate" : string.Empty;
            }
        }

        public string Description
        {
            get
            {
                return 
                    DelegatePermission != null ? DelegatePermission.adminConsentDescription :
                    ApplicationPermission != null ? ApplicationPermission.description : string.Empty;
            }
        }

        public string DisplayName
        {
            get
            {
                return
                    DelegatePermission != null ? DelegatePermission.adminConsentDisplayName :
                    ApplicationPermission != null ? ApplicationPermission.displayName : string.Empty;
            }
        }

        public int UriCount
        {
            get
            {
                return (Uris == null) ? 0 : Uris.Count();
            }
        }
    }
}
