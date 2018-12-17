namespace Verse
{
	public struct AcceptanceReport
	{
		private string reasonTextInt;

		private bool acceptedInt;

		public string Reason => reasonTextInt;

		public bool Accepted => acceptedInt;

		public static AcceptanceReport WasAccepted
		{
			get
			{
				AcceptanceReport result = new AcceptanceReport(string.Empty);
				result.acceptedInt = true;
				return result;
			}
		}

		public static AcceptanceReport WasRejected
		{
			get
			{
				AcceptanceReport result = new AcceptanceReport(string.Empty);
				result.acceptedInt = false;
				return result;
			}
		}

		public AcceptanceReport(string reasonText)
		{
			acceptedInt = false;
			reasonTextInt = reasonText;
		}

		public static implicit operator AcceptanceReport(bool value)
		{
			if (value)
			{
				return WasAccepted;
			}
			return WasRejected;
		}

		public static implicit operator AcceptanceReport(string value)
		{
			return new AcceptanceReport(value);
		}
	}
}
