using Unity.Entities;

namespace NumPzl
{
	public struct InitBlockInfo : IComponentData
	{
		public bool Initialized;
		public int PreIdx;			// 前回セットしたインデックス(落ちるブロック用).
	}
}
