// ***********************************************************************
// Assembly         : SharpDXWrapper
// Author           : Andrew
// Created          : 07-21-2017
//
// Last Modified By : Andrew
// Last Modified On : 07-21-2017
// ***********************************************************************
using SharpDX;
using System;

namespace SharpDXWrapper
{
	public static class MathHelper
	{
		public static Vector3 Vector3FromYawPitch(float yaw, float pitch)
		{
			return new Vector3(
			   (float)(System.Math.Sin(yaw) * System.Math.Cos(pitch)),
			   (float)-System.Math.Sin(pitch),
			   (float)(System.Math.Cos(yaw) * System.Math.Cos(pitch)));
		}

		public static Vector3 YawPitchRollFromVector3(Vector3 vector)
		{
			float yaw = 0;
			float pitch = 0;

			vector.Normalize();

			if (Math.Abs(vector.Z) < float.Epsilon)
			{
				if (vector.X > 0) yaw = MathUtil.PiOverTwo;
				else if (vector.X < 0) yaw = -MathUtil.PiOverTwo;
			}
			else
			{
				yaw = (float)Math.Atan(vector.X / vector.Z);
				if (vector.Z < 0) yaw += MathUtil.Pi;
			}

			if (Math.Abs(vector.X) > float.Epsilon || Math.Abs(vector.Z) > float.Epsilon)
			{
				pitch = -(float)Math.Atan(vector.Y / (float)Math.Sqrt(vector.X * vector.X + vector.Z * vector.Z));
			}
			else
			{
				if (vector.Y > 0)
					pitch = -MathUtil.PiOverTwo;
				if (vector.Y < 0)
					pitch = MathUtil.PiOverTwo;
			}
			return new Vector3(yaw, pitch, 0);
		}

		public static Vector3 NormalFromTriangle(Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 dir = Vector3.Cross(b - a, c - a);
			return Vector3.Normalize(dir);
		}
	}
}