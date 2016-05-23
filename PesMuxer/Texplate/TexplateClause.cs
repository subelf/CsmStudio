using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PesMuxer.Texplate
{
	public class TexplateClause : ITexplateClause
	{
		public TexplateClause(Func<ITexplate, string> renderer)
		{
			this.renderer = renderer;
		}

		public TexplateClause(StreamReader reader)
		{
			this.renderer =
				xTexplate => TextReaderRenderer(xTexplate, reader);
		}

		public string Render(ITexplate texplate)
		{
			return renderer(texplate);
		}
		
		private Func<ITexplate, string> renderer;

		private static string TextReaderRenderer(ITexplate texplate, StreamReader reader)
		{
			reader.AssertNotNull("reader");

			lock (reader)
			{
				var tRes = string.Empty;
				reader.BaseStream.Position = 0;

				for (;;)
				{
					var tLine = reader.ReadLine();

					if (tLine == null) break;

					tLine = Regex.Replace(
						tLine,
						@"\$\{(\w+)\}",
						xMatch => TexplateReplace(texplate, xMatch)
						);
					tRes += tLine + Environment.NewLine;
				}

				return tRes;
			}
		}

		private static string TexplateReplace(ITexplate texplate, Match match)
		{
			string tRet = string.Empty;

			if(match.Groups.Count == 2 && texplate != null)
			{
				var tResValue = texplate[match.Groups[1].Value];
				if (tResValue != null)
				{
					tRet = tResValue.Render(texplate);
				}
			}

			return tRet;
		}
	}
}
