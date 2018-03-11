using System;
using System.Windows.Media;

namespace simpleCAD
{
	internal class Tooltip : ITooltip
	{
		internal Tooltip(string strHeader, ImageSource image, string strText, TooltipType type)
		{
			m_strHeader = strHeader;
			m_ImageSource = image;
			m_strTooltipText = strText;
			m_Type = type;
		}

		//=============================================================================
		private string m_strHeader = string.Empty;
		public string Header
		{
			get
			{
				return m_strHeader;
			}
		}

		//=============================================================================
		private ImageSource m_ImageSource = null;
		public ImageSource Image
		{
			get
			{
				return m_ImageSource;
			}
		}

		//=============================================================================
		private string m_strTooltipText = string.Empty;
		public string TooltipText
		{
			get
			{
				return m_strTooltipText;
			}
		}

		//=============================================================================
		private TooltipType m_Type = TooltipType.eTooltip;
		public TooltipType Type
		{
			get
			{
				return m_Type;
			}
		}
	}
}
