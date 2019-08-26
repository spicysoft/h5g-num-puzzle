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


		protected override void OnCreate()
		{
			_random = new Random();
			_random.InitState();
		}

		protected override void OnUpdate()
		{
			int count = 0;
			float3 orgPos = new float3( OrgX, OrgY, 0 );

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
						//h = _random.NextInt( 6 );
						h = 1;
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
