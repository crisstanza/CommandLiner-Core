using System;
using System.Reflection;

namespace io.github.crisstanza.commandliner
{
	public class CommandLinerCore
	{
		public static string Version()
		{
			return Assembly.GetExecutingAssembly().GetName().Version.ToString();
		}
	}
}
