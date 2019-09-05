using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Input;
using Unity.Tiny.Scenes;

namespace NumPzl
{
	/// <summary>
	/// 落ちてくるブロック生成.
	/// </summary>
	public class GeneratorSystem : ComponentSystem
	{
		public const int StNorm = 0;
		public const int StGenerate = 1;
		public const float IntervalTime = 2f;
		public const float TimeForAdjust = 1f;  // 調整用.
		public const float GenTimeDifference = 0.7f;    // ブロック連続生成の時間差.

		protected override void OnUpdate()
		{
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
						// 初期化.
						info.Initialized = true;
						info.IntvlTime = IntervalTime;
						info.GenerateNum = 1;
						info.Timer = TimeForAdjust;
						info.GenCnt = 0;
						info.Status = StNorm;
						return;
					}

					float dt = World.TinyEnvironment().frameDeltaTime;

					if( info.Status == StNorm ) {
						info.Timer += dt;
						if( info.Timer > info.IntvlTime ) {
							info.Timer = 0;
							info.Status = StGenerate;
							info.GenCnt = 0;
							CheckGenerateNum( ref info, gameTime );
							genNum = info.GenerateNum;
						}
					}
					else if( info.Status == StGenerate ) {
						// 連続的に生成.
						info.TimeDifference -= dt;
						if( info.TimeDifference <= 0 ) {
							if( info.GenCnt < info.GenerateNum ) {
								isRequest = true;
								info.TimeDifference = GenTimeDifference;
								if( ++info.GenCnt >= info.GenerateNum ) {
									info.Status = StNorm;
									info.GenCnt = 0;
								}
							}
						}
					}
				} );


				if( isRequest ) {
					// ブロック生成.
					var env = World.TinyEnvironment();
					SceneReference blockBase = new SceneReference();
					blockBase = env.GetConfigData<GameConfig>().PrefabBlock;
					SceneService.LoadSceneAsync( blockBase );
				}
			}
		}

		// 生成するブロックの数.
		void CheckGenerateNum( ref GeneratorInfo info, float gameTime )
		{
			if( gameTime > 80f ) {
				if( info.GenerateNum == 4 )
					++info.GenerateNum;
			}
			if( gameTime > 60f ) {
				if( info.GenerateNum == 3 )
					++info.GenerateNum;
			}
			if( gameTime > 40f ) {
				if( info.GenerateNum == 2 )
					++info.GenerateNum;
			}
			else if( gameTime > 20f ) {
				if( info.GenerateNum == 1 )
					++info.GenerateNum;
			}
		}

	}
}
