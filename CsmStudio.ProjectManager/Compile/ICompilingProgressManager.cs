using Spp2PgsNet;

namespace CsmStudio.ProjectManager.Compile
{
	internal interface ICompilingProgressManager
	{
		void Register(PgsProgressReporter reporter);
		void Unregister(PgsProgressReporter reporter);
		void AmountUpdated(PgsProgressReporter reporter, int increasement);
		void ProgressUpdated(PgsProgressReporter reporter, int increasement);
		void TaskEnd(PgsProgressReporter reporter);
		bool IsCanceled { get; }
	}
}