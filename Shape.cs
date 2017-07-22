// ***********************************************************************
// Assembly         : SharpDXWrapper
// Author           : Andrew
// Created          : 07-21-2017
//
// Last Modified By : Andrew
// Last Modified On : 07-21-2017
// ***********************************************************************
// <copyright file="Shape.cs" company="">
//     Copyright ©  2017
// </copyright>
// <summary></summary>
// ***********************************************************************
using SharpDX;
using System;
using System.Globalization;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace SharpDXWrapper
{
	/// <summary>
	/// Struct DirectXVertex
	/// </summary>
	public struct DirectXVertex
	{
		/// <summary>
		/// The position
		/// </summary>
		public Vector3 Position;

		/// <summary>
		/// The normal
		/// </summary>
		public Vector3 Normal;

		/// <summary>
		/// The color
		/// </summary>
		public Color4 Color;

		/// <summary>
		/// The uv
		/// </summary>
		public Vector2 UV;
	}

	/// <summary>
	/// Class Shape.
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	/// <seealso cref="SharpDXWrapper.IApply{SharpDX.Direct3D11.DeviceContext}" />
	/// <seealso cref="SharpDXWrapper.IDraw{SharpDX.Direct3D11.DeviceContext}" />
	public class Shape : IDisposable, IApply<D3D11.DeviceContext>, IDraw<D3D11.DeviceContext>
	{
		/// <summary>
		/// The input elements
		/// </summary>
		public static readonly D3D11.InputElement[] InputElements = new D3D11.InputElement[]
{
    new D3D11.InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0, D3D11.InputClassification.PerVertexData, 0),
	new D3D11.InputElement("NORMAL", 0, DXGI.Format.R32G32B32_Float, 12, 0, D3D11.InputClassification.PerVertexData, 0),
	new D3D11.InputElement("COLOR", 0, DXGI.Format.R32G32B32_Float, 24, 0, D3D11.InputClassification.PerVertexData, 0),
	new D3D11.InputElement("TEXCOORD", 0, DXGI.Format.R32G32_Float, 36, 0, D3D11.InputClassification.PerVertexData, 0),
};

		/// <summary>
		/// The vertex buffer
		/// </summary>
		protected D3D11.Buffer VertexBuffer;

		/// <summary>
		/// The vertex buffer binding
		/// </summary>
		protected D3D11.VertexBufferBinding VertexBufferBinding;

		/// <summary>
		/// The device
		/// </summary>
		protected D3D11.Device Device;

		/// <summary>
		/// Gets the vertices.
		/// </summary>
		/// <value>The vertices.</value>
		public DirectXVertex[] Vertices { get; private set; }

		/// <summary>
		/// Gets the vertex count.
		/// </summary>
		/// <value>The vertex count.</value>
		public int VertexCount
		{
			get
			{
				return Vertices.Length;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Shape" /> class.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="vertices">The vertices.</param>
		/// <exception cref="System.ArgumentNullException">device</exception>
		/// <exception cref="ArgumentNullException">device</exception>
		public Shape(D3D11.Device device, DirectXVertex[] vertices = null)
		{
			if (device == null)
			{
				throw new ArgumentNullException("device");
			}
			Device = device;

			if (vertices != null)
			{
				SetVertices(vertices);
			}
		}

		/// <summary>
		/// Выполняет определяемые приложением задачи, связанные с высвобождением или сбросом неуправляемых ресурсов.
		/// </summary>
		public virtual void Dispose()
		{
			Vertices = null;

			if (VertexBuffer != null)
			{
				VertexBuffer.Dispose();
			}
		}

		/// <summary>
		/// Applies the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public virtual void Apply(D3D11.DeviceContext context)
		{
			context.InputAssembler.SetVertexBuffers(0, VertexBufferBinding);
		}

		/// <summary>
		/// Draws the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public virtual void Draw(D3D11.DeviceContext context)
		{
			if (Vertices != null)
			{
				context.Draw(Vertices.Length, 0);
			}
		}

		/// <summary>
		/// Sets the vertices.
		/// </summary>
		/// <param name="vertices">The vertices.</param>
		/// <exception cref="System.ArgumentNullException">vertices</exception>
		/// <exception cref="ArgumentNullException">vertices</exception>
		public void SetVertices(DirectXVertex[] vertices)
		{
			if (vertices == null)
			{
				throw new ArgumentNullException("vertices");
			}
			Vertices = vertices;
			if (VertexBuffer != null)
			{
				VertexBuffer.Dispose();
				VertexBuffer = null;
			}
			D3D11.BufferDescription VertexBufferDesc = new D3D11.BufferDescription()
			{
				BindFlags = D3D11.BindFlags.VertexBuffer,
				Usage = D3D11.ResourceUsage.Default,
				CpuAccessFlags = D3D11.CpuAccessFlags.None,
				SizeInBytes = Utilities.SizeOf<DirectXVertex>() * Vertices.Length,
			};
			try
			{
				VertexBuffer = D3D11.Buffer.Create<DirectXVertex>(Device, Vertices, VertexBufferDesc);
				VertexBufferBinding = new D3D11.VertexBufferBinding(VertexBuffer, Utilities.SizeOf<DirectXVertex>(), 0);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Calculates the normals.
		/// </summary>
		/// <param name="vertices">The vertices.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static bool CalculateNormals(DirectXVertex[] vertices)
		{
			if (vertices == null)
			{
				throw new ArgumentNullException();
			}
			if (vertices.Length % 3 != 0)
			{
				System.Diagnostics.Debug.WriteLine("Количество вершин должно быть кратно 3");
				return false;
			}
			for (int i = 0; i < vertices.Length; i += 3)
			{
				Vector3 normal = MathHelper.NormalFromTriangle(vertices[i].Position, vertices[i + 1].Position, vertices[i + 2].Position);
				vertices[i].Normal = vertices[i + 1].Normal = vertices[i + 2].Normal = normal;
			}

			return true;
		}

		/// <summary>
		/// Creates the indexed vertices.
		/// </summary>
		/// <param name="vertices">The vertices.</param>
		/// <param name="indices">The indices.</param>
		/// <returns>DirectXVertex[].</returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static DirectXVertex[] CreateIndexedVertices(DirectXVertex[] vertices, uint[] indices)
		{
			if (vertices == null || indices == null)
			{
				throw new ArgumentNullException();
			}
			DirectXVertex[] indicesVertices = new DirectXVertex[indices.Length];
			for (int i = 0; i < indicesVertices.Length; i++)
			{
				indicesVertices[i] = vertices[indices[i]];
			}
			return indicesVertices;
		}

		/// <summary>
		/// Saves to object file.
		/// </summary>
		/// <param name="vertices">The vertices.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static void SaveToObjFile(DirectXVertex[] vertices, string fileName)
		{
			if (vertices == null || fileName == string.Empty)
			{
				throw new ArgumentNullException();
			}

			using (System.IO.FileStream stream = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
			{
				using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream))
				{
					CultureInfo culture = new CultureInfo("en-US");

					writer.WriteLine("#This is obj file created by Andrew Yakovlev. Date " + DateTime.Now);

					writer.WriteLine(Environment.NewLine + "#Vertices " + vertices.Length);
					foreach (var vertex in vertices)
					{
						writer.WriteLine(String.Format(culture, "v {0:0.000} {1:0.000} {2:0.000}",
							vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
					}

					writer.WriteLine(Environment.NewLine + "#UV " + vertices.Length);
					foreach (var vertex in vertices)
					{
						writer.WriteLine(String.Format(culture, "vt {0:0.000} {1:0.000}",
							vertex.UV.X, vertex.UV.Y));
					}

					writer.WriteLine(Environment.NewLine + "#Normals " + vertices.Length);
					foreach (var vertex in vertices)
					{
						writer.WriteLine(String.Format(culture, "vn {0:0.000} {1:0.000} {2:0.000}",
							vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z));
					}

					writer.WriteLine(Environment.NewLine + "#Faces");
					writer.WriteLine("g Terrain");
					for (int i = 0; i < vertices.Length; i += 3)
					{
						writer.Write("f");
						for (int j = 1; j <= 3; j++)
						{
							writer.Write(" {0}/{1}/{2}", i + j, i + j, i + j);
						}
						writer.WriteLine();
					}
				}
			}
		}
	}

	/// <summary>
	/// Class IndexedShape.
	/// </summary>
	/// <seealso cref="SharpDXWrapper.Shape" />
	public class IndexedShape : Shape
	{
		/// <summary>
		/// The index buffer
		/// </summary>
		private D3D11.Buffer IndexBuffer;

		/// <summary>
		/// Gets the indices.
		/// </summary>
		/// <value>The indices.</value>
		public uint[] Indices { get; private set; }

		/// <summary>
		/// Gets the indices count.
		/// </summary>
		/// <value>The indices count.</value>
		public int IndicesCount
		{
			get
			{
				return Indices.Length;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexedShape" /> class.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="vertices">The vertices.</param>
		/// <param name="indices">The indices.</param>
		public IndexedShape(D3D11.Device device, DirectXVertex[] vertices = null, uint[] indices = null)
			: base(device, vertices)
		{
			if (indices != null)
			{
				SetIndices(indices);
			}
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public override void Dispose()
		{
			Indices = null;
			if (IndexBuffer != null)
			{
				IndexBuffer.Dispose();
			}
			base.Dispose();
		}

		/// <summary>
		/// Applies the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public override void Apply(D3D11.DeviceContext context)
		{
			base.Apply(context);
			context.InputAssembler.SetIndexBuffer(IndexBuffer, DXGI.Format.R32_UInt, 0);
		}

		/// <summary>
		/// Draws the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public override void Draw(D3D11.DeviceContext context)
		{
			if (Vertices != null && Indices != null)
			{
				context.DrawIndexed(Indices.Length, 0, 0);
			}
		}

		/// <summary>
		/// Sets the indices.
		/// </summary>
		/// <param name="indices">The indices.</param>
		/// <exception cref="System.ArgumentNullException">indices</exception>
		/// <exception cref="ArgumentNullException">indices</exception>
		public void SetIndices(uint[] indices)
		{
			if (indices == null)
			{
				throw new ArgumentNullException("indices");
			}
			Indices = indices;
			if (IndexBuffer != null)
			{
				IndexBuffer.Dispose();
				IndexBuffer = null;
			}

			IndexBuffer = D3D11.Buffer.Create<uint>(Device, D3D11.BindFlags.IndexBuffer, Indices);
		}
	}
}