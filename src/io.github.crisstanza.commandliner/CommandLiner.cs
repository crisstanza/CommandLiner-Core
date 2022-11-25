using System;
using System.Collections.Generic;
using System.Reflection;

namespace io.github.crisstanza.commandliner
{
	public class CommandLiner
	{
		private readonly string[] args;
		private readonly Dictionary<string, PropertyInfo> propertiesToSet = new Dictionary<string, PropertyInfo>();

		public CommandLiner(string[] args)
		{
			this.args = args;
		}
		public T Fill<T>(T obj)
		{
			Type type = obj.GetType();
			IEnumerable<PropertyInfo> propertyInfos = type.GetRuntimeProperties();
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				Attribute attribute = propertyInfo.GetCustomAttribute(typeof(CommandLineArgumentAttribute));
				if (attribute != null)
				{
					propertiesToSet.Add(propertyInfo.Name, propertyInfo);
				}
			}
			for (int i = 0; i < args.Length; i++)
			{
				String arg = args[i];
				string argName, argValue;
				if (arg.StartsWith("--"))
				{
					string[] argNameArgValue = arg.Substring(2).Split("=".ToCharArray());
					argName = argNameArgValue[0];
					argValue = argNameArgValue[1];
				}
				else if (arg.StartsWith("-"))
				{
					argName = arg.Substring(1);
					argValue = args[++i];
				}
				else
				{
					Console.WriteLine("Unexpected argument: " + arg);
					argName = null;
					argValue = null;
				}
				argName = argName.Substring(0, 1).ToUpper() + argName.Substring(1);
				if (propertiesToSet.ContainsKey(argName))
				{
					PropertyInfo propertyInfo = propertiesToSet[argName];
					propertiesToSet.Remove(argName);
					CommandLineArgumentAttribute attribute = (CommandLineArgumentAttribute)propertyInfo.GetCustomAttribute(typeof(CommandLineArgumentAttribute));
					object typedArgValue = GetTypedValue(propertyInfo.PropertyType, argValue, attribute);
					propertyInfo.SetValue(obj, typedArgValue);
				}
				else
				{
					Console.WriteLine("Unmapped argument: " + argName);
				}
			}
			foreach (string name in propertiesToSet.Keys)
			{
				PropertyInfo propertyInfo = propertiesToSet[name];
				CommandLineArgumentAttribute attribute = (CommandLineArgumentAttribute)propertyInfo.GetCustomAttribute(typeof(CommandLineArgumentAttribute));
				object typedArgValue;
				if (attribute.EnvironmentVariable != null)
				{
					string argValue = Environment.GetEnvironmentVariable(attribute.EnvironmentVariable);
					if (argValue == null)
					{
						typedArgValue = attribute.DefaultValue;
					}
					else
					{
						typedArgValue = GetTypedValue(propertyInfo.PropertyType, argValue, attribute);
					}
				}
				else
				{
					typedArgValue = attribute.DefaultValue;
				}
				propertyInfo.SetValue(obj, typedArgValue);
			}
			if (obj is ICommandLineArguments)
			{
				((ICommandLineArguments)obj).Defaults();
			}
			return obj;
		}

		private object GetTypedValue(Type type, string value, CommandLineArgumentAttribute attribute)
		{
			if (value == null)
			{
				return null;
			}
			else if (type.IsAssignableFrom(typeof(Double)))
			{
				return Double.Parse(value);
			}
			else if (type.IsAssignableFrom(typeof(Int32)))
			{
				return Int32.Parse(value);
			}
			else if (type.IsAssignableFrom(typeof(Boolean)))
			{
				return Boolean.Parse(value);
			}
			else
			{
				return value;
			}
		}
	}
}
