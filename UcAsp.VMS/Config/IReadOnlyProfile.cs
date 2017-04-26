using System;
using System.Data;

namespace UcAsp.VMS
{
    public interface IReadOnlyProfile : ICloneable
    {
        /// <summary>
        ///   Gets the name associated with the profile. 
        /// </summary>
        string Name
        {
            get;
        }

        object GetValue(string section, string entry);

        string GetValue(string section, string entry, string defaultValue);

        int GetValue(string section, string entry, int defaultValue);

        double GetValue(string section, string entry, double defaultValue);

        bool GetValue(string section, string entry, bool defaultValue);

        bool HasEntry(string section, string entry);

        bool HasSection(string section);

        string[] GetEntryNames(string section);

        string[] GetSectionNames();

        DataSet GetDataSet();
    }
}
