using System;
using System.Threading.Tasks;
using System.Diagnostics;

using Xamarin.Forms;
//https://github.com/marcofolio/XamLights/blob/master/XamLights/Views/Tile.cs
namespace BrainGames.Controls
{
    public class TileTappedEventArgs : EventArgs
    {
        public int XPos { get; set; }
        public int YPos { get; set; }
    }
    public class Tile : Grid
    {
        public event EventHandler<TileTappedEventArgs> Tapped;

        private readonly Stopwatch _stopWatch = new Stopwatch();
        private TimeSpan ts;

        private int _xPos;
        private int _yPos;
        private bool _frontIsShowing = true;
        public int XPos { get { return _xPos; } }
        public int YPos { get { return _yPos; } }
        public bool FrontIsShowing { get { return _frontIsShowing; } }

        private BoxView _background;
        private Image _foreground_img;
        private BoxView _foreground;
        private float TILE_SIZE;

        public Tile(int xPos, int yPos, float ts, Color bgColor, Color fgColor, string fg = null)
        {
            _xPos = xPos;
            _yPos = yPos;
            TILE_SIZE = ts;

            

            // Background
            _background = new BoxView { Color = bgColor, CornerRadius=10 };

            // Foreground
            if (fg != "")
            {
                _foreground_img = new Image
                {
                    RotationY = -90,
                    Source = ImageSource.FromResource($"XamLights.images.row-{yPos + 1}-col-{xPos + 1}.jpg")
                };
            }
            else
            {
                _foreground = new BoxView { /*RotationY = -90, */Color = fgColor, CornerRadius = 10 };
            }

            // Tapframe
            var tapFrame = CreateTapFrame();

            // The Background, Foreground and tapGrid are placed in the same Cell of the Grid
            // which causes them to stack on top of eachother
            Children.Add(_background, 0, 0);
            Children.Add(_foreground, 0, 0);
            Children.Add(tapFrame, 0, 0);
        }

        public void FlipIt()
        {
            if (_frontIsShowing)
            {
//                _foreground.Opacity = 0;
                _foreground.Color = Color.Yellow;

                _frontIsShowing = false;
            }
            else
            {
//                _foreground.Opacity = 1;
                _foreground.Color = Color.Gray;

                _frontIsShowing = true;
            }
        }

        public async Task Flip()
        {
            if (_frontIsShowing)
            {
                await _foreground.RotateYTo(-90, 400, Easing.CubicIn);
                _foreground.Opacity = 0;
                await _background.RotateYTo(0, 400, Easing.CubicOut);

                Console.WriteLine("flipping to gray {0},{1}",this.XPos,this.YPos);
                _frontIsShowing = false;
            }
            else
            {
                await _background.RotateYTo(90, 400, Easing.CubicIn);
                _foreground.Opacity = 1;
                await _foreground.RotateYTo(0, 400, Easing.CubicOut);

                Console.WriteLine("flipping to yellow {0},{1}", this.XPos, this.YPos);
                _frontIsShowing = true;
            }
        }

        private Frame CreateTapFrame()
        {
            var frame = new Frame()
            {
                WidthRequest = TILE_SIZE,
                HeightRequest = TILE_SIZE,
                BackgroundColor = Color.Transparent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HasShadow = false
            };
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnFrameTapped;
            frame.GestureRecognizers.Add(tapGestureRecognizer);
            return frame;
        }

        protected async Task Flash()
        {
            this._foreground.Color = Color.Green;
            await Task.Delay(100);
//            ts = TimeSpan.FromMilliseconds(30);
//            _stopWatch.Restart();
//            Device.StartTimer(ts, TimerLoop);
            this._foreground.Color = Color.Gray;
        }

        private bool TimerLoop()
        {
            var dt = _stopWatch.Elapsed.TotalMilliseconds;
            if (dt < 10000) return true;
            return false;
        }

        protected async virtual void OnFrameTapped(object sender, EventArgs e)
        {
            this.Flash();
            var handler = Tapped;
            if (handler != null)
            {
                handler(this, new TileTappedEventArgs() { XPos = _xPos, YPos = _yPos });
            }
        }
    }
}
