using Unity.Entities;

namespace NumPzl
{
	public struct GeneratorInfo : IComponentData
	{
		public bool Initialized;
		public bool Request;
		public float Timer;
		public float IntvlTime;
		public float TimeDiferrence;
		public int GenerateNum;
		public int Status;
	}
}
