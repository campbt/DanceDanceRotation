using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace DanceDanceRotationModule.Util
{
    public class ControlExtensions
    {
        public const float ButtonUnhoveredOpacity = 0.7f;
        public const float ButtonHoveredOpacity = 1.0f;
        public const float ButtonHoveredAnimationDuration = 0.2f;

        public static readonly Point ImageButtonSmallSize = new Point(24, 24);

        public static void ConvertToButton(
            Control image,
            float unhoveredOpacity = ButtonUnhoveredOpacity,
            float hoveredOpacity = ButtonHoveredOpacity
        )
        {
            image.MouseEntered += delegate
            {
                GameService.Animation.Tweener.Tween<Control>(
                    image,
                    (object) new
                    {
                        Opacity = hoveredOpacity
                    },
                    ButtonHoveredAnimationDuration
                );
            };
            image.MouseLeft += delegate
            {
                GameService.Animation.Tweener.Tween<Control>(
                    image,
                    (object) new
                    {
                        Opacity = unhoveredOpacity
                    },
                    ButtonHoveredAnimationDuration
                );
            };
            image.Opacity = unhoveredOpacity;
        }
    }
}