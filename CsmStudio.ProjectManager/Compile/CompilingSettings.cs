using System;
using System.IO;

namespace CsmStudio.ProjectManager.Compile
{
	public class CompilingSettings
	{
		public FileInfo MuxServerExeFile { get; set; }
		public DirectoryInfo SchemaDir { get; set; }
		public DirectoryInfo TempDir { get; set; }
		public Uri MuxServerUri { get; set; }

		public void Validate()
		{
			MuxServerExeFile.AssertExists();
			SchemaDir.AssertNotNull("SchemaDir").AssertExists();
			TempDir.SafeCreate("TempDir");
		}

	}
}