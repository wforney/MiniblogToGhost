namespace MiniblogToGhost.Miniblog
{
    using System.Collections.Generic;

    public class PostAnalysis
    {
        public int Failures { get; set; }
        public List<post> Posts { get; set; }
        public int Successes { get; set; }
    }
}
