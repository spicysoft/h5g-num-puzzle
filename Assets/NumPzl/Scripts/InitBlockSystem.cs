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
		public const float OrgY = -128f * 2f + BlkSize;
		private int preIdx;

		protected override void OnCreate()
		{
			_random = new Random();
			_random.InitState();
		}

		protected override void OnUpdate()
		{
			int count = 0;
			float3 orgPos = new float3( OrgX, OrgY, 0 );

			// 初期配置ブロック待ち.
			bool isInitialized = false;
			Entities.ForEach( ( Entity entity, ref InitBlockInfo info ) => {
				isInitialized = info.Initialized;
			} );

			if( !isInitialized ) {
#if true
				int blkNum = 0;
				Entities.ForEach( ( Entity entity, ref BlockInfo block ) => {
					++blkNum;
				} );

				if( blkNum < 12 ) {
					return;
				}
#endif
				Entities.ForEach( ( Entity entity, ref InitBlockInfo info ) => {
					info.Initialized = true;
				} );

				// 乱数シードセット.
				uint val = (uint)(World.TinyEnvironment().frameTime * 100f);
				Debug.LogFormatAlways( "seed {0}", (int)val);
				_random.InitState( val );
			}



			Entities.ForEach( ( Entity entity, ref BlockInfo block, ref Translation trans ) => {
				if( !block.Initialized ) {
					block.Initialized = true;

					int i = _random.NextInt( 9 ) + 1;
					block.Num = i;
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
						block.Status = BlockSystem.BlkStMove;
						int rnd = _random.NextInt( 6 );     // 0 ~ 5.
						if( rnd == preIdx ) {
							if( ++rnd >= 6 )
								rnd = 0;
						}
						preIdx = rnd;
						h = rnd;
						v = 7;
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

		}

	}
}
