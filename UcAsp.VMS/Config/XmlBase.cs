using System.IO;
using System.Text;
using System.Xml;

namespace UcAsp.VMS
{
    /// <summary>
    ///   Abstract base class for all XML-base Profile classes.  Xml and Config
    /// </summary>
    public abstract class XmlBase : Profile
    {
        private Encoding _encoding = Encoding.UTF8;
        internal XmlBuffer _buffer;

        protected XmlBase()
        {
        }

        protected XmlBase(string fileName) :
            base(fileName)
        {
        }

        protected XmlBase(XmlBase profile) :
            base(profile)
        {
            _encoding = profile.Encoding;
        }

        protected XmlDocument GetXmlDocument()
        {
            if (_buffer != null)
                return _buffer.XmlDocument;

            VerifyName();
            if (!File.Exists(Name))
                return null;

            XmlDocument doc = new XmlDocument();
            doc.Load(Name);
            return doc;
        }

        protected void Save(XmlDocument doc)
        {
            lock (doc)
            {
                if (_buffer != null)
                    _buffer._needsFlushing = true;
                else
                    doc.Save(Name);
            }
        }

        public XmlBuffer Buffer(bool lockFile)
        {
            if (_buffer == null)
                _buffer = new XmlBuffer(this, lockFile);
            return _buffer;
        }

        public XmlBuffer Buffer()
        {
            return Buffer(true);
        }

        public bool Buffering
        {
            get
            {
                return _buffer != null;
            }
        }

        public Encoding Encoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                VerifyNotReadOnly();
                if (Equals(_encoding, value))
                    return;

                if (!RaiseChangeEvent(true, ProfileChangeType.Other, null, "Encoding", value))
                    return;

                _encoding = value;
                RaiseChangeEvent(false, ProfileChangeType.Other, null, "Encoding", value);
            }
        }
    }
}
