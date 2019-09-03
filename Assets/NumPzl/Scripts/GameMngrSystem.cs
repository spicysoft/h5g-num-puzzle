using Unity.Entities;
using Unity.Tiny.Debugging;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Input;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;

namespace NumPzl
{
	public class GameMngrSystem : ComponentSystem
	{
		public const float GameTimeLimit = 190f;        // ゲーム時間.
		public const int MdTitle = 0;
		public const int MdGame = 1;
		public const int MdGameOver = 2;
		public const int MdResult = 3;


		protected override void OnUpdate()
		{
#if false
			bool isTitleFinished = false;
			Entities.ForEach( ( ref GameMngr mngr ) => {
				isTitleFinished = mngr.IsTitleFinished;
				if( !isTitleFinished ) {
					mngr.IsTitleFinished = true;
					mngr.IsPause = true;
				}
			} );

			if( !isTitleFinished ) {
				SceneReference panelBase = new SceneReference();
				panelBase = World.TinyEnvironment().GetConfigData<PanelConfig>().TitleScn;
				SceneService.LoadSceneAsync( panelBase );
				return;
			}
#endif

			float timer = 0;
			int score = 0;
			//bool isEnd = false;
			bool isPause = false;
			bool reqGameOver = false;
			bool reqResult = false;

			Entities.ForEach( ( ref GameMngr mngr ) => {

				float dt = World.TinyEnvironment().frameDeltaTime;

				switch( mngr.Mode ) {
				case MdTitle:
					mngr.Mode = MdGame;
					break;
				case MdGame:
					if( mngr.ReqGameOver ) {
						mngr.ReqGameOver = false;
						reqGameOver = true;
						mngr.Mode = MdGameOver;
						mngr.ModeTimer = 0;
					}
					break;
				case MdGameOver:
					mngr.ModeTimer += dt;
					if( mngr.ModeTimer > 1.5f ) {
						mngr.Mode = MdResult;
						reqResult = true;
					}
					break;

				}


				if( mngr.IsPause ) {
					isPause = true;
					return;
				}


				score = mngr.Score;

				// タイマー.
				mngr.GameTimer += dt;
				timer = mngr.GameTimer;
				if( timer >= GameTimeLimit ) {
					//isEnd = true;
					//mngr.GameTimer = 0;
					mngr.IsPause = true;
				}
			} );


#if false
			if( isEnd ) {
				// リザルト表示.
				SceneReference panelBase = new SceneReference();
				panelBase = World.TinyEnvironment().GetConfigData<PanelConfig>().ResultScn;
				SceneService.LoadSceneAsync( panelBase );
			}
#endif

#if false
			// タイマー表示.
			if( !isPause ) {
				Entities.WithAll<TextTimerTag>().ForEach( ( Entity entity ) => {
					int t = (int)( GameTimeLimit - timer );
					EntityManager.SetBufferFromString<TextString>( entity, t.ToString() );
				} );
			}
#endif

			if( reqResult ) {
				// ゲームオーバーシーンアンロード.
				SceneReference panelBase = new SceneReference();
				panelBase = World.TinyEnvironment().GetConfigData<GameConfig>().GameOverScn;
				SceneService.UnloadAllSceneInstances( panelBase );
				// リザルト表示.
				//SceneReference panelBase = new SceneReference();
				panelBase = World.TinyEnvironment().GetConfigData<GameConfig>().ResultScn;
				SceneService.LoadSceneAsync( panelBase );
			}
			else if( reqGameOver ) {
				// ゲームオーバー表示.
				SceneReference panelBase = new SceneReference();
				panelBase = World.TinyEnvironment().GetConfigData<GameConfig>().GameOverScn;
				SceneService.LoadSceneAsync( panelBase );
			}

		}

	}
}
