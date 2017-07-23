// ***********************************************************************
// Assembly         : SharpDXWrapper
// Author           : Andrew
// Created          : 07-21-2017
//
// Last Modified By : Andrew
// Last Modified On : 07-21-2017
// ***********************************************************************
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Windows;
using System;
using System.Reflection;
using System.Windows.Forms;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace SharpDXWrapper
{
	/// <summary>
	/// Defines generic method for applying resources.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IApply<T>
	{
		/// <summary>
		/// Applies resources.
		/// </summary>
		/// <param name="t">Context to which to apply resources. Example DeviceContext</param>
		void Apply(T t);
	}

	/// <summary>
	/// Defines generic method for drawing resources.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IDraw<T>
	{
		/// <summary>
		/// Draws resources.
		/// </summary>
		/// <param name="t">Context to which to draw resources. Example DeviceContext</param>
		void Draw(T t);
	}

	/// <summary>
	/// Provides the ability to quickly create a Direct3D11Device
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class SharpDXDevice : IDisposable
	{
		/// <summary>
		/// Occurs after preparing the device for rendering.
		/// </summary>
		public event EventHandler OnDraw;

		#region VARS

		private Control renderControl;

		private D3D11.Device d3dDevice;

		private D3D11.DeviceContext d3dContext;

		private DXGI.SwapChain swapChain;

		private D3D11.RenderTargetView d3dRenderTarget;

		private D3D11.DepthStencilView d3dDepthStencil;

		private Viewport viewport;

		/// <summary>
		/// The width of back buffer
		/// </summary>
		private int width;

		/// <summary>
		/// The height of back buffer
		/// </summary>
		private int height;

		/// <summary>
		/// The synchronize interval
		/// </summary>
		private int syncInterval = 1;

		#endregion VARS

		#region FIELDS

		/// <summary>
		/// Gets a value indicating whether this <see cref="SharpDXDevice"/> is initialized.
		/// </summary>
		/// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
		public bool Initialized { get; private set; }

		/// <summary>
		/// Gets the control that is used for rendering.
		/// </summary>
		/// <value>The render control.</value>
		public Control RenderControl
		{
			get
			{
				return renderControl;
			}
		}

		/// <summary>
		/// Gets the D3D11Device.
		/// </summary>
		/// <value>The device.</value>
		public D3D11.Device Device
		{
			get
			{
				return d3dDevice;
			}
		}

		/// <summary>
		/// Gets the immediate context.
		/// </summary>
		/// <value>The context.</value>
		public D3D11.DeviceContext Context
		{
			get
			{
				return d3dContext;
			}
		}

		/// <summary>
		/// Gets the swap chain.
		/// </summary>
		/// <value>The swap chain.</value>
		public DXGI.SwapChain SwapChain
		{
			get
			{
				return swapChain;
			}
		}

		/// <summary>
		/// Gets or sets the background color of backbuffer.
		/// </summary>
		/// <value>The color of the background.</value>
		public SharpDX.Color BackgroundColor { get; set; }

		/// <summary>
		/// Gets or sets the synchronize interval.
		/// </summary>
		/// <value>The synchronize interval.</value>
		public int SyncInterval
		{
			get
			{
				return syncInterval;
			}
			set
			{
				syncInterval = MathUtil.Clamp(value, 0, 3);
			}
		}

		/// <summary>
		/// Gets the width.
		/// </summary>
		/// <value>The width.</value>
		public int Width
		{
			get
			{
				return width;
			}
		}

		/// <summary>
		/// Gets the height.
		/// </summary>
		/// <value>The height.</value>
		public int Height
		{
			get
			{
				return height;
			}
		}

		#endregion FIELDS

		/// <summary>
		/// Initializes a new instance of the <see cref="SharpDXDevice"/> class.
		/// </summary>
		/// <param name="control">The control.</param>
		/// <exception cref="System.ArgumentNullException">control</exception>
		public SharpDXDevice(Control control)
		{
			if (control == null)
			{
				throw new ArgumentNullException("control");
			}

			renderControl = control;
			width = renderControl.ClientSize.Width;
			height = renderControl.ClientSize.Height;

			BackgroundColor = new Color(51, 51, 51);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="SharpDXDevice"/> class.
		/// </summary>
		~SharpDXDevice()
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
				if (d3dRenderTarget != null)
					d3dRenderTarget.Dispose();
				if (d3dDepthStencil != null)
					d3dDepthStencil.Dispose();
				if (swapChain != null)
					swapChain.Dispose();
				if (d3dDevice != null)
					d3dDevice.Dispose();
				if (d3dContext != null)
					d3dContext.Dispose();
			}
		}

		/// <summary>
		/// Creates Direct3D11 Device, RenderTargetView, DepthStencilView, Viewport
		/// </summary>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		public bool Initialize()
		{
			FeatureLevel[] levels = new FeatureLevel[]{
				FeatureLevel.Level_10_0,
				FeatureLevel.Level_10_1,
				FeatureLevel.Level_11_0,
				FeatureLevel.Level_11_1,
			};

			d3dDevice = new D3D11.Device(DriverType.Hardware, D3D11.DeviceCreationFlags.Debug, levels);

			if (d3dDevice == null)
			{
				return false;
			}

			const DXGI.Format bufferFormat = DXGI.Format.R8G8B8A8_UNorm;

			DXGI.ModeDescription backBufferDesc = new DXGI.ModeDescription()
			{
				Width = width,
				Height = height,
				Format = bufferFormat,
				RefreshRate = new DXGI.Rational(60, 1),
				Scaling = DXGI.DisplayModeScaling.Unspecified,
				ScanlineOrdering = DXGI.DisplayModeScanlineOrder.Progressive,
			};

			DXGI.SwapChainDescription swapChainDesc = new DXGI.SwapChainDescription()
			{
				BufferCount = 1,
				Flags = DXGI.SwapChainFlags.None,
				IsWindowed = true,
				ModeDescription = backBufferDesc,
				OutputHandle = renderControl.Handle,
				SampleDescription = new DXGI.SampleDescription(1, 0),
				SwapEffect = DXGI.SwapEffect.Discard,
				Usage = DXGI.Usage.RenderTargetOutput,
			};

			for (int samplerCount = 8; samplerCount > 0; samplerCount--)
			{
				int quality = d3dDevice.CheckMultisampleQualityLevels(bufferFormat, samplerCount);
				if (quality > 0)
				{
					swapChainDesc.SampleDescription = new DXGI.SampleDescription(samplerCount, quality - 1);
					break;
				}
				else if (samplerCount == 1)
				{
					return false;
				}
			}

			DXGI.Device device = d3dDevice.QueryInterface<DXGI.Device>();
			DXGI.Adapter adapter = device.GetParent<DXGI.Adapter>();
			DXGI.Factory factory = adapter.GetParent<DXGI.Factory>();

			swapChain = new DXGI.SwapChain(factory, d3dDevice, swapChainDesc);
			if (swapChain == null)
			{
				return false;
			}

			d3dContext = d3dDevice.ImmediateContext;

			D3D11.RasterizerStateDescription rasterDesc = new D3D11.RasterizerStateDescription()
			{
				CullMode = D3D11.CullMode.Back,
				FillMode = D3D11.FillMode.Solid,
				IsAntialiasedLineEnabled = true,
				IsMultisampleEnabled = true,
				IsDepthClipEnabled = true,
			};

			viewport = new Viewport(0, 0, width, height);

			Initialized = true;

			this.SetRasterizerState(rasterDesc);
			d3dRenderTarget = CreateRenderTarget();
			d3dDepthStencil = CreateDepthStencil();

			return true;
		}

		/// <summary>
		/// Runs the main loop for the specified control.
		/// </summary>
		/// <exception cref="System.Exception"></exception>
		public void Run()
		{
			if (!Initialized)
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + " Device not initialized");
			}
			RenderLoop.Run(renderControl, () => { BeginDraw(); Present(); }, true);
		}

		/// <summary>
		/// Sets the state of the rasterizer.
		/// </summary>
		/// <param name="desc">The desc.</param>
		/// <exception cref="System.Exception"></exception>
		public void SetRasterizerState(D3D11.RasterizerStateDescription desc)
		{
			if (!Initialized)
			{
				System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Device not initialized");
				return;
			}

			if (d3dDevice == null || d3dContext == null)
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + "Device or DeviceContext is null");
			}

			D3D11.RasterizerState rasterState = new D3D11.RasterizerState(d3dDevice, desc);

			if (d3dContext.Rasterizer.State != null)
			{
				d3dContext.Rasterizer.State.Dispose();
			}
			d3dContext.Rasterizer.State = rasterState;
		}

		/// <summary>
		/// Sets the fill mode.
		/// </summary>
		/// <param name="fillMode">The fill mode.</param>
		/// <exception cref="System.Exception"></exception>
		public void SetFillMode(D3D11.FillMode fillMode)
		{
			if (!Initialized)
			{
				System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Device not initialized");
				return;
			}

			if (d3dContext == null)
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + "DeviceContext is null");
			}

			D3D11.RasterizerStateDescription rasterDesc = d3dContext.Rasterizer.State.Description;
			rasterDesc.FillMode = fillMode;

			SetRasterizerState(rasterDesc);
		}

		/// <summary>
		/// Sets the cull mode.
		/// </summary>
		/// <param name="cullMode">The cull mode.</param>
		/// <exception cref="System.Exception"></exception>
		public void SetCullMode(D3D11.CullMode cullMode)
		{
			if (!Initialized)
			{
				System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Device not initialized");
				return;
			}

			if (d3dContext == null)
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + "DeviceContext is null");
			}

			D3D11.RasterizerStateDescription rasterDesc = d3dContext.Rasterizer.State.Description;
			rasterDesc.CullMode = cullMode;

			SetRasterizerState(rasterDesc);
		}

		/// <summary>
		/// Resizes the specified width.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		/// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
		/// <exception cref="System.Exception"></exception>
		public bool Resize(int width, int height)
		{
			if (!Initialized)
			{
				System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Device not initialized");
				return false;
			}

			if (d3dDevice == null || swapChain == null || d3dContext == null)
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + "Device, DeviceContext or SwapChain is null");
			}

			if (width <= 0 || height <= 0)
			{
				System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Width or height less than or equal to zero");
				return false;
			}

			this.width = width;
			this.height = height;

			if (d3dRenderTarget != null)
			{
				d3dContext.OutputMerger.SetRenderTargets(null, (D3D11.RenderTargetView)null);
				d3dRenderTarget.Dispose();
				d3dRenderTarget = null;
			}

			swapChain.ResizeBuffers(1,
				width, height,
				swapChain.Description.ModeDescription.Format,
				swapChain.Description.Flags);

			d3dRenderTarget = CreateRenderTarget();
			d3dDepthStencil = CreateDepthStencil();

			System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Buffers resized " + width + ":" + height);

			viewport = new Viewport(0, 0, width, height);
			return true;
		}

		/// <summary>
		/// Prepares the device for drawing and invoke OnDraw event
		/// </summary>
		/// <exception cref="System.Exception">
		/// </exception>
		public void BeginDraw()
		{
			if (!Initialized)
			{
				System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Device not initialized");
				return;
			}

			if (d3dContext == null)
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + "DeviceContext is null");
			}

			if (d3dRenderTarget != null && viewport != null)
			{
				d3dContext.OutputMerger.SetRenderTargets(d3dDepthStencil, d3dRenderTarget);
				d3dContext.Rasterizer.SetViewport(viewport);
				d3dContext.ClearRenderTargetView(d3dRenderTarget, BackgroundColor);
				d3dContext.ClearDepthStencilView(d3dDepthStencil,
					D3D11.DepthStencilClearFlags.Depth | D3D11.DepthStencilClearFlags.Stencil, 1, 0);

				if (OnDraw != null)
				{
					OnDraw.Invoke(this, EventArgs.Empty);
				}
			}
			else
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + " RenderTarget or Viewport is null");
			}
		}

		/// <summary>
		/// Presents this instance. SwapChain::Present()
		/// </summary>
		/// <exception cref="System.Exception"></exception>
		public void Present()
		{
			if (!Initialized)
			{
				System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Device not initialized");
				return;
			}

			if (swapChain != null)
			{
				swapChain.Present(syncInterval, DXGI.PresentFlags.None);
			}
			else
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + " SwapChain is null");
			}
		}

		/// <summary>
		/// Creates the render target.
		/// </summary>
		/// <returns>D3D11.RenderTargetView.</returns>
		/// <exception cref="System.Exception"></exception>
		private D3D11.RenderTargetView CreateRenderTarget()
		{
			if (!Initialized)
			{
				System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Device not initialized");
				return null;
			}

			if (d3dDevice == null || swapChain == null)
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + "Device or SwapChain is null");
			}

			using (D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0))
			{
				return new D3D11.RenderTargetView(d3dDevice, backBuffer);
			}
		}

		/// <summary>
		/// Creates the depth stencil.
		/// </summary>
		/// <returns>D3D11.DepthStencilView.</returns>
		/// <exception cref="System.Exception"></exception>
		private D3D11.DepthStencilView CreateDepthStencil()
		{
			if (!Initialized)
			{
				System.Diagnostics.Debug.WriteLine(MethodBase.GetCurrentMethod().Name + " Device not initialized");
				return null;
			}

			if (swapChain == null || d3dDevice == null)
			{
				throw new Exception(MethodBase.GetCurrentMethod().Name + "Device or SwapChain is null");
			}

			var DepthStencilTextureDesc = new D3D11.Texture2DDescription
			{
				Format = DXGI.Format.D16_UNorm,
				ArraySize = 1,
				MipLevels = 1,
				Width = width,
				Height = height,
				SampleDescription = swapChain.Description.SampleDescription,
				Usage = D3D11.ResourceUsage.Default,
				BindFlags = D3D11.BindFlags.DepthStencil,
				CpuAccessFlags = D3D11.CpuAccessFlags.None,
				OptionFlags = D3D11.ResourceOptionFlags.None
			};

			using (D3D11.Texture2D backBuffer = new D3D11.Texture2D(d3dDevice, DepthStencilTextureDesc))
			{
				return new D3D11.DepthStencilView(d3dDevice, backBuffer);
			}
		}
	}
}