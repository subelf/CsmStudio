using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PesMuxer;
using Spp2PgsNet;

namespace CsmStudio.ProjectManager.Compile
{
	class CompilingProgressManager : PesMuxer.IProgressReporter, ICompilingProgressManager
	{
		private ICompilingProgressReporter reporter;

		public CompilingProgressManager(ICompilingProgressReporter reporter, int numClips)
		{
			this.reporter = reporter;
			this.threadLimit = new Semaphore(0, Environment.ProcessorCount);
		}

		private const float ProgressAmountScaled = 100f;
		private const float PgsProgressAmountScaled = 50f;
		private const float MuxProgressAmountScaled = ProgressAmountScaled - PgsProgressAmountScaled;
		private const float MuxProgressRate = MuxProgressAmountScaled / ProgressAmountScaled;
		private float MuxProgressAmount = 100f;

		#region PgsEncoding

		private object lkPgsProgress = new object();
		private Semaphore threadLimit;

		private int numPgses;
		private int numPgsesDone;

		private int pgsAmount = 0;
		private int pgsProgress = 0;

		public void PgsUpdateProgress()
		{
			lock (lkPgsProgress)
			{
				reporter.Amount = ProgressAmountScaled;
				if (numPgses == 0)
				{
					reporter.Progress = 0;
				}
				else
				{
					float PgsDoneAmountScaled = numPgsesDone * PgsProgressAmountScaled / numPgses;
					if (pgsAmount != 0)
					{
						float PgsInProgressScaled = pgsProgress * this.pgsProgressReporters.Count * PgsProgressAmountScaled / numPgses / pgsAmount;
						PgsDoneAmountScaled += PgsInProgressScaled;
					}
					reporter.Progress = PgsDoneAmountScaled;
				}
			}
		}

		public PgsProgressReporter CreatePgsProgressReporter()
		{
			return new PgsProgressReporter(this);
		}

		private IList<PgsProgressReporter> pgsProgressReporters =
			new List<PgsProgressReporter>();


		void ICompilingProgressManager.Register(PgsProgressReporter reporter)
		{
			threadLimit.WaitOne();
			lock (lkPgsProgress)
			{
				pgsProgressReporters.Add(reporter);

				this.PgsUpdateProgress();
			}
		}

		void ICompilingProgressManager.Unregister(PgsProgressReporter reporter)
		{
			threadLimit.Release();
			lock (lkPgsProgress)
			{
				pgsProgressReporters.Remove(reporter);

				pgsAmount -= reporter.Amount;
				pgsProgress -= reporter.Progress;
				numPgsesDone++;

				this.PgsUpdateProgress();
			}
		}

		bool ICompilingProgressManager.IsCanceled
		{
			get { return reporter.IsCanceled; }
		}

		void ICompilingProgressManager.AmountUpdated(PgsProgressReporter reporter, int increasement)
		{
			lock (lkPgsProgress)
			{
				pgsAmount += increasement;

				this.PgsUpdateProgress();
			}
		}

		void ICompilingProgressManager.ProgressUpdated(PgsProgressReporter reporter, int increasement)
		{
			lock (lkPgsProgress)
			{
				pgsProgress += increasement;

				this.PgsUpdateProgress();
			}
		}

		void ICompilingProgressManager.TaskEnd(PgsProgressReporter reporter)
		{
			return;
		}

		#endregion

		#region PesMuxing

		float PesMuxer.IProgressReporter.Amount
		{
			set
			{
				MuxProgressAmount = value;
				reporter.Amount = ProgressAmountScaled;
			}
		}

		bool PesMuxer.IProgressReporter.IsCanceled
		{
			get
			{
				return reporter.IsCanceled;
			}
		}

		float PesMuxer.IProgressReporter.Progress
		{
			set
			{
				reporter.Progress =
					PgsProgressAmountScaled +
					value * (MuxProgressRate * MuxProgressAmountScaled) / MuxProgressAmount;
			}
		}

		void PesMuxer.IProgressReporter.OnTaskEnd()
		{
			this.reporter.OnTaskEnd();
		}

		#endregion
		
	}

}
