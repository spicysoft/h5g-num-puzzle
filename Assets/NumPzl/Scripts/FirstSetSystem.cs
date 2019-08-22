using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;

namespace NumPzl
{
	public class FirstSetSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool isGenerate = false;
			Entities.ForEach( ( ref FirstSetInfo info ) => {
				if( !info.Initialized ) {
					info.Initialized = true;
					isGenerate = true;
				}
			} );


			Entity blkEntity = Entity.Null;
			if( isGenerate ) {
				var env = World.TinyEnvironment();
				SceneReference blockBase = new SceneReference();

				blockBase = env.GetConfigData<GameConfig>().BlockScn;

				blkEntity = SceneService.LoadSceneAsync( blockBase );
				//SceneService.LoadSceneAsync( blockBase );
				//SceneService.LoadSceneAsync( blockBase );
			}

#if false
			if( blkEntity != Entity.Null ) {

				Debug.LogAlways("--------------");
				SceneStatus st = SceneService.GetSceneStatus( blkEntity );
				switch( st ) {
				case SceneStatus.Loading:
					Debug.LogAlways( "loading" );
					break;
				case SceneStatus.Loaded:
					Debug.LogAlways( "loaded" );
					break;

				}
				Debug.LogFormatAlways("{0}", (int)st);
			}
#endif
		}
	}
}