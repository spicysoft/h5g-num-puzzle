using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace NumPzl
{
	public class InitEffStarSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach( ( Entity entity, ref EffStarInfo eff, ref Translation trans, ref Sprite2DSequencePlayer seq ) => {
				if( !eff.Initialized ) {
					float3 effpos = new float3( 0, 0, 0 );
					Entities.ForEach( ( ref BlockInfo block, ref Translation blkTrans ) => {
						// エフェクトジェネレートしたか.
						if( block.EffGen ) {
							block.EffGen = false;
							effpos = blkTrans.Value;
							//Debug.LogFormatAlways( "---- eff {0} {1}", effpos.x, effpos.y );
						}
					} );

					effpos.y += 0.5f * InitBlockSystem.BlkSize;	// 半ブロック上.
					trans.Value = effpos;
					eff.Initialized = true;	// ここでInitialize終了に.
					seq.paused = false;		// エフェクト再生開始.
				}
			} );


		}
	}
}
