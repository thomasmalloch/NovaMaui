using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Diagnostics;

namespace NovaMaui;

public partial class MainPage : ContentPage
{
	private Stopwatch Clock = new Stopwatch();

	public MainPage()
	{
		this.InitializeComponent();
		SKCanvasView view = new SKCanvasView();
		view.PaintSurface += this.OnCanvasViewPaintSurface;
		this.Content = view;
	}

	private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
	{
		SKImageInfo info = args.Info;
		SKSurface surface = args.Surface;
		SKCanvas canvas = surface.Canvas;

		canvas.Clear();

		SKPaint paint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = Colors.Red.ToSKColor(),
			StrokeWidth = 25
		};

		canvas.DrawCircle(info.Width / 2, info.Height / 2, 100, paint);
		paint.Style = SKPaintStyle.Fill;
		paint.Color = SKColors.Blue;
		canvas.DrawCircle(args.Info.Width / 2, args.Info.Height / 2, 100, paint);		
	}

	public void Clear() 
	{
	}	
}

