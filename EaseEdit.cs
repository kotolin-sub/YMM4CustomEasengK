using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Effects;

namespace YMMCustomEaseK
{
    [VideoEffect("カスタムイージングK", new[] { "配置" }, new string[] { }, isAviUtlSupported: false)]
    internal class EaseEdit : VideoEffectBase
    {

        public override string Label => "カスタムイージングK";

        [Display(Name = "Func")]
        [TextEditor(AcceptsReturn = true, MinHeight = 100)]
        public string Text { get => text; set => Set(ref text, value); }
        string text = "return 0";

        [Display(Name = "対象")]
        [EnumComboBox()]
        public Subject Enum { get => subject; set => Set(ref subject, value); }
        Subject subject = Subject.X;

        [Display(Name = "インスタンスの共有")]
        [ToggleSlider]
        public bool Toggle { get => toggle; set => Set(ref toggle, value); }
        bool toggle = true;

        [Display(Name = "スクリプト")]
        [FileSelector(YukkuriMovieMaker.Settings.FileGroupType.None)]
        public string File { get => file; set => Set(ref file, value); }
        string file = "";


        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return Enumerable.Empty<string>();
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new EaseEditP(this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => Array.Empty<IAnimatable>();

        public enum Subject
        {
            [Display(Name = "X")]
            X,
            [Display(Name = "Y")]
            Y,
            [Display(Name = "Z")]
            ZX,
            [Display(Name = "拡大率")]
            Z,
            [Display(Name = "Z回転角")]
            R,
            [Display(Name = "X回転角")]
            XR,
            [Display(Name = "Y回転角")]
            YR,
        }
    }
}
