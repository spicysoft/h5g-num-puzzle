using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Input;
using Unity.Tiny.Scenes;

namespace NumPzl
{
	public class GeneratorSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool isRequest = false;
			Entities.ForEach( ( ref GeneratorInfo info ) => {
				if( info.Request ) {
					info.Request = false;
					isRequest = true;
				}
			} );

			if( isRequest ) {

			}


			bool mouseOn = false;
			var inputSystem = World.GetExistingSystem<InputSystem>();
			mouseOn = inputSystem.GetMouseButtonDown( 1 );
			if( mouseOn ) {
				var env = World.TinyEnvironment();
				SceneReference blockBase = new SceneReference();

				blockBase = env.GetConfigData<GameConfig>().PrefabBlock;

				SceneService.LoadSceneAsync( blockBase );

			}
		}
	}
}
