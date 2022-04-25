using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Diagnostics;
using Microsoft.Maui.Dispatching;
using NovaTools.Nova3D;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Markup;
using MvvmHelpers;
using System.Threading;

namespace NovaMaui;

public partial class MainPage : ContentPage
{
	const int ConstWidth = 640;
	const int ConstHeight = 480;

	private readonly Stopwatch Clock = new Stopwatch();
	private readonly Stopwatch DispacherStopwatch = new Stopwatch();
	private readonly SKBitmap PixelBuffer = new(ConstWidth, ConstHeight);
	private volatile bool IsAnimating = false;
	private EngineTest Test = new();
	private Input InputMap = new();
	private int FrameCount = 0;
	private float LastFrameUpdate;

	private volatile int FrameRate = 60;

	public MainPage()
	{
		this.InitializeComponent();
		/*this.View = new SKCanvasView();
		this.View.PaintSurface += this.OnCanvasViewPaintSurface;
		this.Content = this.View;*/
		
		this.Clock.Start();
		this.DispacherStopwatch.Start();

		Test.Setup(ConstWidth, ConstHeight, 1);
		Test.Run();

		//new Thread(this.AnimateGame).Start();
		IDispatcherTimer timer = this.Dispatcher.CreateTimer();
		timer.Interval = TimeSpan.FromTicks((long)((1000f / 60f) * TimeSpan.TicksPerMillisecond));
		timer.Tick += (sender, e) => this.AnimateGame();
		timer.Start();		
	}

    private async void AnimateGame()
    {
		while (this.IsAnimating)
			await Task.Delay(1);

		this.View.InvalidateSurface();
	}


    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
	{
		if (this.Test?.Camera?.CurrentNode == null)
			return;

		this.IsAnimating = true;		
		SKImageInfo info = args.Info;
		SKSurface surface = args.Surface;
		SKCanvas canvas = surface.Canvas;		
		float delta = this.Clock.ElapsedMilliseconds;
		this.LastFrameUpdate += delta;
		if (this.LastFrameUpdate < 1000f)
		{
			this.FrameCount++;
		}
		else 
		{
			this.FrameRate = this.FrameCount;
			this.FrameCount = 0;
			this.LastFrameUpdate = 0;
		}

		this.Clock.Restart();
		this.Test.DrawFrame(this.PixelBuffer, delta, this.InputMap);

		canvas.Clear();
		SKPoint p = new((float)this.Width / 2f, (float)this.Height / 2f);		
		canvas.DrawBitmap(this.PixelBuffer, p);
		this.IsAnimating = false;
	}

    private void Left_Pressed(object sender, EventArgs e)
    {
		if (this.InputMap.RotateLeft) 
		{
			this.InputMap.RotateLeft = false;
			this.InputMap.RotateRight = false;
		}
		else
		{
			this.InputMap.RotateLeft = true;
			this.InputMap.RotateRight = false;
		}
    }

	private void Right_Pressed(object sender, EventArgs e)
	{
		if (this.InputMap.RotateRight)
		{
			this.InputMap.RotateLeft = false;
			this.InputMap.RotateRight = false;
		}
		else
		{
			this.InputMap.RotateLeft = false;
			this.InputMap.RotateRight = true;
		}
	}

	private void Up_Pressed(object sender, EventArgs e)
	{
		if (this.InputMap.Forward)
		{
			this.InputMap.Forward = false;
			this.InputMap.Back = false;
		}
		else
		{
			this.InputMap.Forward = true;
			this.InputMap.Back = false;
		}
	}

	private void Down_Pressed(object sender, EventArgs e)
	{
		if (this.InputMap.Back)
		{
			this.InputMap.Forward = false;
			this.InputMap.Back = false;
		}
		else
		{
			this.InputMap.Back = true;
			this.InputMap.Forward = false;
		}
	}
}

