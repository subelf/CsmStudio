namespace CsmStudio.ProjectManager.Compile
{
	public interface ICompilingLogger
	{
		void Log(int level, string fmt, params object[] param);
	}
}