using Unity.Entities;

namespace NumPzl
{
	public struct InitBlockInfo : IComponentData
	{
		public bool Initialized;
		public int PreIdx;
	}
}
