using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Tiny.Input;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;

namespace NumPzl
{
	/// <summary>
	/// パネルの挙動.
	/// セルは左下が0,0
	/// </summary>
	[UpdateAfter( typeof( InitBlockSystem ) )]
	public class BlockSystem : ComponentSystem
	{
		public const int BlkStAppear = 0;
		public const int BlkStStay = 1;
		public const int BlkStPrepare = 2;	// 準備.
		public const int BlkStMove = 3;
		public const int BlkStDisappear = 4;
		public const int BlkStEnd = 5;


		protected override void OnUpdate()
		{
			Entity delEntity = Entity.Null;
			var inputSystem = World.GetExistingSystem<InputSystem>();

			bool IsPause = false;
			Entities.ForEach( ( ref GameMngr mngr ) => {
				if( mngr.IsPause ) {
					IsPause = true;
				}
			} );
			if( IsPause )
				return;


			bool mouseOn = false;
			mouseOn = inputSystem.GetMouseButtonDown( 0 );

			// ゲームタイム.
			float gameTime = 0;
			Entities.ForEach( ( ref GameMngr mngr ) => {
				gameTime = mngr.GameTimer;
			} );

			// 盤面情報収集.
			// 盤面情報用配列.
			NativeArray<Entity> infoAry = new NativeArray<Entity>( 6*8, Allocator.Temp );
			for( int i = 0; i < 6*8; ++i ) {
				infoAry[i] = Entity.Null;
			}


			Entities.ForEach( ( Entity entity, ref BlockInfo block ) => {
				if( !block.Initialized ) {
					return;
				}
				// 情報.
				int idx = block.CellPos.x + block.CellPos.y * 6;
				infoAry[idx] = entity;
			} );


			NativeArray<Entity> delAry = new NativeArray<Entity>( 6, Allocator.Temp );
			for( int i = 0; i < 6; ++i ) {
				delAry[i] = Entity.Null;
			}
			int delCnt = 0;
			int effReqCnt = 0;		// エフェクトリクエスト数.

			Entities.ForEach( ( Entity entity, ref BlockInfo block, ref Translation trans ) => {
				// 状態チェック.
				//if( !panel.Initialized || panel.Status != PnlStNormal ) {
				//	isReadyToMove = false;
				//	return;
				//}


				switch( block.Status ) {
				case BlkStPrepare:
					prepareMove( ref entity, ref block, ref trans, ref infoAry );
					break;
				case BlkStMove:
					float vel = getBlockVelocity( gameTime );
					blockMove( ref entity, ref block, ref trans, ref infoAry, vel, ref effReqCnt );
					break;
				case BlkStDisappear:
					block.Timer += World.TinyEnvironment().frameDeltaTime;
					if( block.Timer > 0.2f ) {
						delAry[delCnt++] = entity;
						block.Status = BlkStEnd;

						float3 effpos = trans.Value;
						/*
						// エフェクト.
						Entities.ForEach( ( ref EffStarMngr mngr ) => {
							mngr.Requested = true;
							mngr.xpos = effpos.x;
							mngr.ypos = effpos.y;
						} );
						*/
					}
					break;
				}

				//EntityManager.SetBufferFromString<TextString>( entity, block.Status.ToString() );

				// マウスとのあたりチェック.
				if( mouseOn && block.Status == BlkStMove ) {
					float2 size = new float2( InitBlockSystem.BlkSize, InitBlockSystem.BlkSize );

					float3 mypos = trans.Value;
					float3 mousePos = inputSystem.GetWorldInputPosition();
					bool res = OverlapsObjectCollider( mypos, mousePos, size );
					if( res ) {
						//Debug.LogAlways( "hit" );
						if( ++block.Num > 9 )
							block.Num = 1;
						EntityManager.SetBufferFromString<TextString>( entity, block.Num.ToString() );
					}
				}

			} );

			infoAry.Dispose();

			// ブロック削除.
			for( int i = 0; i < 6; ++i ) {
				if( delAry[i] != Entity.Null ) {
					// エンティティ削除.
					SceneService.UnloadSceneInstance( delAry[i] );
				}
			}
			delAry.Dispose();

			// エフェクト生成.
			var env = World.TinyEnvironment();
			for( int i = 0; i < effReqCnt; ++i ) {
				SceneService.LoadSceneAsync( env.GetConfigData<GameConfig>().PrefabStar );
			}

		}

		void prepareMove( ref Entity entity, ref BlockInfo block, ref Translation trans, ref NativeArray<Entity> infoAry )
		{
			// 底.
			int btmY = -1;
			Entity btmEntity = Entity.Null;
			for( int j = 0; j < 7; ++j ) {
				int idx = block.CellPos.x + j * 6;
				if( infoAry[idx] != Entity.Null ) {
					BlockInfo blk = EntityManager.GetComponentData<BlockInfo>( infoAry[idx] );
					if( blk.Status == BlkStStay ) {
						btmY = j;
						btmEntity = infoAry[idx];
					}
				}
				else {
					break;
				}
			}

			if( btmY == block.CellPos.y - 1 ) {		// てっぺん?
				// 下のブロック.
				BlockInfo btmBlk = EntityManager.GetComponentData<BlockInfo>( btmEntity );
				if( block.Num + btmBlk.Num != 10 ) {
					gameOverRequest();
					Debug.LogAlways("GAME OVER");
				}
				else {
					// 消す.
					block.Status = BlkStDisappear;
					// 下のブロック書き換え.
					btmBlk.Status = BlkStDisappear;
					EntityManager.SetComponentData( btmEntity, btmBlk );
					// スコア.
					addScore();
				}
			}
			else {
				block.Status = BlkStMove;
			}
		}

		void blockMove( ref Entity entity, ref BlockInfo block, ref Translation trans, ref NativeArray<Entity> infoAry, float vel, ref int effReqCnt )
		{
			// 底.
			int btmY = -1;
			Entity btmEntity = Entity.Null;
			for( int j = 0; j < InitBlockSystem.BlkVNum; ++j ) {
				int idx = block.CellPos.x + j * 6;
				if( infoAry[idx] != Entity.Null ) {
					BlockInfo blk = EntityManager.GetComponentData<BlockInfo>( infoAry[idx] );
					if( blk.Status == BlkStStay ) {
						btmY = j;
						btmEntity = infoAry[idx];
					}
				}
				else {
					break;
				}
			}


			/*if( btmY == block.CellPos.y ) {
				//Debug.LogAlways("GAME OVER");
				gameOverRequest();
				return;
			}*/

			float dt = World.TinyEnvironment().frameDeltaTime;
			float3 pos = trans.Value;
			pos.y -= vel * dt;

			float tarY = InitBlockSystem.OrgY + InitBlockSystem.BlkSize*(btmY+1);
			if( pos.y <= tarY ) {
				pos.y = tarY;
				if( btmEntity != Entity.Null ) {
					// 下のブロック.
					BlockInfo btmBlk = EntityManager.GetComponentData<BlockInfo>( btmEntity );
					// 数字判定.
					if( block.Num + btmBlk.Num == 10 ) {
						block.Status = BlkStDisappear;
						// 下のブロック書き換え.
						btmBlk.Status = BlkStDisappear;
						// スコア.
						addScore();
						// エフェクトリクエスト.
						++effReqCnt;
						btmBlk.EffGen = true;
						EntityManager.SetComponentData( btmEntity, btmBlk );	// コンポーネント更新.
					}
					else {
						block.Status = BlkStStay;
					}
				}
				else {
					block.Status = BlkStStay;
				}
			}

			block.CellPos.y = (int)( ( pos.y + 0.5f*InitBlockSystem.BlkSize - InitBlockSystem.OrgY ) / InitBlockSystem.BlkSize );

			trans.Value = pos;
		}

		float getBlockVelocity( float gameTime )
		{
			float vel = 60f;

			float t = gameTime / 10f;
			vel += t * 1.2f;

			if( vel > 90f )
				vel = 90f;

			return vel;
		}

		void gameOverRequest()
		{
			Entities.ForEach( ( ref GameMngr mngr ) => {
				mngr.IsPause = true;
				mngr.ReqGameOver = true;
			} );
		}

		void addScore()
		{
			int score = 0;
			Entities.ForEach( ( Entity entity, ref GameMngr mngr ) => {
				mngr.Score += 100;
				score = mngr.Score;
			} );

			if( score != 0 ) {
				Entities.WithAll<TextScoreTag>().ForEach( ( Entity scoreEntity ) => {
					EntityManager.SetBufferFromString<TextString>( scoreEntity, score.ToString() );
				} );
			}
		}


		bool OverlapsObjectCollider( float3 position, float3 inputPosition, float2 size )
		{
			var rect = new Rect( position.x - size.x * 0.5f, position.y - size.y * 0.5f, size.x, size.y );
			return rect.Contains( inputPosition.xy );
		}
	}
}
