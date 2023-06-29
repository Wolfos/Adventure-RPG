using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using Dialogue;
using Player;
using UnityEngine;
using XNode;

namespace UI
{
	public class DialogueWindow : Window
	{
		[SerializeField] private DialogueTextDisplay textDisplay;
		[SerializeField] private DialogueResponseDisplay responseDisplay;

		private List<Node> _nextNodes;
		
		private static DialogueNodeGraph _nodeGraph;
		private static NPC _dialogueNpc;
		private static CharacterBase _interactingCharacter;


		public static void SetData(DialogueNodeGraph asset, NPC dialogueNpc, CharacterBase interactingCharacter)
		{
			_dialogueNpc = dialogueNpc;
			_nodeGraph = asset;
			_interactingCharacter = interactingCharacter;
		}

		private void OnEnable()
		{
			StartCoroutine(StartDialogue(_nodeGraph));
			ShopMenuWindow.OnShoppingDone += OnShoppingDone;
		}

		private void OnDisable()
		{
			ShopMenuWindow.OnShoppingDone -= OnShoppingDone;
		}

		private void OnShoppingDone()
		{
			OnNodeEnded(0);
		}

		private IEnumerator StartDialogue(DialogueNodeGraph asset)
		{
			yield return null;
			// Start reading at the first inputless node
			ReadNode(_nodeGraph.nodes.Find(x => x.Inputs.All(y => !y.IsConnected)));
		}

		private void EndDialogue()
		{
			WindowManager.Close(this);
		}

		private void OnNodeEnded(int choice)
		{
			if (_nextNodes.Count <= choice)
			{
				EndDialogue();
				return;
			}
			
			ReadNode(_nextNodes[choice]);
		}
		
		private void ReadNode(Node node)
		{
			_nextNodes = new();
			textDisplay?.DeActivate();
			responseDisplay.DeActivate();

			if (node == null)
			{
				EndDialogue();
				return;
			}

			switch (node)
			{
				case TextNode tn:
				{
					textDisplay.Activate(tn.text, OnNodeEnded);

					var port = tn.GetOutputPort("next");
				
					if(port.IsConnected && port.Connection != null) _nextNodes.Add(port.Connection.node);

					break;
				}

				case ResponseNode rn:
				{
					responseDisplay.Activate(rn.answers, OnNodeEnded);
					foreach (var np in rn.Outputs)
					{
						if(np.IsConnected) _nextNodes.Add(np.Connection.node);
					}
					
					break;
				}

				case StartQuestNode qn:
				{
					qn.Execute();
					var port = qn.GetOutputPort("next");
				
					if(port.IsConnected) _nextNodes.Add(port.Connection.node);
					
					OnNodeEnded(0);
					break;
				}
				
				case SetQuestStageNode qn:
				{
					qn.Execute();
					EndDialogue();
					var port = qn.GetOutputPort("next");
					
					if(port.IsConnected) _nextNodes.Add(port.Connection.node);
					
					OnNodeEnded(0);
					break;
				}

				case GetQuestStageNode qn:
				{
					var n = qn.GetNextNode();
					_nextNodes.Add(n);
					OnNodeEnded(0);
					break;
				}

				case GiveItemNode gin:
				{
					gin.Execute(_interactingCharacter);
					var port = gin.GetOutputPort("next");
				
					if(port.IsConnected) _nextNodes.Add(port.Connection.node);
					
					OnNodeEnded(0);
					break;
				}

				case OpenShopNode osn:
				{
					var port = osn.GetOutputPort("next");
					if(port.IsConnected) _nextNodes.Add(port.Connection.node);

					_dialogueNpc?.OpenShop();
					
					break;
				}

				default:
					throw new System.NotImplementedException();
			}
		}
	}
}