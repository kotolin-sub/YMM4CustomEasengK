using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace YMMCustomEaseK
{
    [VideoEffect("カスタムイージングK", new[] { "" }, new string[] { })]
    internal class EaseEdit : VideoEffectBase
    {


        public override string Label => "カスタムイージングK";
        /*[Display(Name = "X1", Description = "X1")]
        [AnimationSlider("F0", "", -100, 100)]
        public Animation X1 { get; } = new Animation(0, -10000, 10000);

        [Display(Name = "X2", Description = "X2")]
        [AnimationSlider("F0", "", -100, 100)]
        public Animation X2 { get; } = new Animation(0, -10000, 10000);*/

        [Display(Name = "Func", Description = "Func")]
        [TextEditor(AcceptsReturn = true)]
        public string Text { get => text; set => Set(ref text, value); }
        string text = string.Empty;

        [Display(Name = "error", Description = "error")]
        [TextEditor(AcceptsReturn = true)]
        public string Error { get => text; set => Set(ref text, value); }
        string error = string.Empty;

        //対象を設定できるようにしてた時の残骸
        //かと思ったけど普通にそのまま
        [Display(Name = "対象")]
        [EnumComboBox]
        public Subject Enum { get => subject; set => Set(ref subject, value); }
        Subject subject = Subject.X;

        [Display(GroupName = "グループ名", Name = "(関係ないよ)こいつを消す方法を教えてくれnull入れると堕ちるんだ", Description = "項目の説明")]
        [AnimationSlider("F0", "%", 0, 100)]
        public Animation Animation { get; } = new Animation(100, 0, 100);

        //よく見てないのでサンプルのまま
        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return Enumerable.Empty<string>();
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new EaseEditP(this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => new[] { Animation };

        public enum Subject
        {
            [Display(Name = "X")]
            X,
            [Display(Name = "Y")]
            Y,
            [Display(Name = "拡大率")]
            Z,
            [Display(Name ="回転角")]
            R
            /*[Display(Name ="中心座標(X)")]
            CX,
            [Display(Name ="中心座標(Y)")]
            CY*/
        }
    }
}
