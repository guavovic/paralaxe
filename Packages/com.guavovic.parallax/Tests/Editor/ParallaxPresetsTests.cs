using Guavovic.Parallax.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Guavovic.Parallax.Tests
{
    public sealed class ParallaxPresetsTests
    {
        [Test]
        public void EveryBuiltInPresetGoesFromFarToNear()
        {
            foreach (string name in ParallaxPresets.BuiltInNames)
            {
                var preset = ParallaxPresets.CreateBuiltIn(name);
                try
                {
                    Assert.GreaterOrEqual(preset.Layers.Count, 5, name);
                    for (int i = 1; i < preset.Layers.Count; i++)
                        Assert.Less(preset.Layers[i].Factor.x, preset.Layers[i - 1].Factor.x, name + ": " + preset.Layers[i].Name);
                }
                finally
                {
                    Object.DestroyImmediate(preset);
                }
            }
        }

        [Test]
        public void ApplyingSpreadsThePresetByRelativePositionAndKeepsNames()
        {
            var preset = ParallaxPresets.CreateBuiltIn("Floresta");
            var target = ScriptableObject.CreateInstance<ParallaxProfile>();
            try
            {
                for (int i = 0; i < 4; i++)
                    target.AddLayer(new ParallaxLayerSettings("Minha " + i, new Vector2(0.5f, 0f), 0f));

                ParallaxPresets.Apply(preset, target);

                var first = preset.Layers[0];
                var last = preset.Layers[preset.Layers.Count - 1];
                Assert.AreEqual(first.Factor, target.Layers[0].Factor);
                Assert.AreEqual(last.Factor, target.Layers[3].Factor);
                Assert.AreEqual(last.Blur, target.Layers[3].Blur);
                Assert.AreEqual("Minha 0", target.Layers[0].Name);
                Assert.AreEqual(preset.WindStrength, target.WindStrength);
            }
            finally
            {
                Object.DestroyImmediate(preset);
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void SavedPresetShowsUpInTheUserList()
        {
            var source = ParallaxPresets.CreateBuiltIn("Neve");
            string path = null;
            bool folderExisted = AssetDatabase.IsValidFolder(ParallaxPresets.UserFolder);
            try
            {
                path = ParallaxPresets.SaveAsPreset(source, "Teste de preset");

                var saved = AssetDatabase.LoadAssetAtPath<ParallaxProfile>(path);
                Assert.IsNotNull(saved);
                Assert.AreEqual(source.Layers.Count, saved.Layers.Count);
                Assert.IsTrue(ParallaxPresets.FindUserPresets().Contains(saved));
            }
            finally
            {
                Object.DestroyImmediate(source);
                if (path != null)
                    AssetDatabase.DeleteAsset(path);
                if (!folderExisted)
                    AssetDatabase.DeleteAsset(ParallaxPresets.UserFolder);
            }
        }
    }
}
