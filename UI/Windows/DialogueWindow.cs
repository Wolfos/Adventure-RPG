using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using Dialogue;
using UnityEngine;
using Utility;
using XNode;
using Yarn.Unity;

namespace UI
{
	public class DialogueWindow : Window
	{
		[SerializeField] private DialogueTextDisplay textDisplay;
		[SerializeField] private DialogueResponseDisplay responseDisplay;

		private List<Node> _nextNodes;
		
		private static string _startNode;
		private static NPC _dialogueNpc;
		private static CharacterBase _interactingCharacter;
		[SerializeField] private DialogueRunner yarnDialogueRunner;
		[SerializeField] private LineView yarnLineView;
		

		public static void SetData(string startNode, NPC dialogueNpc, CharacterBase interactingCharacter)
		{
			_dialogueNpc = dialogueNpc;
			_startNode = startNode;
			_interactingCharacter = interactingCharacter;
		}

		private void OnEnable()
		{
			StartCoroutine(StartDialogue(_startNode));
			ShopMenuWindow.OnShoppingDone += OnShoppingDone;
			yarnDialogueRunner.onDialogueComplete.AddListener(EndDialogue);
			yarnLineView.OnRunLine += OnRunLine;
			yarnLineView.OnLinePresentFinished += OnLinePresentFinished;
		}

		private void OnDisable()
		{
			ShopMenuWindow.OnShoppingDone -= OnShoppingDone;
			yarnDialogueRunner.onDialogueComplete.RemoveListener(EndDialogue);
			yarnLineView.OnRunLine -= OnRunLine;
			yarnLineView.OnLinePresentFinished -= OnLinePresentFinished;

		}

		private void OnShoppingDone()
		{
		}

		private IEnumerator StartDialogue(string startNode)
		{
			yield return null;
			
			_dialogueNpc.StartDialogue();
			_dialogueNpc.LookAt(_interactingCharacter.transform.position);

			_interactingCharacter.LookAt(_dialogueNpc.transform.position);
			
			EventManager.OnDialogueStarted?.Invoke(_dialogueNpc);
			yarnDialogueRunner.StartDialogue(startNode);
		}

		private void OnRunLine(LocalizedLine line)
		{
			var animation = 0;
			if (ContainsKey(line.Metadata, "animation"))
			{
				animation = GetMetadataValue(line.Metadata, "animation");
			}
			_dialogueNpc.Talk(animation, line.TextWithoutCharacterName.Text);
		}

		private void OnLinePresentFinished()
		{
			_dialogueNpc.StopTalk();
		}

		private bool ContainsKey(string[] metadata, string key)
		{
			return metadata.Any(item => item.StartsWith($"{key}:"));
		}

		private int GetMetadataValue(string[] metadata, string key)
		{
			return (from item in metadata where item.StartsWith($"{key}:") select item.Split(':') into parts where parts.Length == 2 select int.Parse(parts[1])).FirstOrDefault();
		}

		private void EndDialogue()
		{
			_dialogueNpc.StopTalk();
			_dialogueNpc.StopDialogue();
			WindowManager.Close(this);
			EventManager.OnDialogueEnded?.Invoke();
		}
		
	}
}