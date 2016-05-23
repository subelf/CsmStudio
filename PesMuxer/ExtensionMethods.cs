using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluraySharp.Common.BdStandardPart;
using System.IO;

namespace PesMuxer
{
	static class ExtensionMethods
	{
		public static T AssertNotNull<T>(this T argument, string argumentName)
		{
			if(object.ReferenceEquals(argument, null))
			{
				throw new ArgumentNullException(argumentName);
			}

			return argument;
		}

		public static uint ToBdTimeValue(this TimeSpan span)
		{
			return BdTime.TicksToTimeValue(span.Ticks);
		}
		
		public static DirectoryInfo NavigateTo(this DirectoryInfo dir, string path)
		{
			var tPath = Path.Combine(dir.FullName, path);
			return new DirectoryInfo(tPath);
		}

		public static FileInfo PickFile(this DirectoryInfo dir, string path)
		{
			var tPath = Path.Combine(dir.FullName, path);
			return new FileInfo(tPath);
		}
	}
}
