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
	/// </summary>
	[UpdateAfter( typeof( InitBlockSystem ) )]
	public class BlockSystem : ComponentSystem
	{
		public const int BlkStAppear = 0;
		public const int BlkStStay = 1;
		public const int BlkStMove = 2;
		public const int BlkStDisappear = 3;

		public const int BlkTypeNone = 0;
		public const int BlkTypeRed = 1;
		public const int BlkTypeWhite = 2;


		protected override void OnUpdate()
		{
			Entity delEntity = Entity.Null;
			var inputSystem = World.GetExistingSystem<InputSystem>();

			bool mouseOn = false;
			//if( !isPause() ) {
				mouseOn = inputSystem.GetMouseButtonDown( 0 );
			//}


			// 盤面情報収集.
			Entities.ForEach( ( Entity entity, ref BlockInfo block, ref Translation trans ) => {
				// 状態チェック.
				//if( !panel.Initialized || panel.Status != PnlStNormal ) {
				//	isReadyToMove = false;
				//	return;
				//}


				// マウスとのあたりチェック.
				if( mouseOn ) {
					float2 size = new float2( 64f, 64f );

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
			} );


#if false
			// 盤面情報用配列.
			NativeArray<int> InfoAry = new NativeArray<int>( 16, Allocator.Temp );
			bool isPanelHit = false;    // パネルタップしたか?
			bool isReadyToMove = true;	// パネル動けるか?
			int2 HitCell = new int2( -1, -1 );
			int2 BlankCell = new int2( -1, -1 );

			// 盤面情報収集.
			Entities.ForEach( ( ref PanelInfo panel, ref Translation trans ) => {
				// 状態チェック.
				if( !panel.Initialized || panel.Status != PnlStNormal ) {
					isReadyToMove = false;
					return;
				}

				// 情報.
				int idx = panel.CellPos.x + panel.CellPos.y * 4;
				InfoAry[idx] = panel.Type;

				// マウスとのあたりチェック.
				if( mouseOn ) {
					float2 size = new float2( 128f, 128f );

					float3 mypos = trans.Value;
					float3 mousePos = inputSystem.GetWorldInputPosition();
					bool res = OverlapsObjectCollider( mypos, mousePos, size );
					if( res ) {
						HitCell = panel.CellPos;
						isPanelHit = true;
					}
				}
			} );


			// パネル動ける状態でない場合.
			if( !isReadyToMove ) {
				isPanelHit = false;
			}

			if( isPanelHit ) {
				// 入力回数.
				Entities.ForEach( ( ref GameMngr mngr ) => {
					++mngr.InputCnt;
				} );
				// 空きを探す.
				for( int j = 0; j < 4; ++j ) {
					for( int i = 0; i < 4; ++i ) {
						int idx = i + j * 4;
						if( InfoAry[idx] == 0 ) {
							BlankCell.x = i;
							BlankCell.y = j;
						}
					}
				}
			}

			// パネル動けるか.
			bool isMovable = false;
			int moveDir = -1;	// 0:up 1:down 2:left 3:right.
			if( HitCell.x == BlankCell.x ) {
				isMovable = true;
				if( HitCell.y < BlankCell.y )
					moveDir = 1;
				else if( HitCell.y > BlankCell.y )
					moveDir = 0;
			}
			else if ( HitCell.y == BlankCell.y ) {
				isMovable = true;
				if( HitCell.x < BlankCell.x )
					moveDir = 3;
				else if( HitCell.x > BlankCell.x )
					moveDir = 2;
			}

			Entities.ForEach( ( Entity entity, ref PanelInfo panel, ref Translation trans, ref NonUniformScale scale, ref Sprite2DRenderer sprite ) => {
				if( !panel.Initialized )
					return;

				switch( panel.Status ) {
				case PnlStAppear:
					panelAppear( ref panel, ref sprite );
					break;

				case PnlStNormal:
					if( isMovable )
						panelNormNew( ref panel, ref InfoAry, ref HitCell, ref BlankCell, moveDir );
					//if( mouseOn ) {
					//	panelNorm( ref panel, ref trans, ref InfoAry );
					//}
					break;

				case PnlStMove:
					panelMove( ref panel, ref trans );
					break;

				case PnlStDisappear:
					if( panelDisapper( ref panel, ref scale, ref sprite ) ) {
						delEntity = entity;
					}
					break;
				}
			} );

			InfoAry.Dispose();

			if( delEntity != Entity.Null ) {

				// パネル全消しするかチェック.
				bool reqReflesh = false;
				Entities.ForEach( ( ref GameMngr mngr ) => {
					if( mngr.ReqReflesh ) {
						mngr.ReqReflesh = false;
						reqReflesh = true;
					}
				} );

				if( reqReflesh ) {
					// パネル全消し.
					var env = World.TinyEnvironment();
					SceneService.UnloadAllSceneInstances( env.GetConfigData<PanelConfig>().PanelRed );
					SceneService.UnloadAllSceneInstances( env.GetConfigData<PanelConfig>().PanelWhite );
					// 盤面生成.
					Entities.ForEach( ( ref PuzzleGen gen ) => {
						gen.IsGenerate = true;
					} );
					setPause( false );
				}
				else {
					// エンティティ削除.
					SceneService.UnloadSceneInstance( delEntity );
					// パネル追加.
					Entities.ForEach( ( ref PuzzleGen gen ) => {
						gen.IsGenAdditive = true;
					} );
					setPause( false );
				}
			}
#endif
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
