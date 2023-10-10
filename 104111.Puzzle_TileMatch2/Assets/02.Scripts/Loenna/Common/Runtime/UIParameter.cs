using System;

namespace NineTap.Common
{
	public abstract record UIParameter(params HUDType[] VisibleHUD);

	public record DefaultParameterWithoutHUD() : UIParameter(Array.Empty<HUDType>());
	public record DefaultParameter() : UIParameter(HUDType.ALL);
}
