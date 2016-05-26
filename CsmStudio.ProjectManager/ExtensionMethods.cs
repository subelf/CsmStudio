using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluraySharp.Common.BdStandardPart;
using System.IO;
using System.Diagnostics;

namespace CsmStudio.ProjectManager.Compile
{
	static class ExtensionMethods
	{
		[Conditional("DEBUG")]
		public static void AssertNotNull(this object argument)
		{
			object.ReferenceEquals(argument, null).FalseOrThrow(new NullReferenceException());
		}

		public static T AssertNotNull<T>(this T argument, string paramName)
		{
			object.ReferenceEquals(argument, null).FalseOrThrow(new ArgumentNullException(paramName));

			return argument;
		}

		public static void TrueOrThrow<T>(this bool flag, T ex) where T : Exception
		{
			if (!flag) throw ex;
		}

		public static void FalseOrThrow<T>(this bool flag, T ex) where T : Exception
		{
			if (flag) throw ex;
		}

		public static uint ToBdTimeValue(this TimeSpan span)
		{
			span.AssertNotNull();
			return BdTime.TicksToTimeValue(span.Ticks);
		}

		public static DirectoryInfo SafeCreate(this DirectoryInfo dirInfo, string paramName)
		{
			dirInfo.AssertNotNull(paramName).Create();
			dirInfo.AssertExists();
			return dirInfo;
		}

		public static T AssertExists<T>(this T fsInfo)
			where T : FileSystemInfo
		{
			fsInfo.AssertNotNull();
			fsInfo.Exists.TrueOrThrow(new FileNotFoundException(fsInfo.FullName));
			return fsInfo;
		}

		public static DirectoryInfo NavigateTo(this DirectoryInfo dir, string path)
		{
			dir.AssertNotNull();
			var tPath = Path.Combine(dir.FullName, path);
			return new DirectoryInfo(tPath);
		}

		public static FileInfo PickFile(this DirectoryInfo dir, string path)
		{
			dir.AssertNotNull();
			var tPath = Path.Combine(dir.FullName, path);
			return new FileInfo(tPath);
		}
	}
}
