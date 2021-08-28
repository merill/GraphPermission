using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphMarkdown.Data
{

    public class GraphResponse
    {
        public string odatacontext { get; set; }
        public MicrosoftGraphObject[] value { get; set; }
    }

    public class MicrosoftGraphObject
    {
        public string odataid { get; set; }
        public string id { get; set; }
        public object deletedDateTime { get; set; }
        public bool accountEnabled { get; set; }
        public object[] alternativeNames { get; set; }
        public string appDisplayName { get; set; }
        public object appDescription { get; set; }
        public string appId { get; set; }
        public object applicationTemplateId { get; set; }
        public string appOwnerOrganizationId { get; set; }
        public bool appRoleAssignmentRequired { get; set; }
        public DateTime createdDateTime { get; set; }
        public object description { get; set; }
        public object disabledByMicrosoftStatus { get; set; }
        public string displayName { get; set; }
        public object homepage { get; set; }
        public object loginUrl { get; set; }
        public object logoutUrl { get; set; }
        public object notes { get; set; }
        public object[] notificationEmailAddresses { get; set; }
        public object preferredSingleSignOnMode { get; set; }
        public object preferredTokenSigningKeyThumbprint { get; set; }
        public object[] replyUrls { get; set; }
        public string[] servicePrincipalNames { get; set; }
        public string servicePrincipalType { get; set; }
        public string signInAudience { get; set; }
        public object[] tags { get; set; }
        public object tokenEncryptionKeyId { get; set; }
        public object[] resourceSpecificApplicationPermissions { get; set; }
        public object samlSingleSignOnSettings { get; set; }
        public Verifiedpublisher verifiedPublisher { get; set; }
        public object[] addIns { get; set; }
        public Approle[] appRoles { get; set; }
        public Info info { get; set; }
        public object[] keyCredentials { get; set; }
        public Oauth2permissionscopes[] oauth2PermissionScopes { get; set; }
        public object[] passwordCredentials { get; set; }
    }

    public class Verifiedpublisher
    {
        public object displayName { get; set; }
        public object verifiedPublisherId { get; set; }
        public object addedDateTime { get; set; }
    }

    public class Info
    {
        public object logoUrl { get; set; }
        public object marketingUrl { get; set; }
        public object privacyStatementUrl { get; set; }
        public object supportUrl { get; set; }
        public object termsOfServiceUrl { get; set; }
    }

    public class Approle
    {
        public string[] allowedMemberTypes { get; set; }
        public string description { get; set; }
        public string displayName { get; set; }
        public string id { get; set; }
        public bool isEnabled { get; set; }
        public string origin { get; set; }
        public string value { get; set; }
    }

    public class Oauth2permissionscopes
    {
        public string adminConsentDescription { get; set; }
        public string adminConsentDisplayName { get; set; }
        public string id { get; set; }
        public bool isEnabled { get; set; }
        public string type { get; set; }
        public string userConsentDescription { get; set; }
        public string userConsentDisplayName { get; set; }
        public string value { get; set; }
    }

}
