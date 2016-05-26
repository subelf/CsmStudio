using System;
using System.IO;

namespace PesMuxer.MuxProject
{
	public class ProjectSettings
	{
		public DirectoryInfo TempDir { get; set; }
		public DirectoryInfo OutputDir { get; set; }

		public void Validate()
		{
			TempDir.SafeCreate("TempDir");
			OutputDir.SafeCreate("OutputDir");
		}
	}
}