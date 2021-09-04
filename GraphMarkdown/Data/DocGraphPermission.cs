using System;
using System.Collections.Generic;

namespace GraphMarkdown.Data
{
    public class DocGraphPermission
    {
        public string SourceFile { get; set; }
        public string PermissionType { get; set; }
        public string PermissionName { get; set; }
        public string HttpRequest { get; set; }
        public bool IsBeta { get; set; }
        public List<string> Resources { get; set; }

        public bool IsApplicationPermission
        {
            get
            {
                return PermissionType.Contains("application", StringComparison.InvariantCultureIgnoreCase);
            }
        }
        public bool IsDelegatePermission
        {
            get
            {
                return PermissionType.Contains("delegate", StringComparison.InvariantCultureIgnoreCase);
            }
        }

    }
}
