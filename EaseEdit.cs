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

        /*[Display(Name = "error", Description = "error")]
        [TextEditor(AcceptsReturn = true)]
        public string Error { get => text; set => Set(ref text, value); }
        string error = string.Empty;*/

        //対象を設定できるようにしてた時
        //かと思ったけど普通にそのまま
        [Display(Name = "対象")]
        [EnumComboBox()]
        public Subject Enum { get => subject; set => Set(ref subject, value); }
        Subject subject = Subject.X;

        [Display(Name = "スクリプト")]
        [FileSelector(YukkuriMovieMaker.Settings.FileGroupType.None)]
        public string File { get => file; set => Set(ref file, value); }
        string file = "";

        public Animation piyo { get; } = new Animation(100, 0, 100);

        //よく見てないのでサンプルのまま
        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return Enumerable.Empty<string>();
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new EaseEditP(this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => new[] { piyo };

        public enum Subject
        {
            [Display(Name = "X")]
            X,
            [Display(Name = "Y")]
            Y,
            [Display(Name = "拡大率")]
            Z,
            [Display(Name = "Z回転角")]
            R,
            [Display(Name = "X回転角")]
            XR,
            [Display(Name = "Y回転角")]
            YR,
            /*[Display(Name = "反転")]
            INV
            [Display(Name ="中心座標(X)")]
            CX,
            [Display(Name ="中心座標(Y)")]
            CY*/
        }
    }

    /*public class EaseEditStart : IPlugin //ここコンボボックスに一覧で出そうとした残骸誰か実装したらプルリクしてね♡
    {
        public string Name => "カスタムイージングKStartUp";

        static EaseEditStart()
        {
            string directoryPath = @".\user\plugin\CustomEasengK\script";

            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            List<FileData> filesData = GetFilesData(directoryPath);

            string json = JsonSerializer.Serialize(filesData, new JsonSerializerOptions
            {
                WriteIndented = true
            });


            File.WriteAllText(@".\user\plugin\CustomEasengK\anmlist.json", json);
        }

        static List<FileData> GetFilesData(string directoryPath)
        {
            List<FileData> filesData = new List<FileData>();

            try
            {
                string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);

                    FileData fileData = new FileData
                    {
                        FileName = fileName,
                        FilePath = filePath 
                    };

                    filesData.Add(fileData);
                }
            }
            catch (Exception ex)
            {
            }

            return filesData;
        }
    }*/
}
