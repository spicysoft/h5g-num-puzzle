using Unity.Entities;
using Unity.Tiny.Scenes;

namespace NumPzl
{
	public struct GameConfig : IComponentData
	{
		public SceneReference PrefabBlock;
		public SceneReference PrefabBlockStay;
	}
}
