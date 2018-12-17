namespace Verse
{
	public class LoadedContentItem<T> where T : class
	{
		public string internalPath;

		public T contentItem;

		public LoadedContentItem(string path, T contentItem)
		{
			internalPath = path;
			this.contentItem = contentItem;
		}
	}
}
