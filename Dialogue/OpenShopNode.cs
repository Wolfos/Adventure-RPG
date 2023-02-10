using XNode;

namespace Dialogue
{
	public class OpenShopNode : Node
	{
		[Input(ShowBackingValue.Never)] public Node previous;
		[Output] public Node next;
	}
}