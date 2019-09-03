using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;
using Unity.Tiny.UIControls;

namespace NumPzl
{
	public class BtnRetrySystem : ComponentSystem
	{
//		bool isLoaded;


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

				//SceneReference scn = new SceneReference();
				//scn = env.GetConfigData<GameConfig>().ResultScn;
				SceneService.UnloadAllSceneInstances( env.GetConfigData<GameConfig>().ResultScn );


				Entities.ForEach( ( ref FirstSetInfo info ) => {
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



#if false
				// 盤面リフレッシュ.
				var env = World.TinyEnvironment();
				SceneService.UnloadAllSceneInstances( env.GetConfigData<PanelConfig>().PanelRed );
				SceneService.UnloadAllSceneInstances( env.GetConfigData<PanelConfig>().PanelWhite );

				Entities.ForEach( ( ref PuzzleGen gen ) => {
					gen.IsGenerate = true;
				} );
				SceneReference panelBase = new SceneReference();
				panelBase = env.GetConfigData<PanelConfig>().ResultScn;
				SceneService.UnloadAllSceneInstances( panelBase );

				// ポーズ解除 & 初期化.
				Entities.ForEach( ( ref GameMngr mngr ) => {
					mngr.IsPause = false;
					mngr.Score = 0;
					mngr.GameTimer = 0;
					mngr.InputCnt = 0;
					mngr.InputCntGoal = 0;
					mngr.ComboCnt = 0;
				} );
#endif

				// スコア表示.
				Entities.WithAll<TextScoreTag>().ForEach( ( Entity entity ) => {
					EntityManager.SetBufferFromString<TextString>( entity, "0" );
				} );

			}
		}
	}
}
