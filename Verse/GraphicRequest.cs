using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public struct GraphicRequest : IEquatable<GraphicRequest>
	{
		public Type graphicClass;

		public string path;

		public Shader shader;

		public Vector2 drawSize;

		public Color color;

		public Color colorTwo;

		public GraphicData graphicData;

		public int renderQueue;

		public List<ShaderParameter> shaderParameters;

		public GraphicRequest(Type graphicClass, string path, Shader shader, Vector2 drawSize, Color color, Color colorTwo, GraphicData graphicData, int renderQueue, List<ShaderParameter> shaderParameters)
		{
			this.graphicClass = graphicClass;
			this.path = path;
			this.shader = shader;
			this.drawSize = drawSize;
			this.color = color;
			this.colorTwo = colorTwo;
			this.graphicData = graphicData;
			this.renderQueue = renderQueue;
			this.shaderParameters = ((!shaderParameters.NullOrEmpty()) ? shaderParameters : null);
		}

		public override int GetHashCode()
		{
			if (path == null)
			{
				path = BaseContent.BadTexPath;
			}
			int seed = 0;
			seed = Gen.HashCombine(seed, graphicClass);
			seed = Gen.HashCombine(seed, path);
			seed = Gen.HashCombine(seed, shader);
			seed = Gen.HashCombineStruct(seed, drawSize);
			seed = Gen.HashCombineStruct(seed, color);
			seed = Gen.HashCombineStruct(seed, colorTwo);
			seed = Gen.HashCombine(seed, graphicData);
			seed = Gen.HashCombine(seed, renderQueue);
			return Gen.HashCombine(seed, shaderParameters);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is GraphicRequest))
			{
				return false;
			}
			return Equals((GraphicRequest)obj);
		}

		public bool Equals(GraphicRequest other)
		{
			return graphicClass == other.graphicClass && path == other.path && shader == other.shader && drawSize == other.drawSize && color == other.color && colorTwo == other.colorTwo && graphicData == other.graphicData && renderQueue == other.renderQueue && shaderParameters == other.shaderParameters;
		}

		public static bool operator ==(GraphicRequest lhs, GraphicRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(GraphicRequest lhs, GraphicRequest rhs)
		{
			return !(lhs == rhs);
		}
	}
}
