﻿namespace FrostHelper;

[CustomEntity("FrostHelper/NoDashArea")]
public class NoDashArea : Entity {
    Color color = Color.Red * 0.15f;
    Color color2 = Color.Red * 0.25f;
    PlayerCollider pc;
    bool colliding;

    public static float[] speeds = new float[]
    {
        12f,
        20f,
        40f
    };

    public float Flash;

    public float Solidify;

    public bool Flashing => Flash > 0f;

    private List<Vector2> particles;

    public Vector2? Node;

    bool fastMoving;

    public NoDashArea(EntityData data, Vector2 offset) : base(data.Position + offset) {
        fastMoving = data.Bool("fastMoving", false);
        Collider = new Hitbox(data.Width, data.Height);
        Add(pc = new PlayerCollider(new Action<Player>(OnPlayer), null, null));
        Add(new DisplacementRenderHook(new Action(RenderDisplacement)));
        float num = 0;
        particles = new List<Vector2>();
        while (num < Width * Height / 16f) {
            particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
            num++;
        }
        Node = data.FirstNodeNullable(new Vector2?(offset));
        if (Node != null) {
            Vector2 start = Position;
            Vector2 end = Node.Value;
            float num2 = Vector2.Distance(start, end) / 12f;
            bool flag2 = fastMoving;
            if (flag2) {
                num2 /= 3f;
            }
            Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, num2, true);
            tween.OnUpdate = delegate (Tween t) {
                bool collidable = Collidable;
                if (collidable) {
                    Position = Vector2.Lerp(start, end, t.Eased);
                } else {
                    Position = Vector2.Lerp(start, end, t.Eased);
                }
            };
            Add(tween);
        }
    }

    public override void Added(Scene scene) {
        base.Added(scene);
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
    }

    public void OnPlayer(Player player) {
        Player_dashCooldownTimer.SetValue(player, Engine.DeltaTime + 0.1f);
    }

    private FieldInfo Player_dashCooldownTimer = typeof(Player).GetField("dashCooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance);

    public override void Update() {
        base.Update();
        foreach (Player player in SceneAs<Level>().Tracker.GetEntities<Player>()) {
            colliding = pc.Check(player);
        }
        if (colliding && Input.Dash.Pressed) {
            Solidify = 1f;
            Flash = 1f;
        }
        bool flag3 = Solidify > 0f;
        if (flag3) {
            Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
        }
        if (Flashing)
            Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
        int num = speeds.Length;
        float height = Height;
        int i = 0;
        int count = particles.Count;
        while (i < count) {
            Vector2 value = particles[i] + Vector2.UnitY * speeds[i % num] * Engine.DeltaTime;
            value.Y %= height - 1f;
            particles[i] = value;
            i++;
        }

    }

    public override void Render() {
        Color color = Color.White * 0.5f;
        Draw.Rect(Collider, Color.Red * 0.25f);
        foreach (Vector2 value in particles) {
            Draw.Pixel.Draw(Position + value, Vector2.Zero, color);
        }
        if (Flashing) {
            Draw.Rect(Collider, Color.White * Flash * 0.25f);
        }
    }

    public void RenderDisplacement() {
        Draw.Rect(X, Y, Width, Height, new Color(0.5f, 0.5f, 0.8f, 1f));
    }

}
