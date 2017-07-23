// ***********************************************************************
// Assembly         : SharpDXWrapper
// Author           : Andrew
// Created          : 07-21-2017
//
// Last Modified By : Andrew
// Last Modified On : 07-21-2017
// ***********************************************************************
using SharpDX;
using SharpDX.D3DCompiler;
using System;
using D3D11 = SharpDX.Direct3D11;

namespace SharpDXWrapper
{
	/// <summary>
	/// Create vertex and pixel shader from loaded shader file
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	/// <seealso cref="SharpDXWrapper.IApply{T}" />
	public class Shader : IDisposable, IApply<D3D11.DeviceContext>
	{
		/// <summary>
		/// Gets or sets the vertex shader.
		/// </summary>
		/// <value>The vertex shader.</value>
		public D3D11.VertexShader VertexShader { get; set; }

		/// <summary>
		/// Gets or sets the pixel shader.
		/// </summary>
		/// <value>The pixel shader.</value>
		public D3D11.PixelShader PixelShader { get; set; }

		/// <summary>
		/// Gets or sets the input layout.
		/// </summary>
		/// <value>The input layout.</value>
		public D3D11.InputLayout InputLayout { get; set; }

		/// <summary>
		/// Gets or sets the input signature.
		/// </summary>
		/// <value>The input signature.</value>
		public ShaderSignature InputSignature { get; set; }

		/// <summary>
		/// Gets or sets the D3D11Device.
		/// </summary>
		/// <value>The device.</value>
		public D3D11.Device Device { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Shader"/> class.
		/// </summary>
		/// <param name="device">The device.</param>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="vertexShaderEntryPoint">The vertex shader entry point.</param>
		/// <param name="pixelShaderEntryPoint">The pixel shader entry point.</param>
		/// <param name="elements">The elements.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="SharpDX.CompilationException">
		/// </exception>
		public Shader(D3D11.Device device, string fileName, string vertexShaderEntryPoint,
			string pixelShaderEntryPoint, D3D11.InputElement[] elements)
		{
			if (device == null || elements == null)
			{
				throw new ArgumentNullException();
			}

			this.Device = device;

			using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(fileName, vertexShaderEntryPoint, "vs_4_0", ShaderFlags.Debug))
			{
				if (vertexShaderByteCode.Bytecode != null)
				{
					using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(fileName, pixelShaderEntryPoint, "ps_4_0", ShaderFlags.Debug))
					{
						if (pixelShaderByteCode.Bytecode != null)
						{
							this.VertexShader = new D3D11.VertexShader(device, vertexShaderByteCode);
							this.PixelShader = new D3D11.PixelShader(device, pixelShaderByteCode);
							this.InputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
							this.InputLayout = new D3D11.InputLayout(device, InputSignature, elements);
							this.Apply(device.ImmediateContext);
						}
						else
						{
							throw new CompilationException(pixelShaderByteCode.ResultCode, pixelShaderByteCode.Message);
						}
					}
				}
				else
				{
					throw new CompilationException(vertexShaderByteCode.ResultCode, vertexShaderByteCode.Message);
				}
			}
		}

		/// <summary>
		/// Apply resources to device context.
		/// </summary>
		/// <param name="deviceContext">The device context.</param>
		public void Apply(D3D11.DeviceContext deviceContext)
		{
			deviceContext.InputAssembler.InputLayout = InputLayout;
			deviceContext.VertexShader.Set(VertexShader);
			deviceContext.PixelShader.Set(PixelShader);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="Shader"/> class.
		/// </summary>
		~Shader()
		{
			Dispose(false);
		}

		/// <summary>
		/// Performs application-defined tasks related to the release or reset of unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (VertexShader != null)
				{
					if (Device.ImmediateContext.VertexShader.Get().NativePointer == VertexShader.NativePointer)
					{
						Device.ImmediateContext.VertexShader.Set(null);
						VertexShader.Dispose();
					}
				}

				if (PixelShader != null)
				{
					if (Device.ImmediateContext.PixelShader.Get().NativePointer == PixelShader.NativePointer)
					{
						Device.ImmediateContext.PixelShader.Set(null);
						PixelShader.Dispose();
					}
				}

				if (InputLayout != null)
					InputLayout.Dispose();
				if (InputSignature != null)
					InputSignature.Dispose();
			}
		}
	}
}