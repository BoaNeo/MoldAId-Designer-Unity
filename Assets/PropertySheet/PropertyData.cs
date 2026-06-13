using System;
using System.Reflection;
using System.Text;

namespace PropertySheet
{
	public struct PropertyData
	{
		public object target;
		public PropertyInfo property;
		public MethodInfo method;
		public FieldInfo field;
		public ShowPropertyAttribute attribute;
		public int order;
		public string name => attribute.name!=null ? attribute.name : PrettifyName(property != null ? property.Name : field != null ? field.Name : method!=null ? method.Name : "??");

		public object value => property != null ? property.GetValue(target) : field != null ? field.GetValue(target) : null;
		public Type type => property != null ? property.PropertyType : field != null ? field.FieldType : null;

		public void SetValue(object value)
		{
			if(property!=null)
				property.SetValue(target, value);
			else if(field!=null)
				field.SetValue(target, value);
		}
		
		private string PrettifyName(string auto_name)
		{
			StringBuilder sb = new StringBuilder();
			if (auto_name.StartsWith("in"))
				auto_name = auto_name.Substring(2);
			if (auto_name.StartsWith("out"))
				auto_name = auto_name.Substring(3);
			
			char[] chars = auto_name.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				if (i == 0)
					sb.Append(char.ToUpper(chars[i]));
				else 
				{
					if (char.IsUpper(chars[i]))
						sb.Append(' ');
					sb.Append(chars[i]);
				}
			}
			return sb.ToString();
		}

	}
}