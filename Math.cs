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
	/// <summary>
	/// Provides helper methods.
	/// </summary>
	public static class MathHelper
	{
		/// <summary>
		/// Gets vector from yaw and pitch angles
		/// </summary>
		/// <param name="yaw">The yaw angle.</param>
		/// <param name="pitch">The pitch angle.</param>
		/// <returns>Vector3.</returns>
		public static Vector3 Vector3FromYawPitch(float yaw, float pitch)
		{
			return new Vector3(
			   (float)(System.Math.Sin(yaw) * System.Math.Cos(pitch)),
			   (float)-System.Math.Sin(pitch),
			   (float)(System.Math.Cos(yaw) * System.Math.Cos(pitch)));
		}

		/// <summary>
		///  Gets yaws, pitch and roll angles from vector.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>Vector3.</returns>
		public static Vector3 YawPitchRoll(this Vector3 vector)
		{
			return YawPitchRollFromVector3(vector);
		}

		/// <summary>
		/// Gets yaws, pitch and roll angles from vector.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>Vector3.</returns>
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

		/// <summary>
		/// Compute normal from triangle.
		/// </summary>
		/// <param name="a">a.</param>
		/// <param name="b">The b.</param>
		/// <param name="c">The c.</param>
		/// <returns>Vector3.</returns>
		public static Vector3 NormalFromTriangle(Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 dir = Vector3.Cross(b - a, c - a);
			return Vector3.Normalize(dir);
		}
	}
}