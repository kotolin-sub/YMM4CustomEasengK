using System;
using System.Runtime.InteropServices;
using YukkuriMovieMaker.Player.Audio;
using YukkuriMovieMaker.Player.Audio.Effects;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Commons;
using DotnetWorld.API.Structs;
using DotnetWorld.API;
using System.ComponentModel.DataAnnotations;
using static YMMCustomEase_K_.WorldSample;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;

namespace YMMCustomEase_K_
{   //ここのコメントアウトを外すと音程操作が使えるようになる。ただしDotNetWorldが必要
    /*[AudioEffect("音程操作", new[] { "フィルター" }, new string[] { }, isAviUtlSupported: false)]*/
    public class EaseEditA : AudioEffectBase
    {
        public override string Label => "音程操作";

        [Display(Name = "Func")]
        [TextEditor(AcceptsReturn = true, MinHeight = 100)]
        public string Text { get => text; set => Set(ref text, value); }
        string text = "return 1";

        public Animation piyo { get; } = new Animation(100, 0, 100);

        public override IAudioEffectProcessor CreateAudioEffect(TimeSpan duration) => new EaseEditAP(this, duration);

        public override IEnumerable<string> CreateExoAudioFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => new[] { $"hoge=foo\r\n" };

        protected override IEnumerable<IAnimatable> GetAnimatables() => new[] { piyo };
    }

    internal class EaseEditAP : AudioEffectProcessorBase
    {
        readonly EaseEditA item;
        readonly TimeSpan duration;
        double ReT = 1;

        public override int Hz => Input?.Hz ?? 0;
        public override long Duration => (long)(duration.TotalSeconds * (Input?.Hz ?? 0)) * 2;

        public EaseEditAP(EaseEditA item, TimeSpan duration)
        {
            this.item = item;
            this.duration = duration;
        }

        protected override void seek(long position) => Input?.Seek(position);

        protected override int read(float[] destBuffer, int offset, int count)
        {
            Input?.Read(destBuffer, offset, count);
            string luaScript = $"{item.Text}";

            IntPtr luaState = LuaJIT.luaL_newstate();
            LuaJIT.luaL_openlibs(luaState);
            if (LuaJIT.luaL_loadstring(luaState, luaScript) == 0 && LuaJIT.lua_pcall(luaState, 0, 1, 0) == 0)
            {
                ReT = LuaJIT.lua_tonumber(luaState, -1);
            }
            LuaJIT.lua_close(luaState);

            int fs = Hz, x_length = count / 2;
            double[] x = new double[x_length];
            for (int i = 0; i < x_length; i++) x[i] = destBuffer[offset + i * 2];

            WorldParameters parameters = new WorldParameters { fs = fs, frame_period = 5.0 };
            WorldSample world = new WorldSample();
            world.F0EstimationHarvest(x, x_length, parameters);
            world.SpectralEnvelopeEstimation(x, x_length, parameters);
            world.AperiodicityEstimation(x, x_length, parameters);

            for (int i = 0; i < parameters.f0.Length; i++) parameters.f0[i] *= ReT;

            int y_length = (int)((parameters.f0_length - 1) * parameters.frame_period / 1000.0 * fs) + 1;
            double[] y = new double[y_length];
            world.WaveformSynthesis(parameters, fs, y_length, y);

            for (int i = 0; i < y_length; i++)
            {
                if (i * 2 < count)
                {
                    destBuffer[offset + i * 2] = (float)y[i];
                    destBuffer[offset + i * 2 + 1] = (float)y[i];
                }
            }

            return count;
        }
    }

    public class WorldParameters
    {
        public double frame_period;
        public int fs;
        public double[] f0;
        public double[] time_axis;
        public int f0_length;
        public double[,] spectrogram;
        public double[,] aperiodicity;
        public int fft_size;
    }

    public class WorldSample
    {
        public void F0EstimationHarvest(double[] x, int x_length, WorldParameters world_parameters)
        {
            var option = new HarvestOption();
            Core.InitializeHarvestOption(option);
            option.frame_period = world_parameters.frame_period;
            option.f0_floor = 71.0;

            world_parameters.f0_length = Core.GetSamplesForDIO(world_parameters.fs, x_length, world_parameters.frame_period);
            world_parameters.f0 = new double[world_parameters.f0_length];
            world_parameters.time_axis = new double[world_parameters.f0_length];

            Core.Harvest(x, x_length, world_parameters.fs, option, world_parameters.time_axis, world_parameters.f0);
        }

        public void SpectralEnvelopeEstimation(double[] x, int x_length, WorldParameters world_parameters)
        {
            var option = new CheapTrickOption();
            Core.InitializeCheapTrickOption(world_parameters.fs, option);
            option.q1 = -0.15;
            option.f0_floor = 71.0;

            world_parameters.fft_size = Core.GetFFTSizeForCheapTrick(world_parameters.fs, option);
            world_parameters.spectrogram = new double[world_parameters.f0_length, world_parameters.fft_size / 2 + 1];

            Core.CheapTrick(x, x_length, world_parameters.fs, world_parameters.time_axis, world_parameters.f0, world_parameters.f0_length, option, world_parameters.spectrogram);
        }

        public void AperiodicityEstimation(double[] x, int x_length, WorldParameters world_parameters)
        {
            var option = new D4COption();
            Core.InitializeD4COption(option);
            option.threshold = 0.85;

            world_parameters.aperiodicity = new double[world_parameters.f0_length, world_parameters.fft_size / 2 + 1];
            Core.D4C(x, x_length, world_parameters.fs, world_parameters.time_axis, world_parameters.f0, world_parameters.f0_length, world_parameters.fft_size, option, world_parameters.aperiodicity);
        }

        public void WaveformSynthesis(WorldParameters world_parameters, int fs, int y_length, double[] y)
        {
            Core.Synthesis(world_parameters.f0, world_parameters.f0_length, world_parameters.spectrogram, world_parameters.aperiodicity, world_parameters.fft_size, world_parameters.frame_period, fs, y_length, y);
        }

        public class LuaJIT
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
        }
    }
}
