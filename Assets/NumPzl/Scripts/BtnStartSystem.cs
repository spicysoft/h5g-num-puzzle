using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;
using Unity.Tiny.Scenes;
using Unity.Tiny.Text;
using Unity.Tiny.UIControls;

namespace NumPzl
{
	public class BtnStartSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool btnOn = false;
			Entities.WithAll<BtnStartTag>().ForEach( ( Entity entity, ref PointerInteraction pointerInteraction ) => {
				if( pointerInteraction.clicked ) {
					Debug.LogAlways( "btn ret click" );
					btnOn = true;
				}
			} );


			if( btnOn ) {
				// ポーズ解除.
				Entities.ForEach( ( ref GameMngr mngr ) => {
					mngr.IsPause = false;
				} );

				// タイトルシーンアンロード.
				SceneReference panelBase = new SceneReference();
				panelBase = World.TinyEnvironment().GetConfigData<GameConfig>().TitleScn;
				SceneService.UnloadAllSceneInstances( panelBase );
			}
		}
	}
}
