﻿namespace FrostHelper;

[CustomEntity("FrostHelper/RainbowTilesetController")]
[Tracked]
public class RainbowTilesetController : Entity {
    #region Hooks
    private static bool _loadedHooks;

    public static void LoadHooksIfNeeded() {
        if (_loadedHooks)
            return;
        _loadedHooks = true;

        IL.Monocle.TileGrid.RenderAt += TileGrid_RenderAt;
    }

    private static byte GetFirstLocalId(ILCursor cursor, string typeName) {
        return (byte) cursor.Body.Variables.First(v => v.VariableType.Name.Contains(typeName)).Index;
    }

    private static void TileGrid_RenderAt(ILContext il) {
        ILCursor cursor = new ILCursor(il);

        var positionId = GetFirstLocalId(cursor, "Vector2");
        var mTextureId = GetFirstLocalId(cursor, "MTexture");

        VariableDefinition controllerId = new VariableDefinition(il.Import(typeof(RainbowTilesetController)));
        il.Body.Variables.Add(controllerId);

        cursor.EmitCall(getController);
        cursor.Emit(OpCodes.Stloc, controllerId);

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<SpriteBatch>("Draw"))) {
            cursor.Index--; // go back to the last step, which is when the color is loaded
            cursor.Emit(OpCodes.Ldloc_S, positionId); // pos
            cursor.Emit(OpCodes.Ldloc_S, mTextureId); // mTexture
            cursor.Emit(OpCodes.Ldarg_0); // this
            cursor.Emit(OpCodes.Ldloc, controllerId);
            cursor.EmitCall(getColor);
        }
    }

    private static RainbowTilesetController getController() {
        return Engine.Scene.Tracker.SafeGetEntity<RainbowTilesetController>()!;
    }

    private static Color getColor(Color col, Vector2 position, MTexture texture, TileGrid self, RainbowTilesetController controller) {
        if (controller is { } && contains(controller.TilesetTextures, texture.Parent)) {
            return ColorHelper.GetHue(Engine.Scene, position) * self.Alpha;
        }

        return col;
    }

    private static bool contains(MTexture[] arr, MTexture check) {
        for (int i = 0; i < arr.Length; i++) {
            //if (ReferenceEquals(arr[i], check))
            // SpeedrunTool's savestates will clone the tilegrid, desyncing the MTexture instances
            // from those generated by the autotiler
            // TODO:(Perf) Use string comparison only if savestates were used?
            if (arr[i].AtlasPath == check.AtlasPath)
                return true;
        }
        return false;
    }

    // OLD
    private static void TileGrid_RenderAt1(On.Monocle.TileGrid.orig_RenderAt orig, TileGrid self, Vector2 position) {
        if (self.Scene is null) {
            return;
        }

        var controller = self.Scene.Tracker.SafeGetEntity<RainbowTilesetController>();
        if (controller is null || self.Alpha <= 0f) {
            orig(self, position);
            return;
        }

        ColorHelper.SetGetHueScene(Engine.Scene);

        Rectangle clippedRenderTiles = self.GetClippedRenderTiles();

        int tileWidth = self.TileWidth;
        int tileHeight = self.TileHeight;

        Color color = self.Color * self.Alpha;
        Vector2 renderPos = new Vector2(position.X + clippedRenderTiles.Left * tileWidth, position.Y + clippedRenderTiles.Top * tileHeight);
        for (int i = clippedRenderTiles.Left; i < clippedRenderTiles.Right; i++) {
            for (int j = clippedRenderTiles.Top; j < clippedRenderTiles.Bottom; j++) {
                MTexture mtexture = self.Tiles[i, j];
                if (mtexture != null) {
                    Draw.SpriteBatch.Draw(mtexture.Texture.Texture, renderPos, mtexture.ClipRect,
                        color: contains(controller.TilesetTextures, mtexture.Parent)
                        ? ColorHelper.GetHue(renderPos) * self.Alpha
                        : color
                    );
                }
                renderPos.Y += tileHeight;
            }
            renderPos.X += tileWidth;
            renderPos.Y = position.Y + clippedRenderTiles.Top * tileHeight;
        }
    }

    [OnUnload]
    public static void Unload() {
        if (!_loadedHooks)
            return;
        _loadedHooks = false;

        On.Monocle.TileGrid.RenderAt -= TileGrid_RenderAt1;
        IL.Monocle.TileGrid.RenderAt -= TileGrid_RenderAt;
    }
    #endregion

    public MTexture[] TilesetTextures;
    public bool BG;

    public RainbowTilesetController(EntityData data, Vector2 offset) : base(data.Position + offset) {
        LoadHooksIfNeeded();

        BG = data.Bool("bg", false);
        var all = data.Attr("tilesets") == "*";
        var autotiler = BG ? GFX.BGAutotiler : GFX.FGAutotiler;
        Tag = Tags.Persistent;

        if (!all) {
            var TilesetIDs = FrostModule.GetCharArrayFromCommaSeparatedList(data.Attr("tilesets"));

            TilesetTextures = new MTexture[TilesetIDs.Length];
            for (int i = 0; i < TilesetTextures.Length; i++) {
                TilesetTextures[i] = autotiler.GenerateMap(new VirtualMap<char>(new char[,] { { TilesetIDs[i] } }), true).TileGrid.Tiles[0, 0].Parent;
            }
        } else {
            // Autotiler.lookup is Dictionary<char, Autotiler.TerrainType>
            // Autotiler.TerrainType is private, let's do some trickery
            var autotilerLookupKeys = (Autotiler_lookup.GetValue(autotiler) as IDictionary)!.Keys;
            TilesetTextures = new MTexture[autotilerLookupKeys.Count];
            var enumerator = autotilerLookupKeys.GetEnumerator();
            for (int i = 0; i < TilesetTextures.Length; i++) {
                enumerator.MoveNext();
                TilesetTextures[i] = autotiler.GenerateMap(new VirtualMap<char>(new char[,] { { (char) enumerator.Current } }), true).TileGrid.Tiles[0, 0].Parent;
            }
        }
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        var controllers = Scene.Tracker.SafeGetEntities<RainbowTilesetController>();
        if (controllers.Count > 1) {
            var first = controllers.First(c => c.Scene == scene) as RainbowTilesetController;
            if (first != this) {
                first!.TilesetTextures = first.TilesetTextures.Union(TilesetTextures).ToArray();
                RemoveSelf();
            }
        }
    }

    // Dictionary<char, Autotiler.TerrainType>
    private static FieldInfo Autotiler_lookup = typeof(Autotiler).GetField("lookup", BindingFlags.NonPublic | BindingFlags.Instance);
}
