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
		public const int BlkStMove = 2;
		public const int BlkStDisappear = 3;


		protected override void OnUpdate()
		{
			Entity delEntity = Entity.Null;
			var inputSystem = World.GetExistingSystem<InputSystem>();

			bool mouseOn = false;
			//if( !isPause() ) {
				mouseOn = inputSystem.GetMouseButtonDown( 0 );
			//}


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


			NativeArray<Entity> delAry = new NativeArray<Entity>( 3, Allocator.Temp );
			for( int i = 0; i < 3; ++i ) {
				delAry[i] = Entity.Null;
			}
			int delCnt = 0;

			Entities.ForEach( ( Entity entity, ref BlockInfo block, ref Translation trans ) => {
				// 状態チェック.
				//if( !panel.Initialized || panel.Status != PnlStNormal ) {
				//	isReadyToMove = false;
				//	return;
				//}


				switch( block.Status ) {
				case BlkStMove:
					blockMove( ref entity, ref block, ref trans, ref infoAry );
					break;
				case BlkStDisappear:
					delAry[delCnt++] = entity;
					break;
				}

				//EntityManager.SetBufferFromString<TextString>( entity, block.Status.ToString() );
#if true
				// マウスとのあたりチェック.
				if( mouseOn && block.Status == BlkStMove ) {
					float2 size = new float2( InitBlockSystem.BlkSize, InitBlockSystem.BlkSize );

					float3 mypos = trans.Value;
					float3 mousePos = inputSystem.GetWorldInputPosition();
					bool res = OverlapsObjectCollider( mypos, mousePos, size );
					if( res ) {
						Debug.LogAlways( "hit" );
						if( ++block.Num > 9 )
							block.Num = 1;
						EntityManager.SetBufferFromString<TextString>( entity, block.Num.ToString() );
					}
				}
#endif
			} );

			infoAry.Dispose();

			for( int i = 0; i < 3; ++i ) {
				if( delAry[i] != Entity.Null ) {
					// エンティティ削除.
					SceneService.UnloadSceneInstance( delAry[i] );
				}
			}

			delAry.Dispose();

		}


		void blockMove( ref Entity entity, ref BlockInfo block, ref Translation trans, ref NativeArray<Entity> infoAry )
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

			float dt = World.TinyEnvironment().frameDeltaTime;
			float3 pos = trans.Value;
			pos.y -= 100f * dt;

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
						EntityManager.SetComponentData( btmEntity, btmBlk );
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

#if false
		bool isPause()
		{
			bool _isPause = false;
			Entities.ForEach( ( ref GameMngr mngr ) => {
				if( mngr.IsPause )
					_isPause = true;
			} );
			return _isPause;
		}

		void setPause( bool bPause )
		{
			Entities.ForEach( ( ref GameMngr mngr ) => {
				mngr.IsPause = bPause;
			} );
		}

		void panelNormNew( ref PanelInfo panel, ref NativeArray<int> infoAry, ref int2 hitCell, ref int2 blankCell, int dir )
		{
			if( dir == 0 ) { // 上.
				if( panel.CellPos.x == hitCell.x && (panel.CellPos.y > blankCell.y && panel.CellPos.y <= hitCell.y ) ) {
					panel.Status = PnlStMove;
					panel.NextPos = panel.CellPos;
					panel.NextPos.y--;
				}
			}
			else if( dir == 1 ) {
				if( panel.CellPos.x == hitCell.x && ( panel.CellPos.y < blankCell.y && panel.CellPos.y >= hitCell.y ) ) {
					panel.Status = PnlStMove;
					panel.NextPos = panel.CellPos;
					panel.NextPos.y++;
				}
			}
			else if( dir == 2 ) {
				if( panel.CellPos.y == hitCell.y && ( panel.CellPos.x > blankCell.x && panel.CellPos.x <= hitCell.x ) ) {
					panel.Status = PnlStMove;
					panel.NextPos = panel.CellPos;
					panel.NextPos.x--;
				}
			}
			else if( dir == 3 ) {
				if( panel.CellPos.y == hitCell.y && ( panel.CellPos.x <blankCell.x && panel.CellPos.x >= hitCell.x ) ) {
					panel.Status = PnlStMove;
					panel.NextPos = panel.CellPos;
					panel.NextPos.x++;
				}
			}

		}


		void panelMove( ref PanelInfo panel, ref Translation trans )
		{
			//Debug.LogFormatAlways( "nxt {0} {1}", panel.NextPos.x, panel.NextPos.y );
			var dt = World.TinyEnvironment().frameDeltaTime;

			float t = 0.1f;		// 移動時間.
			float spd = 1f / t;	// 移動速度.

			float vx = (panel.NextPos.x - panel.CellPos.x) * 128f * spd * dt;
			float vy = -( panel.NextPos.y - panel.CellPos.y ) * 128f * spd * dt;


			var pos = trans.Value;
			pos.x += vx;
			pos.y += vy;
			trans.Value = pos;

			panel.Timer += dt;
			if( panel.Timer >= t ) {
				panel.CellPos = panel.NextPos;

				float3 orgPos = new float3( InitPanelSystem.OrgX, InitPanelSystem.OrgY, 0 );
				//orgPos.x = -128f * 2f + 64f;
				//orgPos.y = 128f * 2f - 64f;

				float3 newpos = new float3( panel.NextPos.x * 128f, -panel.NextPos.y * 128f, 0 );
				newpos += orgPos;
				trans.Value = newpos;

				panel.Timer = 0;
				panel.Status = PnlStNormal;

				//if( panel.Type == 1 && panel.NextPos.x == 3 && panel.NextPos.y == 3 ) {
				//	Debug.LogAlways("GOAL");
				//}
			}

		}

		void panelAppear( ref PanelInfo panel, ref Sprite2DRenderer sprite )
		{
			var dt = World.TinyEnvironment().frameDeltaTime;
			panel.Timer += dt;

//			var scl = scale.Value;
//			scl -= new float3( 0.9f * dt, 0.9f * dt, 0 );
//			scale.Value = scl;

			var col = sprite.color;
			col.a = panel.Timer * 2f;
			if( col.a > 1f )
				col.a = 1f;

			if( panel.Timer >= 0.5f ) {
				panel.Status = PnlStNormal;
				panel.Timer = 0;
				col.a = 1f;
			}
			sprite.color = col;
		}

		bool panelDisapper( ref PanelInfo panel, ref NonUniformScale scale, ref Sprite2DRenderer sprite )
		{
			//Debug.LogAlways("disapp");
			var dt = World.TinyEnvironment().frameDeltaTime;
			panel.Timer += dt;

			var scl = scale.Value;
			scl -= new float3( 1.9f*dt, 1.9f*dt, 0 );
			scale.Value = scl;

			var col = sprite.color;
			col.a -= 1.9f * dt;
			sprite.color = col;


			if( panel.Timer >= 0.5f ) {
				return true;
			}
			return false;
		}
#endif


		bool OverlapsObjectCollider( float3 position, float3 inputPosition, float2 size )
		{
			var rect = new Rect( position.x - size.x * 0.5f, position.y - size.y * 0.5f, size.x, size.y );
			return rect.Contains( inputPosition.xy );
		}
	}
}
