using System;
using System.IO;
using System.Runtime.InteropServices;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace YMMCustomEaseK
{
    internal class EaseEditP : IVideoEffectProcessor
    {
        private readonly EaseEdit item;
        private ID2D1Image? input;
        private double ReT = 0;

        private static readonly IntPtr sharedLuaState;
        private static readonly object luaLock = new object();

        static EaseEditP()
        {
            sharedLuaState = LuaJIT.luaL_newstate();
            LuaJIT.luaL_openlibs(sharedLuaState);
        }

        public ID2D1Image Output => input ?? throw new NullReferenceException(nameof(input) + " は null です");

        public EaseEditP(EaseEdit item)
        {
            this.item = item;
        }

        [Obsolete]
        public DrawDescription Update(EffectDescription effectDescription)
        {
            var (frame, length, fps, layer) = (effectDescription.ItemPosition.Frame, effectDescription.ItemDuration.Frame, effectDescription.FPS, effectDescription.Layer);
            var drawDesc = effectDescription.DrawDescription;
            var (x, y, zp, r, xr, yr, zo, cx, cy) = (drawDesc.DrawPoint.X, drawDesc.DrawPoint.Y, drawDesc.DrawPointZ, drawDesc.Rotation.Z, drawDesc.Rotation.X, drawDesc.Rotation.Y, drawDesc.Zoom.X, drawDesc.CenterPoint.X, drawDesc.CenterPoint.Y);

            var (file, subject, text) = (item.File, item.Enum, item.Text);

            ReT = 0;

            string luaScriptPrefix = $@"
    local _FRAME={frame} 
    local _LENGTH={length} 
    local _FPS={fps} 
    local _LAYER={layer} 
    local _X={x} 
    local _Y={y} 
    local _Z={zp}
    local _R={r}
    local _RX={xr}
    local _RY={yr}
    local _ZO={zo} 
    local _CX={cx} 
    local _CY={cy}
    ";

            try { text = File.ReadAllText(file); } catch { }

            string luaScript = luaScriptPrefix + "\n" + text;

            lock (luaLock)
            {
                ExecLuaScript(luaScript);
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
                EaseEdit.Subject.ZX => new DrawDescription(
                    drawDesc,
                    draw: new System.Numerics.Vector3(
                        drawDesc.DrawPoint.X,
                        drawDesc.DrawPoint.Y,
                        (float)(drawDesc.DrawPointZ + ReT)
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

        private void ExecLuaScript(string luaScript)
        {
            if (LuaJIT.luaL_loadstring(sharedLuaState, luaScript) == 0 && LuaJIT.lua_pcall(sharedLuaState, 0, 1, 0) == 0)
            {
                int top = LuaJIT.lua_gettop(sharedLuaState);
                if (top == 1)
                {
                    ReT = LuaJIT.lua_tonumber(sharedLuaState, -1);
                }
            }
            LuaJIT.lua_settop(sharedLuaState, 0);
        }

        public void ClearInput() => input = null;
        public void SetInput(ID2D1Image input) => this.input = input;
        public void Dispose() { }

        private static class LuaJIT
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
            public static extern double lua_tonumber(IntPtr luaState, int index);

            [DllImport(Lua51Dll, CallingConvention = CallingConvention.Cdecl)]
            public static extern int lua_gettop(IntPtr luaState);

            [DllImport(Lua51Dll, CallingConvention = CallingConvention.Cdecl)]
            public static extern void lua_settop(IntPtr luaState, int index);
        }
    }
}
