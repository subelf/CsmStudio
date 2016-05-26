using MonteCarlo.External.MuxRemoting;
using PesMuxer.Texplate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PesMuxer.MuxProject;

namespace PesMuxer
{
	public class Muxer : TexplateContextBase, IDisposable
	{
		private const ushort PgsPidStart = 0x1200;
		public static ushort GetPgsPid(byte id)
		{
			if(id >= 32)
			{
				throw new InvalidOperationException("Id of cannot exceed 31.");
			}
			return (ushort)(PgsPidStart + id);
		}

		public Muxer(MuxerSettings settings)
		{
			settings.AssertNotNull("settings").Validate();

			this.RegisterIncludes(settings.SchemaDir.AssertNotNull("settings.SchemaDir"));
			this.RegisterTmls(settings.TmlDir);

			this.ConnectMuxServer(settings.MuxServerUri);
		}

		#region Initialization

		const string ClipDesc = "CLIPDescriptor";
		const string ProjDef = "ProjectDefinition";
		const string XsdExt = ".xsd";
		const string TmlExt = ".tml";

		static readonly List<string> TmlList = new List<string>()
		{
			"ClipInfo",
			"ClipRefEntry",
			"PgsProgInfo",
			"PgsEntry",
			ClipDesc,
			ProjDef
		};

		static readonly Dictionary<string, string> IncludeList =
			new Dictionary<string, string>()
			{
				{ "IndexXml" , @"IndexTable.xml" },
				{ "MvObjXml" ,  @"MovieObject.xml" },
				{ "MplsXml" ,  @"MoviePlayList.xml" },

				{ "ProjDefSchema" ,  ProjDef + XsdExt },
				{ "ClipDescSchema" ,  ClipDesc + XsdExt }
			};
		
		private void ConnectMuxServer(Uri muxServerUri)
		{
			muxService = (IMuxRemotingService)
				Activator.GetObject(
					typeof(IMuxRemotingService), muxServerUri.AbsoluteUri
					);
		}

		private static string GetFileName(DirectoryInfo dir, string fileName)
		{
			var tFileInfo = dir.PickFile(fileName);
			if(! tFileInfo.Exists)
			{
				throw new FileNotFoundException(tFileInfo.FullName);
			}

			return tFileInfo.FullName;
		}

		private StreamReader OpenTmlFile(DirectoryInfo dir, string fileName)
		{
			var tFileInfo = dir.PickFile(fileName + TmlExt);
			if (!tFileInfo.Exists)
			{
				throw new FileNotFoundException(tFileInfo.FullName);
			}

			return CreateStreamReader(tFileInfo.FullName);
		}

		private void RegisterIncludes(DirectoryInfo schemaDir)
		{
			schemaDir.AssertExists();

			foreach (var iInclude in IncludeList)
			{
				this[iInclude.Key] = Clause(GetFileName(schemaDir, iInclude.Value));
			}
		}

		private void RegisterTmls(DirectoryInfo tmlDir)
		{
			tmlDir.AssertExists();

			foreach (var iTml in TmlList)
			{
				this[iTml] = Clause(OpenTmlFile(tmlDir, iTml));
			}
		}

		#endregion

		#region Muxer

		private IMuxRemotingService muxService;

		const string XmlExt = @".xml";
		const string ClipDescFile = ClipDesc + XmlExt;
		const string ProjDefFile = ProjDef + XmlExt;

		private class ClipDescFileContext : TexplateContextBase
		{
			public ClipDescFileContext(FileInfo clipDescFile)
			{
				this["ClipXml"] = Clause(clipDescFile.FullName);
			}
		}

		public async Task<bool> Mux(Project project, IProgressReporter reporter)
		{
			var tTpl = new Texplate.Texplate();
			using (tTpl.EnterContext(this))
			using (tTpl.EnterContext(project))
			{
				var tXmlDir = project.XmlTempDir;

				var tClipDescFile = tXmlDir.PickFile(ClipDescFile);
				using (var tClipDescWriter = tClipDescFile.CreateText())
				{
					var tRenderer = this[ClipDesc];
					tClipDescWriter.Write(tRenderer.Render(tTpl));
				}

				var tProjDefFile = tXmlDir.PickFile(ProjDefFile);
				using (tTpl.EnterContext(new ClipDescFileContext(tClipDescFile)))
				using (var tProjDefWriter = tProjDefFile.CreateText())
				{
					var tRenderer = this[ProjDef];
					tProjDefWriter.Write(tRenderer.Render(tTpl));
				}

				MuxEnqueueStruct tMuxTaskDef = new MuxEnqueueStruct(tProjDefFile.FullName, project.ClipCount);
				var tMuxTask = muxService.Enqueue(tMuxTaskDef);

				return await Task.Run(() => WaitMuxTask(tMuxTask, reporter));
			}
		}

		private bool WaitMuxTask(Guid muxTaskId, IProgressReporter reporter)
		{
			reporter.Amount = 100f;

			for (;;)
			{
				if(reporter.IsCanceled)
				{
					muxService.Cancel(muxTaskId);
				}

				var tStatus = muxService.GetRequestStatus(muxTaskId);
				if (tStatus.HasFlag(MuxRequestStatus.EndFlag))
				{
					var tIsOk = tStatus.HasFlag(MuxRequestStatus.Processed);
					muxService.Confirm(muxTaskId);
					reporter.OnTaskEnd();
					return tIsOk;
				}
				else
				{
					var tInfo = muxService.GetRequestInfo(muxTaskId);
					reporter.Progress = tInfo.TotalProgress;
				}

				Thread.Sleep(1000);
			}
		}

		#endregion

		#region IDisposable

		private List<StreamReader> tmlReaders = new List<StreamReader>();
		
		private StreamReader CreateStreamReader(string name)
		{
			var tReader = new StreamReader(name);
			tmlReaders.Add(tReader);
			return tReader;
		}

		private bool isDisposed = false;
		private void Dispose(bool isDisposing)
		{
			if(!isDisposed)
			{
				if(isDisposing)
				{
					foreach (var tXml in tmlReaders)
					{
						if (tXml != null)
						{
							lock (tXml)
							{
								tXml.Dispose();
							}
						}
					}

					tmlReaders = null;
				}

				isDisposed = true;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		~Muxer()
		{
			this.Dispose(false);
		}

		#endregion
	}
}
