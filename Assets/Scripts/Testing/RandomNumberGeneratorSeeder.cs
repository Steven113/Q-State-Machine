using System;

namespace AssemblyCSharp
{
	/// <summary>
	/// User to seed the random number generator to allow consistent experiment results
	/// </summary>
	public static class RandomNumberGeneratorSeeder
	{
		static RandomNumberGeneratorSeeder ()
		{
			//UnityEngine.Random.InitState (3354);
			//UnityEngine.Random.InitState (200);
			//UnityEngine.Random.InitState (9131548);
			//UnityEngine.Random.InitState (746453);
			UnityEngine.Random.InitState (23248427);
		}
	}
}

