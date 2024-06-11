using System;
using System.IO;
using System.Runtime.InteropServices;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Player.Video;

namespace YMMCustomEaseK
{
    internal class EaseEditP : IVideoEffectProcessor
    {
        string temp;

        readonly EaseEdit item;
        ID2D1Image? input;

        public ID2D1Image Output => input ?? throw new NullReferenceException(nameof(input) + " は null です");

        public EaseEditP(EaseEdit item)
        {
            this.item = item;
        }

        double ReT = 0;

        [Obsolete]
        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;
            var layer = effectDescription.Layer;

            var drawDesc = effectDescription.DrawDescription;
            var x = drawDesc.DrawPoint.X;
            var y = drawDesc.DrawPoint.Y;
            var zp = drawDesc.DrawPointZ;
            var r = drawDesc.Rotation.Z;
            var xr = drawDesc.Rotation.X;
            var yr = drawDesc.Rotation.Y;
            var zo = drawDesc.Zoom.X;
            var cx = drawDesc.CenterPoint.X;
            var cy = drawDesc.CenterPoint.Y;

            var file = item.File;
            var subject = item.Enum;
            var text = item.Text;


            ReT = 0;
            IntPtr luaState = LuaJIT.luaL_newstate();
            LuaJIT.luaL_openlibs(luaState);


            string luaScriptPrefix = $@"
            _FRAME={frame} 
            _LENGTH={length} 
            _FPS={fps} 
            _LAYER={layer} 
            _X={x} 
            _Y={y} 
            _Z={zp}
            _R={r}
            _RX={xr}
            _RY={yr}
            _ZO={zo} 
            _CX={cx} 
            _CY={cy}
            ";

            try { text = File.ReadAllText(file); } catch { } 

            string luaScript = luaScriptPrefix + "\n" + text;

            try
            {
                if (LuaJIT.luaL_loadstring(luaState, luaScript) == 0 && LuaJIT.lua_pcall(luaState, 0, 1, 0) == 0)
                {
                    int top = LuaJIT.lua_gettop(luaState);

                    if (top == 1)
                    {
                        ReT = LuaJIT.lua_tonumber(luaState, -1);
                    }
                }
            }
            finally
            {
                LuaJIT.lua_close(luaState);
            }


            return subject switch
            {
                EaseEdit.Subject.X => new DrawDescription(
                    drawDesc,
                    draw: new System.Numerics.Vector2(
                        drawDesc.DrawPoint.X + Convert.ToSingle(ReT),
                        drawDesc.DrawPoint.Y
                    )),
                EaseEdit.Subject.Y => new DrawDescription(
                    drawDesc,
                    draw: new System.Numerics.Vector2(
                        drawDesc.DrawPoint.X,
                        drawDesc.DrawPoint.Y + Convert.ToSingle(ReT)
                    )),
                EaseEdit.Subject.R => new DrawDescription(
                    drawDesc,
                    rotation: new System.Numerics.Vector3(
                        drawDesc.Rotation.X,
                        drawDesc.Rotation.Y,
                        drawDesc.Rotation.Z + Convert.ToSingle(ReT)
                    )),
                EaseEdit.Subject.Z => new DrawDescription(
                    drawDesc,
                    zoom: new System.Numerics.Vector2(
                        drawDesc.Zoom.X + Convert.ToSingle(ReT)
                    )),
                EaseEdit.Subject.XR => new DrawDescription(
                    drawDesc,
                    rotation: new System.Numerics.Vector3(
                        drawDesc.Rotation.X + Convert.ToSingle(ReT),
                        drawDesc.Rotation.Y,
                        drawDesc.Rotation.Z
                    )),
                EaseEdit.Subject.YR => new DrawDescription(
                    drawDesc,
                    rotation: new System.Numerics.Vector3(
                        drawDesc.Rotation.X,
                        drawDesc.Rotation.Y + Convert.ToSingle(ReT),
                        drawDesc.Rotation.Z
                    )),
                _ => drawDesc,
            };
        }

        public void ClearInput() => input = null;

        public void SetInput(ID2D1Image input) => this.input = input;

        public void Dispose()
        {
        }

        // LuaJIT用の関数群
        class LuaJIT
        {
            private const string Lua51Dll = "lua51.dll";

            [DllImport(Lua51Dll, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr luaL_newstate();

            [DllImport(Lua51Dll, CallingConvention = CallingConvention.Cdecl)]
            public static extern void luaL_openlibs(IntPtr luaState);

            [DllImport(Lua51Dll, CallingConvention = CallingConvention.Cdecl)]
            public static extern int luaL_loadstring(IntPtr luaState, string chunk);

            [DllImport(Lua51Dll, CallingConvention = CallingConvention.Cdecl)]
            public static extern int lua_pcall(IntPtr luaState, int nargs, int nresults, int errfunc);

            [DllImport(Lua51Dll, CallingConvention = CallingConvention.Cdecl)]
            public static extern void lua_close(IntPtr luaState);

            [DllImport(Lua51Dll, CallingConvention = CallingConvention.Cdecl)]
            public static extern double lua_tonumber(IntPtr luaState, int index);

            [DllImport(Lua51Dll, CallingConvention = CallingConvention.Cdecl)]
            public static extern int lua_gettop(IntPtr luaState);
        }
    }
}
