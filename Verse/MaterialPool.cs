using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class MaterialPool
	{
		private static Dictionary<MaterialRequest, Material> matDictionary = new Dictionary<MaterialRequest, Material>();

		public static Material MatFrom(string texPath, bool reportFailure)
		{
			if (texPath == null || texPath == "null")
			{
				return null;
			}
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath, reportFailure));
			return MatFrom(req);
		}

		public static Material MatFrom(string texPath)
		{
			if (texPath == null || texPath == "null")
			{
				return null;
			}
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath));
			return MatFrom(req);
		}

		public static Material MatFrom(Texture2D srcTex)
		{
			MaterialRequest req = new MaterialRequest(srcTex);
			return MatFrom(req);
		}

		public static Material MatFrom(Texture2D srcTex, Shader shader, Color color)
		{
			MaterialRequest req = new MaterialRequest(srcTex, shader, color);
			return MatFrom(req);
		}

		public static Material MatFrom(Texture2D srcTex, Shader shader, Color color, int renderQueue)
		{
			MaterialRequest req = new MaterialRequest(srcTex, shader, color);
			req.renderQueue = renderQueue;
			return MatFrom(req);
		}

		public static Material MatFrom(string texPath, Shader shader)
		{
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath), shader);
			return MatFrom(req);
		}

		public static Material MatFrom(string texPath, Shader shader, int renderQueue)
		{
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath), shader);
			req.renderQueue = renderQueue;
			return MatFrom(req);
		}

		public static Material MatFrom(string texPath, Shader shader, Color color)
		{
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath), shader, color);
			return MatFrom(req);
		}

		public static Material MatFrom(string texPath, Shader shader, Color color, int renderQueue)
		{
			MaterialRequest req = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath), shader, color);
			req.renderQueue = renderQueue;
			return MatFrom(req);
		}

		public static Material MatFrom(MaterialRequest req)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to get a material from a different thread.");
				return null;
			}
			if (req.mainTex == null)
			{
				Log.Error("MatFrom with null sourceTex.");
				return BaseContent.BadMat;
			}
			if (req.shader == null)
			{
				Log.Warning("Matfrom with null shader.");
				return BaseContent.BadMat;
			}
			if (req.maskTex != null && !req.shader.SupportsMaskTex())
			{
				Log.Error("MaterialRequest has maskTex but shader does not support it. req=" + req.ToString());
				req.maskTex = null;
			}
			if (!matDictionary.TryGetValue(req, out Material value))
			{
				value = MaterialAllocator.Create(req.shader);
				value.name = req.shader.name + "_" + req.mainTex.name;
				value.mainTexture = req.mainTex;
				value.color = req.color;
				if (req.maskTex != null)
				{
					value.SetTexture(ShaderPropertyIDs.MaskTex, req.maskTex);
					value.SetColor(ShaderPropertyIDs.ColorTwo, req.colorTwo);
				}
				if (req.renderQueue != 0)
				{
					value.renderQueue = req.renderQueue;
				}
				if (!req.shaderParameters.NullOrEmpty())
				{
					for (int i = 0; i < req.shaderParameters.Count; i++)
					{
						req.shaderParameters[i].Apply(value);
					}
				}
				matDictionary.Add(req, value);
				if (!matDictionary.ContainsKey(req))
				{
					Log.Error("MaterialRequest is not present in the dictionary even though we've just added it there. The equality operators are most likely defined incorrectly.");
				}
				if (req.shader == ShaderDatabase.CutoutPlant || req.shader == ShaderDatabase.TransparentPlant)
				{
					WindManager.Notify_PlantMaterialCreated(value);
				}
			}
			return value;
		}
	}
}
