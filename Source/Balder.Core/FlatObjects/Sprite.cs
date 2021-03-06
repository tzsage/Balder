﻿using Balder.Core.Imaging;
using Balder.Core.Interfaces;
using Balder.Core.Math;
//us ing Balder.Core.Animation;
using Balder.Core.Services;

namespace Balder.Core.FlatObjects
{
	public class Sprite : RenderableNode, IAsset
	{
		private readonly IAssetLoaderService _assetLoaderService;
		private readonly ISpriteContext _spriteContext;

		public Sprite(IAssetLoaderService assetLoaderService, ISpriteContext spriteContext)
		{
			_assetLoaderService = assetLoaderService;
			_spriteContext = spriteContext;
		}


		//public Animatable Animation { get; private set; }

		private Image[] _frames;

		public Image CurrentFrame { get { return _frames[0]; } }


		public override void Render(IViewport viewport, Matrix view, Matrix projection)
		{
			/* From DirectX sample
				w = width passed to D3DXMatrixPerspectiveLH
				h = height passed to D3DXMatrixPerspectiveLH
				n = z near passed to D3DXMatrixPerspectiveLH
				f = z far passed to D3DXMatrixPerspectiveLH
				d = distance of sprite from camera along Z
				qw = width of sprite quad
				qh = height of sprite quad
				vw = viewport height
				vh = viewport width
				scale = n / d
				(i.e. near/distance, such that at d = n, scale = 1.0)
				renderedWidth = vw * qw * scale / w 
				renderedHeight = vh * qh * scale / h
			 */

			var position = new Vector(0, 0, 0);
            var actualPosition = new Vector(World.data[12], World.data[13], World.data[14]);
			var transformedPosition = Vector.Transform(position, World, view);
			var translatedPosition = Vector.Translate(transformedPosition, projection, viewport.Width, viewport.Height);

            var distanceVector = viewport.Camera.Position - actualPosition;
            var distance = distanceVector.Length;
            var n = 100.0f;
            distance = MathHelper.Abs(distance);

            var scale = 0.0f + ((2 * n) / distance);
			if (scale <= 0)
			{
				scale = 0;
			}

			var xscale = scale;
			var yscale = scale;

			_spriteContext.Render(viewport,this,view,projection,World,xscale,yscale,0f);
		}

		public void Load(string assetName)
		{
			var loader = _assetLoaderService.GetLoader<Image>(assetName);
			_frames = loader.Load(assetName);
		}
	}
}
