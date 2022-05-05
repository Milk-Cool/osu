// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneDistanceSnapGrid : EditorClockTestScene
    {
        private const double beat_length = 100;
        private static readonly Vector2 grid_position = new Vector2(512, 384);

        [Cached(typeof(EditorBeatmap))]
        private readonly EditorBeatmap editorBeatmap;

        [Cached(typeof(IDistanceSnapProvider))]
        private readonly SnapProvider snapProvider = new SnapProvider();

        public TestSceneDistanceSnapGrid()
        {
            editorBeatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo
                }
            });
            editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = beat_length });
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.SlateGray
                },
                new TestDistanceSnapGrid()
            };
        });

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        [TestCase(12)]
        [TestCase(16)]
        public void TestBeatDivisor(int divisor)
        {
            AddStep($"set beat divisor = {divisor}", () => BeatDivisor.Value = divisor);
        }

        [Test]
        public void TestLimitedDistance()
        {
            AddStep("create limited grid", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SlateGray
                    },
                    new TestDistanceSnapGrid(100)
                };
            });
        }

        private class TestDistanceSnapGrid : DistanceSnapGrid
        {
            public new float DistanceBetweenTick => base.DistanceBetweenTick;

            public TestDistanceSnapGrid(double? endTime = null)
                : base(new HitObject(), grid_position, 0, endTime)
            {
            }

            protected override void CreateContent()
            {
                AddInternal(new Circle
                {
                    Origin = Anchor.Centre,
                    Size = new Vector2(5),
                    Position = StartPosition
                });

                int indexFromPlacement = 0;

                for (float s = StartPosition.X + DistanceBetweenTick; s <= DrawWidth && indexFromPlacement < MaxIntervals; s += DistanceBetweenTick, indexFromPlacement++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(5, 10),
                        Position = new Vector2(s, StartPosition.Y),
                        Colour = GetColourForIndexFromPlacement(indexFromPlacement)
                    });
                }

                indexFromPlacement = 0;

                for (float s = StartPosition.X - DistanceBetweenTick; s >= 0 && indexFromPlacement < MaxIntervals; s -= DistanceBetweenTick, indexFromPlacement++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(5, 10),
                        Position = new Vector2(s, StartPosition.Y),
                        Colour = GetColourForIndexFromPlacement(indexFromPlacement)
                    });
                }

                indexFromPlacement = 0;

                for (float s = StartPosition.Y + DistanceBetweenTick; s <= DrawHeight && indexFromPlacement < MaxIntervals; s += DistanceBetweenTick, indexFromPlacement++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(10, 5),
                        Position = new Vector2(StartPosition.X, s),
                        Colour = GetColourForIndexFromPlacement(indexFromPlacement)
                    });
                }

                indexFromPlacement = 0;

                for (float s = StartPosition.Y - DistanceBetweenTick; s >= 0 && indexFromPlacement < MaxIntervals; s -= DistanceBetweenTick, indexFromPlacement++)
                {
                    AddInternal(new Circle
                    {
                        Origin = Anchor.Centre,
                        Size = new Vector2(10, 5),
                        Position = new Vector2(StartPosition.X, s),
                        Colour = GetColourForIndexFromPlacement(indexFromPlacement)
                    });
                }
            }

            public override (Vector2 position, double time) GetSnappedPosition(Vector2 screenSpacePosition)
                => (Vector2.Zero, 0);
        }

        private class SnapProvider : IDistanceSnapProvider
        {
            public SnapResult FindSnappedPosition(Vector2 screenSpacePosition) =>
                new SnapResult(screenSpacePosition, null);

            public SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition) => new SnapResult(screenSpacePosition, 0);

            public IBindable<double> DistanceSpacingMultiplier { get; } = new BindableDouble(1);

            public float GetBeatSnapDistanceAt(HitObject referenceObject) => 10;

            public float DurationToDistance(HitObject referenceObject, double duration) => (float)duration;

            public double DistanceToDuration(HitObject referenceObject, float distance) => distance;

            public double FindSnappedDuration(HitObject referenceObject, float distance) => 0;

            public float FindSnappedDistance(HitObject referenceObject, float distance) => 0;
        }
    }
}
