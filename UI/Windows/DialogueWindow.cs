using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using Dialogue;
using UnityEngine;
using Utility;
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
			
			_dialogueNpc.LookAt(_interactingCharacter.transform.position);
			_interactingCharacter.LookAt(_dialogueNpc.transform.position);
			
			EventManager.OnDialogueStarted?.Invoke(_dialogueNpc);
		}

		private void EndDialogue()
		{
			_dialogueNpc.StopTalk();
			WindowManager.Close(this);
			EventManager.OnDialogueEnded?.Invoke();
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
			_dialogueNpc.StopTalk();

			if (node == null)
			{
				EndDialogue();
				return;
			}
			
			switch (node)
			{
				
				case TextNode tn:
				{
					var text = tn.GetLocalized();
					textDisplay.Activate(text, OnNodeEnded);

					var port = tn.GetOutputPort("next");
				
					if(port.IsConnected && port.Connection != null) _nextNodes.Add(port.Connection.node);

					_dialogueNpc.Talk(tn.animation, text);

					break;
				}

				case ResponseNode rn:
				{
					responseDisplay.Activate(rn.GetLocalized(), OnNodeEnded);
					foreach (var np in rn.Outputs)
					{
						if(np.IsConnected) _nextNodes.Add(np.Connection.node);
					}
					
					break;
				}

				case StartQuestNode qn:
				{
					qn.Execute(_interactingCharacter);
					var port = qn.GetOutputPort("next");
				
					if(port.IsConnected) _nextNodes.Add(port.Connection.node);
					
					OnNodeEnded(0);
					break;
				}
				
				case SetQuestStageNode qn:
				{
					qn.Execute(_interactingCharacter);
					EndDialogue();
					var port = qn.GetOutputPort("next");
					
					if(port.IsConnected) _nextNodes.Add(port.Connection.node);
					
					OnNodeEnded(0);
					break;
				}

				case GetQuestStageNode qn:
				{
					var n = qn.GetNextNode(_interactingCharacter);
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