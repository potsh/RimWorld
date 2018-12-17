using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public struct MaterialRequest : IEquatable<MaterialRequest>
	{
		public Shader shader;

		public Texture2D mainTex;

		public Color color;

		public Color colorTwo;

		public Texture2D maskTex;

		public int renderQueue;

		public List<ShaderParameter> shaderParameters;

		public string BaseTexPath
		{
			set
			{
				mainTex = ContentFinder<Texture2D>.Get(value);
			}
		}

		public MaterialRequest(Texture2D tex)
		{
			shader = ShaderDatabase.Cutout;
			mainTex = tex;
			color = Color.white;
			colorTwo = Color.white;
			maskTex = null;
			renderQueue = 0;
			shaderParameters = null;
		}

		public MaterialRequest(Texture2D tex, Shader shader)
		{
			this.shader = shader;
			mainTex = tex;
			color = Color.white;
			colorTwo = Color.white;
			maskTex = null;
			renderQueue = 0;
			shaderParameters = null;
		}

		public MaterialRequest(Texture2D tex, Shader shader, Color color)
		{
			this.shader = shader;
			mainTex = tex;
			this.color = color;
			colorTwo = Color.white;
			maskTex = null;
			renderQueue = 0;
			shaderParameters = null;
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombine(seed, shader);
			seed = Gen.HashCombineStruct(seed, color);
			seed = Gen.HashCombineStruct(seed, colorTwo);
			seed = Gen.HashCombine(seed, mainTex);
			seed = Gen.HashCombine(seed, maskTex);
			seed = Gen.HashCombineInt(seed, renderQueue);
			return Gen.HashCombine(seed, shaderParameters);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MaterialRequest))
			{
				return false;
			}
			return Equals((MaterialRequest)obj);
		}

		public bool Equals(MaterialRequest other)
		{
			return other.shader == shader && other.mainTex == mainTex && other.color == color && other.colorTwo == colorTwo && other.maskTex == maskTex && other.renderQueue == renderQueue && other.shaderParameters == shaderParameters;
		}

		public static bool operator ==(MaterialRequest lhs, MaterialRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(MaterialRequest lhs, MaterialRequest rhs)
		{
			return !(lhs == rhs);
		}

		public override string ToString()
		{
			return "MaterialRequest(" + shader.name + ", " + mainTex.name + ", " + color.ToString() + ", " + colorTwo.ToString() + ", " + maskTex.ToString() + ", " + renderQueue.ToString() + ")";
		}
	}
}
