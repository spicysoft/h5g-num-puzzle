using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;

namespace NumPzl
{
	public class FirstSetSystem : ComponentSystem
	{
		// 初期配置数.
		public const int FirstBlockNum = 6;

		protected override void OnUpdate()
		{
			bool isGenerate = false;
			Entities.ForEach( ( ref FirstSetInfo info ) => {
				if( !info.Initialized ) {
					info.Initialized = true;
					isGenerate = true;
				}
			} );


			//Entity blkEntity = Entity.Null;
			if( isGenerate ) {
				// 初期配置ブロック生成.
				var env = World.TinyEnvironment();
				for( int i = 0; i < FirstBlockNum; ++i ) {
					SceneService.LoadSceneAsync( env.GetConfigData<GameConfig>().PrefabBlockStay );
				}
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
