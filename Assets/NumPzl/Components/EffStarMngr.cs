using Unity.Entities;

namespace NumPzl
{
	public struct EffStarMngr : IComponentData
	{
		public bool Requested;
		public float xpos;
		public float ypos;
	}
}
