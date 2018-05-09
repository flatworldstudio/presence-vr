using UnityEngine;
using UnityEngine.Events;

namespace CloudStorage{

	public class CS_Events{
		public static string FILE_COMPLETED = "file_completed";
		public static string FILE_ERROR = "file_error";
	}

	[System.Serializable]
	public class CS_Event : UnityEvent<string> {}
	public class CS_Internal_Event : UnityEvent<string> {}
	public class CS_TextFile_Event : UnityEvent<string,long> {}
	public class CS_BinaryFile_Event : UnityEvent<byte[]> {}
	public class CS_Exist_Event : UnityEvent<string , bool> {}

	[System.Serializable]
	public class CS_Progress_Event : UnityEvent <string , string , int, int> {
		// Filename , status , total, downloaded , error
	}


	[System.Serializable]
	public class CS_BinaryProgress_Event : UnityEvent <float> {
		// downloadprogress
	}

	[System.Serializable]
	public class CS_Complete_Event : UnityEvent<int, int> {
		// downloaded , error
	}
}