namespace MiniblogToGhost.Miniblog
{
    using System.IO;
    using System.Xml.Serialization;

    public static class Utils
    {
        public static post ExtractPostFromFile(string filename)
        {
            var serialiser = new XmlSerializer(typeof(post));

            post post;
            using (var reader = new StreamReader(filename))
            {
                post = (post)serialiser.Deserialize(reader);
                reader.Close();
            }

            return post;
        }
    }
}
