﻿using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;

namespace FrostTempleHelper
{
    public class CustomRisingLava : Entity
    {
        public bool ReverseCoreMode;
        public CustomRisingLava(bool intro, float speed, Color[] coldColors, Color[] hotColors, bool reverseCoreMode)
        {
            delay = 0f;
            this.intro = intro;
            Speed = speed;
            Depth = -1000000;
            Collider = new Hitbox(340f, 120f, 0f, 0f);
            Visible = false;
            Add(new PlayerCollider(new Action<Player>(OnPlayer), null, null));
            Add(new CoreModeListener(new Action<Session.CoreModes>(OnChangeMode)));
            Add(loopSfx = new SoundSource());
            Add(bottomRect = new LavaRect(400f, 200f, 4));
            bottomRect.Position = new Vector2(-40f, 0f);
            bottomRect.OnlyMode = LavaRect.OnlyModes.OnlyTop;
            bottomRect.SmallWaveAmplitude = 2f;
            Cold = coldColors;
            Hot = hotColors;
            ReverseCoreMode = reverseCoreMode;
        }

        public static Color[] GetColors(EntityData data, bool cold)
        {
            Color[] colors = new Color[3];
            for (int i = 0; i < colors.Length; i++)
            {
                string path = (cold ? "cold" : "hot") + "Color" + (i + 1);
                colors[i] = data.HexColor(path);
            }
            return colors;
        }

        public CustomRisingLava(EntityData data, Vector2 offset) : this(data.Bool("intro", false), data.Float("speed", -30f), GetColors(data, true), GetColors(data, false), data.Bool("reverseCoreMode", false))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            X = (SceneAs<Level>().Bounds.Left - 10);
            Y = (SceneAs<Level>().Bounds.Bottom + 16);
            iceMode = (SceneAs<Level>().Session.CoreMode == Session.CoreModes.Cold);
            if (ReverseCoreMode)
                iceMode = !iceMode;
            loopSfx.Play("event:/game/09_core/rising_threat", "room_state", iceMode ? 1 : 0);
            loopSfx.Position = new Vector2(Width / 2f, 0f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (intro)
            {
                waiting = true;
            }
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                bool flag2 = entity != null && entity.JustRespawned;
                if (flag2)
                {
                    waiting = true;
                }
            }
            bool flag3 = intro;
            if (flag3)
            {
                Visible = true;
            }
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            iceMode = (mode == Session.CoreModes.Cold);
            if (ReverseCoreMode)
                iceMode = !iceMode;
            loopSfx.Param("room_state", iceMode ? 1 : 0);
        }

        private void OnPlayer(Player player)
        {
            bool invincible = SaveData.Instance.Assists.Invincible;
            if (invincible)
            {
                bool flag = delay <= 0f;
                if (flag)
                {
                    float from = Y;
                    float to = Y + 48f;
                    player.Speed.Y = -200f;
                    player.RefillDash();
                    Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, delegate (Tween t)
                    {
                        Y = MathHelper.Lerp(from, to, t.Eased);
                    }, null);
                    delay = 0.5f;
                    loopSfx.Param("rising", 0f);
                    Audio.Play("event:/game/general/assist_screenbottom", player.Position);
                }
            }
            else
            {
                player.Die(-Vector2.UnitY, false, true);
            }
        }

        public override void Update()
        {
            delay -= Engine.DeltaTime;
            X = SceneAs<Level>().Camera.X;
            Player entity = Scene.Tracker.GetEntity<Player>();
            base.Update();
            Visible = true;
            bool flag = waiting;
            if (flag)
            {
                loopSfx.Param("rising", 0f);
                bool flag2 = !intro && entity != null && entity.JustRespawned;
                if (flag2)
                {
                    Y = Calc.Approach(Y, entity.Y + 32f, 32f * Engine.DeltaTime);
                }
                bool flag3 = (!iceMode || !intro) && (entity == null || !entity.JustRespawned);
                if (flag3)
                {
                    waiting = false;
                }
            }
            else
            {
                float num = SceneAs<Level>().Camera.Bottom - 12f;
                bool flag4 = Top > num + 96f;
                if (flag4)
                {
                    Top = num + 96f;
                }
                bool flag5 = Top > num;
                float num2;
                if (flag5)
                {
                    num2 = Calc.ClampedMap(Top - num, 0f, 96f, 1f, 2f);
                }
                else
                {
                    num2 = Calc.ClampedMap(num - Top, 0f, 32f, 1f, 0.5f);
                }
                bool flag6 = delay <= 0f;
                if (flag6)
                {
                    loopSfx.Param("rising", 1f);
                    Y += Speed * num2 * Engine.DeltaTime;
                }
            }
            lerp = Calc.Approach(lerp, (iceMode ? 1 : 0), Engine.DeltaTime * 4f);
            bottomRect.SurfaceColor = Color.Lerp(Hot[0], Cold[0], lerp);
            bottomRect.EdgeColor = Color.Lerp(Hot[1], Cold[1], lerp);
            bottomRect.CenterColor = Color.Lerp(Hot[2], Cold[2], lerp);
            bottomRect.Spikey = lerp * 5f;
            bottomRect.UpdateMultiplier = (1f - lerp) * 2f;
            bottomRect.Fade = (iceMode ? 128 : 32);
        }

        Color[] Hot = new Color[]
        {
            Calc.HexToColor("ff8933"),
            Calc.HexToColor("f25e29"),
            Calc.HexToColor("d01c01")
        };

        Color[] Cold = new Color[]
        {
            Calc.HexToColor("33ffe7"),
            Calc.HexToColor("4ca2eb"),
            Calc.HexToColor("0151d0")
        };

        private float Speed = -30f;

        private bool intro;

        private bool iceMode;

        public bool waiting;

        private float lerp;

        private LavaRect bottomRect;

        private float delay;

        private SoundSource loopSfx;
    }
}