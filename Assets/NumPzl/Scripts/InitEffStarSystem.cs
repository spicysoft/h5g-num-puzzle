using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Debugging;

namespace NumPzl
{
	public class EffStarMngrSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
#if false
			// エフェクト.
			Entities.ForEach( ( ref EffStarMngr mngr ) => {
				if( mngr.Requested ) {
					float3 effpos = new float3( mngr.xpos, mngr.ypos, 0 );
					bool initialized = false;
					Entities.ForEach( ( Entity entity, ref EffStarInfo eff, ref Translation trans, ref Sprite2DSequencePlayer seq ) => {
						if( !eff.Initialized ) {
							trans.Value = effpos;
							Debug.LogFormatAlways( "eff {0} {1}", effpos.x, effpos.y );
							eff.Initialized = true;
							initialized = true;
							seq.paused = false;
						}
					} );
					if( initialized )
						mngr.Requested = false;
				}
			} );
#endif

			Entities.ForEach( ( Entity entity, ref EffStarInfo eff, ref Translation trans, ref Sprite2DSequencePlayer seq ) => {
				if( !eff.Initialized ) {
					float3 effpos = new float3( 0, 0, 0 );
					Entities.ForEach( ( ref BlockInfo block, ref Translation blkTrans ) => {
						// エフェクトジェネレートしたか.
						Debug.LogAlways( "block check" );
						if( block.EffGen ) {
							block.EffGen = false;
							effpos = blkTrans.Value;
							Debug.LogFormatAlways( "---- eff {0} {1}", effpos.x, effpos.y );
						}
					} );

					effpos.y += 0.5f * InitBlockSystem.BlkSize;
					trans.Value = effpos;
					eff.Initialized = true;
					seq.paused = false;
				}
			} );


		}
	}
}
