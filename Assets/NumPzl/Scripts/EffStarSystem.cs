using Unity.Collections;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;

namespace NumPzl
{
	public class EffStarSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			NativeArray<Entity> delAry = new NativeArray<Entity>( 6, Allocator.Temp );
			for( int i = 0; i < 6; ++i ) {
				delAry[i] = Entity.Null;
			}
			int delCnt = 0;

			Entities.ForEach( ( Entity entity, ref EffStarInfo eff, ref Sprite2DSequencePlayer seq ) => {
				if( !eff.Initialized ) {
					//eff.Initialized = true;

					return;
				}

				eff.Timer += World.TinyEnvironment().frameDeltaTime;
				if( eff.Timer > 0.8f ) {
					delAry[delCnt++] = entity;
				}

				/*Debug.LogFormatAlways("t {0}", seq.time);
				if( seq.time > 1f ) {
					Debug.LogAlways("seq end");
				}*/
			} );


			for( int i = 0; i < delCnt; ++i ) {
				if( delAry[i] != Entity.Null ) {
					// エンティティ削除.
					Debug.LogAlways("eff del");
					SceneService.UnloadSceneInstance( delAry[i] );
				}
			}

			delAry.Dispose();

		}
	}
}
