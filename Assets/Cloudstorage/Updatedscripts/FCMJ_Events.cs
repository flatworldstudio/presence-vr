using UnityEngine;
using UnityEngine.Events;

namespace FMCJ{

	public class FCMJ_Events{
		public static string FILE_COMPLETED = "filecompleted";
		public static string FILE_ERROR = "fileerror";
	}

	[System.Serializable]
	public class FCMJ_Event : UnityEvent<string> {}
	public class FCMJ_Internal_Event : UnityEvent<string> {}
	public class FCMJ_BinaryFile_Event : UnityEvent<byte[]> {}
	public class FCMJ_Exist_Event : UnityEvent<string , bool> {}

	[System.Serializable]
	public class FCMJ_Progress_Event : UnityEvent<string , string , int, int> {
		// Filename , status , downloaded , error
	}

	[System.Serializable]
	public class FCMJ_Complete_Event : UnityEvent<int, int> {
		// downloaded , error
	}
}