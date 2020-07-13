namespace MiniblogToGhost.Miniblog
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class post
    {
        private string authorField;
        private string[] categoriesField;
        private postComment[] commentsField;
        private string contentField;
        private bool ispublishedField;
        private string lastModifiedField;
        private string pubDateField;
        private string slugField;
        private string titleField;

        public string author
        {
            get => this.authorField;
            set => this.authorField = value;
        }

        [XmlArrayItem("category", IsNullable = false)]
        public string[] categories
        {
            get => this.categoriesField;
            set => this.categoriesField = value;
        }

        [XmlArrayItem("comment", IsNullable = false)]
        public postComment[] comments
        {
            get => this.commentsField;
            set => this.commentsField = value;
        }

        public string content
        {
            get => this.contentField;
            set => this.contentField = value;
        }

        public bool ispublished
        {
            get => this.ispublishedField;
            set => this.ispublishedField = value;
        }

        public string lastModified
        {
            get => this.lastModifiedField;
            set => this.lastModifiedField = value;
        }

        public string pubDate
        {
            get => this.pubDateField;
            set => this.pubDateField = value;
        }

        public string slug
        {
            get => this.slugField;
            set => this.slugField = value;
        }

        public string title
        {
            get => this.titleField;
            set => this.titleField = value;
        }
    }
}
