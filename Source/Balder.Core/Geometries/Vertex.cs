﻿#if(SILVERLIGHT)
using System.Windows.Media;
#else
using System.Drawing;
#endif
using Balder.Core.Math;
using Matrix = Balder.Core.Math.Matrix;

namespace Balder.Core.Geometries
{
	public struct Vertex
	{
		public Vertex(float x, float y, float z) : this()
		{
			Vector = new Math.Vector(x, y, z);
			TransformedVector = new Math.Vector(x, y, z);
			TranslatedVector = new Math.Vector(x, y, z);
			Normal = new Math.Vector();
		}

		public Vector Vector;
		public Vector TransformedVector;
		public Vector TranslatedVector;
		public Vector Normal;
		public Vector TransformedNormal;
		public Vector TransformedVectorNormalized;
		public Vector TranslatedScreenCoordinates;
		public float DepthBufferAdjustedZ;
		public Color Color;

		public void Transform(Matrix world, Matrix view)
		{
			TransformedVector = Vector.Transform(Vector, world);
			TransformedVector = Vector.Transform(TransformedVector, view);
			TransformedNormal = Vector.TransformNormal(Normal, world);
			TransformedNormal = Vector.TransformNormal(TransformedNormal, view);
		}


		public void Translate(Matrix projectionMatrix, float width, float height)
		{
			TranslatedVector = Vector.Translate(TransformedVector, projectionMatrix, width, height);
		}

		public void MakeScreenCoordinates()
		{
			TranslatedScreenCoordinates.X = (int)TranslatedVector.X;
			TranslatedScreenCoordinates.Y = (int)TranslatedVector.Y;
			TranslatedScreenCoordinates.Z = (int)TranslatedVector.Z;
		}
	}
}