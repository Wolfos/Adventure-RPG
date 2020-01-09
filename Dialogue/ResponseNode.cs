﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Dialogue
{
	[NodeWidth(300)]
	public class ResponseNode : Node
	{
		[Input(ShowBackingValue.Never)] public Node previous;
		[Output(instancePortList = true)] public List<string> answers = new List<string>();

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