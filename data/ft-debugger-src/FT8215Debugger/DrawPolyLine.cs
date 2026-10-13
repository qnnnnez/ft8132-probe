using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace FT8215Debugger;

internal class DrawPolyLine : FrameworkElement
{
	private Pen m_polyPen = new Pen(Brushes.Black, 1.0);

	private Pen m_dashPen = new Pen(Brushes.Gray, 1.0);

	private Pen m_axisPen = new Pen(Brushes.Black, 1.0);

	private double m_xMax = 100.0;

	private double m_xMin = 0.0;

	private double m_yMax = 100.0;

	private double m_yMin = 0.0;

	private double m_FontSize = 12.0;

	private Brush m_textColor = Brushes.Black;

	private Point m_startPosition = new Point(0.0, 0.0);

	private Point m_endPosition;

	private PointCollection m_dataSource = new PointCollection();

	private PointCollection m_points = new PointCollection();

	public Pen PolyPen
	{
		set
		{
			m_polyPen = value;
		}
	}

	public Pen DashPen
	{
		set
		{
			m_dashPen = value;
		}
	}

	public Pen AxisPen
	{
		set
		{
			m_axisPen = value;
		}
	}

	public Thickness Padding { get; set; }

	public PointCollection DataSource
	{
		get
		{
			return m_dataSource;
		}
		set
		{
			m_dataSource = value;
		}
	}

	public string XUnit { get; set; }

	public string YUnit { get; set; }

	public double FontSize
	{
		set
		{
			m_FontSize = value;
		}
	}

	public Brush TextColor
	{
		set
		{
			m_textColor = value;
		}
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);
		DrawCurve(drawingContext);
	}

	private void DrawCurve(DrawingContext drawingContext)
	{
		drawingContext.PushTransform(new TranslateTransform(0.0, base.RenderSize.Height));
		drawingContext.PushTransform(new ScaleTransform(1.0, -1.0));
		m_endPosition = new Point(base.RenderSize.Width - Padding.Right, base.RenderSize.Height - Padding.Top);
		DrawAxis(drawingContext);
		DrawXAxisTicks(drawingContext);
		DrawYAxisTicks(drawingContext);
		if (m_xMax != m_xMin && m_yMax != m_yMin)
		{
			m_xMax = ComputeXMax();
			m_yMax = ComputeYMax();
			double num = Math.Abs(base.RenderSize.Width * 0.9 / (m_xMax - m_xMin));
			double num2 = Math.Abs(base.RenderSize.Height * 0.9 / (m_yMax - m_yMin));
			for (int i = 0; i < m_dataSource.Count; i++)
			{
				Point value = new Point(m_dataSource[i].X * num, m_dataSource[i].Y * num2);
				m_points.Add(value);
			}
			DrawPlLine(drawingContext, m_points, showDashLine: true);
		}
	}

	private void DrawPlLine(DrawingContext drawingContext, PointCollection points, bool showDashLine)
	{
		m_dashPen.DashStyle = new DashStyle(new List<double> { 3.0, 6.0 }, 2.0);
		for (int i = 0; i < points.Count - 1; i++)
		{
			drawingContext.DrawLine(m_polyPen, points[i], points[i + 1]);
			if (showDashLine)
			{
				if (i == 0)
				{
					drawingContext.DrawLine(m_dashPen, new Point(m_startPosition.X, points[i].Y), points[i]);
					drawingContext.DrawLine(m_dashPen, new Point(points[i].X, m_startPosition.Y), points[i]);
				}
				drawingContext.DrawLine(m_dashPen, new Point(m_startPosition.X, points[i + 1].Y), points[i + 1]);
				drawingContext.DrawLine(m_dashPen, new Point(points[i + 1].X, m_startPosition.Y), points[i + 1]);
			}
		}
		for (int i = 0; i < points.Count - 1; i++)
		{
			if (i == 0)
			{
				if (points[i].X != m_startPosition.X)
				{
					DrawXText(drawingContext, m_dataSource[i].X.ToString(), new Point(points[i].X, m_startPosition.Y));
				}
				if (points[i].Y != m_startPosition.Y)
				{
					DrawYText(drawingContext, m_dataSource[i].Y.ToString("0.000"), new Point(m_startPosition.X, points[i].Y));
				}
			}
			DrawXText(drawingContext, m_dataSource[i + 1].X.ToString(), new Point(points[i + 1].X, m_startPosition.Y));
			DrawYText(drawingContext, m_dataSource[i + 1].Y.ToString("0.000"), new Point(m_startPosition.X, points[i + 1].Y));
		}
	}

	private void DrawAxis(DrawingContext drawingContext)
	{
		Point point = new Point(m_endPosition.X, m_startPosition.Y);
		Point point2 = new Point(m_startPosition.X, m_endPosition.Y);
		drawingContext.DrawLine(m_axisPen, m_startPosition, point);
		drawingContext.DrawLine(m_axisPen, new Point(point.X - 4.0, point.Y - 3.0), point);
		drawingContext.DrawLine(m_axisPen, new Point(point.X - 4.0, point.Y + 3.0), point);
		drawingContext.DrawLine(m_axisPen, m_startPosition, point2);
		drawingContext.DrawLine(m_axisPen, new Point(point2.X - 3.0, point2.Y - 4.0), point2);
		drawingContext.DrawLine(m_axisPen, new Point(point2.X + 3.0, point2.Y - 4.0), point2);
		DrawYText(drawingContext, "0", m_startPosition);
		DrawXText(drawingContext, XUnit, new Point(m_endPosition.X - 7.0, m_startPosition.Y - (6.0 + m_FontSize + 3.0)));
		DrawYText(drawingContext, YUnit, new Point(m_startPosition.X + (5.0 + m_FontSize / 2.0 + 3.0), m_endPosition.Y - 8.0));
	}

	private void DrawXAxisTicks(DrawingContext drawingContext)
	{
	}

	private void DrawYAxisTicks(DrawingContext drawingContext)
	{
	}

	private void DrawXText(DrawingContext drawingContext, string text, Point position)
	{
		drawingContext.PushTransform(new ScaleTransform(1.0, -1.0));
		FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"), m_FontSize, m_textColor);
		Point origin = new Point(position.X - m_FontSize / 2.0, position.Y + 2.0);
		drawingContext.DrawText(formattedText, origin);
		drawingContext.PushTransform(new ScaleTransform(1.0, -1.0));
	}

	private void DrawYText(DrawingContext drawingContext, string text, Point position)
	{
		drawingContext.PushTransform(new ScaleTransform(1.0, -1.0));
		FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.RightToLeft, new Typeface("Verdana"), m_FontSize, m_textColor);
		Point origin = new Point(position.X - 5.0, 0.0 - (position.Y + 8.0));
		drawingContext.DrawText(formattedText, origin);
		drawingContext.PushTransform(new ScaleTransform(1.0, -1.0));
	}

	private double ComputeXMax()
	{
		double num = 0.0;
		for (int i = 0; i < m_dataSource.Count; i++)
		{
			if (num < m_dataSource[i].X)
			{
				num = m_dataSource[i].X;
			}
		}
		return num;
	}

	private double ComputeYMax()
	{
		double num = 0.0;
		for (int i = 0; i < m_dataSource.Count; i++)
		{
			if (num < m_dataSource[i].Y)
			{
				num = m_dataSource[i].Y;
			}
		}
		return num;
	}

	public PointCollection GetCurvePoints()
	{
		return m_points.Clone();
	}
}
