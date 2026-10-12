using System;

namespace UnityX.Rhythm {
	// Turns a judge's results into a score. Watch a Judge, or call the methods yourself.
	public interface IScoreModel {
		double Score { get; }
		void Reset();
		void OnJudged(Judgement judgement);
		void OnMissed(NoteInstance note);
		void OnStrayHit(RhythmInput input);
		void OnHoldCompleted(NoteInstance note);
		void OnHoldDropped(NoteInstance note);
	}

	public static class ScoreModelExtensions {
		// Feeds the judge's results to the model until the returned handle is disposed
		public static IDisposable Watch(this IScoreModel model, Judge judge) => new Subscription(model, judge);

		sealed class Subscription : IDisposable {
			readonly IScoreModel model;
			Judge judge;

			public Subscription(IScoreModel model, Judge judge) {
				this.model = model ?? throw new ArgumentNullException(nameof(model));
				this.judge = judge ?? throw new ArgumentNullException(nameof(judge));
				judge.Judged += model.OnJudged;
				judge.Missed += model.OnMissed;
				judge.StrayHit += model.OnStrayHit;
				judge.HoldCompleted += model.OnHoldCompleted;
				judge.HoldDropped += OnHoldDropped;
			}

			public void Dispose() {
				if (judge == null) return;
				judge.Judged -= model.OnJudged;
				judge.Missed -= model.OnMissed;
				judge.StrayHit -= model.OnStrayHit;
				judge.HoldCompleted -= model.OnHoldCompleted;
				judge.HoldDropped -= OnHoldDropped;
				judge = null;
			}

			void OnHoldDropped(NoteInstance note, RhythmInput input) => model.OnHoldDropped(note);
		}
	}
}
