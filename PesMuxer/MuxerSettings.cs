using System;
using System.IO;

namespace PesMuxer
{
	public class MuxerSettings
	{
		public DirectoryInfo SchemaDir { get; set; }
		public Uri MuxServerUri { get; set; }
	}
}