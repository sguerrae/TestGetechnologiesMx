using System;
namespace Microsoft.Web.XmlTransform
{
    // Customs
    public class XmlTransformationException : Exception
    {
        public XmlTransformationException(string message) : base(message) { }
    }

    // Customs
    public class XmlTransformableDocument : IDisposable
    {
        public bool PreserveWhitespace { get; set; }

        public void Load(string path)
        {
            // dummy: no validation
        }

        public void Save(string path)
        {
            // dummy: do nothing
        }

        public void Dispose()
        {
        }
    }

    // Customs
    public class XmlTransformation : IDisposable
    {
        private readonly string source;
        public XmlTransformation(string source)
        {
            this.source = source;
        }

        public bool Apply(XmlTransformableDocument doc)
        {
            // dummy: pretend transform succeeded
            return true;
        }

        public void Dispose()
        {
        }
    }
}
