using UnityEngine;

namespace Utility
{
	public class AnimatorSetBool : StateMachineBehaviour
	{
		[SerializeField] private string variableName;
		[SerializeField] private bool setting;


		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateEnter(animator, stateInfo, layerIndex);
			animator.SetBool(variableName, setting);
		}
	}
}