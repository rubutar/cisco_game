// Person A. Pax, the visiting learner (PRD-v2 4.2a): walks the town with WASD / arrow keys, or along the roads
// to a tapped station ("walk:<id>"), raises HeroArrived / HeroLeft within 26 map px of a station's door, and
// turns the Enter/E, M and Esc keys into UI commands for Person B's popup:
//   "interact:<id>" (expand the card, or Go in when it's already expanded), "materials:<id>", "back".
// flags.json heroMode "teleport" places Pax at the station at once; heroSpeed is map pixels per second.
// Lives under the World map's content, so it hides while a station world is open.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ByteWorld
{
    public class HeroController : MonoBehaviour
    {
        public static HeroController Instance { get; private set; }

        const float ArriveRadius = 26f;
        const float LeaveRadius = 30f;

        // The data river's centre line (map px) and the bridge where it can be crossed.
        static readonly Vector2[] River =
        {
            new(410, 58), new(402, 82), new(388, 104), new(378, 128), new(372, 150), new(382, 174), new(372, 198),
            new(356, 222), new(346, 246), new(342, 272),
        };
        static readonly Rect Bridge = new(352, 120, 56, 24);

        Vector2 position;           // map px, feet
        readonly Queue<Vector2> route = new();
        string arrivedAt;
        SpriteRenderer body;
        Sprite[] idleDown, idleSide, walkSide;
        float animTimer;
        int frame;
        bool facingLeft;
        bool sideways;
        InputAction move, interact, materials, back;

        public Vector2 MapPosition => position;
        public string ArrivedAt => arrivedAt;

        void Awake()
        {
            Instance = this;
            var plaza = Services.Data.Checkpoints?.plaza;
            position = plaza != null ? new Vector2(plaza.x, plaza.y + 20) : new Vector2(240, 160);

            Sprite Fallback() => PixelArt.GridSprite(PixelArt.Byte, Palette.GridNormal, "Pax");
            idleDown = new[] { PixelArt.Load("Characters/pax_idle_down_0", Fallback), PixelArt.Load("Characters/pax_idle_down_1", Fallback) };
            idleSide = new[] { PixelArt.Load("Characters/pax_idle_side_0", Fallback), PixelArt.Load("Characters/pax_idle_side_1", Fallback) };
            walkSide = new Sprite[4];
            for (var i = 0; i < 4; i++) walkSide[i] = PixelArt.Load($"Characters/pax_walk_side_{i}", Fallback);

            body = PixelArt.AddSprite(transform, idleDown[0], "Pax", 30);

            // The Hero action map (PRD-v2 6.1), built in code.
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            interact = new InputAction("Interact", binding: "<Keyboard>/enter");
            interact.AddBinding("<Keyboard>/e");
            interact.AddBinding("<Keyboard>/numpadEnter");
            materials = new InputAction("Materials", binding: "<Keyboard>/m");
            back = new InputAction("Back", binding: "<Keyboard>/escape");
            Place();
        }

        void OnEnable()
        {
            move?.Enable(); interact?.Enable(); materials?.Enable(); back?.Enable();
        }

        void OnDisable()
        {
            move?.Disable(); interact?.Disable(); materials?.Disable(); back?.Disable();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            move?.Dispose(); interact?.Dispose(); materials?.Dispose(); back?.Dispose();
        }

        /// <summary>Walk to a station along the roads (or appear there in teleport mode). A new call turns Pax around.</summary>
        public void WalkTo(string checkpointId)
        {
            var c = Services.Checkpoints.Get(checkpointId);
            if (c == null) return;
            route.Clear();
            var door = new Vector2(c.x, c.y);
            if (Services.Data.Flags?.HeroTeleports ?? false)
            {
                position = door;
                Place();
                CheckArrival();
                return;
            }
            // Back to the plaza first, then out along the station's road.
            var plaza = Services.Data.Checkpoints?.plaza;
            if (plaza != null) route.Enqueue(new Vector2(plaza.x, plaza.y));
            if (c.path != null)
                for (var i = 1; i < c.path.Length; i++) route.Enqueue(new Vector2(c.path[i].x, c.path[i].y));
            route.Enqueue(door);
        }

        void Update()
        {
            var state = Services.Flow.State;
            var free = state == GameState.WorldPeace || state == GameState.Alert || state == GameState.FinalAttack;
            var speed = Mathf.Max(20f, Services.Data.Flags?.heroSpeed ?? 110f);
            var moved = Vector2.zero;

            if (free)
            {
                var input = move.ReadValue<Vector2>();
                if (input.sqrMagnitude > 0.01f)
                {
                    route.Clear(); // the keyboard takes over from a tapped walk
                    moved = new Vector2(input.x, -input.y).normalized * speed * Time.deltaTime; // map y grows downward
                    var next = position + moved;
                    if (Walkable(new Vector2(next.x, position.y))) position.x = next.x;
                    if (Walkable(new Vector2(position.x, next.y))) position.y = next.y;
                }
                else if (route.Count > 0)
                {
                    var target = route.Peek();
                    var before = position;
                    position = Vector2.MoveTowards(position, target, speed * Time.deltaTime);
                    moved = position - before;
                    if ((position - target).sqrMagnitude < 0.25f) route.Dequeue();
                }

                if (interact.WasPressedThisFrame() && arrivedAt != null) GameEvents.RaiseUiAction($"interact:{arrivedAt}");
                if (materials.WasPressedThisFrame() && arrivedAt != null) GameEvents.RaiseUiAction($"materials:{arrivedAt}");
            }
            if (back.WasPressedThisFrame()) GameEvents.RaiseUiAction("back");

            Animate(moved);
            Place();
            if (free) CheckArrival();
        }

        bool Walkable(Vector2 p)
        {
            if (p.x < 8 || p.x > PixelArt.MapWidth - 8 || p.y < 30 || p.y > PixelArt.MapHeight - 4) return false;
            if (Bridge.Contains(p)) return true;
            foreach (var r in River)
                if ((p - r).sqrMagnitude < 13f * 13f) return false;
            return true;
        }

        void CheckArrival()
        {
            string nearest = null;
            var best = float.MaxValue;
            foreach (var c in Services.Checkpoints.All)
            {
                var d = Vector2.Distance(position, new Vector2(c.x, c.y));
                if (d < best) { best = d; nearest = c.id; }
            }

            if (arrivedAt != null)
            {
                var c = Services.Checkpoints.Get(arrivedAt);
                if (c != null && Vector2.Distance(position, new Vector2(c.x, c.y)) <= LeaveRadius && nearest == arrivedAt) return;
                var left = arrivedAt;
                arrivedAt = null;
                GameEvents.RaiseHeroLeft(left);
            }
            if (nearest != null && best <= ArriveRadius)
            {
                arrivedAt = nearest;
                GameEvents.RaiseHeroArrived(nearest);
            }
        }

        void Animate(Vector2 moved)
        {
            var walking = moved.sqrMagnitude > 0.0001f;
            if (walking && Mathf.Abs(moved.x) > 0.001f)
            {
                sideways = true;
                facingLeft = moved.x < 0f;
            }
            else if (walking)
            {
                sideways = false;
            }

            animTimer -= Time.deltaTime;
            if (animTimer <= 0f)
            {
                frame++;
                animTimer = walking ? 0.12f : 0.5f;
            }
            body.sprite = walking && sideways ? walkSide[frame % 4] : (sideways ? idleSide : idleDown)[frame % 2];
            body.flipX = sideways && facingLeft;
            body.sortingOrder = 30;
        }

        void Place() => transform.localPosition = PixelArt.Snap(PixelArt.MapToWorld(position.x, position.y));
    }
}
