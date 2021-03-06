﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Balder.Core.Exceptions;
using Balder.Core.Geometries;
using Balder.Core.Imaging;
using Balder.Core.Interfaces;
using Balder.Core.Materials;
using Balder.Core.Services;

namespace Balder.Core.AssetLoaders
{
	/// <summary>
	/// Notes:
	/// 3dsmax plugin can be found at: http://www.ozone3d.net/wak/
	/// </summary>
	public class Demoniak3DLoader : AssetLoader<Geometry>
	{
		private IAssetLoaderService _assetLoaderService;

		public override string[] FileExtensions { get { return new[] {"xml"}; } }

		public Demoniak3DLoader(IAssetLoaderService assetLoaderService, IFileLoader fileLoader, IContentManager contentManager)
			: base(fileLoader,contentManager)
		{
			_assetLoaderService = assetLoaderService;
			
		}

		public override Geometry[] Load(string assetName)
		{
			var stream = FileLoader.GetStream(assetName);
			if( null == stream )
			{
				throw new AssetNotFoundException(assetName);
			}
#if(SILVERLIGHT)
			var xmlDocument = XDocument.Load(stream);
#else
			var xmlDocument = XDocument.Load(new StreamReader(stream));
#endif
			var materials = LoadMaterials(xmlDocument);
			var geometries = LoadGeometries(xmlDocument,materials);

			return geometries;
		}

		private Material[]	LoadMaterials(XDocument xmlDocument)
		{
			var materials = new List<Material>();

			var rawMaterials = xmlDocument.Root.Elements("material");

			foreach( var rawMaterial in rawMaterials )
			{
				var material = new Material();
				var textures = rawMaterial.Elements("texture");
				foreach( var texture in textures )
				{
					var filenameAttribute = texture.Attribute("filename");
					if (null != filenameAttribute)
					{
						var filename = filenameAttribute.Value;
						var loader = _assetLoaderService.GetLoader<Image>(filename);
						var frames = loader.Load(filename);
						material.DiffuseMap = frames[0];
						break;
					}
				}
				materials.Add(material);
			}

			return materials.ToArray();
			
		}

		private Geometry[] LoadGeometries(XDocument xmlDocument, Material[] materials)
		{
			var geometries = new List<Geometry>();
			var meshes = xmlDocument.Root.Elements("mesh");
			foreach( var mesh in meshes )
			{
				var geometry = ContentManager.CreateAssetPart<Geometry>();
				geometries.Add(geometry);

				var numVertices = Convert.ToInt32(mesh.Attribute("num_vertices").Value);
				var numFaces = Convert.ToInt32(mesh.Attribute("num_faces").Value);

				geometry.GeometryContext.AllocateVertices(numVertices);
				geometry.GeometryContext.AllocateTextureCoordinates(numVertices);
				geometry.GeometryContext.AllocateFaces(numFaces);

				var vertices = mesh.Elements("v");

				foreach( var vertex in vertices )
				{
					var index = int.Parse(vertex.Attribute("i").Value);
					var x = float.Parse(vertex.Attribute("px").Value);
					var y = -float.Parse(vertex.Attribute("py").Value);
					var z = float.Parse(vertex.Attribute("pz").Value);
					var u = float.Parse(vertex.Attribute("u0").Value);
					var v = float.Parse(vertex.Attribute("v0").Value);
					
					v = 1f - v;

					var actualVertex = new Vertex(x, y, z);
					geometry.GeometryContext.SetVertex(index,actualVertex);

					var textureCoordinate = new TextureCoordinate(u, v);
					geometry.GeometryContext.SetTextureCoordinate(index,textureCoordinate);
				}

				var faces = mesh.Elements("f");

				foreach( var face in faces )
				{
					var index = int.Parse(face.Attribute("i").Value);
					var a = int.Parse(face.Attribute("a").Value);
					var b = int.Parse(face.Attribute("b").Value);
					var c = int.Parse(face.Attribute("c").Value);
					var materialIndex = int.Parse(face.Attribute("matidx").Value);

					var actualFace = new Face
					                 	{
					                 		A = c,
					                 		B = b,
					                 		C = a,
					                 		DiffuseA = a,
					                 		DiffuseB = b,
					                 		DiffuseC = c,
					                 		Material = materials[materialIndex]
					                 	};
					geometry.GeometryContext.SetFace(index,actualFace);
				}
			}

			return geometries.ToArray();
			
		}
	}
}
