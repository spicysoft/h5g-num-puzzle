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
			// エフェクト.
			Entities.ForEach( ( ref EffStarMngr mngr ) => {
				if( mngr.Requested ) {
					float3 effpos = new float3( mngr.xpos, mngr.ypos, 0 );
					bool initialized = false;
					Entities.ForEach( ( Entity entity, ref EffStarInfo eff, ref Translation trans ) => {
						if( !eff.Initialized ) {
							trans.Value = effpos;
							Debug.LogFormatAlways( "eff {0} {1}", effpos.x, effpos.y );
							eff.Initialized = true;
							initialized = true;
						}
					} );
					if( initialized )
						mngr.Requested = false;
				}
			} );
		}
	}
}
