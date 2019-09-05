using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;
using Unity.Tiny.UIControls;

namespace NumPzl
{
	// リトライボタン.
	public class BtnRetrySystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool btnOn = false;
			Entities.WithAll<BtnRetryTag>().ForEach( ( Entity entity, ref PointerInteraction pointerInteraction ) => {
				if( pointerInteraction.clicked ) {
					//Debug.LogAlways("btn ret click");
					btnOn = true;
				}
			} );


			if( btnOn ) {
				var env = World.TinyEnvironment();
				SceneService.UnloadAllSceneInstances( env.GetConfigData<GameConfig>().PrefabBlock );
				SceneService.UnloadAllSceneInstances( env.GetConfigData<GameConfig>().PrefabBlockStay );
				SceneService.UnloadAllSceneInstances( env.GetConfigData<GameConfig>().PrefabStar );

				SceneService.UnloadAllSceneInstances( env.GetConfigData<GameConfig>().ResultScn );


				// 各種パラメータ初期化.
				Entities.ForEach( ( ref FirstSetInfo info ) => {
					info.Initialized = false;
				} );

				Entities.ForEach( ( ref InitBlockInfo info ) => {
					info.Initialized = false;
				} );

				// ポーズ解除 & 初期化.
				Entities.ForEach( ( ref GameMngr mngr ) => {
					mngr.IsPause = false;
					mngr.Mode = GameMngrSystem.MdGame;
					mngr.Score = 0;
					mngr.GameTimer = 0;
					mngr.ModeTimer = 0;
				} );

				Entities.ForEach( ( ref GeneratorInfo info ) => {
					info.Initialized = false;
				} );


				// スコア表示.
				Entities.WithAll<TextScoreTag>().ForEach( ( Entity entity ) => {
					EntityManager.SetBufferFromString<TextString>( entity, "0" );
				} );

			}
		}
	}
}
