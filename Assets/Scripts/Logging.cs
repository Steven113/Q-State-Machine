using System;
using UnityEngine;
using System.IO;

namespace AssemblyCSharp
{
	public class Logging : MonoBehaviour
	{
		public static Logging globalLogger;

		public string fileName;

		public void Start(){
			

			globalLogger = this;
			fileName = fileName + "_" +DateTime.Now.Ticks;

		}

		public void Log(string str){
			File.AppendAllText (fileName, str+Environment.NewLine);
		}

		public void Log(string file_name, string str, bool addNewline){
			File.AppendAllText (file_name, str+((addNewline)?Environment.NewLine:""));
		}

		public void OnDestroy(){
			globalLogger = null;
		}
	}
}

