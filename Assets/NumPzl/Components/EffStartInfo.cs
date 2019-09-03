using Unity.Entities;
using Unity.Mathematics;

namespace NumPzl
{
	public struct EffStarInfo : IComponentData
	{
		public bool Initialized;    // 初期化したか.

		public float Timer;         // タイマー.
		//public float[] pos;
	}
}
