﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace org.theGecko.Utilities
{
	public sealed class SettingsUtil
	{
		#region Singleton

		private static readonly SettingsUtil _Instance = new SettingsUtil();
        private static readonly SettingsUtil _CachedInstance = new SettingsUtil(true);

		public static SettingsUtil Instance
		{
			get
			{
				return _Instance;
			}
		}

        public static SettingsUtil Cached
        {
            get
            {
                return _CachedInstance;
            }
        }

		// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
		static SettingsUtil()
		{
		}
        
        #endregion

		#region Settings Section

		// Internal settings section
		private readonly NameValueCollection _SettingsSection;
	    private NameValueCollection SettingsSection
	    {
	        get
	        {
	            if (_SettingsSection != null)
	            {
	                return _SettingsSection;
	            }

	            return GetSettings();
	        }
	    }

		public SettingsUtil()
		{
		}

        public SettingsUtil(bool cached)
        {
            if (cached)
            {
                _SettingsSection = GetSettings();
            }
        }

        private NameValueCollection GetSettings()
        {
	        // Default section
            ConfigurationManager.RefreshSection("appSettings");
            NameValueCollection settings = new NameValueCollection(ConfigurationManager.AppSettings);
			
			// Read the environment variable
            string environment = settings.Get("Environment");

			// Merge environment
			if (!string.IsNullOrEmpty(environment))
			{
			    ConfigurationManager.RefreshSection(environment);
                OverrideSections(settings, (NameValueCollection)ConfigurationManager.GetSection(environment), true);
			}

#if DEBUG
			// Location of debug override
			string overrideConfigPath = @"..\..\..\DebugOverride.config";

			// Override with debug section
			if (File.Exists(overrideConfigPath))
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load(overrideConfigPath);

				IConfigurationSectionHandler handler = new NameValueSectionHandler();
				NameValueCollection overrideConfig = (NameValueCollection)handler.Create(null, null, xmlDoc.SelectSingleNode("//appSettings"));

                OverrideSections(settings, overrideConfig, false);
			}
#endif
            return settings;
		}

		private void OverrideSections(NameValueCollection target, NameValueCollection source, bool merge)
		{
			foreach (string key in source.AllKeys)
			{
				if (target.AllKeys.Contains(key))
				{
					target.Set(key, source.Get(key));
				}
				else if (merge)
				{
					target.Add(key, source.Get(key));
				}
			}
		}

		#endregion

		#region String Settings

		/// <summary>
		/// Function to read a specified application setting and return a string[] representation of it based on delimiting using ','
		/// If an error occurs, a blank string[] is returned
		/// </summary>
		/// <param name="setting">The name of the setting to retrieve</param>
		/// <returns>String[] result of setting</returns>
		public String[] GetArraySetting(string setting)
		{
			return GetArraySetting(setting, ',');
		}

		/// <summary>
		/// Function to read a specified application setting and return a string[] representation of it based on a delimeter
		/// If an error occurs, a blank string[] is returned
		/// </summary>
		/// <param name="setting">The name of the setting to retrieve</param>
		/// <param name="delimeter">The delimiter used to split the string</param>
		/// <returns>String[] result of setting</returns>
		public String[] GetArraySetting(string setting, char delimeter)
		{
			string value = GetSetting(setting);

			if (value != String.Empty)
			{
				return value.ToLower().Split(delimeter);
			}

			return new string[] { };
		}

		/// <summary>
		/// Function to read a specified application setting and return a string representation of it
		/// If an error occurs, a blank string is returned
		/// </summary>
		/// <param name="setting">The name of the setting to retrieve</param>
		/// <returns>String result of setting</returns>
		public string GetSetting(string setting)
		{
			return GetSetting(setting, string.Empty);
		}

		/// <summary>
		/// Function to read a specified application setting and return a string representation of it
		/// If an error occurs, a blank string is returned
		/// </summary>
		/// <param name="setting">The name of the setting to retrieve</param>
		/// <param name="defaultValue">The default value to return if the conversion fails</param>
		/// <returns>String result of setting</returns>
		public string GetSetting(string setting, string defaultValue)
		{
			return GetSetting<string>(setting, defaultValue);
		}

		#endregion

		#region Settings Functions

		/// <summary>
		/// Function to read a specified application setting and return a string representation of it
		/// If an error occurs, the default value passed in will be returned
		/// </summary>
		/// <param name="setting">The name of the setting to retrieve</param>
		/// <returns>Value of setting</returns>
		public T GetSetting<T>(string setting)
		{
			return GetSetting(setting, default(T));
		}

		/// <summary>
		/// Function to read a specified application setting and return a string representation of it
		/// If an error occurs, the default value passed in will be returned
		/// </summary>
		/// <param name="setting">The name of the setting to retrieve</param>
		/// <param name="defaultValue">The default value to return if the conversion fails</param>
		/// <returns>Value of setting</returns>
		public T GetSetting<T>(string setting, T defaultValue)
		{
			// Get the setting value from the section
			string value = SettingsSection.Get(setting);

			// Return it if found
			if (value != null)
			{
				try
				{
					return (T)Convert.ChangeType(value, typeof(T));
				}
				catch
				{
				}
			}

			return defaultValue;
		}

		#endregion

        #region ConfigSection Functions

        /// <summary>
        /// Returns the requested config section using the current configuration source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetConfigSection<T>() where T : class
        {
            return GetConfigSection<T>(typeof(T).Name);
        }

        /// <summary>
        /// Returns the requested config section using the current configuration source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetConfigSection<T>(string name) where T : class
        {
            if (!typeof(ConfigurationSection).IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException("GetConfigSection only supports .Net ConfigurationSections");
            }

            return ConfigurationManager.GetSection(name) as T;
        }

        #endregion
	}
}
