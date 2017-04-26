using System;
using System.Data;

namespace UcAsp.VMS
{
    /// <summary>
    ///   Abstract base class for all Profile classes in this namespace. 
    /// </summary>
    public abstract class Profile
    {
        // Fields
        private string _name;
        private bool _readOnly;

        /// <summary>
        ///   Event used to notify that the profile is about to be changed. 
        /// </summary>
        public event ProfileChangingHandler Changing;

        /// <summary>
        ///   Event used to notify that the profile has been changed. 
        /// </summary>
        public event ProfileChangedHandler Changed;

        protected Profile()
        {
            _name = DefaultName;
        }

        protected Profile(string name)
        {
            _name = name;
        }

        protected Profile(Profile profile)
        {
            _name = profile._name;
            _readOnly = profile._readOnly;
            Changing = profile.Changing;
            Changed = profile.Changed;
        }

        /// <summary>
        ///   Gets or sets the name associated with the profile. 
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                VerifyNotReadOnly();
                if (_name == value.Trim())
                    return;

                if (!RaiseChangeEvent(true, ProfileChangeType.Name, null, null, value))
                    return;

                _name = value.Trim();
                RaiseChangeEvent(false, ProfileChangeType.Name, null, null, value);
            }
        }

        /// <summary>
        ///   Gets or sets whether the profile is read-only or not. 
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return _readOnly;
            }

            set
            {
                VerifyNotReadOnly();
                if (_readOnly == value)
                    return;

                if (!RaiseChangeEvent(true, ProfileChangeType.ReadOnly, null, null, value))
                    return;

                _readOnly = value;
                RaiseChangeEvent(false, ProfileChangeType.ReadOnly, null, null, value);
            }
        }

        /// <summary>
        ///   Gets the name associated with the profile by default. 
        /// </summary>
        public abstract string DefaultName
        {
            get;
        }

        public abstract object Clone();

        public abstract void SetValue(string section, string entry, object value);

        

        public abstract object GetValue(string section, string entry);

        public virtual string GetValue(string section, string entry, string defaultValue)
        {
            object value = GetValue(section, entry);
            if (!HasEntry(section, entry)) SetValue(section, entry, defaultValue);
            return (value == null ? defaultValue : value.ToString());
        }

        public virtual int GetValue(string section, string entry, int defaultValue)
        {
            object value = GetValue(section, entry);
            if (!HasEntry(section, entry)) SetValue(section, entry, defaultValue);
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        public virtual double GetValue(string section, string entry, double defaultValue)
        {
            object value = GetValue(section, entry);
            if (!HasEntry(section, entry)) SetValue(section, entry, defaultValue);
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return 0;
            }
        }

        public virtual bool GetValue(string section, string entry, bool defaultValue)
        {
            object value = GetValue(section, entry);
            if (!HasEntry(section, entry)) SetValue(section, entry, defaultValue);
            if (value == null)
                return defaultValue;
            if (value is string)
            {
                if (value.ToString().ToLower().Equals("true"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            try
            {
                return Convert.ToBoolean(value);
            }
            catch
            {
                return false;
            }
        }

        public virtual bool HasEntry(string section, string entry)
        {
            string[] entries = GetEntryNames(section);

            if (entries == null)
                return false;

            VerifyAndAdjustEntry(ref entry);
            return Array.IndexOf(entries, entry) >= 0;
        }

        public virtual bool HasSection(string section)
        {
            string[] sections = GetSectionNames();

            if (sections == null)
                return false;

            VerifyAndAdjustSection(ref section);
            return Array.IndexOf(sections, section) >= 0;
        }

        public abstract void RemoveEntry(string section, string entry);

        public abstract void RemoveSection(string section);

        public abstract string[] GetEntryNames(string section);
        public abstract string[] GetSectionNames();
        public abstract string[] GetSectionValues();
        public virtual IReadOnlyProfile CloneReadOnly()
        {
            Profile profile = (Profile)Clone();
            profile._readOnly = true;
            return (IReadOnlyProfile)profile;
        }

        public virtual DataSet GetDataSet()
        {
            VerifyName();

            string[] sections = GetSectionNames();
            if (sections == null)
                return null;

            DataSet ds = new DataSet(Name);

            // Add a table for each section
            foreach (string section in sections)
            {
                DataTable table = ds.Tables.Add(section);

                // Retrieve the column names and values
                string[] entries = GetEntryNames(section);
                DataColumn[] columns = new DataColumn[entries.Length];
                object[] values = new object[entries.Length];

                int i = 0;
                foreach (string entry in entries)
                {
                    object value = GetValue(section, entry);

                    columns[i] = new DataColumn(entry, value.GetType());
                    values[i++] = value;
                }

                // Add the columns and values to the table
                table.Columns.AddRange(columns);
                table.Rows.Add(values);
            }

            return ds;
        }

        public virtual void SetDataSet(DataSet ds)
        {
            if (ds == null)
                throw new ArgumentNullException("ds");

            // Create a section for each table
            foreach (DataTable table in ds.Tables)
            {
                string section = table.TableName;
                DataRowCollection rows = table.Rows;
                if (rows.Count == 0)
                    continue;

                // Loop through each column and add it as entry with value of the first row				
                foreach (DataColumn column in table.Columns)
                {
                    string entry = column.ColumnName;
                    object value = rows[0][column];

                    SetValue(section, entry, value);
                }
            }
        }
        protected string DefaultNameWithoutExtension
        {
            get
            {
                try
                {
                    string file = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                    return file.Substring(0, file.LastIndexOf('.'));
                }
                catch
                {
                    return "profile";  // if all else fails
                }
            }
        }

        protected virtual void VerifyAndAdjustSection(ref string section)
        {
            if (section == null)
                throw new ArgumentNullException("section");

            section = section.Trim();
        }

        protected virtual void VerifyAndAdjustEntry(ref string entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            entry = entry.Trim();
        }

        protected internal virtual void VerifyName()
        {
            if (string.IsNullOrEmpty(_name))
                throw new InvalidOperationException("Operation not allowed because Name property is null or empty.");
        }

        protected internal virtual void VerifyNotReadOnly()
        {
            if (_readOnly)
                throw new InvalidOperationException("Operation not allowed because ReadOnly property is true.");
        }

        protected bool RaiseChangeEvent(bool changing, ProfileChangeType changeType, string section, string entry, object value)
        {
            if (changing)
            {
                // Don't even bother if there are no handlers.
                if (Changing == null)
                    return true;

                ProfileChangingArgs e = new ProfileChangingArgs(changeType, section, entry, value);
                OnChanging(e);
                return !e.Cancel;
            }

            // Don't even bother if there are no handlers.
            if (Changed != null)
                OnChanged(new ProfileChangedArgs(changeType, section, entry, value));
            return true;
        }

        protected virtual void OnChanging(ProfileChangingArgs e)
        {
            if (Changing == null)
                return;

            foreach (ProfileChangingHandler handler in Changing.GetInvocationList())
            {
                handler(this, e);

                // If a particular handler cancels the event, stop
                if (e.Cancel)
                    break;
            }
        }

        protected virtual void OnChanged(ProfileChangedArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }
    }
}
