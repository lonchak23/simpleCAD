using System.Windows.Media;

namespace simpleCAD
{
	public enum TooltipType
	{
		eTooltip,
		eWarning,
		eError
	};

	public interface ITooltip
	{
		ImageSource Image { get; }
		string Header { get; }
		string TooltipText { get; }
		TooltipType Type { get; }
	}
}
