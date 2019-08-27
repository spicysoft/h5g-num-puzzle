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
		public const float BlkSize = 64f;
		public const float OrgX = -128f * 2f + BlkSize;
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

			/*int setCnt = 0;
			NativeArray<int> idxAry = new NativeArray<int>( 4, Allocator.Temp );
			for( int i = 0; i < 4; ++i ) {
				idxAry[i] = -1;
			}*/

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
						/*h = CheckIdx( rnd, ref idxAry );
						idxAry[setCnt++] = h;*/
						if( rnd == preIdx ) {
							if( ++rnd >= 6 )
								rnd = 0;
						}
						preIdx = rnd;
						h = rnd;
						//h = 1;
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

			//idxAry.Dispose();
		}

#if false
		// 重複チェック.
		int CheckIdx( int idx, ref NativeArray<int> idxAry )
		{
			bool isSame = false;
			for( int i = 0; i < idxAry.Length; ++i ) {
				if( idx == idxAry[i] ) {
					isSame = true;
					break;
				}
			}

			if( isSame ) {
				for( int n = 0; n < 6; ++n ) {
					bool noUse = false;
					for( int i = 0; i < idxAry.Length; ++i ) {
						if( n != idxAry[i] && idxAry[i] != -1 ) {
							noUse = true;
							break;
						}
					}
					if( noUse ) {
						return n;
					}
				}
				// ここには来ないはず.
				Debug.Log("ASSERT");
				return 0;
			}
			else {
				// 重複なし.
				return idx;
			}
		}
#endif
	}
}
