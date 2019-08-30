using Unity.Entities;

namespace NumPzl
{
	public struct GeneratorInfo : IComponentData
	{
		public bool Initialized;
		public bool Request;
		public float Timer;				// タイマー.
		public float IntvlTime;			// インターバル.
		public float TimeDifference;	// 時間差.
		public int GenerateNum;			// 生成する個数.
		public int GenCnt;				// 生成した個数.
		public int Status;				// ステータス.
	}
}
