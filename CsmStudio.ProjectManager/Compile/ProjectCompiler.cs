using PesMuxer;
using PesMuxer.MuxProject;
using Spp2PgsNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using BluraySharp;
using BluraySharp.Common;
using BluraySharp.Common.BdStandardPart;
using System.Net.Sockets;

namespace CsmStudio.ProjectManager.Compile
{
	public sealed class ProjectCompiler : IDisposable
	{
		private object lkMuxer = new object();
		private Muxer pesMuxer = null;
		private Process muxProc = null;
		private Spp2Pgs s2pEncoder = null;

		private readonly CompilingSettings settings;
		private readonly ICompilingLogger logger;

		public ProjectCompiler(CompilingSettings settings, ICompilingLogger logger)
		{
			settings.AssertNotNull("settings").Validate();
			this.settings = settings;
			this.logger = logger;

			InitPgsEncoder();
			InitPesMuxer();
		}

		private void InitPgsEncoder()
		{
			s2pEncoder = new Spp2Pgs(new MyS2PSettings(settings), new MyS2PLogger(logger));
		}

		private void InitPesMuxer()
		{
			var tPmSettings = new MuxerSettings()
			{
				MuxServerUri = settings.MuxServerUri,
				SchemaDir = settings.SchemaDir
			};

			lock (lkMuxer)
			{
				try
				{
					pesMuxer = new Muxer(tPmSettings);  //assume muxer already started.

					if (muxProc == null || muxProc.HasExited)
					{
						Dispose(ref muxProc);

						var tProcesses = Process.GetProcessesByName(settings.MuxServerExeFile.Name);
						if (tProcesses.Length > 0)
						{
							muxProc = tProcesses[0];
						}
					}

					return;

				}
				catch (RemotingException)
				{
				}
				catch (NullReferenceException)
				{
				}
				catch (SocketException)
				{
				}

				ProcessStartInfo tStartInfo = new ProcessStartInfo(settings.MuxServerExeFile.FullName);
				tStartInfo.CreateNoWindow = true;
				muxProc = Process.Start(tStartInfo);

				pesMuxer = new Muxer(tPmSettings);  //retry.
			}
		}

		private void MuxerProcess_Exited(object sender, EventArgs e)
		{
			lock (lkMuxer)
			{
				Dispose(ref muxProc);
				Dispose(ref pesMuxer);
			}
		}

		private Muxer GetPesMuxer()
		{
			lock (lkMuxer)
			{
				if (pesMuxer == null)
				{
					InitPesMuxer();
				}

				return pesMuxer;
			}
		}

		#region Compiling

		private const string PesMuxOutputPath = "MuxTemp.{0}";
		private const string PesEncOutputPath = "PesTemp.{0}";

		public async Task<bool> Compile(ICompilingProgressReporter reporter, Guid projectId, DirectoryInfo outputDir, params DocumentClipDescriptor[] clips)
		{
			AssertNotDisposed();

			var tPgsNum = clips.Sum(xClip => xClip.Tracks.Count);
			var tProgressManager = new CompilingProgressManager(reporter, tPgsNum);

			ProjectSettings tProjSettings = new ProjectSettings()
			{
				OutputDir = outputDir,
				TempDir = settings.TempDir.NavigateTo(string.Format(PesMuxOutputPath, projectId))
			};
			Project tProj = new Project(tProjSettings);

			var tPesDir = settings.TempDir.NavigateTo(string.Format(PesEncOutputPath, projectId)).SafeCreate(string.Empty);
			var tClipTasks =
				(from iClip in clips select CompileDocumentClip(tProgressManager, iClip, tPesDir)).ToArray();

			var tPgsEncTask = Task.WhenAll(tClipTasks);
			try { await tPgsEncTask; } catch { }

			if (tPgsEncTask.Exception != null || reporter.IsCanceled)
			{
				ReportSummary(reporter, tPgsEncTask.Exception);
				return false;
			}

			tProj.AddClipList(tClipTasks.Select(xClipTask => xClipTask.Result));
			var tPesMuxTask = this.GetPesMuxer().Mux(tProj, tProgressManager);
			try { await tPesMuxTask; } catch { }

			if (tPesMuxTask.Exception != null || reporter.IsCanceled)
			{
				ReportSummary(reporter, tPesMuxTask.Exception);
				return false;
			}

			ReportSummary(reporter, null);
			return true;
		}

		private void ReportSummary(ICompilingProgressReporter reporter, AggregateException ex)
		{
			if(ex != null)
			{
				foreach(var iEx in ex.InnerExceptions)
				{
					this.logger.Log(0, iEx.Message);
				}

				this.logger.Log(128, "Project compiling failed.");
			}
			else if (reporter.IsCanceled)
			{
				this.logger.Log(128, "Project compiling canceled.");
			}
			else
			{
				this.logger.Log(128, "Project compiled successful.");
			}

			reporter.OnTaskEnd();
		}

		private async Task<ClipEntry> CompileDocumentClip(CompilingProgressManager reporter, DocumentClipDescriptor clip, DirectoryInfo pesDir)
		{
			byte tId = 0;
			var tPgsTaskList = new List<Task<PgsEntryDescriptor>>();
			foreach (var iTrack in clip.Tracks)
			{
				tPgsTaskList.Add(Task.Run(
					() => CompileTrack(reporter, clip, iTrack, pesDir, Muxer.GetPgsPid(tId++))
					));
			}
			await Task.WhenAll(tPgsTaskList);

			TimeSpan tMaxLength = tPgsTaskList.Max(xPgsTask => xPgsTask.Result.Length);
			ClipEntry tClipEntry = new ClipEntry(clip.ClipId, clip.InTimeOffset, clip.InTimeOffset + tMaxLength);

			foreach (var iPgsTask in tPgsTaskList)
			{
				var tPgsDesc = iPgsTask.Result;
				var tPgs = new PgsEntry(tPgsDesc.Pid, tPgsDesc.PesFile, tPgsDesc.Lang);
				tClipEntry.AddPgs(tPgs);
			}

			return tClipEntry;
		}

		private PgsEntryDescriptor CompileTrack(CompilingProgressManager reporter, DocumentClipDescriptor clip, EsTrackDescriptor track, DirectoryInfo pesDir, ushort pid)
		{
			const string PesFileNameFmt = "{0:00000}.{1:x4}.pes";

			using (var tPgsStreamServer = new AnonymousPipeServerStream(PipeDirection.Out))
			using (var tPgsStream = new AnonymousPipeClientStream(PipeDirection.In, tPgsStreamServer.ClientSafePipeHandle))
			using (var tReporter = reporter.CreatePgsProgressReporter())
			{
				Func<int> tEncodeTaskAction = () =>
				{
					using (tPgsStreamServer)
					{
						return EncodePgs(clip, track, tPgsStreamServer, tReporter);
					}
				};

				var tEncodeTask = Task.Run(tEncodeTaskAction);

				var tPesFileName = string.Format(PesFileNameFmt, clip.ClipId, pid);
				FileInfo tPesFile = pesDir.PickFile(tPesFileName);
				Pgs2Pes(tPgsStream, tPesFile);
				
				return new PgsEntryDescriptor(
					pid,
					tPesFile.FullName,
					TimeFromFrameCount(tEncodeTask.Result, clip.Rate),
					track.Track.Lang
					);
			}
		}	

		private int EncodePgs(DocumentClipDescriptor clip, EsTrackDescriptor track, Stream output, Spp2PgsNet.IProgressReporter reporter)
		{
			int maxFrameCount = 0;

			using (var tCtx = s2pEncoder.CreateSubPicProviderContext())
			using (var tOutput = s2pEncoder.CreatePgsEncoder(output, clip.Format, clip.Rate))
			{
				tOutput.RegistAnchor(0);

				foreach (var iEsEntry in track)
				{
					if (iEsEntry == null)
					{
						continue;
					}

					using (var tSpp = s2pEncoder.CreateSubPicProvider(tCtx, iEsEntry.Source))
					{
						var tAdv =
							s2pEncoder.CreateSppFrameStreamAdvisor(
								tSpp, clip.Format, clip.Rate, -1, -1,
								(int)FrameCountFromTime(iEsEntry.SyncOffset, clip.Rate)
								);

						maxFrameCount = tAdv.LastPossibleImage;

						using (var tInput = s2pEncoder.CreateFrameStream(tSpp, tAdv))
						{
							s2pEncoder.Encode(tInput, tOutput, reporter);
						}
					}
				}

				tOutput.FlushAnchor();
			}

			return maxFrameCount;
		}
		
		#region Pgs2Pes

		private static ulong ReadBE(byte[] tBuffer, int index, byte length)
		{
			var tByteIndex = (index >> 3);
			var tShift = (sbyte)(length - (tByteIndex << 3) + index);
			ulong tMaskAll = unchecked(0UL - 1);
			tMaskAll = ~(tMaskAll << length);

			ulong tRet = 0UL;

			while ((tShift -= 8) >= 0)
			{
				ulong tByte = tBuffer[tByteIndex++];
				tRet |= (tByte << tShift);
			}

			tRet |= (((ulong)tBuffer[tByteIndex++]) >> (-tShift));
			return tRet & tMaskAll;
		}

		private static void WriteBE(byte[] tBuffer, int index, byte length, ulong value)
		{
			var tByteIndex = (index >> 3);
			var tShift = (sbyte)(length + index - (tByteIndex << 3));
			ulong tMaskAll = unchecked(0UL - 1);
			tMaskAll = ~(tMaskAll << length);

			byte tMask = 0;
			value &= tMaskAll;

			while ((tShift -= 8) >= 0)
			{
				tMask = (byte)(tMaskAll >> tShift);
				tBuffer[tByteIndex] &= (byte)~tMask;
				tBuffer[tByteIndex] |= (byte)(value >> tShift);

				tByteIndex++;
			}

			tMask = (byte)(tMaskAll << -tShift);
			tBuffer[tByteIndex] &= (byte)~tMask;
			tBuffer[tByteIndex] |= (byte)(value << -tShift);
		}

		private static int ForceRead(Stream pgs, byte[] buffer, int offset, int count)
		{
			int tRead = 0;
			while(tRead < count)
			{
				int tReadLen = pgs.Read(buffer, offset + tRead, count - tRead);
				if (tReadLen == 0) break;

				tRead += tReadLen;
			}
			return tRead;
		}

		private static void Pgs2Pes(Stream pgs, FileInfo pesFile)
		{
			using (FileStream tPes = pesFile.Create())
			using (FileStream tMui = new FileStream(pesFile.FullName + ".mui", FileMode.Create))
			{
				byte[] buffer = new byte[65536];
				WriteBE(buffer, 0, 32, 3);
				tMui.Write(buffer, 0, 4);   //file header;

				long tPtsOffset = 54000000;
				int tReadLen = 0;

				for (;;)
				{
					tReadLen = ForceRead(pgs, buffer, 0, 13);
					if (tReadLen != 13 || buffer[0] != 'P' || buffer[1] != 'G') break;

					uint tPts = (uint)((long)ReadBE(buffer, 16, 32) + tPtsOffset);
					uint tDts = (uint)((long)ReadBE(buffer, 48, 32) + tPtsOffset);

					byte tType = (byte)ReadBE(buffer, 80, 8);
					int tLen = (int)ReadBE(buffer, 88, 16);

					WriteBE(buffer, 0, 8, tType);
					WriteBE(buffer, 8, 32, (ulong)(tLen + 3));
					WriteBE(buffer, 40, 33, (ulong)tDts);
					WriteBE(buffer, 73, 33, (ulong)tPts);
					WriteBE(buffer, 106, 6, 0);

					tMui.Write(buffer, 0, 14);

					WriteBE(buffer, 0, 8, tType);
					WriteBE(buffer, 8, 16, (ulong)tLen);

					tReadLen = ForceRead(pgs, buffer, 3, tLen);
					if (tReadLen != tLen) break;

					tPes.Write(buffer, 0, tLen + 3);
				}

				Array.Clear(buffer, 0, 14);
				WriteBE(buffer, 0, 8, 0xff);

				tMui.Write(buffer, 0, 14);
			}
		}
		
		#endregion
		
		private static uint FrameCountFromTime(TimeSpan syncOffset, BdViFrameRate rate)
		{
			return Convert.ToUInt32(rate.ToDouble() * syncOffset.TotalSeconds);
		}

		private static TimeSpan TimeFromFrameCount(int frames, BdViFrameRate rate)
		{
			return TimeSpan.FromSeconds(frames / rate.ToDouble());
		}

		#endregion Compiling

		#region IDisposable

		private bool isDisposed = false;

		private void AssertNotDisposed()
		{
			isDisposed.FalseOrThrow(new ObjectDisposedException("ProjectCompiler"));
		}

		private static void Dispose<T>(ref T managedRes)
			where T : class, IDisposable
		{
			if (managedRes != null)
			{
				managedRes.Dispose();
				managedRes = null;
			}
		}

		public void Dispose()
		{
			if (!isDisposed)
			{
				lock (lkMuxer)
				{
					Dispose(ref pesMuxer);
					Dispose(ref muxProc);
				}
				Dispose(ref s2pEncoder);

				isDisposed = true;
			}

			GC.SuppressFinalize(this);
		}

		#endregion

		#region S2P callbacks

		class MyS2PSettings : IS2PSettings
		{
			private readonly CompilingSettings settings;

			public MyS2PSettings(CompilingSettings settings)
			{
				this.settings = settings;
			}

			public bool IsForcingTmtCompat { get { return false; } }

			public ulong MaxCachingSize { get { return 1UL << 32; } }

			public int MaxImageBlockSize { get { return 0; } }

			public string TempOutputPath { get { return settings.TempDir.FullName; } }
		}
		
		class MyS2PLogger : IS2PLogger
		{
			private readonly ICompilingLogger logger;

			public MyS2PLogger(ICompilingLogger logger)
			{
				this.logger = logger;
			}

			public void Vlog(int level, string msg)
			{
				logger.Log(level, msg);
			}
		}

		#endregion
	}
}
