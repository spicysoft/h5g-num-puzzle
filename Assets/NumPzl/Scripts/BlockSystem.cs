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


				switch( block.Status ) {
				case BlkStMove:
					blockMove( ref entity, ref block, ref trans, mouseOn );
					break;
				}

				// マウスとのあたりチェック.
				if( mouseOn && block.Status == BlkStMove ) {
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

		}


		void blockMove( ref Entity entity, ref BlockInfo block, ref Translation trans, bool mouseOn )
		{
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
