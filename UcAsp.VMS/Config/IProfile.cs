using System.Data;

namespace UcAsp.VMS
{
    public interface IProfile : IReadOnlyProfile
    {
        /// <summary>
        ///   Gets or sets the name associated with the profile. 
        /// </summary>
        new string Name
        {
            get;
            set;
        }

        string DefaultName
        {
            get;
        }

        bool ReadOnly
        {
            get;
            set;
        }

        void SetValue(string section, string entry, object value);

        void RemoveEntry(string section, string entry);

        void RemoveSection(string section);

        void SetDataSet(DataSet ds);

        IReadOnlyProfile CloneReadOnly();

        event ProfileChangingHandler Changing;

        event ProfileChangedHandler Changed;	
    }
}
