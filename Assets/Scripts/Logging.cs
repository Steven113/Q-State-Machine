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

		}

		public void Log(string str){
			File.AppendAllText (fileName, str+Environment.NewLine);
		}

		public void OnDestroy(){
			globalLogger = null;
		}
	}
}

