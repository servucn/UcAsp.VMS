using System;
using System.IO;
using System.Xml;

namespace UcAsp.VMS
{
    public class Config : XmlBase
    {
        // Fields
        private string _groupName = "profile";
        private string _sectionValue = "";
        private const string SectionType = "System.Configuration.NameValueSectionHandler, System, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, Custom=null";

        public Config()
        {
        }

        public Config(string fileName) :
            base(fileName)
        {
        }

        public Config(string fileName, string groupName) :
            base(fileName)
        {
            _groupName = groupName;
        }
        public Config(string fileName, string groupName, string sectionValue) :
            base(fileName)
        {
            _groupName = groupName;
            _sectionValue = sectionValue;
        }


        public Config(Config config) :
            base(config)
        {
            _groupName = config._groupName;
        }

        /// <summary>
        ///   Gets the default name for the Config file. 
        /// </summary>
        public override string DefaultName
        {
            get
            {
                return DefaultNameWithoutExtension + ".config";
            }
        }

        /// <summary>
        ///   Retrieves a copy of itself. 
        /// </summary>
        public override object Clone()
        {
            return new Config(this);
        }

        public string GroupName
        {
            get
            {
                return _groupName;
            }
            set
            {
                VerifyNotReadOnly();
                if (_groupName == value)
                    return;

                if (!RaiseChangeEvent(true, ProfileChangeType.Other, null, "GroupName", value))
                    return;

                _groupName = value;
                if (_groupName != null)
                {
                    _groupName = _groupName.Replace(' ', '_');

                    if (_groupName.IndexOf(':') >= 0)
                        throw new Exception("GroupName may not contain a namespace prefix.");
                }

                RaiseChangeEvent(false, ProfileChangeType.Other, null, "GroupName", value);
            }
        }

        public string Section
        {
            get { return this._sectionValue; }
            set { this._sectionValue = value; }
        }

        /// <summary>
        ///   Gets whether we have a valid GroupName. </summary>
        private bool HasGroupName
        {
            get
            {
                return _groupName != null && _groupName != "";
            }
        }

        /// <summary>
        ///   Gets the name of the GroupName plus a slash or an empty string is HasGroupName is false. </summary>
        private string GroupNameSlash
        {
            get
            {
                return (HasGroupName ? (_groupName + "/") : "");
            }
        }

        /// <summary>
        ///   Retrieves whether we don't have a valid GroupName and a given section is 
        ///   equal to "appSettings". </summary>
        private bool IsAppSettings(string section)
        {
            return !HasGroupName && section != null && section == "appSettings";
        }

        /// <summary>
        ///   Verifies the given section name is not null and trims it. </summary>
        protected override void VerifyAndAdjustSection(ref string section)
        {
            base.VerifyAndAdjustSection(ref section);
            if (section.IndexOf(' ') >= 0)
                section = section.Replace(' ', '_');
        }

        /// <summary>
        ///   Sets the value for an entry inside a section. </summary>
        public override void SetValue(string section, string entry, object value)
        {
            // If the value is null, remove the entry
            if (value == null)
            {
                RemoveEntry(section, entry);
                return;
            }

            VerifyNotReadOnly();
            VerifyName();
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);

            if (!RaiseChangeEvent(true, ProfileChangeType.SetValue, section, entry, value))
                return;

            bool hasGroupName = HasGroupName;
            bool isAppSettings = IsAppSettings(section);

            // If the file does not exist, use the writer to quickly create it
            if ((_buffer == null || _buffer.IsEmpty) && !File.Exists(Name))
            {
                XmlTextWriter writer = null;

                // If there's a buffer, write to it without creating the file
                if (_buffer == null)
                    writer = new XmlTextWriter(Name, Encoding);
                else
                    writer = new XmlTextWriter(new MemoryStream(), Encoding);

                writer.Formatting = Formatting.Indented;

                writer.WriteStartDocument();

                writer.WriteStartElement("configuration");
                if (!isAppSettings)
                {
                    writer.WriteStartElement("configSections");
                    if (hasGroupName)
                    {
                        writer.WriteStartElement("sectionGroup");
                        writer.WriteAttributeString("name", null, _groupName);
                    }
                    writer.WriteStartElement("section");
                    writer.WriteAttributeString("name", null, section);
                    writer.WriteAttributeString("type", null, SectionType);
                    writer.WriteEndElement();

                    if (hasGroupName)
                        writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                if (hasGroupName)
                    writer.WriteStartElement(_groupName);
                writer.WriteStartElement(section);
                if (!string.IsNullOrEmpty(_sectionValue))
                {
                    writer.WriteAttributeString("value", null, _sectionValue.ToString());
                }
                writer.WriteStartElement("add");
                writer.WriteAttributeString("key", null, entry);
                writer.WriteAttributeString("value", null, value.ToString());
                writer.WriteEndElement();
                writer.WriteEndElement();
                if (hasGroupName)
                    writer.WriteEndElement();
                writer.WriteEndElement();

                if (_buffer != null)
                    _buffer.Load(writer);
                writer.Close();

                RaiseChangeEvent(false, ProfileChangeType.SetValue, section, entry, value);
                return;
            }

            // The file exists, edit it

            XmlDocument doc = GetXmlDocument();
            XmlElement root = doc.DocumentElement;

            XmlAttribute attribute = null;
            XmlNode sectionNode = null;

            // Check if we need to deal with the configSections element
            if (!isAppSettings)
            {
                // Get the configSections element and add it if it's not there
                XmlNode sectionsNode = root.SelectSingleNode("configSections");
                if (sectionsNode == null)
                    sectionsNode = root.AppendChild(doc.CreateElement("configSections"));

                XmlNode sectionGroupNode = sectionsNode;
                if (hasGroupName)
                {
                    // Get the sectionGroup element and add it if it's not there
                    sectionGroupNode = sectionsNode.SelectSingleNode("sectionGroup[@name=\"" + _groupName + "\"]");
                    if (sectionGroupNode == null)
                    {
                        XmlElement element = doc.CreateElement("sectionGroup");
                        attribute = doc.CreateAttribute("name");
                        attribute.Value = _groupName;
                        element.Attributes.Append(attribute);
                        sectionGroupNode = sectionsNode.AppendChild(element);
                    }
                }

                // Get the section element and add it if it's not there
                sectionNode = sectionGroupNode.SelectSingleNode("section[@name=\"" + section + "\"]");
                if (sectionNode == null)
                {
                    XmlElement element = doc.CreateElement("section");
                    attribute = doc.CreateAttribute("name");
                    attribute.Value = section;
                    element.Attributes.Append(attribute);

                    sectionNode = sectionGroupNode.AppendChild(element);
                }

                // Update the type attribute
                attribute = doc.CreateAttribute("type");
                attribute.Value = SectionType;
                sectionNode.Attributes.Append(attribute);
            }

            // Get the element with the sectionGroup name and add it if it's not there
            XmlNode groupNode = root;
            if (hasGroupName)
            {
                groupNode = root.SelectSingleNode(_groupName);
                if (groupNode == null)
                    groupNode = root.AppendChild(doc.CreateElement(_groupName));
            }

            // Get the element with the section name and add it if it's not there
            sectionNode = groupNode.SelectSingleNode(section);
            if (!string.IsNullOrEmpty(_sectionValue))
            {
                sectionNode = groupNode.SelectSingleNode(section + "[@value=\"" + _sectionValue + "\"]");
            }
            if (sectionNode == null)
            {
                if (!string.IsNullOrEmpty(_sectionValue))
                {
                    XmlElement element = doc.CreateElement(section);
                    attribute = doc.CreateAttribute("value");
                    attribute.Value = _sectionValue;
                    element.Attributes.Append(attribute);
                    sectionNode = groupNode.AppendChild(element);
                }
                else
                {
                    sectionNode = groupNode.AppendChild(doc.CreateElement(section));
                }

            }

            // Get the 'add' element and add it if it's not there
            XmlNode entryNode = sectionNode.SelectSingleNode("add[@key=\"" + entry + "\"]");
            if (entryNode == null)
            {
                XmlElement element = doc.CreateElement("add");
                attribute = doc.CreateAttribute("key");
                attribute.Value = entry;
                element.Attributes.Append(attribute);

                entryNode = sectionNode.AppendChild(element);
            }

            // Update the value attribute
            attribute = doc.CreateAttribute("value");
            attribute.Value = value.ToString();
            entryNode.Attributes.Append(attribute);

            // Save the file
            Save(doc);
            RaiseChangeEvent(false, ProfileChangeType.SetValue, section, entry, value);
        }


        /// <summary>
        ///   Retrieves the value of an entry inside a section. </summary>
        public override object GetValue(string section, string entry)
        {
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);

            try
            {
                XmlDocument doc = GetXmlDocument();
                XmlElement root = doc.DocumentElement;
                XmlNode entryNode = root.SelectSingleNode(GroupNameSlash + section + "/add[@key=\"" + entry + "\"]");
                if (!string.IsNullOrEmpty(_sectionValue))
                {
                    entryNode = root.SelectSingleNode(GroupNameSlash + section + "[@value=\"" + _sectionValue + "\"]" + "/add[@key=\"" + entry + "\"]");//
                }
                return entryNode.Attributes["value"].Value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public object GetValue(int i, string section, string entry)
        {
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);

            try
            {
                XmlDocument doc = GetXmlDocument();
                XmlElement root = doc.DocumentElement;
                XmlNodeList list = root.SelectNodes(_groupName);
                XmlNode entryNode = list[i].SelectSingleNode(string.Format("{0}/add[@key=\"{1}\"]", section, entry));
                if (entryNode == null)
                    return null;
                return entryNode.Attributes["value"].Value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        /// <summary>
        ///   Removes an entry from a section. </summary>
        public override void RemoveEntry(string section, string entry)
        {
            VerifyNotReadOnly();
            VerifyAndAdjustSection(ref section);
            VerifyAndAdjustEntry(ref entry);

            // Verify the document exists
            XmlDocument doc = GetXmlDocument();
            if (doc == null)
                return;

            // Get the entry's node, if it exists
            XmlElement root = doc.DocumentElement;
            XmlNode entryNode = root.SelectSingleNode(GroupNameSlash + section + "/add[@key=\"" + entry + "\"]");
            if (!string.IsNullOrEmpty(_sectionValue))
            {
                entryNode = root.SelectSingleNode(GroupNameSlash + section + "[@value=\"" + _sectionValue + "\"]" + "/add[@key=\"" + entry + "\"]");
            }
            if (entryNode == null)
                return;

            if (!RaiseChangeEvent(true, ProfileChangeType.RemoveEntry, section, entry, null))
                return;

            entryNode.ParentNode.RemoveChild(entryNode);
            Save(doc);
            RaiseChangeEvent(false, ProfileChangeType.RemoveEntry, section, entry, null);
        }

        /// <summary>
        ///   Removes a section. </summary>
        public override void RemoveSection(string section)
        {
            VerifyNotReadOnly();
            VerifyAndAdjustSection(ref section);

            // Verify the document exists
            XmlDocument doc = GetXmlDocument();
            if (doc == null)
                return;

            // Get the root node, if it exists
            XmlElement root = doc.DocumentElement;
            if (root == null)
                return;

            // Get the section's node, if it exists
            XmlNode sectionNode = root.SelectSingleNode(GroupNameSlash + section);
            if (!string.IsNullOrEmpty(_sectionValue))
            {
                sectionNode = root.SelectSingleNode(GroupNameSlash + section + "[@value=\"" + _sectionValue + "\"]");
            }
            if (sectionNode == null)
                return;

            if (!RaiseChangeEvent(true, ProfileChangeType.RemoveSection, section, null, null))
                return;

            sectionNode.ParentNode.RemoveChild(sectionNode);

            // Delete the configSections entry also			
            if (!IsAppSettings(section))
            {
                sectionNode = root.SelectSingleNode("configSections/" + (HasGroupName ? ("sectionGroup[@name=\"" + _groupName + "\"]") : "") + "/section[@name=\"" + section + "\"]");
                if (sectionNode == null)
                    return;

                sectionNode.ParentNode.RemoveChild(sectionNode);
            }

            Save(doc);
            RaiseChangeEvent(false, ProfileChangeType.RemoveSection, section, null, null);
        }

        /// <summary>
        ///   Retrieves the names of all the entries inside a section. </summary>
        public override string[] GetEntryNames(string section)
        {
            // Verify the section exists
            if (!HasSection(section))
                return null;

            VerifyAndAdjustSection(ref section);
            XmlDocument doc = GetXmlDocument();
            XmlElement root = doc.DocumentElement;

            // Get the entry nodes
            XmlNodeList entryNodes = root.SelectNodes(GroupNameSlash + section + "/add[@key]");
            if (!string.IsNullOrEmpty(_sectionValue))
            {
                entryNodes = root.SelectNodes(GroupNameSlash + section + "[@value=\"" + _sectionValue + "\"]" + "/add[@key]");
            }
            if (entryNodes == null)
                return null;

            // Add all entry names to the string array			
            string[] entries = new string[entryNodes.Count];
            int i = 0;

            foreach (XmlNode node in entryNodes)
                entries[i++] = node.Attributes["key"].Value;

            return entries;
        }


        /// <summary>
        ///   Retrieves the names of all the sections. </summary>
        public override string[] GetSectionNames()
        {
            // Verify the document exists
            XmlDocument doc = GetXmlDocument();
            if (doc == null)
                return null;

            // Get the root node, if it exists
            XmlElement root = doc.DocumentElement;
            if (root == null)
                return null;

            // Get the group node
            XmlNode groupNode = (HasGroupName ? root.SelectSingleNode(_groupName) : root);
            if (groupNode == null)
                return null;

            // Get the section nodes
            XmlNodeList sectionNodes = groupNode.ChildNodes;
            if (sectionNodes == null)
                return null;

            // Add all section names to the string array			
            string[] sections = new string[sectionNodes.Count];
            int i = 0;

            foreach (XmlNode node in sectionNodes)
                sections[i++] = node.Name;

            return sections;
        }
        /// <summary>
        ///   Retrieves the names of all the sections. </summary>
        public override string[] GetSectionValues()
        {
            // Verify the document exists
            XmlDocument doc = GetXmlDocument();
            if (doc == null)
                return null;

            // Get the root node, if it exists
            XmlElement root = doc.DocumentElement;
            if (root == null)
                return null;

            // Get the group node
            XmlNode groupNode = (HasGroupName ? root.SelectSingleNode(_groupName) : root);
            if (groupNode == null)
                return null;

            // Get the section nodes
            XmlNodeList sectionNodes = groupNode.ChildNodes;
            if (sectionNodes == null)
                return null;

            // Add all section names to the string array			
            string[] sections = new string[sectionNodes.Count];
            int i = 0;

            foreach (XmlNode node in sectionNodes)
                sections[i++] = node.Attributes["value"].Value;

            return sections;
        }

        public int GetGroupCount()
        {
            XmlDocument doc = GetXmlDocument();
            if (doc == null)
                return 0;
            XmlElement root = doc.DocumentElement;
            if (root == null)
                return 0;
            XmlNodeList note = root.SelectNodes(_groupName);
            return note.Count;
        }
    }
}
