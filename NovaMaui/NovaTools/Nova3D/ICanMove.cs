using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaTools.Nova3D;

public interface ICanMove
{
	float Angle { get; set; }
	float Sin { get; set; }
	float Cos { get; set; }
	float Sin90 { get; set; }
	float Cos90 { get; set; }

	public void UpdateAngle(float angle)
	{
		this.Angle = angle;
		this.Cos = MathF.Cos(angle);
		this.Sin = MathF.Sin(angle);
		this.Cos90 = MathF.Cos(angle + MathF.PI / 2f);
		this.Sin90 = MathF.Sin(angle + MathF.PI / 2f);
		while (this.Angle > (MathF.PI * 2))
			this.Angle -= (MathF.PI * 2);
		while (this.Angle < 0)
			this.Angle += (MathF.PI * 2);
	}
}
