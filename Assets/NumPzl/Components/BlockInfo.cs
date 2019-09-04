using Unity.Entities;
using Unity.Mathematics;

namespace NumPzl
{
	public struct BlockInfo : IComponentData
	{
		public bool Initialized;    // 初期化したか.
		public bool IsStayFirst;    // 初期状態Stayか.
		public bool EffGen;			// エフェクト生成するか.
		public int2 CellPos;		// 現在のセル単位の位置.
		public int2 NextPos;        // 移動先セル.
		public int Num;             // 数値.
		public int Status;			// 状態.
		public float Timer;			// タイマー.
	}
}
