using System;
using UnityEngine;
using System.IO;

namespace AssemblyCSharp
{
	/// <summary>
	/// Monobehaviour that logs to a given file
	/// </summary>
	public class Logging : MonoBehaviour
	{
		/// <summary>
		/// reference to currently active logger
		/// </summary>
		public static Logging globalLogger; 

		/// <summary>
		/// Base name of file to log to. The date will be appended to this file name
		/// </summary>
		public string fileName;

		public void Start(){
			

			globalLogger = this; //set this logger instance to be the global logger
			fileName = fileName + "_" +DateTime.Now.Ticks; //generate filename

		}

		public void Log(string str){
			File.AppendAllText (fileName, str+Environment.NewLine);
		}

		public void Log(string file_name, string str, bool addNewline){
			File.AppendAllText (file_name, str+((addNewline)?Environment.NewLine:""));
		}

		/// <summary>
		/// Destructor of script
		/// </summary>
		public void OnDestroy(){
			globalLogger = null;
		}
	}
}

