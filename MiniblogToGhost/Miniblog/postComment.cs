namespace MiniblogToGhost.Miniblog
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class postComment
    {
        private string authorField;
        private string contentField;
        private string dateField;
        private string emailField;
        private string idField;
        private string ipField;
        private bool isAdminField;
        private bool isApprovedField;
        private string userAgentField;
        private string websiteField;

        public string author
        {
            get => this.authorField;
            set => this.authorField = value;
        }

        public string content
        {
            get => this.contentField;
            set => this.contentField = value;
        }

        public string date
        {
            get => this.dateField;
            set => this.dateField = value;
        }

        public string email
        {
            get => this.emailField;
            set => this.emailField = value;
        }

        [XmlAttribute()]
        public string id
        {
            get => this.idField;
            set => this.idField = value;
        }

        public string ip
        {
            get => this.ipField;
            set => this.ipField = value;
        }

        [XmlAttribute()]
        public bool isAdmin
        {
            get => this.isAdminField;
            set => this.isAdminField = value;
        }

        [XmlAttribute()]
        public bool isApproved
        {
            get => this.isApprovedField;
            set => this.isApprovedField = value;
        }

        public string userAgent
        {
            get => this.userAgentField;
            set => this.userAgentField = value;
        }

        public string website
        {
            get => this.websiteField;
            set => this.websiteField = value;
        }
    }
}
