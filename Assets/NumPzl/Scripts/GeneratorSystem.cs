using Unity.Entities;

namespace NumPzl
{
	public class GeneratorSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			bool isRequest = false;
			Entities.ForEach( ( ref GeneratorInfo info ) => {
				if( info.Request ) {
					info.Request = false;
					isRequest = true;
				}
			} );

			if( isRequest ) {

			}
		}
	}
}
