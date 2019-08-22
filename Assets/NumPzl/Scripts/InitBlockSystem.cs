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
		public const float OrgX = -128f * 2f + 64f;
		public const float OrgY = -128f * 2f + 64f;

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

					int v = count / 4;
					int h = count % 4;
					float3 pos = new float3( h * 64, v * 64f, 0 );
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
