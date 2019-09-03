using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;

namespace NumPzl
{
	/// <summary>
	/// 位置調節等初期化.
	/// </summary>
	[UpdateAfter( typeof( FirstSetSystem ) )]
	public class InitBlockSystem : ComponentSystem
	{
		Random _random;
		public const float BlkSize = 80f;
		public const float OrgX = -BlkSize * 3f + 0.5f * BlkSize;
		public const float OrgY = -BlkSize * 3f + 0.5f * BlkSize;
		public const int BlkVNum = 7;

		protected override void OnCreate()
		{
			_random = new Random();
			_random.InitState();
		}

		protected override void OnUpdate()
		{
			int count = 0;
			float3 orgPos = new float3( OrgX, OrgY, 0 );
			int preIdx = 0;     // 前回セットしたインデックス.
			bool preIdxUpdated = false;		// preIdx更新したか?

			// 初期配置ブロック, Entity作成待ち.
			bool isInitialized = false;
			Entities.ForEach( ( Entity entity, ref InitBlockInfo info ) => {
				isInitialized = info.Initialized;
				preIdx = info.PreIdx;
			} );

			if( !isInitialized ) {
				int blkNum = 0;
				Entities.ForEach( ( Entity entity, ref BlockInfo block ) => {
					++blkNum;
				} );

				if( blkNum < FirstSetSystem.FirstBlockNum ) {
					return;
				}

				Entities.ForEach( ( Entity entity, ref InitBlockInfo info ) => {
					info.Initialized = true;
				} );

				// 乱数シードセット.
				uint val = (uint)(World.TinyEnvironment().frameTime * 1000f);
				Debug.LogFormatAlways( "seed {0}", (int)val);
				_random.InitState( val );
			}



			Entities.ForEach( ( Entity entity, ref BlockInfo block, ref Translation trans ) => {
				if( !block.Initialized ) {
					block.Initialized = true;

					int no = _random.NextInt( 9 ) + 1;
					block.Num = no;
					EntityManager.SetBufferFromString<TextString>( entity, block.Num.ToString() );

					int v = 0;
					int h = 0;
					if( block.IsStayFirst ) {
						// 最初に配置.
						block.Status = BlockSystem.BlkStStay;
						h = count % 6;
						v = count / 6;
					}
					else {
						// 落下ブロック.
						block.Status = BlockSystem.BlkStPrepare;
						int rnd = _random.NextInt( 6 );     // 0 ~ 5.
						if( rnd == preIdx ) {
							if( ++rnd >= 6 )
								rnd = 0;
						}
						preIdx = rnd;
						preIdxUpdated = true;
						//h = rnd;
						h = 1;
						v = BlkVNum;
					}
					float3 pos = new float3( h * BlkSize, v * BlkSize, 0 );
					pos += orgPos;

					block.CellPos.x = h;
					block.CellPos.y = v;
					block.NextPos = block.CellPos;
					trans.Value = pos;
					++count;
				}
			} );

			// preIdx更新.
			if( preIdxUpdated ) {
				Entities.ForEach( ( Entity entity, ref InitBlockInfo info ) => {
					info.PreIdx = preIdx;
				} );
			}
		}

	}
}
