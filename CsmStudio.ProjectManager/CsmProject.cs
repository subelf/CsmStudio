using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsmStudio.ProjectManager
{
	public class CsmProject
	{
		public CsmProject(FileInfo mplsFile)
		{
			//load single-mlps-project

		}

		public CsmProject(DirectoryInfo directory)
		{
			if(directory.Name == "BDMV")
			{
				//load single-BDMV-project
			}
			else
			{
				//recursively add all BDMV directory
			}
		}

		
	}
}
