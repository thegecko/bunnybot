using System;

namespace org.theGecko.Utilities
{
	public static class EnumExtensions
	{
		public static T EnumParse<T>(this string value)
		{
			T result = default(T);

			if (!string.IsNullOrEmpty(value))
			{
				if (Enum.IsDefined(typeof(T), value))
				{
					result = (T)Enum.Parse(typeof(T), value, true);
				}
				else
				{
					foreach (string s in Enum.GetNames(typeof(T)))
					{
						if (s.Equals(value, StringComparison.OrdinalIgnoreCase))
						{
							result = (T)Enum.Parse(typeof(T), value, true);
						}
					}
				}
			}
			return result;
		}
	}
}
