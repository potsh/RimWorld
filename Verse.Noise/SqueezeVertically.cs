namespace Verse.Noise
{
	public class SqueezeVertically : ModuleBase
	{
		private float factor;

		public SqueezeVertically(ModuleBase input, float factor)
			: base(1)
		{
			modules[0] = input;
			this.factor = factor;
		}

		public override double GetValue(double x, double y, double z)
		{
			return modules[0].GetValue(x, y * (double)factor, z);
		}
	}
}
