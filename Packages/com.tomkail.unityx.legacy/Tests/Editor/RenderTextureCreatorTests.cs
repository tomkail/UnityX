using NUnit.Framework;
using UnityEngine;

namespace UnityX.LegacyTests {
	public class RenderTextureCreatorTests {
		GameObject gameObject;
		RenderTextureCreator creator;
		int creations;

		[SetUp]
		public void SetUp() {
			gameObject = new GameObject(nameof(RenderTextureCreatorTests));
			creator = gameObject.AddComponent<RenderTextureCreator>();
			creator.fullScreen = false;
			creator.renderTextureSize = new Vector2Int(64, 64);
			creations = 0;
			creator.OnCreateRenderTexture += _ => creations++;
		}

		[TearDown]
		public void TearDown() {
			creator.DestroyRenderTexture();
			Object.DestroyImmediate(gameObject);
		}

		static Color ReadPixel(RenderTexture texture) {
			var previous = RenderTexture.active;
			RenderTexture.active = texture;
			var readback = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			readback.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
			readback.Apply();
			RenderTexture.active = previous;
			var pixel = readback.GetPixel(0, 0);
			Object.DestroyImmediate(readback);
			return pixel;
		}

		static void Fill(RenderTexture texture, Color color) {
			var previous = RenderTexture.active;
			RenderTexture.active = texture;
			GL.Clear(true, true, color);
			RenderTexture.active = previous;
		}

		// Metal substitutes a 32-bit depth buffer for 24, which used to make every refresh recreate the texture
		[Test]
		public void RefreshingWithUnchangedSettingsKeepsTheTexture() {
			creator.renderTextureDepth = RenderTextureCreator.RenderTextureDepth._24;
			int warnings = 0;
			void CountWarnings(string message, string stackTrace, LogType type) {
				if (type == LogType.Warning && message.Contains("Depth")) warnings++;
			}
			Application.logMessageReceived += CountWarnings;
			try {
				creator.RefreshRenderTexture();
				var texture = creator.renderTexture;
				Fill(texture, Color.red);

				creator.RefreshRenderTexture();
				creator.RefreshRenderTexture();

				Assert.AreEqual(1, creations);
				Assert.AreSame(texture, creator.renderTexture);
				Assert.IsTrue(texture.IsCreated());
				Assert.AreEqual(Color.red, ReadPixel(texture));
				Assert.LessOrEqual(warnings, 1);
			} finally {
				Application.logMessageReceived -= CountWarnings;
			}
		}

		[Test]
		public void ChangingTheSizeRecreatesTheTexture() {
			creator.RefreshRenderTexture();
			creator.renderTextureSize = new Vector2Int(128, 32);
			creator.RefreshRenderTexture();

			Assert.AreEqual(2, creations);
			Assert.AreEqual(128, creator.renderTexture.width);
			Assert.AreEqual(32, creator.renderTexture.height);
		}

		[Test]
		public void ChangingTheDepthRecreatesTheTexture() {
			creator.renderTextureDepth = RenderTextureCreator.RenderTextureDepth._0;
			creator.RefreshRenderTexture();
			creator.renderTextureDepth = RenderTextureCreator.RenderTextureDepth._16;
			creator.RefreshRenderTexture();

			Assert.AreEqual(2, creations);
			Assert.AreEqual(16, creator.renderTexture.depth);
		}

		[Test]
		public void RefreshingAfterAReleaseGivesACreatedTexture() {
			creator.RefreshRenderTexture();
			creator.ReleaseRenderTexture();
			creator.RefreshRenderTexture();

			Assert.IsTrue(creator.renderTexture.IsCreated());
		}
	}
}
