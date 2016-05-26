using System;
using System.IO;

namespace PesMuxer
{
	public class MuxerSettings
	{
		public const string TmlPath = "Templates";

		public DirectoryInfo SchemaDir { get; set; }
		public DirectoryInfo TmlDir { get { return this.SchemaDir.NavigateTo(TmlPath); } }
		public Uri MuxServerUri { get; set; }

		public void Validate()
		{
			SchemaDir.AssertNotNull("SchemaDir").AssertExists();
			TmlDir.AssertExists();
		}

	}
}