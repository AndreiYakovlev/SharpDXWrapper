// ***********************************************************************
// Assembly         : SharpDXWrapper
// Author           : Andrew
// Created          : 07-21-2017
//
// Last Modified By : Andrew
// Last Modified On : 07-21-2017
// ***********************************************************************
// <copyright file="Camera.cs" company="">
//     Copyright ©  2017
// </copyright>
// <summary></summary>
// ***********************************************************************
using SharpDX;

namespace SharpDXWrapper
{
	/// <summary>
	/// Class Camera.
	/// </summary>
	public abstract class Camera
	{
		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		/// <value>The position.</value>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Gets or sets the target.
		/// </summary>
		/// <value>The target.</value>
		public Vector3 Target { get; set; }

		/// <summary>
		/// Gets the view.
		/// </summary>
		/// <value>The view.</value>
		public Matrix View
		{
			get
			{
				return Matrix.LookAtLH(Position, Target, Vector3.Up);
			}
		}

		/// <summary>
		/// Gets or sets the rotation.
		/// </summary>
		/// <value>The rotation.</value>
		public abstract Vector3 Rotation { get; set; }

		/// <summary>
		/// Gets or sets the length.
		/// </summary>
		/// <value>The length.</value>
		public abstract float Length { get; set; }

		/// <summary>
		/// Rotates the specified angle.
		/// </summary>
		/// <param name="yawAngle">The yaw angle.</param>
		/// <param name="pitchAngle">The pitch angle.</param>
		/// <param name="rollAngle">The roll angle.</param>
		public abstract void Rotate(float yawAngle, float pitchAngle, float rollAngle);

		/// <summary>
		/// Moves the specified direction.
		/// </summary>
		/// <param name="direction">The direction.</param>
		public void Move(Vector3 direction)
		{
			Position += direction;
			Target += direction;
		}
	}

	/// <summary>
	/// Class FirstPersonCamera.
	/// </summary>
	/// <seealso cref="SharpDXWrapper.Camera" />
	public class FirstPersonCamera : Camera
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FirstPersonCamera"/> class.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="target">The target.</param>
		public FirstPersonCamera(Vector3 position = default(Vector3), Vector3 target = default(Vector3))
		{
			Position = position;
			Target = target;
			Length = (Position - Target).Length();
			Rotation = MathHelper.YawPitchRollFromVector3(Target - Position);
		}

		/// <summary>
		/// The length
		/// </summary>
		private float length;

		/// <summary>
		/// The rotation
		/// </summary>
		private Vector3 rotation;

		/// <summary>
		/// Gets or sets the length.
		/// </summary>
		/// <value>The length.</value>
		public override float Length
		{
			get
			{
				return length;
			}
			set
			{
				length = MathUtil.Clamp(value, 0, float.MaxValue);
				Vector3 vector = Vector3.Normalize(Target - Position);
				Target = Position + vector * length;
			}
		}

		/// <summary>
		/// Gets or sets the rotation.
		/// </summary>
		/// <value>The rotation.</value>
		public override Vector3 Rotation
		{
			get
			{
				return rotation;
			}
			set
			{
				Vector3 vectorRotate = MathHelper.Vector3FromYawPitch(value.X, value.Y);
				vectorRotate.Normalize();

				float length = (Position - Target).Length();
				Target = Position + vectorRotate * length;
				rotation = value;
			}
		}

		/// <summary>
		/// Rotates the specified yaw angle.
		/// </summary>
		/// <param name="yawAngle">The yaw angle.</param>
		/// <param name="pitchAngle">The pitch angle.</param>
		/// <param name="rollAngle">The roll angle.</param>
		public override void Rotate(float yawAngle, float pitchAngle, float rollAngle)
		{
			Rotation += new Vector3(yawAngle, pitchAngle, rollAngle);
		}
	}

	/// <summary>
	/// Class ThirdPersonCamera.
	/// </summary>
	/// <seealso cref="SharpDXWrapper.Camera" />
	public class ThirdPersonCamera : Camera
	{
		/// <summary>
		/// The length
		/// </summary>
		private float length;

		/// <summary>
		/// The rotation
		/// </summary>
		private Vector3 rotation;

		/// <summary>
		/// Initializes a new instance of the <see cref="ThirdPersonCamera"/> class.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="target">The target.</param>
		public ThirdPersonCamera(Vector3 position = default(Vector3), Vector3 target = default(Vector3))
		{
			Position = position;
			Target = target;
			length = (Position - Target).Length();
			rotation = MathHelper.YawPitchRollFromVector3(position - target);
		}

		/// <summary>
		/// Gets or sets the length.
		/// </summary>
		/// <value>The length.</value>
		public override float Length
		{
			get
			{
				return length;
			}
			set
			{
				length = MathUtil.Clamp(value, 0, float.MaxValue);
				Vector3 vector = MathHelper.Vector3FromYawPitch(Rotation.X, Rotation.Y);
				vector.Normalize();
				Position = Target + vector * length;
			}
		}

		/// <summary>
		/// Gets or sets the rotation.
		/// </summary>
		/// <value>The rotation.</value>
		public override Vector3 Rotation
		{
			get
			{
				return rotation;
			}
			set
			{
				value.Y = MathUtil.Clamp(value.Y, -MathUtil.PiOverTwo + MathUtil.DegreesToRadians(1),
					MathUtil.PiOverTwo - MathUtil.DegreesToRadians(1));
				Vector3 vectorRotate = MathHelper.Vector3FromYawPitch(value.X, value.Y);
				vectorRotate.Normalize();

				float length = (Position - Target).Length();
				Position = Target + vectorRotate * length;
				rotation = value;
			}
		}

		/// <summary>
		/// Rotates the specified yaw angle.
		/// </summary>
		/// <param name="yawAngle">The yaw angle.</param>
		/// <param name="pitchAngle">The pitch angle.</param>
		/// <param name="rollAngle">The roll angle.</param>
		public override void Rotate(float yawAngle, float pitchAngle, float rollAngle)
		{
			Rotation += new Vector3(yawAngle, pitchAngle, rollAngle);
		}
	}

	/// <summary>
	/// Interface IProjectionCamera
	/// </summary>
	public interface IProjectionCamera
	{
		/// <summary>
		/// Gets the projection.
		/// </summary>
		/// <value>The projection.</value>
		Matrix Projection { get; }
	}

	/// <summary>
	/// Class PerspectiveCamera.
	/// </summary>
	/// <seealso cref="SharpDXWrapper.IProjectionCamera" />
	public class PerspectiveCamera : IProjectionCamera
	{
		/// <summary>
		/// Gets or sets the fov.
		/// </summary>
		/// <value>The fov.</value>
		public float FOV { get; set; } //Degrees

		/// <summary>
		/// Gets or sets the near z.
		/// </summary>
		/// <value>The near z.</value>
		public float NearZ { get; set; }

		/// <summary>
		/// Gets or sets the far z.
		/// </summary>
		/// <value>The far z.</value>
		public float FarZ { get; set; }

		/// <summary>
		/// Gets or sets the aspect ratio.
		/// </summary>
		/// <value>The aspect ratio.</value>
		public float AspectRatio { get; set; }

		/// <summary>
		/// Gets the projection.
		/// </summary>
		/// <value>The projection.</value>
		public Matrix Projection
		{
			get
			{
				return Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(FOV), AspectRatio, NearZ, FarZ);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerspectiveCamera"/> class.
		/// </summary>
		/// <param name="fov">The fov.</param>
		/// <param name="nearZ">The near z.</param>
		/// <param name="farZ">The far z.</param>
		/// <param name="aspectRation">The aspect ration.</param>
		public PerspectiveCamera(float fov, float nearZ, float farZ, float aspectRation)
		{
			FOV = fov;
			NearZ = nearZ;
			FarZ = farZ;
			AspectRatio = aspectRation;
		}
	}

	/// <summary>
	/// Class OrthogonalCamera.
	/// </summary>
	/// <seealso cref="SharpDXWrapper.IProjectionCamera" />
	public class OrthogonalCamera : IProjectionCamera
	{
		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		/// <value>The width.</value>
		public float Width { get; set; }

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		/// <value>The height.</value>
		public float Height { get; set; }

		/// <summary>
		/// Gets or sets the near z.
		/// </summary>
		/// <value>The near z.</value>
		public float NearZ { get; set; }

		/// <summary>
		/// Gets or sets the far z.
		/// </summary>
		/// <value>The far z.</value>
		public float FarZ { get; set; }

		/// <summary>
		/// Gets the projection.
		/// </summary>
		/// <value>The projection.</value>
		public Matrix Projection
		{
			get
			{
				return Matrix.OrthoLH(Width, Height, NearZ, FarZ);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrthogonalCamera"/> class.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <param name="nearZ">The near z.</param>
		/// <param name="farZ">The far z.</param>
		public OrthogonalCamera(float width, float height, float nearZ, float farZ)
		{
			Width = width;
			Height = height;
			NearZ = nearZ;
			FarZ = farZ;
		}
	}
}