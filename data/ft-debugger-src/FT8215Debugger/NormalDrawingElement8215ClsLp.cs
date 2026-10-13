using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace FT8215Debugger;

internal class NormalDrawingElement8215ClsLp : FrameworkElement
{
	public const ushort MAX_BUF_SIZE = 10;

	public const ushort MAX_SPEED_CNT5 = 5;

	public const ushort MAX_SPEED_PWM_CNT6 = 6;

	public const ushort MAX_SPD_98EN_CNT7 = 7;

	public const ushort MAX_VOL_CNT5 = 5;

	public Pen blackPen = new Pen(Brushes.Yellow, DrawingConstants.PenThickness);

	public Pen redPen = new Pen(Brushes.Black, DrawingConstants.PenThickness);

	public Pen whitePen = new Pen(null, DrawingConstants.PenThickness);

	public double xLineWidth;

	public double yLineHeight;

	public Point stopPoint1;

	public Point stopPoint2;

	public Point stopPoint3;

	public Point runPoint1;

	public Point runPoint2;

	public Point runPoint3;

	public ushort wStopVLow = 1000;

	public ushort wStopVRange = 2000;

	public ushort wStopVLowVRange = 5000;

	public float fStopSminDutyHyster = 10f;

	public float fStopVLowVRangeMax = 10000f;

	public ushort wRunVLow = 0;

	public ushort wRunVRange = 0;

	public ushort wRunVLowVRange = 0;

	public float fRunSminDutyHyster = 0f;

	public float fRunVLowVRangeMax = 0f;

	public float fDutyMax = 100f;

	public byte bySminMode = 1;

	private byte m_byCtrlMode = 2;

	public float[] m_fTableCurveDuty = new float[10];

	public float[] m_fTableCurvesRPM = new float[10];

	private int m_i32Cnt = 5;

	private byte m_byPwmOffDis = 0;

	private byte m_byCurveEnSpd100 = 0;

	private byte m_byX98Dis = 0;

	private byte m_byPwmRevDis = 0;

	private byte m_byYMaxSel = 0;

	public void SetCtrlCurveParam(byte byCurveEnSpd, byte byX98Dis, byte byPwmRevDis)
	{
		m_byCurveEnSpd100 = byCurveEnSpd;
		m_byX98Dis = byX98Dis;
		m_byPwmRevDis = byPwmRevDis;
	}

	public void SetYMaxSel(byte value)
	{
		m_byYMaxSel = value;
	}

	public void InitEfuseBuffer(byte byPwmOffDis, byte byCtrlMode)
	{
		int num = 0;
		for (num = 0; num < 10; num++)
		{
			m_fTableCurveDuty[num] = 0f;
			m_fTableCurvesRPM[num] = 0f;
		}
		m_byCtrlMode = byCtrlMode;
		m_byPwmOffDis = byPwmOffDis;
		if (m_byCtrlMode == 0)
		{
			if (m_byCurveEnSpd100 == 1)
			{
				if (m_byX98Dis == 1)
				{
					m_i32Cnt = 6;
				}
				else
				{
					m_i32Cnt = 5;
				}
			}
			else
			{
				m_i32Cnt = 5;
			}
		}
		else if (m_byCurveEnSpd100 == 1)
		{
			if (m_byX98Dis == 1)
			{
				if (byPwmOffDis == 1)
				{
					m_i32Cnt = 6;
				}
				else
				{
					m_i32Cnt = 7;
				}
			}
			else if (byPwmOffDis == 1)
			{
				m_i32Cnt = 5;
			}
			else
			{
				m_i32Cnt = 6;
			}
		}
		else if (m_byX98Dis == 1)
		{
			if (byPwmOffDis == 1)
			{
				m_i32Cnt = 6;
			}
			else
			{
				m_i32Cnt = 7;
			}
		}
		else if (byPwmOffDis == 1)
		{
			m_i32Cnt = 5;
		}
		else
		{
			m_i32Cnt = 6;
		}
	}

	public void strAxis(DrawingContext dct, string strText, Point xTextPnt)
	{
		int length = strText.Length;
		FormattedText formattedText = new FormattedText(strText, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 32.0, Brushes.LightBlue);
		formattedText.MaxTextWidth = 90.0;
		formattedText.MaxTextHeight = 80.0;
		formattedText.SetFontSize(12.0, 0, length);
		dct.DrawText(formattedText, xTextPnt);
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);
		if (m_byCtrlMode == 0)
		{
			DrawingVoltageCurve(drawingContext);
		}
		else if (m_byCtrlMode == 1)
		{
			DrawingCurrentCurve(drawingContext);
		}
		else
		{
			DrawingSpeedCurve(drawingContext);
		}
	}

	protected void DrawingSpeedCurve(DrawingContext drawingContext)
	{
		double num = Math.Floor(base.RenderSize.Width / (double)DrawingConstants.Columms);
		double offsetY = Math.Floor(base.RenderSize.Height / (double)DrawingConstants.Rows);
		xLineWidth = Math.Floor(base.RenderSize.Width);
		yLineHeight = Math.Floor(base.RenderSize.Height);
		byte b = 50;
		yLineHeight -= 20.0;
		drawingContext.DrawLine(point0: new Point((int)b, yLineHeight), point1: new Point(xLineWidth, yLineHeight), pen: blackPen);
		drawingContext.DrawLine(point0: new Point(xLineWidth, yLineHeight), point1: new Point(xLineWidth - 5.0, yLineHeight - 3.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point(xLineWidth, yLineHeight), point1: new Point(xLineWidth - 5.0, yLineHeight + 3.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point((int)b, yLineHeight), point1: new Point((int)b, 0.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point((int)b, 0.0), point1: new Point(b + 3, 5.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point((int)b, 0.0), point1: new Point(b - 3, 5.0), pen: blackPen);
		xLineWidth -= 40.0;
		float num2 = 0f;
		Pen pen = new Pen(Brushes.LightGray, 1.0);
		pen.DashStyle = new DashStyle(new List<double> { 3.0, 6.0 }, 2.0);
		if (m_byPwmRevDis == 0)
		{
			for (int i = 0; i < m_i32Cnt - 1; i += 2)
			{
				float num3;
				float num4;
				float num5;
				float num6;
				Point point;
				Point point2;
				float num7;
				float num8;
				Point point9;
				Point point10;
				Point point11;
				if (m_i32Cnt == 7)
				{
					num3 = ((m_fTableCurveDuty[i] != 98f) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i] - 5.0) / (double)fDutyMax) + (double)(int)b)));
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 1] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
					if (m_fTableCurveDuty[i + 1] == 98f)
					{
						num5 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 1] - 5.0) / (double)fDutyMax) + (double)(int)b);
						num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
					}
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point5 = new Point(num2, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
					num7 = ((m_fTableCurveDuty[i + 2] != 98f) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 2] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 2] - 5.0) / (double)fDutyMax) + (double)(int)b)));
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 2] / fStopVLowVRangeMax));
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 7 && i + 2 == 6)
					{
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				Point point14;
				Point point15;
				if (m_i32Cnt == 5)
				{
					num3 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i] / fDutyMax) + (double)(int)b);
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 1] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					if (m_i32Cnt == 5 && i == 2)
					{
						Point point3 = new Point((int)b, num4);
						Point point4 = point;
						drawingContext.DrawLine(pen, point3, point4);
						Point point12 = point;
						Point point13 = new Point(num3, yLineHeight);
						drawingContext.DrawLine(pen, point12, point13);
						Point point5 = new Point(num2, num6);
						Point point6 = point2;
						Point point7 = point2;
						Point point8 = new Point(num5, yLineHeight);
						drawingContext.DrawLine(pen, point7, point8);
					}
					num7 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 2] / fDutyMax) + (double)(int)b);
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 2] / fStopVLowVRangeMax));
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 5 && i == 2)
					{
						point14 = new Point(num2, num8);
						point15 = point9;
						drawingContext.DrawLine(pen, point14, point15);
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				if (m_byX98Dis == 1 && m_byPwmOffDis == 1)
				{
					num3 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i] / fDutyMax) + (double)(int)b);
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 1] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
					if (m_byX98Dis == 1 && m_byPwmOffDis == 1 && m_fTableCurveDuty[i] == 98f)
					{
						num3 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i] - 5.0) / (double)fDutyMax) + (double)(int)b);
						num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
					}
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					if ((m_i32Cnt == 6 || m_i32Cnt % 2 == 0) && i != 0)
					{
						Point point3 = ((i != 2) ? new Point(num2, num4) : new Point((int)b, num4));
						Point point4 = point;
						drawingContext.DrawLine(pen, point3, point4);
						Point point12 = point;
						Point point13 = new Point(num3, yLineHeight);
						drawingContext.DrawLine(pen, point12, point13);
						Point point5 = new Point(num2, num6);
						Point point6 = point2;
						Point point7 = point2;
						Point point8 = new Point(num5, yLineHeight);
						drawingContext.DrawLine(pen, point7, point8);
					}
					if ((m_i32Cnt == 6 || m_i32Cnt % 2 == 0) && i == m_i32Cnt - 2)
					{
						break;
					}
					num7 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 2] / fDutyMax) + (double)(int)b);
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 2] / fStopVLowVRangeMax));
					if (m_byX98Dis == 1 && m_byPwmOffDis == 1 && m_fTableCurveDuty[i + 2] == 98f)
					{
						num7 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 2] - 5.0) / (double)fDutyMax) + (double)(int)b);
					}
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 5 && i == 2)
					{
						point14 = new Point(num2, num8);
						point15 = point9;
						drawingContext.DrawLine(pen, point14, point15);
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				num3 = (((double)m_fTableCurveDuty[i] != 98.0) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i] - 5.0) / (double)fDutyMax) + (double)(int)b)));
				num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
				num5 = (((double)m_fTableCurveDuty[i + 1] != 98.0) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 1] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 1] - 5.0) / (double)fDutyMax) + (double)(int)b)));
				num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
				if (i + 1 == 1)
				{
					num2 = num5;
				}
				point = new Point(num3, num4);
				point2 = new Point(num5, num6);
				drawingContext.DrawLine(blackPen, point, point2);
				if (point.X == point2.X)
				{
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point5 = new Point(num2, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
				}
				else if (point.Y == point2.Y)
				{
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
				}
				else
				{
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point5 = new Point(num2, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
				}
				if ((m_i32Cnt == 6 || m_i32Cnt % 2 == 0) && i == m_i32Cnt - 2)
				{
					break;
				}
				num7 = (((double)m_fTableCurveDuty[i + 2] != 98.0) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 2] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 2] - 5.0) / (double)fDutyMax) + (double)(int)b)));
				num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 2] / fStopVLowVRangeMax));
				point9 = new Point(num7, num8);
				drawingContext.DrawLine(blackPen, point2, point9);
				point14 = new Point(num2, num8);
				point15 = point9;
				drawingContext.DrawLine(pen, point14, point15);
				point10 = point9;
				point11 = new Point(num7, yLineHeight);
				drawingContext.DrawLine(pen, point10, point11);
			}
		}
		else
		{
			byte b2 = (byte)(m_i32Cnt - 1);
			for (int i = 0; i < m_i32Cnt - 1; i += 2)
			{
				float num3;
				float num4;
				float num5;
				float num6;
				Point point;
				Point point2;
				float num7;
				float num8;
				Point point9;
				Point point10;
				Point point11;
				if (m_i32Cnt == 7)
				{
					num3 = ((m_fTableCurveDuty[b2 - i] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - i] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - i] + 5.0) / (double)fDutyMax) + (double)(int)b)));
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 1)] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					if (m_fTableCurveDuty[b2 - (i + 1)] == (float)((double)fDutyMax - 98.0))
					{
						num5 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 1)] + 5.0) / (double)fDutyMax) + (double)(int)b);
						num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					}
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					if (i != 4)
					{
						drawingContext.DrawLine(pen, point3, point4);
					}
					Point point5 = new Point((int)b, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
					num7 = ((m_fTableCurveDuty[b2 - (i + 2)] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 2)] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 2)] + 5.0) / (double)fDutyMax) + (double)(int)b)));
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 2)] / fStopVLowVRangeMax));
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 7 && i + 2 == 6)
					{
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				Point point14;
				Point point15;
				if (m_i32Cnt == 5)
				{
					num3 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - i] / fDutyMax) + (double)(int)b);
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 1)] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					if (m_i32Cnt == 5 && i == 0)
					{
						Point point3 = new Point((int)b, num4);
						Point point4 = point;
						drawingContext.DrawLine(pen, point3, point4);
						Point point12 = point;
						Point point13 = new Point(num3, yLineHeight);
						drawingContext.DrawLine(pen, point12, point13);
						Point point5 = new Point((int)b, num6);
						Point point6 = point2;
						drawingContext.DrawLine(pen, point5, point6);
						Point point7 = point2;
						Point point8 = new Point(num5, yLineHeight);
						drawingContext.DrawLine(pen, point7, point8);
					}
					if (m_i32Cnt == 5 && i == 2)
					{
						Point point5 = new Point((int)b, num6);
						Point point6 = point2;
						drawingContext.DrawLine(pen, point5, point6);
						Point point7 = point2;
						Point point8 = new Point(num5, yLineHeight);
					}
					num7 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 2)] / fDutyMax) + (double)(int)b);
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 2)] / fStopVLowVRangeMax));
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					point14 = new Point(num2, num8);
					point15 = point9;
					point10 = point9;
					point11 = new Point(num7, yLineHeight);
					drawingContext.DrawLine(pen, point10, point11);
					continue;
				}
				if (m_byX98Dis == 1 && m_byPwmOffDis == 1)
				{
					num3 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - i] / fDutyMax) + (double)(int)b);
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 1)] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					if (m_byX98Dis == 1 && m_byPwmOffDis == 1 && m_fTableCurveDuty[b2 - (i + 1)] == (float)((double)fDutyMax - 98.0))
					{
						num5 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 1)] + 5.0) / (double)fDutyMax) + (double)(int)b);
						num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					}
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					Point point3 = ((i != 4) ? new Point((int)b, num4) : new Point(num2, num4));
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point5 = new Point(num2, num6);
					Point point6 = point2;
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
					if (m_i32Cnt % 2 == 0 && i == m_i32Cnt - 2)
					{
						break;
					}
					num7 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 2)] / fDutyMax) + (double)(int)b);
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 2)] / fStopVLowVRangeMax));
					if (m_byX98Dis == 1 && m_byPwmOffDis == 1 && m_fTableCurveDuty[b2 - (i + 2)] == (float)((double)fDutyMax - 98.0))
					{
						num7 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 2)] + 5.0) / (double)fDutyMax) + (double)(int)b);
					}
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 5 && i == 2)
					{
						point14 = new Point(num2, num8);
						point15 = point9;
						drawingContext.DrawLine(pen, point14, point15);
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				num3 = ((m_fTableCurveDuty[b2 - i] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - i] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - i] + 5.0) / (double)fDutyMax) + (double)(int)b)));
				num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - i] / fStopVLowVRangeMax));
				num5 = ((m_fTableCurveDuty[b2 - (i + 1)] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 1)] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 1)] + 5.0) / (double)fDutyMax) + (double)(int)b)));
				num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
				if (i + 1 == 1)
				{
					num2 = num5;
				}
				point = new Point(num3, num4);
				point2 = new Point(num5, num6);
				drawingContext.DrawLine(blackPen, point, point2);
				if (point.X == point2.X)
				{
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point5 = new Point((int)b, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
				}
				else if (point.Y == point2.Y)
				{
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
				}
				else
				{
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point5 = new Point((int)b, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
				}
				if (m_i32Cnt % 2 == 0 && i == m_i32Cnt - 2)
				{
					break;
				}
				num7 = ((m_fTableCurveDuty[b2 - (i + 2)] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 2)] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 2)] + 5.0) / (double)fDutyMax) + (double)(int)b)));
				num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 2)] / fStopVLowVRangeMax));
				point9 = new Point(num7, num8);
				drawingContext.DrawLine(blackPen, point2, point9);
				point14 = new Point((int)b, num8);
				point15 = point9;
				drawingContext.DrawLine(pen, point14, point15);
				point10 = point9;
				point11 = new Point(num7, yLineHeight);
				drawingContext.DrawLine(pen, point10, point11);
			}
		}
		string strText = ((m_byCtrlMode != 3) ? "Speed(RPM)" : "Power(W)");
		Point xTextPnt = new Point(b + 3, 0.0);
		strAxis(drawingContext, strText, xTextPnt);
		string strText2 = "Duty";
		xTextPnt = new Point(xLineWidth + 10.0, yLineHeight - 18.0);
		strAxis(drawingContext, strText2, xTextPnt);
		string text = "";
		text = ((m_byPwmRevDis != 0) ? (m_fTableCurveDuty[0] + "%") : (m_fTableCurveDuty[m_i32Cnt - 1] + "%"));
		xTextPnt = new Point(xLineWidth - 15.0, yLineHeight);
		strAxis(drawingContext, text, xTextPnt);
		float num9 = 0f;
		string strText3 = num9.ToString();
		xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(num9 / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
		strAxis(drawingContext, strText3, xTextPnt);
		strText3 = m_fTableCurveDuty[1] + "%";
		xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[1] / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
		string text2 = m_fTableCurvesRPM[1].ToString();
		if (text2.CompareTo("0") != 0)
		{
			xTextPnt = new Point(10.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[1] / fStopVLowVRangeMax));
			strAxis(drawingContext, text2, xTextPnt);
		}
		string text3 = m_fTableCurvesRPM[m_i32Cnt - 1].ToString();
		if (m_byCurveEnSpd100 == 1 && m_byX98Dis == 1)
		{
			text3 = "0";
		}
		if (text3.CompareTo("0") != 0)
		{
			xTextPnt = new Point(10.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[m_i32Cnt - 1] / fStopVLowVRangeMax));
			strAxis(drawingContext, text3, xTextPnt);
		}
		if (m_byPwmRevDis == 0)
		{
			int num10 = 0;
			for (num10 = 2; num10 < m_i32Cnt - 1; num10++)
			{
				float num11 = m_fTableCurveDuty[num10];
				if (num11 == 98f)
				{
					num11 -= 5f;
				}
				if (num11 == 0f)
				{
					string text4 = num11.ToString();
				}
				else
				{
					string text4 = num11.ToString("0") + "%";
					xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(num11 / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
					if (m_fTableCurveDuty[num10] == 98f)
					{
						num11 = m_fTableCurveDuty[num10];
						text4 = num11.ToString("0");
					}
					strAxis(drawingContext, text4, xTextPnt);
				}
				string text5 = m_fTableCurvesRPM[num10].ToString();
				if (text5.CompareTo("0") != 0)
				{
					if (num10 != 0 && m_fTableCurveDuty[num10] == 98f && m_fTableCurveDuty[num10 - 1] == 98f)
					{
						num11 = (float)(100.0 - 1.7857142857142858 * (double)(int)m_byYMaxSel);
						text5 = $"{Math.Round(num11, 1):F1}%";
					}
					xTextPnt = new Point(10.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[num10] / fStopVLowVRangeMax));
					strAxis(drawingContext, text5, xTextPnt);
				}
			}
		}
		else
		{
			int num10 = 0;
			for (num10 = 2; num10 < m_i32Cnt - 1; num10++)
			{
				float num11 = m_fTableCurveDuty[num10];
				if (num11 == 2f)
				{
					num11 += 6f;
				}
				if (num11 == (float)((double)fDutyMax - 0.0))
				{
					string text4 = num11.ToString();
				}
				else
				{
					string text4 = num11.ToString("0") + "%";
					xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(num11 / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
					if (m_fTableCurveDuty[num10] == 2f)
					{
						num11 = m_fTableCurveDuty[num10];
						text4 = num11.ToString("0") + "%";
					}
					strAxis(drawingContext, text4, xTextPnt);
				}
				string text5 = m_fTableCurvesRPM[num10].ToString();
				if (text5.CompareTo("0") != 0)
				{
					if (num10 != 0 && m_fTableCurveDuty[num10] == 2f && m_fTableCurveDuty[num10 - 1] == 2f)
					{
						num11 = (float)(100.0 - 1.7857142857142858 * (double)(int)m_byYMaxSel);
						text5 = $"{Math.Round(num11, 1):F1}%";
					}
					xTextPnt = new Point(10.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[num10] / fStopVLowVRangeMax));
					strAxis(drawingContext, text5, xTextPnt);
				}
			}
		}
		Point point16 = new Point((int)b, 0.0);
		Point point17 = new Point(xLineWidth, 0.0);
		for (int j = 0; (double)j <= (double)DrawingConstants.Rows * 1.75; j++)
		{
			drawingContext.PushGuidelineSet(new GuidelineSet(null, new double[2]
			{
				point17.Y - DrawingConstants.HalfOfPenThickness,
				point17.Y + DrawingConstants.HalfOfPenThickness
			}));
			drawingContext.DrawLine(whitePen, point16, point17);
			drawingContext.Pop();
			point16.Offset(0.0, offsetY);
			point17.Offset(0.0, offsetY);
		}
	}

	protected void DrawingCurrentCurve(DrawingContext drawingContext)
	{
		double num = Math.Floor(base.RenderSize.Width / (double)DrawingConstants.Columms);
		double offsetY = Math.Floor(base.RenderSize.Height / (double)DrawingConstants.Rows);
		xLineWidth = Math.Floor(base.RenderSize.Width);
		yLineHeight = Math.Floor(base.RenderSize.Height);
		byte b = 50;
		yLineHeight -= 20.0;
		drawingContext.DrawLine(point0: new Point((int)b, yLineHeight), point1: new Point(xLineWidth, yLineHeight), pen: blackPen);
		drawingContext.DrawLine(point0: new Point(xLineWidth, yLineHeight), point1: new Point(xLineWidth - 5.0, yLineHeight - 3.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point(xLineWidth, yLineHeight), point1: new Point(xLineWidth - 5.0, yLineHeight + 3.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point((int)b, yLineHeight), point1: new Point((int)b, 0.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point((int)b, 0.0), point1: new Point(b + 3, 5.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point((int)b, 0.0), point1: new Point(b - 3, 5.0), pen: blackPen);
		xLineWidth -= 40.0;
		float num2 = 0f;
		Pen pen = new Pen(Brushes.LightGray, 1.0);
		pen.DashStyle = new DashStyle(new List<double> { 3.0, 6.0 }, 2.0);
		if (m_byPwmRevDis == 0)
		{
			for (int i = 0; i < m_i32Cnt - 1; i += 2)
			{
				float num3;
				float num4;
				float num5;
				float num6;
				Point point;
				Point point2;
				float num7;
				float num8;
				Point point9;
				Point point10;
				Point point11;
				if (m_i32Cnt == 7)
				{
					num3 = ((m_fTableCurveDuty[i] != 98f) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i] - 5.0) / (double)fDutyMax) + (double)(int)b)));
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 1] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
					if (m_fTableCurveDuty[i + 1] == 98f)
					{
						num5 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 1] - 5.0) / (double)fDutyMax) + (double)(int)b);
						num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
					}
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point5 = new Point(num2, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
					num7 = ((m_fTableCurveDuty[i + 2] != 98f) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 2] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 2] - 5.0) / (double)fDutyMax) + (double)(int)b)));
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 2] / fStopVLowVRangeMax));
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 7 && i + 2 == 6)
					{
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				Point point14;
				Point point15;
				if (m_i32Cnt == 5)
				{
					num3 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i] / fDutyMax) + (double)(int)b);
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 1] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					if (m_i32Cnt == 5 && i == 2)
					{
						Point point3 = new Point((int)b, num4);
						Point point4 = point;
						drawingContext.DrawLine(pen, point3, point4);
						Point point12 = point;
						Point point13 = new Point(num3, yLineHeight);
						drawingContext.DrawLine(pen, point12, point13);
						Point point5 = new Point(num2, num6);
						Point point6 = point2;
						Point point7 = point2;
						Point point8 = new Point(num5, yLineHeight);
						drawingContext.DrawLine(pen, point7, point8);
					}
					num7 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 2] / fDutyMax) + (double)(int)b);
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 2] / fStopVLowVRangeMax));
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 5 && i == 2)
					{
						point14 = new Point(num2, num8);
						point15 = point9;
						drawingContext.DrawLine(pen, point14, point15);
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				if (m_byX98Dis == 1 && m_byPwmOffDis == 1)
				{
					num3 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i] / fDutyMax) + (double)(int)b);
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 1] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
					if (m_byX98Dis == 1 && m_byPwmOffDis == 1 && m_fTableCurveDuty[i] == 98f)
					{
						num3 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i] - 5.0) / (double)fDutyMax) + (double)(int)b);
						num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
					}
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					if ((m_i32Cnt == 6 || m_i32Cnt % 2 == 0) && i != 0)
					{
						Point point3 = ((i != 2) ? new Point(num2, num4) : new Point((int)b, num4));
						Point point4 = point;
						drawingContext.DrawLine(pen, point3, point4);
						Point point12 = point;
						Point point13 = new Point(num3, yLineHeight);
						drawingContext.DrawLine(pen, point12, point13);
						Point point5 = new Point(num2, num6);
						Point point6 = point2;
						Point point7 = point2;
						Point point8 = new Point(num5, yLineHeight);
						drawingContext.DrawLine(pen, point7, point8);
					}
					if ((m_i32Cnt == 6 || m_i32Cnt % 2 == 0) && i == m_i32Cnt - 2)
					{
						break;
					}
					num7 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 2] / fDutyMax) + (double)(int)b);
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 2] / fStopVLowVRangeMax));
					if (m_byX98Dis == 1 && m_byPwmOffDis == 1 && m_fTableCurveDuty[i + 2] == 98f)
					{
						num7 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 2] - 5.0) / (double)fDutyMax) + (double)(int)b);
					}
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 5 && i == 2)
					{
						point14 = new Point(num2, num8);
						point15 = point9;
						drawingContext.DrawLine(pen, point14, point15);
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				num3 = (((double)m_fTableCurveDuty[i] != 98.0) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i] - 5.0) / (double)fDutyMax) + (double)(int)b)));
				num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
				num5 = (((double)m_fTableCurveDuty[i + 1] != 98.0) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 1] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 1] - 5.0) / (double)fDutyMax) + (double)(int)b)));
				num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
				if (i + 1 == 1)
				{
					num2 = num5;
				}
				point = new Point(num3, num4);
				point2 = new Point(num5, num6);
				drawingContext.DrawLine(blackPen, point, point2);
				if (point.X == point2.X)
				{
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point5 = new Point(num2, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
				}
				else if (point.Y == point2.Y)
				{
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
				}
				else
				{
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point5 = new Point(num2, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
				}
				if ((m_i32Cnt == 6 || m_i32Cnt % 2 == 0) && i == m_i32Cnt - 2)
				{
					break;
				}
				num7 = (((double)m_fTableCurveDuty[i + 2] != 98.0) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 2] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 2] - 5.0) / (double)fDutyMax) + (double)(int)b)));
				num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 2] / fStopVLowVRangeMax));
				point9 = new Point(num7, num8);
				drawingContext.DrawLine(blackPen, point2, point9);
				point14 = new Point(num2, num8);
				point15 = point9;
				drawingContext.DrawLine(pen, point14, point15);
				point10 = point9;
				point11 = new Point(num7, yLineHeight);
				drawingContext.DrawLine(pen, point10, point11);
			}
		}
		else
		{
			byte b2 = (byte)(m_i32Cnt - 1);
			for (int i = 0; i < m_i32Cnt - 1; i += 2)
			{
				float num3;
				float num4;
				float num5;
				float num6;
				Point point;
				Point point2;
				float num7;
				float num8;
				Point point9;
				Point point10;
				Point point11;
				if (m_i32Cnt == 7)
				{
					num3 = ((m_fTableCurveDuty[b2 - i] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - i] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - i] + 5.0) / (double)fDutyMax) + (double)(int)b)));
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 1)] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					if (m_fTableCurveDuty[b2 - (i + 1)] == (float)((double)fDutyMax - 98.0))
					{
						num5 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 1)] + 5.0) / (double)fDutyMax) + (double)(int)b);
						num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					}
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					if (i != 4)
					{
						drawingContext.DrawLine(pen, point3, point4);
					}
					Point point5 = new Point((int)b, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
					num7 = ((m_fTableCurveDuty[b2 - (i + 2)] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 2)] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 2)] + 5.0) / (double)fDutyMax) + (double)(int)b)));
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 2)] / fStopVLowVRangeMax));
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 7 && i + 2 == 6)
					{
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				Point point14;
				Point point15;
				if (m_i32Cnt == 5)
				{
					num3 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - i] / fDutyMax) + (double)(int)b);
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 1)] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					if (m_i32Cnt == 5 && i == 0)
					{
						Point point3 = new Point((int)b, num4);
						Point point4 = point;
						drawingContext.DrawLine(pen, point3, point4);
						Point point12 = point;
						Point point13 = new Point(num3, yLineHeight);
						drawingContext.DrawLine(pen, point12, point13);
						Point point5 = new Point((int)b, num6);
						Point point6 = point2;
						drawingContext.DrawLine(pen, point5, point6);
						Point point7 = point2;
						Point point8 = new Point(num5, yLineHeight);
						drawingContext.DrawLine(pen, point7, point8);
					}
					if (m_i32Cnt == 5 && i == 2)
					{
						Point point5 = new Point((int)b, num6);
						Point point6 = point2;
						drawingContext.DrawLine(pen, point5, point6);
						Point point7 = point2;
						Point point8 = new Point(num5, yLineHeight);
					}
					num7 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 2)] / fDutyMax) + (double)(int)b);
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 2)] / fStopVLowVRangeMax));
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					point14 = new Point(num2, num8);
					point15 = point9;
					point10 = point9;
					point11 = new Point(num7, yLineHeight);
					drawingContext.DrawLine(pen, point10, point11);
					continue;
				}
				if (m_byX98Dis == 1 && m_byPwmOffDis == 1)
				{
					num3 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - i] / fDutyMax) + (double)(int)b);
					num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - i] / fStopVLowVRangeMax));
					num5 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 1)] / fDutyMax) + (double)(int)b);
					num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					if (m_byX98Dis == 1 && m_byPwmOffDis == 1 && m_fTableCurveDuty[b2 - (i + 1)] == (float)((double)fDutyMax - 98.0))
					{
						num5 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 1)] + 5.0) / (double)fDutyMax) + (double)(int)b);
						num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
					}
					if (i + 1 == 1)
					{
						num2 = num5;
					}
					point = new Point(num3, num4);
					point2 = new Point(num5, num6);
					drawingContext.DrawLine(blackPen, point, point2);
					Point point3 = ((i != 4) ? new Point((int)b, num4) : new Point(num2, num4));
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point5 = new Point(num2, num6);
					Point point6 = point2;
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
					if (m_i32Cnt % 2 == 0 && i == m_i32Cnt - 2)
					{
						break;
					}
					num7 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 2)] / fDutyMax) + (double)(int)b);
					num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 2)] / fStopVLowVRangeMax));
					if (m_byX98Dis == 1 && m_byPwmOffDis == 1 && m_fTableCurveDuty[b2 - (i + 2)] == (float)((double)fDutyMax - 98.0))
					{
						num7 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 2)] + 5.0) / (double)fDutyMax) + (double)(int)b);
					}
					point9 = new Point(num7, num8);
					drawingContext.DrawLine(blackPen, point2, point9);
					if (m_i32Cnt == 5 && i == 2)
					{
						point14 = new Point(num2, num8);
						point15 = point9;
						drawingContext.DrawLine(pen, point14, point15);
						point10 = point9;
						point11 = new Point(num7, yLineHeight);
						drawingContext.DrawLine(pen, point10, point11);
					}
					continue;
				}
				num3 = ((m_fTableCurveDuty[b2 - i] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - i] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - i] + 5.0) / (double)fDutyMax) + (double)(int)b)));
				num4 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - i] / fStopVLowVRangeMax));
				num5 = ((m_fTableCurveDuty[b2 - (i + 1)] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 1)] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 1)] + 5.0) / (double)fDutyMax) + (double)(int)b)));
				num6 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
				if (i + 1 == 1)
				{
					num2 = num5;
				}
				point = new Point(num3, num4);
				point2 = new Point(num5, num6);
				drawingContext.DrawLine(blackPen, point, point2);
				if (point.X == point2.X)
				{
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point5 = new Point((int)b, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
				}
				else if (point.Y == point2.Y)
				{
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
				}
				else
				{
					Point point3 = new Point((int)b, num4);
					Point point4 = point;
					drawingContext.DrawLine(pen, point3, point4);
					Point point12 = point;
					Point point13 = new Point(num3, yLineHeight);
					drawingContext.DrawLine(pen, point12, point13);
					Point point5 = new Point((int)b, num6);
					Point point6 = point2;
					drawingContext.DrawLine(pen, point5, point6);
					Point point7 = point2;
					Point point8 = new Point(num5, yLineHeight);
					drawingContext.DrawLine(pen, point7, point8);
				}
				if (m_i32Cnt % 2 == 0 && i == m_i32Cnt - 2)
				{
					break;
				}
				num7 = ((m_fTableCurveDuty[b2 - (i + 2)] != (float)((double)fDutyMax - 98.0)) ? ((float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 2)] / fDutyMax) + (double)(int)b)) : ((float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 2)] + 5.0) / (double)fDutyMax) + (double)(int)b)));
				num8 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 2)] / fStopVLowVRangeMax));
				point9 = new Point(num7, num8);
				drawingContext.DrawLine(blackPen, point2, point9);
				point14 = new Point((int)b, num8);
				point15 = point9;
				drawingContext.DrawLine(pen, point14, point15);
				point10 = point9;
				point11 = new Point(num7, yLineHeight);
				drawingContext.DrawLine(pen, point10, point11);
			}
		}
		string strText = "Current(A)";
		Point xTextPnt = new Point(b + 3, 0.0);
		strAxis(drawingContext, strText, xTextPnt);
		string strText2 = "Duty";
		xTextPnt = new Point(xLineWidth + 10.0, yLineHeight - 18.0);
		strAxis(drawingContext, strText2, xTextPnt);
		string text = "100%";
		text = ((m_byPwmRevDis != 0) ? (m_fTableCurveDuty[0] + "%") : (m_fTableCurveDuty[m_i32Cnt - 1] + "%"));
		xTextPnt = new Point(xLineWidth - 15.0, yLineHeight);
		strAxis(drawingContext, text, xTextPnt);
		float num9 = 0f;
		string strText3 = num9.ToString();
		xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(num9 / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
		strAxis(drawingContext, strText3, xTextPnt);
		strText3 = m_fTableCurveDuty[1] + "%";
		xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[1] / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
		string text2 = m_fTableCurvesRPM[1].ToString("0.000");
		if (text2.CompareTo("0.000") != 0)
		{
			xTextPnt = new Point(6.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[1] / fStopVLowVRangeMax));
			strAxis(drawingContext, text2, xTextPnt);
		}
		string text3 = m_fTableCurvesRPM[m_i32Cnt - 1].ToString("0.000");
		if (m_byCurveEnSpd100 == 1 && m_byX98Dis == 1)
		{
			text3 = "0.000";
		}
		if (text3.CompareTo("0.000") != 0)
		{
			xTextPnt = new Point(6.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[m_i32Cnt - 1] / fStopVLowVRangeMax));
			strAxis(drawingContext, text3, xTextPnt);
		}
		if (m_byPwmRevDis == 0)
		{
			int num10 = 0;
			for (num10 = 2; num10 < m_i32Cnt - 1; num10++)
			{
				float num11 = m_fTableCurveDuty[num10];
				if (num11 == 98f)
				{
					num11 -= 5f;
				}
				if (num11 == 0f)
				{
					string text4 = num11.ToString();
				}
				else
				{
					string text4 = num11.ToString("0") + "%";
					xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(num11 / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
					if (m_fTableCurveDuty[num10] == 98f)
					{
						num11 = m_fTableCurveDuty[num10];
						text4 = num11.ToString("0");
					}
					strAxis(drawingContext, text4, xTextPnt);
				}
				string text5 = m_fTableCurvesRPM[num10].ToString("0.000");
				if (text5.CompareTo("0.000") != 0)
				{
					if (num10 != 0 && m_fTableCurveDuty[num10] == 98f && m_fTableCurveDuty[num10 - 1] == 98f)
					{
						num11 = (float)(100.0 - 1.7857142857142858 * (double)(int)m_byYMaxSel);
						text5 = $"{Math.Round(num11, 1):F1}%";
					}
					xTextPnt = new Point(6.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[num10] / fStopVLowVRangeMax));
					strAxis(drawingContext, text5, xTextPnt);
				}
			}
		}
		else
		{
			int num10 = 0;
			for (num10 = 2; num10 < m_i32Cnt - 1; num10++)
			{
				float num11 = m_fTableCurveDuty[num10];
				if (num11 == 2f)
				{
					num11 += 6f;
				}
				if (num11 == (float)((double)fDutyMax - 0.0))
				{
					string text4 = num11.ToString();
				}
				else
				{
					string text4 = num11.ToString("0") + "%";
					xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(num11 / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
					if (m_fTableCurveDuty[num10] == 2f)
					{
						num11 = m_fTableCurveDuty[num10];
						text4 = num11.ToString("0") + "%";
					}
					strAxis(drawingContext, text4, xTextPnt);
				}
				string text5 = m_fTableCurvesRPM[num10].ToString("0.000");
				if (text5.CompareTo("0.000") != 0)
				{
					if (num10 != 0 && m_fTableCurveDuty[num10] == 2f && m_fTableCurveDuty[num10 - 1] == 2f)
					{
						num11 = (float)(100.0 - 1.7857142857142858 * (double)(int)m_byYMaxSel);
						text5 = $"{Math.Round(num11, 1):F1}%";
					}
					xTextPnt = new Point(6.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[num10] / fStopVLowVRangeMax));
					strAxis(drawingContext, text5, xTextPnt);
				}
			}
		}
		Point point16 = new Point((int)b, 0.0);
		Point point17 = new Point(xLineWidth, 0.0);
		for (int j = 0; (double)j <= (double)DrawingConstants.Rows * 1.75; j++)
		{
			drawingContext.PushGuidelineSet(new GuidelineSet(null, new double[2]
			{
				point17.Y - DrawingConstants.HalfOfPenThickness,
				point17.Y + DrawingConstants.HalfOfPenThickness
			}));
			drawingContext.DrawLine(whitePen, point16, point17);
			drawingContext.Pop();
			point16.Offset(0.0, offsetY);
			point17.Offset(0.0, offsetY);
		}
	}

	protected void DrawingVoltageCurve(DrawingContext drawingContext)
	{
		double num = Math.Floor(base.RenderSize.Width / (double)DrawingConstants.Columms);
		double offsetY = Math.Floor(base.RenderSize.Height / (double)DrawingConstants.Rows);
		xLineWidth = Math.Floor(base.RenderSize.Width);
		yLineHeight = Math.Floor(base.RenderSize.Height);
		byte b = 55;
		yLineHeight -= 20.0;
		drawingContext.DrawLine(point0: new Point((int)b, yLineHeight), point1: new Point(xLineWidth, yLineHeight), pen: blackPen);
		drawingContext.DrawLine(point0: new Point(xLineWidth, yLineHeight), point1: new Point(xLineWidth - 5.0, yLineHeight - 3.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point(xLineWidth, yLineHeight), point1: new Point(xLineWidth - 5.0, yLineHeight + 3.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point((int)b, yLineHeight), point1: new Point((int)b, 0.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point((int)b, 0.0), point1: new Point(b + 3, 5.0), pen: blackPen);
		drawingContext.DrawLine(point0: new Point((int)b, 0.0), point1: new Point(b - 3, 5.0), pen: blackPen);
		xLineWidth -= 40.0;
		Pen pen = new Pen(Brushes.LightGray, 1.0);
		pen.DashStyle = new DashStyle(new List<double> { 3.0, 6.0 }, 2.0);
		if (m_byPwmRevDis == 0)
		{
			for (int i = 0; i < m_i32Cnt - 1; i += 2)
			{
				float num2 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i] / fDutyMax) + (double)(int)b);
				float num3 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
				float num4 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 1] / fDutyMax) + (double)(int)b);
				float num5 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 1] / fStopVLowVRangeMax));
				if (m_fTableCurveDuty[i] == 98f)
				{
					num2 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i] - 5.0) / (double)fDutyMax) + (double)(int)b);
					num3 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i] / fStopVLowVRangeMax));
				}
				if (m_fTableCurveDuty[i + 1] == 98f)
				{
					num4 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 1] - 5.0) / (double)fDutyMax) + (double)(int)b);
				}
				Point point = new Point(num2, num3);
				Point point2 = new Point(num4, num5);
				drawingContext.DrawLine(blackPen, point, point2);
				Point point3 = new Point((int)b, num3);
				Point point4 = point;
				drawingContext.DrawLine(pen, point3, point4);
				Point point5 = point;
				Point point6 = new Point(num2, yLineHeight);
				drawingContext.DrawLine(pen, point5, point6);
				Point point7 = new Point((int)b, num5);
				Point point8 = point2;
				drawingContext.DrawLine(pen, point7, point8);
				Point point9 = point2;
				Point point10 = new Point(num4, yLineHeight);
				drawingContext.DrawLine(pen, point9, point10);
				if (i == m_i32Cnt - 2)
				{
					break;
				}
				float num6 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[i + 2] / fDutyMax) + (double)(int)b);
				if (m_fTableCurveDuty[i + 2] == 98f)
				{
					num6 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[i + 2] - 5.0) / (double)fDutyMax) + (double)(int)b);
				}
				float num7 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[i + 2] / fStopVLowVRangeMax));
				Point point11 = new Point(num6, num7);
				drawingContext.DrawLine(blackPen, point2, point11);
				Point point12 = new Point((int)b, num7);
				Point point13 = point11;
				drawingContext.DrawLine(pen, point12, point13);
				Point point14 = point11;
				Point point15 = new Point(num6, yLineHeight);
				drawingContext.DrawLine(pen, point14, point15);
			}
		}
		else
		{
			byte b2 = (byte)(m_i32Cnt - 1);
			for (int i = 0; i < m_i32Cnt - 1; i += 2)
			{
				float num2 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - i] / fDutyMax) + (double)(int)b);
				float num3 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - i] / fStopVLowVRangeMax));
				float num4 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 1)] / fDutyMax) + (double)(int)b);
				float num5 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
				if (m_fTableCurveDuty[b2 - (i + 1)] == (float)((double)fDutyMax - 98.0))
				{
					num4 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 1)] + 5.0) / (double)fDutyMax) + (double)(int)b);
					num5 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 1)] / fStopVLowVRangeMax));
				}
				Point point = new Point(num2, num3);
				Point point2 = new Point(num4, num5);
				drawingContext.DrawLine(blackPen, point, point2);
				Point point3 = new Point((int)b, num3);
				Point point4 = point;
				drawingContext.DrawLine(pen, point3, point4);
				Point point5 = point;
				Point point6 = new Point(num2, yLineHeight);
				drawingContext.DrawLine(pen, point5, point6);
				Point point7 = new Point((int)b, num5);
				Point point8 = point2;
				drawingContext.DrawLine(pen, point7, point8);
				Point point9 = point2;
				Point point10 = new Point(num4, yLineHeight);
				drawingContext.DrawLine(pen, point9, point10);
				if (i == m_i32Cnt - 2)
				{
					break;
				}
				float num6 = (float)((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[b2 - (i + 2)] / fDutyMax) + (double)(int)b);
				if (m_fTableCurveDuty[b2 - (i + 2)] == (float)((double)fDutyMax - 98.0))
				{
					num6 = (float)((xLineWidth - (double)(int)b) * (((double)m_fTableCurveDuty[b2 - (i + 2)] + 5.0) / (double)fDutyMax) + (double)(int)b);
				}
				float num7 = (float)(yLineHeight - yLineHeight * (double)(m_fTableCurvesRPM[b2 - (i + 2)] / fStopVLowVRangeMax));
				Point point11 = new Point(num6, num7);
				drawingContext.DrawLine(blackPen, point2, point11);
				Point point12 = new Point((int)b, num7);
				Point point13 = point11;
				drawingContext.DrawLine(pen, point12, point13);
				Point point14 = point11;
				Point point15 = new Point(num6, yLineHeight);
				drawingContext.DrawLine(pen, point14, point15);
			}
		}
		string strText = "OutDuty(%)";
		Point xTextPnt = new Point(b + 3, 0.0);
		strAxis(drawingContext, strText, xTextPnt);
		string strText2 = "Duty";
		xTextPnt = new Point(xLineWidth + 10.0, yLineHeight - 18.0);
		strAxis(drawingContext, strText2, xTextPnt);
		string strText3 = "100%";
		if (m_byPwmRevDis == 0)
		{
			strText3 = m_fTableCurveDuty[m_i32Cnt - 1] + "%";
		}
		xTextPnt = new Point(xLineWidth - 15.0, yLineHeight);
		strAxis(drawingContext, strText3, xTextPnt);
		float num8 = 0f;
		string strText4 = num8.ToString();
		xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(num8 / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
		strAxis(drawingContext, strText4, xTextPnt);
		strText4 = m_fTableCurveDuty[1].ToString("0") + "%";
		xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(m_fTableCurveDuty[1] / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
		strAxis(drawingContext, strText4, xTextPnt);
		string strText5 = m_fTableCurvesRPM[1].ToString();
		xTextPnt = new Point(10.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[1] / fStopVLowVRangeMax));
		strAxis(drawingContext, strText5, xTextPnt);
		string strText6 = ((m_fTableCurvesRPM[m_i32Cnt - 1] != 100f) ? m_fTableCurvesRPM[m_i32Cnt - 1].ToString("0.000") : m_fTableCurvesRPM[m_i32Cnt - 1].ToString("0"));
		xTextPnt = new Point(10.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[m_i32Cnt - 1] / fStopVLowVRangeMax));
		strAxis(drawingContext, strText6, xTextPnt);
		if (m_byPwmRevDis == 0)
		{
			int num9 = 0;
			for (num9 = 0; num9 < m_i32Cnt - 1; num9++)
			{
				if (m_fTableCurvesRPM[num9] == 0f || num9 == 1)
				{
					continue;
				}
				float num10 = m_fTableCurveDuty[num9];
				if (num10 == 98f)
				{
					num10 -= 5f;
				}
				if (num10 == 0f)
				{
					string text = num10.ToString();
				}
				else
				{
					string text = num10.ToString("0") + "%";
					xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(num10 / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
					if (m_fTableCurveDuty[num9] == 98f)
					{
						num10 = m_fTableCurveDuty[num9];
						text = num10.ToString("0");
					}
					strAxis(drawingContext, text, xTextPnt);
				}
				string text2 = m_fTableCurvesRPM[num9].ToString("0.000");
				text2 = ((m_fTableCurvesRPM[num9] != 100f) ? m_fTableCurvesRPM[num9].ToString("0.000") : m_fTableCurvesRPM[num9].ToString("0"));
				if (m_fTableCurvesRPM[num9] != 0f)
				{
					xTextPnt = new Point(10.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[num9] / fStopVLowVRangeMax));
					strAxis(drawingContext, text2, xTextPnt);
				}
			}
		}
		else
		{
			int num9 = 0;
			for (num9 = 0; num9 < m_i32Cnt - 1; num9++)
			{
				if (m_fTableCurvesRPM[num9] == 0f || num9 == 1)
				{
					continue;
				}
				float num10 = m_fTableCurveDuty[num9];
				if (num10 == 2f)
				{
					num10 += 6f;
				}
				if (num10 == 0f)
				{
					string text = num10.ToString();
				}
				else
				{
					string text = num10.ToString("0") + "%";
					xTextPnt = new Point((xLineWidth - (double)(int)b) * (double)(num10 / fDutyMax) + (double)(int)b - 10.0, yLineHeight);
					if (m_fTableCurveDuty[num9] == 2f)
					{
						num10 = m_fTableCurveDuty[num9];
						text = num10.ToString("0") + "%";
					}
					strAxis(drawingContext, text, xTextPnt);
				}
				string text2 = m_fTableCurvesRPM[num9].ToString("0.000");
				text2 = ((m_fTableCurvesRPM[num9] != 100f) ? m_fTableCurvesRPM[num9].ToString("0.000") : m_fTableCurvesRPM[num9].ToString("0"));
				if (m_fTableCurvesRPM[num9] != 0f)
				{
					xTextPnt = new Point(10.0, yLineHeight - 8.0 - yLineHeight * (double)(m_fTableCurvesRPM[num9] / fStopVLowVRangeMax));
					strAxis(drawingContext, text2, xTextPnt);
				}
			}
		}
		Point point16 = new Point((int)b, 0.0);
		Point point17 = new Point(xLineWidth, 0.0);
		for (int j = 0; (double)j <= (double)DrawingConstants.Rows * 1.75; j++)
		{
			drawingContext.PushGuidelineSet(new GuidelineSet(null, new double[2]
			{
				point17.Y - DrawingConstants.HalfOfPenThickness,
				point17.Y + DrawingConstants.HalfOfPenThickness
			}));
			drawingContext.DrawLine(whitePen, point16, point17);
			drawingContext.Pop();
			point16.Offset(0.0, offsetY);
			point17.Offset(0.0, offsetY);
		}
	}

	public void SetPointCount(int i32PointCnt)
	{
		m_i32Cnt = i32PointCnt;
	}
}
