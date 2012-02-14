namespace IndexLibrary.Web
{
    using System;
    using System.Collections.Specialized;

    public sealed class SalesForceSubmitter
    {
        #region Fields

        public static string SalesforceUrl = @"https://www.salesforce.com/servlet/servlet.WebToLead?encoding=UTF-8";

        private string campaignId;
        private string lead_source;
        private string oid;
        private string retUrl;
        private NameValueCollection values = new NameValueCollection();

        #endregion Fields

        #region Constructors

        public SalesForceSubmitter()
        {
        }

        #endregion Constructors

        #region Properties

        public string CampaignId
        {
            get { return this.campaignId; }
            set { this.campaignId = value; }
        }

        public string LeadSource
        {
            get { return this.lead_source; }
            set { this.lead_source = value; }
        }

        public string OID
        {
            get { return this.oid; }
            set {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value", "value cannot be null or empty");
                this.oid = value;
            }
        }

        public string ReferringURL
        {
            get { return this.retUrl; }
            set { this.retUrl = value; }
        }

        #endregion Properties

        #region Methods

        public void AddValue(string name, string value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name", "name cannot be null or empty");
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value", "value cannot be null or empty");
            if (name.Equals("oid", StringComparison.OrdinalIgnoreCase)) return;
            else if (name.Equals("retURL", StringComparison.OrdinalIgnoreCase)) return;
            else if (name.Equals("lead_source", StringComparison.OrdinalIgnoreCase)) return;
            else if (name.Equals("Campaign_ID", StringComparison.OrdinalIgnoreCase)) return;
            else if (name.Equals("member_status", StringComparison.OrdinalIgnoreCase)) return;
            this.values.Add(name, value);
        }

        public void Post()
        {
            PostSubmitter submitter = new PostSubmitter(SalesforceUrl);
            submitter.Type = PostTypeEnum.Post;
            submitter.PostItems.Add(this.values);
            submitter.PostItems.Add("oid", this.oid);
            submitter.PostItems.Add("lead_source", this.lead_source);
            submitter.PostItems.Add("Campaign_ID", this.campaignId);
            submitter.PostItems.Add("member_status", "Responded");
            submitter.Post();
        }

        #endregion Methods
    }
}