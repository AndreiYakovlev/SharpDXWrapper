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
	public class Shader : IDisposable, IApply<D3D11.DeviceContext>
	{
		public D3D11.VertexShader VertexShader { get; set; }

		public D3D11.PixelShader PixelShader { get; set; }

		public D3D11.InputLayout InputLayout { get; set; }

		public ShaderSignature InputSignature { get; set; }

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


		public void Apply(D3D11.DeviceContext deviceContext)
		{
			deviceContext.InputAssembler.InputLayout = InputLayout;
			deviceContext.VertexShader.Set(VertexShader);
			deviceContext.PixelShader.Set(PixelShader);
		}

		public void Dispose()
		{
			if (VertexShader != null)
			{
				if (Device.ImmediateContext.VertexShader.Get().NativePointer == VertexShader.NativePointer)
				{
					Device.ImmediateContext.VertexShader.Set(null);
					VertexShader.Dispose();
					VertexShader = null;
				}
			}

			if (PixelShader != null)
			{
				if (Device.ImmediateContext.PixelShader.Get().NativePointer == PixelShader.NativePointer)
				{
					Device.ImmediateContext.PixelShader.Set(null);
					PixelShader.Dispose();
					PixelShader = null;
				}
			}

			InputLayout.Dispose();
			InputSignature.Dispose();
		}
	}
}