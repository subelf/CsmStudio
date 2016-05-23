using PesMuxer.Texplate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PesMuxer.MuxProject
{
	public class Project : TexplateContextBase
	{
		public Project(ProjectSettings settings)
		{
			settings.AssertNotNull("settings");

			this.RegistDirs(settings.OutputDir, OutputDirList);
			this.RegistDirs(settings.TempDir, TempDirList);

			this["ClipInfoList"] = Clause((xTexplate) => RenderClipInfoes(xTexplate));
			this["ClipRefEntryList"] = Clause((xTexplate) => RenderClipRefEntries(xTexplate));
		}
		
		#region Initialization

		const string XmlTempDirKey = "XmlTempDir";
		const string XmlTempPath = "XmlTemp";

		const string ClipTempDirKey = "ClipTempDir";
		const string ClipTempPath = "TsTemp";

		private static readonly Dictionary<string, string> OutputDirList =
			new Dictionary<string, string>()
			{
				{ "BdmvOutDir" , @"" },
				{ "ClipTsOutDir" ,  "STREAM" }
			};

		private static readonly Dictionary<string, string> TempDirList =
			new Dictionary<string, string>()
			{
				{ "BdmvTempDir" ,  @"BdTemp" },
				{ ClipTempDirKey , ClipTempPath },
				{ XmlTempDirKey ,  XmlTempPath }
			};

		private void RegistDirs(DirectoryInfo rootDir, Dictionary<string, string> subDirMap)
		{
			if (!rootDir.Exists)
			{
				throw new DirectoryNotFoundException(rootDir.FullName);
			}

			foreach (var iOutputDir in subDirMap)
			{
				var tDirInfo = rootDir.NavigateTo(iOutputDir.Value);
				tDirInfo.Create();
				this[iOutputDir.Key] = Clause(tDirInfo.FullName);
			}
		}

		private DirectoryInfo GetRegisteredDir(string dirKey)
		{
			return new DirectoryInfo(this[dirKey].Render(null));
		}

		public DirectoryInfo XmlTempDir
		{
			get { return GetRegisteredDir(XmlTempDirKey); }
		}

		public DirectoryInfo ClipTempDir
		{
			get { return GetRegisteredDir(ClipTempDirKey); }
		}

		#endregion

		#region Clip List

		private List<ClipEntry> clipList = new List<ClipEntry>();

		private void CreateTempDirFor(ClipEntry clip)
		{
			this.ClipTempDir.CreateSubdirectory(clip.ClipIdName);
		}

		public void AddClip(ClipEntry clip)
		{
			this.CreateTempDirFor(clip);
			this.clipList.Add(clip);
		}

		public void AddClipList(IEnumerable<ClipEntry> clipList)
		{
			foreach(var iClip in clipList)
			{
				this.CreateTempDirFor(iClip);
			}
			this.clipList.AddRange(clipList);
		}

		public int ClipCount
		{
			get { return this.clipList.Count; }
		}

		private string RenderClipInfoes(ITexplate texplate)
		{
			return RenderList(texplate, texplate.AssertNotNull("texplate")["ClipInfo"]);
		}

		private string RenderClipRefEntries(ITexplate texplate)
		{
			return RenderList(texplate, texplate.AssertNotNull("texplate")["ClipRefEntry"]);
		}

		private string RenderList(ITexplate texplate, ITexplateClause clause)
		{
			string tRet = string.Empty;

			if (clause != null)
			{
				foreach (var iClp in this.clipList)
				{
					using (texplate.EnterContext(iClp))
					{
						tRet += clause.Render(texplate);
					}
				}
			}

			return tRet;
		}

		#endregion

	}
}
