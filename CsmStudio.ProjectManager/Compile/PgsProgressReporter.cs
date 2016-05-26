using System;
using Spp2PgsNet;

namespace CsmStudio.ProjectManager.Compile
{
	internal class PgsProgressReporter : IProgressReporter, IDisposable
	{
		private readonly ICompilingProgressManager progressManager;

		public PgsProgressReporter(ICompilingProgressManager progressManager)
		{
			(this.progressManager = progressManager).Register(this);
		}

		private int amount = 0;

		public int Amount
		{
			get { return amount; }
			set
			{
				var inc = value - amount;
				amount = value;
				progressManager.AmountUpdated(this, inc);
			}
		}

		public bool IsCanceled
		{
			get { return progressManager.IsCanceled; }
		}

		private int progress;

		public int Progress
		{
			get { return progress; }
			set
			{
				var inc = value - progress;
				progress = value;
				progressManager.ProgressUpdated(this, inc);
			}
		}

		public void OnTaskEnd()
		{
			progressManager.TaskEnd(this);
		}

		private bool isDisposed = false;
		public void Dispose()
		{
			if(!isDisposed)
			{
				progressManager.Unregister(this);

				this.isDisposed = true;
			}

			GC.SuppressFinalize(this);
		}
	}
}