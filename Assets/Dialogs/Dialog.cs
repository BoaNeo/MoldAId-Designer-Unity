using UIComponents;
using Utility;

namespace Dialogs
{
	public class Dialog : PooledObject
	{
		public Transition curtain { get; set; }
		public Transition transition { get; set; }
		
		protected void Hide()
		{
			if(curtain)
				curtain.SetVisible(false);
			transition.SetVisible(false);
		}
	}
}