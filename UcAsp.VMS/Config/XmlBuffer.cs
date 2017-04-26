using System;
using System.IO;
using System.Xml;

namespace UcAsp.VMS
{

    /// <summary>
    ///   Buffer class for all <see cref="XmlBase" /> Profile classes. 
    /// </summary>
    public class XmlBuffer : IDisposable
    {
        private XmlBase _profile;
        private XmlDocument _doc;
        private FileStream _file;
        internal bool _needsFlushing;

        internal XmlBuffer(XmlBase profile, bool lockFile)
        {
            _profile = profile;

            if (lockFile)
            {
                _profile.VerifyName();
                if (File.Exists(_profile.Name))
                    _file = new FileStream(_profile.Name, FileMode.Open, _profile.ReadOnly ? FileAccess.Read : FileAccess.ReadWrite, FileShare.Read);
            }
        }

        internal void Load(XmlTextWriter writer)
        {
            writer.Flush();
            writer.BaseStream.Position = 0;
            _doc.Load(writer.BaseStream);

            _needsFlushing = true;
        }

        internal XmlDocument XmlDocument
        {
            get
            {
                if (_doc == null)
                {
                    _doc = new XmlDocument();

                    if (_file != null)
                    {
                        _file.Position = 0;
                        _doc.Load(_file);
                    }
                    else
                    {
                        _profile.VerifyName();
                        if (File.Exists(_profile.Name))
                            _doc.Load(_profile.Name);
                    }
                }
                return _doc;
            }
        }

        /// <summary>
        ///   Gets whether the buffer's XmlDocument object is empty. </summary>
        internal bool IsEmpty
        {
            get
            {
                return XmlDocument.InnerXml == String.Empty;
            }
        }

        public bool NeedsFlushing
        {
            get
            {
                return _needsFlushing;
            }
        }

        public bool Locked
        {
            get
            {
                return _file != null;
            }
        }

        public void Flush()
        {
            if (_profile == null)
                throw new InvalidOperationException("Cannot flush an XmlBuffer object that has been closed.");

            if (_doc == null)
                return;

            if (_file == null)
                _doc.Save(_profile.Name);
            else
            {
                _file.SetLength(0);
                _doc.Save(_file);
            }

            _needsFlushing = false;
        }

        public void Reset()
        {
            if (_profile == null)
                throw new InvalidOperationException("Cannot reset an XmlBuffer object that has been closed.");

            _doc = null;
            _needsFlushing = false;
        }

        public void Close()
        {
            if (_profile == null)
                return;

            if (_needsFlushing)
                Flush();

            _doc = null;

            if (_file != null)
            {
                _file.Close();
                _file = null;
            }

            if (_profile != null)
                _profile._buffer = null;
            _profile = null;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
