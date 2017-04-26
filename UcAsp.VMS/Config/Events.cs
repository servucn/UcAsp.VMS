using System;

namespace UcAsp.VMS
{	
	/// <summary>
	///   Types of changes that may be made to a Profile object. 
	/// </summary>
	public enum ProfileChangeType
	{
		/// <summary> 
		///   The change refers to the <see cref="Profile.Name" /> property. 
		/// </summary>		
		Name,

		/// <summary> 
		///   The change refers to the <see cref="Profile.ReadOnly" /> property. 
		/// </summary>		
		ReadOnly,

		/// <summary> 
		///   The change refers to the <see cref="Profile.SetValue" /> method. 
		/// </summary>		
		SetValue,

		/// <summary> 
		///   The change refers to the <see cref="Profile.RemoveEntry" /> method. 
		/// </summary>		
		RemoveEntry,

		/// <summary> 
		///   The change refers to the <see cref="Profile.RemoveSection" /> method. 
		/// </summary>		
		RemoveSection,

		/// <summary> 
		///   The change refers to method or property specific to the Profile class. 
		/// </summary>		
		Other
	}
	
	/// <summary>
	///   EventArgs class to be passed as the second parameter of a <see cref="Profile.Changed" /> event handler. 
	/// </summary>
	public class ProfileChangedArgs : EventArgs
	{   
		// Fields
		private readonly ProfileChangeType _changeType;
		private readonly string _section;
		private readonly string _entry;
		private readonly object _value;

		public ProfileChangedArgs(ProfileChangeType changeType, string section, string entry, object value) 
		{
			_changeType = changeType;
			_section = section;
			_entry = entry;
			_value = value;
		}
		
		/// <summary>
		///   Gets the type of change that raised the event. </summary>
		public ProfileChangeType ChangeType
		{
			get 
			{
				return _changeType;
			}
		}
		
		/// <summary>
		///   Gets the name of the section involved in the change, or null if not applicable. </summary>
		public string Section
		{
			get 
			{
				return _section;
			}
		}
		
		/// <summary>
		///   Gets the name of the entry involved in the change, or null if not applicable. </summary>
		public string Entry
		{
			get 
			{
				return _entry;
			}
		}
		
		/// <summary>
		///   Gets the new value for the entry or method/property, based on the value of <see cref="ChangeType" />. </summary>
		public object Value
		{
			get 
			{
				return _value;
			}
		}
	}

	/// <summary>
	///   EventArgs class to be passed as the second parameter of a <see cref="Profile.Changing" /> event handler. </summary>
	public class ProfileChangingArgs : ProfileChangedArgs
	{   
		private bool _cancel;
		
		public ProfileChangingArgs(ProfileChangeType changeType, string section, string entry, object value) :
			base(changeType, section, entry, value)
		{
		}
		                    
		/// <summary>
		///   Gets or sets whether the change about to the made should be canceled or not. </summary>
		public bool Cancel
		{
			get 
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}
	}
   
	public delegate void ProfileChangingHandler(object sender, ProfileChangingArgs e);

	public delegate void ProfileChangedHandler(object sender, ProfileChangedArgs e);
}

