using MoonSharp.Interpreter;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Player.Video;

//深夜テンションで作ったクソコード達
namespace YMMCustomEaseK
{
    internal class EaseEditP : IVideoEffectProcessor
    {
        readonly EaseEdit item;
        ID2D1Image? input;

        Script script = new Script();

        public ID2D1Image Output => input ?? throw new NullReferenceException(nameof(input) + "is null");

        public EaseEditP(EaseEdit item)
        {
            this.item = item;
        }

        [Obsolete]
        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var subject = item.Enum;
            var text = item.Text;


            //
            script.Globals["_FRAME"] = frame;
            script.Globals["_LENGTH"] = length;
            script.Globals["_FPS"] = fps;

            /*script.Globals["X1"] = x1;
            script.Globals["X2"] = x2;*/


            try { script.DoString(text); } catch (Exception ex) { /*item.Error = Convert.ToString(ex);*/ };

            var ReT = script.Globals["ReT"];

            /*
            double t = (double)frame / (double)length;
            double easeT = Math.Pow((1 - Math.Cos(t * Math.PI)) / 2, len);
            var ret = x1 + (x2 - x1) * easeT;

            var X = effectDescription.DrawDescription.DrawPoint.X;
            var Y = effectDescription.DrawDescription.DrawPoint.Y;
            */

            switch (Convert.ToString(subject))
            {

                case "X":
                    return new DrawDescription(
                    effectDescription.DrawDescription,
                    draw: new System.Numerics.Vector2(
                        effectDescription.DrawDescription.DrawPoint.X + Convert.ToSingle(ReT),
                        effectDescription.DrawDescription.DrawPoint.Y
                        ));
                case "Y":
                    return new DrawDescription(
                    effectDescription.DrawDescription,
                    draw: new System.Numerics.Vector2(
                        effectDescription.DrawDescription.DrawPoint.X,
                        effectDescription.DrawDescription.DrawPoint.Y + Convert.ToSingle(ReT)
                        ));
                case "R"://TODO:実装
                    return effectDescription.DrawDescription;/*new DrawDescription(
                        effectDescription.DrawDescription,
                        rotation: new System.Numerics.Vector3(
                            effectDescription.DrawDescription.Rotation.X + Convert.ToSingle(ReT),
                            effectDescription.DrawDescription.Rotation.Y + Convert.ToSingle(ReT),
                            effectDescription.DrawDescription.Rotation.Z + Convert.ToSingle(ReT)
                            ));*/
                case "Z":
                    return new DrawDescription(
                        effectDescription.DrawDescription,
                        zoom: new System.Numerics.Vector2(
                            effectDescription.DrawDescription.Zoom.X + Convert.ToSingle(ReT)
                            ));
                default: return effectDescription.DrawDescription;
            }
        }
        public void ClearInput()
        {
            input = null;
        }
        public void SetInput(ID2D1Image input)
        {
            this.input = input;
        }

        public void Dispose()
        {

        }
    }
}
