using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dialogue;
using Player;
using UnityEngine;
using UnityEngine.UI;
using XNode;

namespace UI
{
	public class DialoguePanel : MonoBehaviour
	{
		private static DialoguePanel instance = null;
		[SerializeField] private DialogueTextDisplay textDisplay;
		[SerializeField] private DialogueResponseDisplay responseDisplay;

		private DialogueNodeGraph nodeGraph;
		private List<Node> nextNodes;

		private void Start()
		{
			if (instance != null)
			{
				Destroy(gameObject);
				return;
			}

			instance = this;
			
			gameObject.SetActive(false);
		}
		

		public static void Activate(string assetPath)
		{
			instance.gameObject.SetActive(true);
			instance.StartCoroutine(instance.StartDialogue(assetPath));
		}

		private IEnumerator StartDialogue(string assetPath)
		{
			yield return null;
			PlayerCharacter.LockInput();
			instance.nodeGraph = Resources.Load<DialogueNodeGraph>(assetPath);
			// Start reading at the first inputless node
			instance.ReadNode(instance.nodeGraph.nodes.Find(x => x.Inputs.All(y => !y.IsConnected)));
		}

		private void EndDialogue()
		{
			PlayerCharacter.UnlockInput();
			gameObject.SetActive(false);
		}

		private void OnNodeEnded(int choice)
		{
			if (nextNodes.Count == 0)
			{
				EndDialogue();
				return;
			}
			
			ReadNode(nextNodes[choice]);
		}
		
		private void ReadNode(Node node)
		{
			nextNodes = new List<Node>();
			textDisplay.DeActivate();
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
				
					if(port.IsConnected) nextNodes.Add(port.Connection.node);

					break;
				}

				case ResponseNode rn:
				{
					responseDisplay.Activate(rn.answers, OnNodeEnded);
					foreach (var np in rn.Outputs)
					{
						if(np.IsConnected) nextNodes.Add(np.Connection.node);
					}
					
					break;
				}

				case StartQuestNode qn:
				{
					qn.Execute();
					var port = qn.GetOutputPort("next");
				
					if(port.IsConnected) nextNodes.Add(port.Connection.node);
					
					OnNodeEnded(0);
					break;
				}
				
				case SetQuestStageNode qn:
				{
					qn.Execute();
					EndDialogue();
					var port = qn.GetOutputPort("next");
					
					if(port.IsConnected) nextNodes.Add(port.Connection.node);
					
					OnNodeEnded(0);
					break;
				}

				case GetQuestStageNode qn:
				{
					var n = qn.GetNextNode();
					nextNodes.Add(n);
					OnNodeEnded(0);
					break;
				}

				case GiveItemNode gin:
				{
					gin.Execute();
					var port = gin.GetOutputPort("next");
				
					if(port.IsConnected) nextNodes.Add(port.Connection.node);
					
					OnNodeEnded(0);
					break;
				}

				default:
					throw new System.NotImplementedException();
			}
		}

		public static void DeActivate()
		{
			instance.gameObject.SetActive(false);
		}
	}
}