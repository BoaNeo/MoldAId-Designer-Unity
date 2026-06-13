using System;

namespace Gizmos
{
	[Flags]
	public enum GizmoHandleFlags
	{
		None = 0x00,
		MoveX = 0x01,
		MoveY = 0x02,
		MoveZ = 0x04,
		MoveXYZ = 0x07,
		MoveView = 0x08,
		MoveAll = 0x0f,
		RotateX = 0x10,
		RotateY = 0x20,
		RotateZ = 0x40,
		RotateAll = 0x70,
		All = 0xff,
	}
}