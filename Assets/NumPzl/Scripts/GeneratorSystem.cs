using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Input;
using Unity.Tiny.Scenes;

namespace NumPzl
{
	public class GeneratorSystem : ComponentSystem
	{
		public const int StNorm = 0;
		public const int StGenerate = 1;

		protected override void OnUpdate()
		{
#if false
			bool isRequest = false;
			Entities.ForEach( ( ref GeneratorInfo info ) => {
				if( info.Request ) {
					info.Request = false;
					isRequest = true;
				}
			} );

			if( isRequest ) {
				var env = World.TinyEnvironment();
				SceneReference blockBase = new SceneReference();

				blockBase = env.GetConfigData<GameConfig>().PrefabBlock;

				SceneService.LoadSceneAsync( blockBase );
			}
#endif

#if false
			bool mouseOn = false;
			var inputSystem = World.GetExistingSystem<InputSystem>();
			mouseOn = inputSystem.GetKeyDown( KeyCode.A );
			if( mouseOn ) {
				var env = World.TinyEnvironment();
				SceneReference blockBase = new SceneReference();

				blockBase = env.GetConfigData<GameConfig>().PrefabBlock;

				SceneService.LoadSceneAsync( blockBase );

			}
#endif

			bool isRequest = false;
			bool isPause = false;
			float gameTime = 0;
			int genNum = 0;
			Entities.ForEach( ( ref GameMngr mngr ) => {
				gameTime = mngr.GameTimer;
				isPause = mngr.IsPause;
			} );

			if( !isPause ) {

				Entities.ForEach( ( ref GeneratorInfo info ) => {
					if( !info.Initialized ) {
						info.Initialized = true;
						info.IntvlTime = 3f;
						info.GenerateNum = 1;
						return;
					}

					float dt = World.TinyEnvironment().frameDeltaTime;

					info.Timer += dt;
					if( info.Timer > info.IntvlTime ) {
						info.Timer = 0;
						info.Status = StGenerate;
						//isRequest = true;
						CheckGenerateNum( ref info, gameTime );
						genNum = info.GenerateNum;
					}

					if( info.Status == StGenerate ) {
						info.TimeDiferrence -= dt;
						if( info.TimeDiferrence <= 0 ) {
							if( info.GenerateNum > 0 ) {
								isRequest = true;
								info.TimeDiferrence = 0.6f;
								--info.GenerateNum;
							}
							else {
								info.Status = StNorm;
							}
						}
					}
				} );


				if( isRequest ) {
					var env = World.TinyEnvironment();
					SceneReference blockBase = new SceneReference();

					blockBase = env.GetConfigData<GameConfig>().PrefabBlock;

					//for( int i = 0; i < 4/*genNum*/; ++i ) {
						SceneService.LoadSceneAsync( blockBase );
					//}
				}
			}
		}

		// 生成するブロックの数.
		void CheckGenerateNum( ref GeneratorInfo info, float gameTime )
		{
			info.GenerateNum = 3;
#if false
			if( gameTime > 60f ) {
				if( info.GenerateNum == 3 )
					++info.GenerateNum;
			}
			else if( gameTime > 40f ) {
				if( info.GenerateNum == 2 )
					++info.GenerateNum;
			}
			else if( gameTime > 20f ) {
				if( info.GenerateNum == 1 )
					++info.GenerateNum;
			}
#endif
		}

	}
}
