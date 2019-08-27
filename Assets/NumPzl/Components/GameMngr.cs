using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Scenes;
using Unity.Collections;

namespace NumPzl
{
	public struct GameMngr : IComponentData
	{
		public bool IsTitleFinished;    // タイトル終了したか.
		public bool IsPause;        // ポーズするか.
		public bool ReqReflesh;     // 盤面更新リクエスト.
		public float GameTimer;		// 時間.
		public int Score;			// スコア.
		public int ComboCnt;		// コンボ数 (0, 1, 2, 3).
	}
}
