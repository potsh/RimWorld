using UnityEngine;

namespace Verse
{
	public class ShaderTypeDef : Def
	{
		[NoTranslate]
		public string shaderPath;

		[Unsaved]
		private Shader shaderInt;

		public Shader Shader
		{
			get
			{
				if (shaderInt == null)
				{
					shaderInt = ShaderDatabase.LoadShader(shaderPath);
				}
				return shaderInt;
			}
		}
	}
}
