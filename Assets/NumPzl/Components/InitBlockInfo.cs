using Unity.Entities;

namespace NumPzl
{
	public struct InitBlockInfo : IComponentData
	{
		public bool Initialized;	// 初期配置ブロック初期化したか.
		public int PreIdx;			// 前回セットしたインデックス(落ちるブロック用).
	}
}
