using UnityEngine;
using XNode;

namespace Dialogue
{
	[NodeWidth(300)]
	public class TextNode : Node
	{
		[TextArea] public string text;

		[Input(ShowBackingValue.Never)] public Node previous;

		[Output] public Node next;

		// Use this for initialization
		protected override void Init()
		{
			base.Init();

		}

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return null; // Replace this
		}
	}
}